using System;
using System.IO;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;

public class AudioClipInfoVorbis : IGameDefinitionInfo<UnityEngine.AudioClip>
{
    private readonly ResourceReader _resourceReader;
    private MemoryStream _memoryStream;
    private VorbisReader _reader;
    private bool _readerReady;
    private int _channels;
    private int _sampleRate;
    private int _totalSamples;
    private long _startPos;
    private long _endPos;
    private long _durationSamples;

    public AudioClipInfoVorbis(ResourceReader resourceReader, double startSeconds = 0.0, double durationSeconds = -1.0)
    {
        if (resourceReader == null)
            throw new ArgumentNullException(nameof(resourceReader));

        _resourceReader = resourceReader;
        _readerReady = false;

        // Load the full Ogg data
        byte[] fullData = _resourceReader.GetData();
        using (var stream = new MemoryStream(fullData, writable: false))
        using (var tempReader = new VorbisReader(stream, true))
        {
            _channels = tempReader.Channels;
            _sampleRate = tempReader.SampleRate;
            _totalSamples = (int)tempReader.TotalSamples;
        }

        // Calculate playback range
        _startPos = (long)(startSeconds * _sampleRate);
        if (durationSeconds > 0.0)
        {
            _durationSamples = (long)(durationSeconds * _sampleRate);
            _endPos = _startPos + _durationSamples;
        }
        else
        {
            _endPos = _totalSamples;
            _durationSamples = _endPos - _startPos;
        }

        if (_startPos < 0)
            _startPos = 0;
        if (_endPos > _totalSamples)
            _endPos = _totalSamples;
    }

    public void SetData(ref UnityEngine.AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = UnityEngine.AudioClip.Create(
            string.Empty,
            (int)_durationSamples,
            _channels,
            _sampleRate,
            true,
            this.OnPCMReaderCallback,
            this.OnPCMSetPositionCallback
        );
    }

    private void EnsureReaderInitialized()
    {
        if (_reader != null)
            return;

        _memoryStream = new MemoryStream(_resourceReader.GetData(), writable: false);
        _reader = new VorbisReader(_memoryStream, true);
        _readerReady = false;
    }

    private void OnPCMReaderCallback(float[] data)
    {
        this.EnsureReaderInitialized();

        int read = _reader.ReadSamples(data);
        if (!_readerReady && read > 0)
            _readerReady = true;

        if (read < data.Length)
        {
            for (int i = read; i < data.Length; i++)
                data[i] = 0f;
        }
    }

    private void OnPCMSetPositionCallback(int position)
    {
        this.EnsureReaderInitialized();

        if (!_readerReady)
            return;

        long absolutePos = _startPos + position;
        if (absolutePos < _startPos)
            absolutePos = _startPos;
        if (absolutePos >= _endPos)
            absolutePos = _endPos - 1;

        _reader.SamplePosition = absolutePos;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        // No-op
    }

    /// <summary>
    /// Reads the exact Ogg Vorbis header (3 packets) from the BinaryReader.
    /// Returns the total byte length to extract.
    /// </summary>
    private static int ReadExactOggHeader(BinaryReader reader, int maxSize)
    {
        long startPos = reader.BaseStream.Position;
        int headerCount = 0;

        while (headerCount < 3 && reader.BaseStream.Position - startPos < maxSize)
        {
            long pageStart = reader.BaseStream.Position;
            byte[] capture = reader.ReadBytes(4);
            if (capture.Length < 4 || capture[0] != (byte)'O' || capture[1] != (byte)'g' ||
                capture[2] != (byte)'g' || capture[3] != (byte)'S')
                break;

            reader.BaseStream.Position = pageStart + 26;
            int segmentCount = reader.ReadByte();
            reader.BaseStream.Position = pageStart + 27;
            byte[] segmentTable = reader.ReadBytes(segmentCount);

            int pageDataSize = 0;
            for (int i = 0; i < segmentCount; i++)
                pageDataSize += segmentTable[i];

            reader.BaseStream.Position = pageStart + 27 + segmentCount + pageDataSize;

            byte[] pageData = new byte[pageDataSize];
            reader.BaseStream.Position = pageStart + 27 + segmentCount;
            reader.Read(pageData, 0, pageDataSize);

            for (int i = 0; i < pageData.Length - 6; i++)
            {
                if (pageData[i] == (byte)'v' && pageData[i + 1] == (byte)'o' &&
                    pageData[i + 2] == (byte)'r' && pageData[i + 3] == (byte)'b' &&
                    pageData[i + 4] == (byte)'i' && pageData[i + 5] == (byte)'s')
                {
                    headerCount++;
                    if (headerCount >= 3)
                        break;
                }
            }
        }

        long endPos = reader.BaseStream.Position;
        return (int)Math.Min(endPos - startPos, maxSize);
    }
}
