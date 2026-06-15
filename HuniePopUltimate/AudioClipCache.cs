using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;

/*
    CACHE FILE LAYOUT  (.pcmx)
    [0 ..  3]  magic        "PCMX"   uint32 LE
    [4 ..  7]  version      uint32   (= 2)
    [8 .. 11]  entryCount   uint32
    [12 ..]    entryCount × EntryHeader  (each 48 bytes)
                    clipHash    uint64   FNV-1a of "serializedFileName|pathId"
                    encoding    int32    SampleEncoding enum (0=PCM16, 1=ADPCM)
                    channels    int32
                    sampleRate  int32
                    blockSize   int32   ADPCM: bytes per mono block (0 for PCM16)
                    sampleCount int64   per-channel frames
                    dataOffset  int64   absolute byte offset of blob
                    dataLength  int64   byte count of blob
    [after table]  encoded blobs packed sequentially

    PCM16 blob:  sampleCount * channels * 2 bytes, int16 LE interleaved
    ADPCM blob:  per-channel blocks of `blockSize` bytes, channels interleaved
                Each block decodes to AdpcmDecoder.SamplesPerBlock frames.
                Block layout: [predictor int16][stepIndex byte][reserved byte]
                                [nybble pairs...] (blockSize-4 bytes → (blockSize-4)*2 samples)
*/

public enum SampleEncoding : int
{
    PCM16 = 0,   // lossless int16, 2:1 vs float32
    ADPCM = 1,   // IMA ADPCM, ~4:1 vs float32
}

/*
    IMA ADPCM ENCODER / DECODER
    Standard IMA/DVI ADPCM — identical to what Unity uses internally.
    Block size is fixed at AdpcmDecoder.BlockSize bytes per mono channel.
    Each block is independently decodable → O(1) random seek to block boundary.
*/

public static class AdpcmCodec
{
    // IMA step table (89 entries)
    private static readonly int[] StepTable = {
            7,     8,     9,    10,    11,    12,    13,    14,
           16,    17,    19,    21,    23,    25,    28,    31,
           34,    37,    41,    45,    50,    55,    60,    66,
           73,    80,    88,    97,   107,   118,   130,   143,
          157,   173,   190,   209,   230,   253,   279,   307,
          337,   371,   408,   449,   494,   544,   598,   658,
          724,   796,   876,   963,  1060,  1166,  1282,  1411,
         1552,  1707,  1878,  2066,  2272,  2499,  2749,  3024,
         3327,  3660,  4026,  4428,  4871,  5358,  5894,  6484,
         7132,  7845,  8630,  9493, 10442, 11487, 12635, 13899,
        15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
        32767
    };

    // IMA index adjustment table
    private static readonly int[] IndexTable = {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8
    };

    /// <summary>
    /// Number of PCM samples (per channel) encoded in one ADPCM block.
    /// Formula: header holds 1 sample; remaining (BlockSize-4) bytes hold 2 nybbles each.
    /// </summary>
    public static int SamplesPerBlock(int blockSize) => 1 + (blockSize - 4) * 2;

    /// <summary>Recommended block size: 512 bytes → 1017 samples per block per channel.</summary>
    public const int DefaultBlockSize = 512;

    /// <summary>
    /// Encodes a float[] PCM buffer (interleaved, any channel count) into
    /// a byte[] of interleaved per-channel ADPCM blocks.
    /// </summary>
    public static byte[] Encode(float[] pcm, int channels, int blockSize, out long totalBlocks)
    {
        var samplesPerBlock = SamplesPerBlock(blockSize);
        var totalFrames = pcm.Length / channels;
        var blocksPerChan = (totalFrames + samplesPerBlock - 1) / samplesPerBlock;
        totalBlocks = blocksPerChan;

        // for each frame-group of samplesPerBlock, write channels blocks sequentially
        var output = new byte[blocksPerChan * channels * blockSize];
        var outPos = 0;

        for (int b = 0; b < blocksPerChan; b++)
        {
            int frameStart = b * samplesPerBlock;
            int frameEnd = Math.Min(frameStart + samplesPerBlock, totalFrames);

            for (int ch = 0; ch < channels; ch++)
            {
                EncodeBlock(pcm, channels, ch, frameStart, frameEnd, totalFrames,
                            blockSize, output, outPos);
                outPos += blockSize;
            }
        }

        return output;
    }

    private static void EncodeBlock(
        float[] pcm, int channels, int ch,
        int frameStart, int frameEnd, int totalFrames,
        int blockSize, byte[] output, int outBase)
    {
        int samplesPerBlock = SamplesPerBlock(blockSize);

        // Seed predictor from first sample of block
        int firstSample = frameStart < totalFrames
            ? FloatToInt16(pcm[frameStart * channels + ch])
            : 0;

        // Simple step index initialization: pick based on signal energy
        int stepIndex = 0;

        // Write block header: predictor (int16 LE), stepIndex (byte), reserved (byte)
        output[outBase + 0] = (byte)(firstSample & 0xFF);
        output[outBase + 1] = (byte)((firstSample >> 8) & 0xFF);
        output[outBase + 2] = (byte)stepIndex;
        output[outBase + 3] = 0; // reserved

        var predictor = firstSample;
        var dataPos = outBase + 4;
        var nybbleIdx = 0;
        byte packedByte = 0;

        // Encode samples (skip first — it's in the header)
        for (int frame = frameStart + 1; frame < frameEnd || nybbleIdx % 2 != 0; frame++)
        {
            var sample = (frame < frameEnd && frame < totalFrames)
                ? FloatToInt16(pcm[frame * channels + ch])
                : predictor; // pad with predictor to fill block

            var step = StepTable[stepIndex];
            var diff = sample - predictor;
            var nybble = 0;

            if (diff < 0) { nybble = 8; diff = -diff; }

            if (diff >= step) 
            { 
                nybble |= 4; 
                diff -= step; 
            }

            if (diff >= step >> 1)     
            {
                nybble |= 2; 
                diff -= step >> 1; 
            }

            if (diff >= step >> 2)     
            { 
                nybble |= 1; 
            }

            // Update predictor
            step = StepTable[stepIndex];
            var vpdiff = step >> 3;
            if ((nybble & 4) != 0) vpdiff += step;
            if ((nybble & 2) != 0) vpdiff += step >> 1;
            if ((nybble & 1) != 0) vpdiff += step >> 2;

            if ((nybble & 8) != 0) 
            {
                predictor -= vpdiff;
            }
            else
            {
                predictor += vpdiff;
            }

            predictor = Clamp16(predictor);
            stepIndex = Clamp(stepIndex + IndexTable[nybble & 7], 0, 88);

            // Pack two nybbles per byte (low nybble first)
            if (nybbleIdx % 2 == 0)
            {
                packedByte = (byte)(nybble & 0x0F);
            }
            else
            {
                packedByte |= (byte)((nybble & 0x0F) << 4);
                if (dataPos < outBase + blockSize)
                    output[dataPos++] = packedByte;
            }
            nybbleIdx++;
        }
    }

    /// <summary>
    /// Decodes a single ADPCM block for one channel into <paramref name="outPcm"/>
    /// starting at <paramref name="outOffset"/>.
    /// Returns the number of float samples written.
    /// </summary>
    public static int DecodeBlock(
        byte[] data, int dataOffset, int blockSize,
        float[] outPcm, int outOffset)
    {
        // Read block header
        int predictor = (short)(data[dataOffset] | (data[dataOffset + 1] << 8));
        int stepIndex = Clamp(data[dataOffset + 2], 0, 88);
        // byte 3 is reserved

        var samplesPerBlock = SamplesPerBlock(blockSize);
        var written = 0;

        // First sample from header
        if (outOffset + written < outPcm.Length)
        {
            outPcm[outOffset + written++] = predictor / 32768f;
        }
            

        var src = dataOffset + 4;

        for (var i = 0; i < (blockSize - 4) && written < samplesPerBlock; i++)
        {
            var b = data[src++];

            for (var half = 0; half < 2 && written < samplesPerBlock; half++)
            {
                var nybble = (half == 0) ? (b & 0x0F) : ((b >> 4) & 0x0F);

                var step = StepTable[stepIndex];
                var vpdiff = step >> 3;
                if ((nybble & 4) != 0) vpdiff += step;
                if ((nybble & 2) != 0) vpdiff += step >> 1;
                if ((nybble & 1) != 0) vpdiff += step >> 2;

                if ((nybble & 8) != 0)
                {
                    predictor -= vpdiff;
                }
                else
                {
                    predictor += vpdiff;
                }

                predictor = Clamp16(predictor);
                stepIndex = Clamp(stepIndex + IndexTable[nybble], 0, 88);

                if (outOffset + written < outPcm.Length)
                {
                    outPcm[outOffset + written++] = predictor / 32768f;
                }
            }
        }

        return written;
    }

    public static int FloatToInt16(float f)
        => Clamp16((int)(f * 32768f));

    private static int Clamp16(int v)
        => v < -32768 ? -32768 : v > 32767 ? 32767 : v;

    private static int Clamp(int v, int lo, int hi)
        => v < lo ? lo : v > hi ? hi : v;
}

public static class AudioPcmCache
{
    public const uint Magic = 0x584D4350u; // "PCMX" LE
    public const uint Version = 2u;
    public const int HeaderBase = 12; // magic(4) + version(4) + count(4)
    public const int EntrySize = 48; // see layout above

    public struct EntryHeader
    {
        public ulong ClipHash; // 8
        public SampleEncoding Encoding; // 4
        public int Channels; // 4
        public int SampleRate; // 4
        public int BlockSize; // 4 ADPCM block size in bytes (0 for PCM16)
        public long SampleCount; // 8 per-channel frames
        public long DataOffset; // 8
        public long DataLength; // 8
        // 48 total
    }

    public struct CacheBuildRequest
    {
        /// <summary>
        /// Stable key — use <see cref="MakeKey(SerializedFile, long)"/>.
        /// </summary>
        public string Key;

        /// <summary>Raw Ogg/Vorbis bytes from <c>AudioClip.m_AudioData</c>.</summary>
        public ResourceReader AudioData;
    }

    public static string MakeKey(SerializedFile file, long pathId)
        => $"{file.fileName}|{pathId}";

    public static ulong MakeHash(string key)
    {
        var h = 14695981039346656037UL;
        const ulong p = 1099511628211UL;
        foreach (var b in Encoding.UTF8.GetBytes(key)) { h ^= b; h *= p; }
        return h;
    }

    /// <summary>
    /// Decodes all Ogg clips in <paramref name="requests"/>, encodes them with
    /// <paramref name="encoding"/>, writes a single .pcmx file, and returns an
    /// open reader.
    /// </summary>
    /// <param name="encoding">
    /// <see cref="SampleEncoding.PCM16"/> → lossless, ~1.5 GB for a 3 GB float32 set.<br/>
    /// <see cref="SampleEncoding.ADPCM"/>  → IMA ADPCM, ~750 MB, minimal quality loss.
    /// </param>
    public static AudioPcmCacheReader Build(
        IReadOnlyList<CacheBuildRequest> requests,
        string cacheFilePath,
        SampleEncoding encoding = SampleEncoding.ADPCM)
    {
        if (requests == null || requests.Count == 0)
        {
            throw new ArgumentException("At least one request required.", nameof(requests));
        }

        var blockSize = encoding == SampleEncoding.ADPCM
            ? AdpcmCodec.DefaultBlockSize
            : 0;

        var entries = new EntryHeader[requests.Count];
        var blobs = new byte[requests.Count][];

        for (int i = 0; i < requests.Count; i++)
        {
            var ogg = requests[i].AudioData.GetData();

            int channels, sampleRate, totalSamples;
            using (var ms = new MemoryStream(ogg, false))
            using (var vr = new VorbisReader(ms, closeOnDispose: true))
            {
                channels = vr.Channels;
                sampleRate = vr.SampleRate;
                totalSamples = (int)vr.TotalSamples;
            }

            var interleaved = totalSamples * channels;
            var pcm = new float[interleaved];

            using (var ms = new MemoryStream(ogg, false))
            using (var vr = new VorbisReader(ms, closeOnDispose: true))
            {
                int read = 0;
                while (read < interleaved)
                {
                    int got = vr.ReadSamples(pcm, read, interleaved - read);
                    if (got == 0) break;
                    read += got;
                }
                if (read < interleaved) Array.Clear(pcm, read, interleaved - read);
            }

            byte[] blob;
            if (encoding == SampleEncoding.PCM16)
            {
                blob = EncodePcm16(pcm);
            }
            else
            {
                blob = AdpcmCodec.Encode(pcm, channels, blockSize, out _);
            }

            blobs[i] = blob;
            entries[i] = new EntryHeader
            {
                ClipHash = MakeHash(requests[i].Key),
                Encoding = encoding,
                Channels = channels,
                SampleRate = sampleRate,
                BlockSize = blockSize,
                SampleCount = totalSamples,
            };
        }

        long pos = HeaderBase + (long)requests.Count * EntrySize;
        for (int i = 0; i < requests.Count; i++)
        {
            entries[i].DataOffset = pos;
            entries[i].DataLength = blobs[i].Length;
            pos += blobs[i].Length;
        }

        string dir = Path.GetDirectoryName(cacheFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using (var fs = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: false))
        {
            bw.Write(Magic);
            bw.Write(Version);
            bw.Write((uint)requests.Count);

            foreach (EntryHeader e in entries)
            {
                bw.Write(e.ClipHash);
                bw.Write((int)e.Encoding);
                bw.Write(e.Channels);
                bw.Write(e.SampleRate);
                bw.Write(e.BlockSize);
                bw.Write(e.SampleCount);
                bw.Write(e.DataOffset);
                bw.Write(e.DataLength);
            }

            foreach (byte[] blob in blobs)
                fs.Write(blob, 0, blob.Length);

        } // file closed before reader opens

        return new AudioPcmCacheReader(cacheFilePath);
    }

    internal static byte[] EncodePcm16(float[] pcm)
    {
        var bytes = new byte[pcm.Length * 2];
        for (int i = 0; i < pcm.Length; i++)
        {
            short s = (short)AdpcmCodec.FloatToInt16(pcm[i]);
            bytes[i * 2] = (byte)(s & 0xFF);
            bytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }
        return bytes;
    }
}

/// <summary>
/// Opened once; reads the header index into a dictionary and hands out
/// <see cref="PcmSliceHandle"/>s.
/// </summary>
public sealed class AudioPcmCacheReader
{
    private readonly string _path;
    private readonly Dictionary<ulong, AudioPcmCache.EntryHeader> _index;

    public string FilePath => _path;
    public int Count => _index.Count;

    public AudioPcmCacheReader(string cacheFilePath)
    {
        _path = cacheFilePath ?? throw new ArgumentNullException(nameof(cacheFilePath));
        _index = new Dictionary<ulong, AudioPcmCache.EntryHeader>();

        using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: false);

        var magic = br.ReadUInt32();
        var version = br.ReadUInt32();
        var count = br.ReadUInt32();

        if (magic != AudioPcmCache.Magic)
        {
            throw new InvalidDataException($"Not a .pcmx file: {_path}");
        }
            
        if (version != AudioPcmCache.Version)
        {
            throw new InvalidDataException(
                $"Unsupported .pcmx version {version} (expected {AudioPcmCache.Version}): {_path}. " +
                $"Delete the cache file to regenerate it.");
        }

        for (uint i = 0; i < count; i++)
        {
            var e = new AudioPcmCache.EntryHeader
            {
                ClipHash = br.ReadUInt64(),
                Encoding = (SampleEncoding)br.ReadInt32(),
                Channels = br.ReadInt32(),
                SampleRate = br.ReadInt32(),
                BlockSize = br.ReadInt32(),
                SampleCount = br.ReadInt64(),
                DataOffset = br.ReadInt64(),
                DataLength = br.ReadInt64(),
            };
            _index[e.ClipHash] = e;
        }
    }

    public PcmSliceHandle OpenSlice(string key)
    {
        ulong hash = AudioPcmCache.MakeHash(key);
        if (!_index.TryGetValue(hash, out var e)) return null;

        return e.Encoding switch
        {
            SampleEncoding.PCM16 => new Pcm16SliceHandle(
                _path, e.Channels, e.SampleRate, e.SampleCount, e.DataOffset, e.DataLength),
            SampleEncoding.ADPCM => new AdpcmSliceHandle(
                _path, e.Channels, e.SampleRate, e.SampleCount, e.DataOffset, e.DataLength, e.BlockSize),
            _ => throw new InvalidDataException($"Unknown encoding {e.Encoding}"),
        };
    }

    public PcmSliceHandle OpenSlice(SerializedFile file, long pathId)
        => OpenSlice(AudioPcmCache.MakeKey(file, pathId));

    public bool Contains(string key)
        => _index.ContainsKey(AudioPcmCache.MakeHash(key));

    public bool Contains(SerializedFile file, long pathId)
        => Contains(AudioPcmCache.MakeKey(file, pathId));
}

/// <summary>
/// Abstract base for per-clip streaming handles.
/// Subclasses implement the actual decode in <see cref="FillBuffer"/>.
/// </summary>
public abstract class PcmSliceHandle
{
    public readonly int Channels;
    public readonly int SampleRate;
    public readonly long SampleCount; // per-channel frames

    protected readonly string _filePath;
    protected readonly long _dataOffset;
    protected readonly long _dataLength;

    protected FileStream _fs;
    protected long _pos; // current per-channel frame position
    protected bool _ended;

    protected PcmSliceHandle(
        string filePath, int channels, int sampleRate,
        long sampleCount, long dataOffset, long dataLength)
    {
        _filePath = filePath;
        Channels = channels;
        SampleRate = sampleRate;
        SampleCount = sampleCount;
        _dataOffset = dataOffset;
        _dataLength = dataLength;
    }

    public void OnPCMRead(float[] data)
    {
        if (_ended) { Array.Clear(data, 0, data.Length); return; }

        EnsureStream();

        long remaining = SampleCount - _pos;
        if (remaining <= 0) { _ended = true; Array.Clear(data, 0, data.Length); return; }

        int written = FillBuffer(data, (int)Math.Min(data.Length, remaining * Channels));

        _pos += written / Channels;
        if (_pos >= SampleCount) _ended = true;
        if (written < data.Length) Array.Clear(data, written, data.Length - written);
    }

    public void OnPCMSetPosition(int sampleFrame)
    {
        _ended = false;
        SeekTo(Math.Max(0L, Math.Min(sampleFrame, SampleCount - 1)));
    }

    public void ReleaseStream() { _fs?.Dispose(); _fs = null; }

    /// <summary>
    /// Fill up to <paramref name="maxFloats"/> interleaved floats into
    /// <paramref name="data"/> starting at index 0.
    /// The FileStream is open and positioned correctly before this call.
    /// Returns the number of floats actually written.
    /// </summary>
    protected abstract int FillBuffer(float[] data, int maxFloats);

    /// <summary>
    /// Seek internal state and the FileStream to <paramref name="frame"/>
    /// (per-channel sample frame).
    /// </summary>
    protected abstract void SeekTo(long frame);

    protected void EnsureStream()
    {
        if (_fs == null || !_fs.CanRead)
            _fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read,
                                 FileShare.Read, bufferSize: 4096);
    }
}

/// <summary>
/// Streams int16 PCM from the .pcmx file, converting to float32 on the fly.
/// Per-callback cost: 1 seek + N reads + N multiplies. Zero extra allocations.
/// </summary>
public sealed class Pcm16SliceHandle : PcmSliceHandle
{
    // Reusable byte scratch: sized for 4096 interleaved int16 samples
    private readonly byte[] _scratch;

    internal Pcm16SliceHandle(
        string filePath, int channels, int sampleRate,
        long sampleCount, long dataOffset, long dataLength)
        : base(filePath, channels, sampleRate, sampleCount, dataOffset, dataLength)
    {
        _scratch = new byte[4096 * channels * sizeof(short)];
    }

    protected override void SeekTo(long frame)
    {
        _pos = frame;
        // No cached state to reset for PCM16
    }

    protected override int FillBuffer(float[] data, int maxFloats)
    {
        // Seek to the current frame's byte position
        _fs.Seek(_dataOffset + _pos * Channels * sizeof(short), SeekOrigin.Begin);

        int done = 0;
        while (done < maxFloats)
        {
            int floatsThisChunk = Math.Min(maxFloats - done, _scratch.Length / sizeof(short));
            int bytesToRead = floatsThisChunk * sizeof(short);
            int bytesRead = _fs.Read(_scratch, 0, bytesToRead);
            if (bytesRead == 0) break;

            int shortsRead = bytesRead / sizeof(short);
            for (int j = 0; j < shortsRead && done < maxFloats; j++, done++)
            {
                short s = (short)(_scratch[j * 2] | (_scratch[j * 2 + 1] << 8));
                data[done] = s / 32768f;
            }
        }
        return done;
    }
}

/// <summary>
/// Streams IMA ADPCM from the .pcmx file, decoding one block per channel
/// per callback. Seeking snaps to the nearest block boundary then decodes
/// forward sample-by-sample to the exact frame — O(blockSize) worst case.
/// </summary>
public sealed class AdpcmSliceHandle : PcmSliceHandle
{
    private readonly int _blockSize;
    private readonly int _samplesPerBlock;
    private readonly int _blockStride; // bytes for one full frame-group (all channels)

    // Per-block decode cache: decoded float[] for the current block, all channels interleaved
    private readonly float[] _blockCache;
    private readonly byte[] _rawBlock; // one channel's raw block bytes
    private long _cachedBlockIndex = -1;

    internal AdpcmSliceHandle(
        string filePath, int channels, int sampleRate,
        long sampleCount, long dataOffset, long dataLength, int blockSize)
        : base(filePath, channels, sampleRate, sampleCount, dataOffset, dataLength)
    {
        _blockSize = blockSize;
        _samplesPerBlock = AdpcmCodec.SamplesPerBlock(blockSize);
        _blockStride = blockSize * channels;
        // Cache holds one full interleaved block (all channels)
        _blockCache = new float[_samplesPerBlock * channels];
        _rawBlock = new byte[blockSize];
    }

    protected override void SeekTo(long frame)
    {
        _pos = frame;
        _cachedBlockIndex = -1; // invalidate cache; next FillBuffer will re-decode
    }

    protected override int FillBuffer(float[] data, int maxFloats)
    {
        int done = 0;

        while (done < maxFloats)
        {
            var blockIndex = _pos / _samplesPerBlock;
            var frameInBlock = (int)(_pos % _samplesPerBlock);

            // Decode block into cache if not already there
            if (blockIndex != _cachedBlockIndex)
            {
                DecodeBlock(blockIndex);
                _cachedBlockIndex = blockIndex;
            }

            // Copy from cache: from frameInBlock to end of block (or maxFloats)
            var cacheStart = frameInBlock * Channels;
            var cacheEnd = _samplesPerBlock * Channels;
            var floatsInCache = cacheEnd - cacheStart;
            var floatsToCopy = Math.Min(floatsInCache, maxFloats - done);

            Array.Copy(_blockCache, cacheStart, data, done, floatsToCopy);
            done += floatsToCopy;
            _pos += floatsToCopy / Channels;

            if (_pos >= SampleCount) break;
        }

        return done;
    }

    private void DecodeBlock(long blockIndex)
    {
        EnsureStream();

        // Clear cache in case this is a partial last block
        Array.Clear(_blockCache, 0, _blockCache.Length);

        long blockGroupOffset = _dataOffset + blockIndex * _blockStride;

        for (int ch = 0; ch < Channels; ch++)
        {
            var blockOffset = blockGroupOffset + (ch * _blockSize);
            if (blockOffset >= _dataOffset + _dataLength) break;

            _fs.Seek(blockOffset, SeekOrigin.Begin);
            var bytesRead = _fs.Read(_rawBlock, 0, _blockSize);
            if (bytesRead < 4) break;

            // Decode into a temporary per-channel buffer then interleave into _blockCache
            // We decode directly into _blockCache at interleaved positions
            int predictor = (short)(_rawBlock[0] | (_rawBlock[1] << 8));
            var stepIndex = Math.Max(0, Math.Min((int)_rawBlock[2], 88));

            // Write first sample (from block header)
            if (ch < Channels)
                _blockCache[0 * Channels + ch] = predictor / 32768f;

            var outFrame = 1;
            var src = 4;

            for (int i = 0; i < (bytesRead - 4) && outFrame < _samplesPerBlock; i++)
            {
                byte b = _rawBlock[src++];

                for (int half = 0; half < 2 && outFrame < _samplesPerBlock; half++)
                {
                    int nybble = (half == 0) ? (b & 0x0F) : ((b >> 4) & 0x0F);

                    var step = AdpcmStepTable[stepIndex];
                    var vpdiff = step >> 3;
                    if ((nybble & 4) != 0) vpdiff += step;
                    if ((nybble & 2) != 0) vpdiff += step >> 1;
                    if ((nybble & 1) != 0) vpdiff += step >> 2;

                    if ((nybble & 8) != 0) predictor -= vpdiff;
                    else                   predictor += vpdiff;

                    predictor = predictor < -32768 ? -32768 : predictor > 32767 ? 32767 : predictor;
                    stepIndex = Math.Max(0, Math.Min(stepIndex + AdpcmIndexTable[nybble & 0xF], 88));

                    _blockCache[outFrame * Channels + ch] = predictor / 32768f;
                    outFrame++;
                }
            }
        }
    }

    // Local copies to avoid extra indirection in the hot decode loop
    private static readonly int[] AdpcmStepTable = {
            7,     8,     9,    10,    11,    12,    13,    14,
           16,    17,    19,    21,    23,    25,    28,    31,
           34,    37,    41,    45,    50,    55,    60,    66,
           73,    80,    88,    97,   107,   118,   130,   143,
          157,   173,   190,   209,   230,   253,   279,   307,
          337,   371,   408,   449,   494,   544,   598,   658,
          724,   796,   876,   963,  1060,  1166,  1282,  1411,
         1552,  1707,  1878,  2066,  2272,  2499,  2749,  3024,
         3327,  3660,  4026,  4428,  4871,  5358,  5894,  6484,
         7132,  7845,  8630,  9493, 10442, 11487, 12635, 13899,
        15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
        32767
    };

    private static readonly int[] AdpcmIndexTable = {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8
    };
}

/// <summary>
/// Drop-in replacement for <c>AudioClipInfoVorbis</c>.
/// Wraps a <see cref="PcmSliceHandle"/> in the <see cref="IGameDefinitionInfo{T}"/> contract.
/// </summary>
public sealed class AudioClipInfoCached : IGameDefinitionInfo<UnityEngine.AudioClip>
{
    private readonly PcmSliceHandle _handle;
    private readonly string _name;

    public AudioClipInfoCached(PcmSliceHandle handle, string name = "CachedClip")
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _name = name;
    }

    public void SetData(
        ref UnityEngine.AudioClip def,
        GameDefinitionProvider gameData,
        AssetProvider assetProvider,
        InsertStyle insertStyle)
    {
        def = UnityEngine.AudioClip.Create(
            _name,
            (int)_handle.SampleCount,
            _handle.Channels,
            _handle.SampleRate,
            true,
            _handle.OnPCMRead,
            _handle.OnPCMSetPosition);
    }

    public void RequestInternals(AssetProvider assetProvider) { }
}