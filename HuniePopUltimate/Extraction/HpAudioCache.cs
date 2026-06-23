using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod.Extension;

namespace HuniePopUltimate;

/// <summary>
/// Owns the audio PCM cache and resolves <see cref="AudioClipInfoCached"/>
/// instances from serialized Unity files.
/// </summary>
public class HpAudioCache
{
    // Per-(file, path) memoisation of already-opened slices.
    private readonly Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoCached>> _resolved = new();

    private readonly AudioPcmCacheReader _reader;

    /// <param name="cacheFilePath">
    /// Path to the .pcmx cache file. If it already exists on disk it is opened
    /// directly (fast path). Otherwise every Ogg clip found in
    /// <paramref name="serializedFiles"/> is decoded, the cache is written,
    /// and the reader is opened over the result.
    /// </param>
    public HpAudioCache(string cacheFilePath, IEnumerable<SerializedFile> serializedFiles)
    {
        if (File.Exists(cacheFilePath))
        {
            HpDebugLog.AudioMessage($"Loading existing cache: {cacheFilePath}");
            _reader = new AudioPcmCacheReader(cacheFilePath);
            HpDebugLog.AudioMessage($"{_reader.Count} entries loaded.");
            return;
        }

        HpDebugLog.AudioMessage("Cache not found — decoding all Ogg clips…");

        var requests = new List<AudioPcmCache.CacheBuildRequest>();

        foreach (var file in serializedFiles)
        {
            foreach (var kvp in file.ObjectsDic)
            {
                if (kvp.Value is not AudioClip ac) continue;
                if (ac.m_Type != FMODSoundType.OGGVORBIS) continue;

                requests.Add(new AudioPcmCache.CacheBuildRequest
                {
                    Key      = AudioPcmCache.MakeKey(file, kvp.Key),
                    AudioData = ac.m_AudioData,
                });
            }
        }

        if (requests.Count == 0)
        {
            HpDebugLog.AudioWarning("No Ogg clips found — cache not written.");
            return;
        }

        HpDebugLog.AudioMessage($"Decoding {requests.Count} clips…");

        var sw = Stopwatch.StartNew();
        _reader = AudioPcmCache.Build(requests, cacheFilePath, SampleEncoding.PCM16);
        sw.Stop();

        HpDebugLog.AudioMessage($"Done in {sw.Elapsed.TotalSeconds:F2}s ({sw.ElapsedMilliseconds:N0} ms)");
        HpDebugLog.AudioMessage($"Cache written to: {cacheFilePath}");
    }

    /// <summary>
    /// Resolves the clip referenced by a raw HP1 audio definition dictionary,
    /// which is expected to contain a "clip" key holding a Unity asset reference.
    /// </summary>
    public bool TryExtractAudioDef(
        OrderedDictionary audioDef,
        SerializedFile file,
        out AudioClipInfoCached clipInfo)
    {
        if (audioDef.TryGetValue("clip", out OrderedDictionary clip)
            && UnityAssetPath.TryExtract(clip, out var path)
            && TryGetClipInfo(file, path, out clipInfo))
        {
            return true;
        }

        clipInfo = null;
        return false;
    }

    /// <summary>
    /// Returns an <see cref="AudioClipInfoCached"/> for the clip at
    /// <paramref name="path"/> inside <paramref name="file"/>, resolving
    /// cross-file references via the file's external list.
    /// Results are memoized per (file, path) pair.
    /// </summary>
    public bool TryGetClipInfo(
        SerializedFile file,
        UnityAssetPath path,
        out AudioClipInfoCached clipInfo)
    {
        var perFile = _resolved.GetOrNew(file);
        if (perFile.TryGetValue(path, out clipInfo)) return true;

        if (_reader == null)
        {
            HpDebugLog.AudioWarning("TryGetClipInfo called but cache reader is null (no Ogg clips found during build?).");
            clipInfo = null;
            return false;
        }

        var targetFile = ResolveFile(file, path);
        if (targetFile == null)
        {
            clipInfo = null;
            return false;
        }

        var handle = _reader.OpenSlice(targetFile, path.PathId);
        if (handle == null)
        {
            clipInfo = null;
            return false;
        }

        clipInfo = new AudioClipInfoCached(handle, path.ToString());
        perFile[path] = clipInfo;
        return true;
    }

    /// <summary>
    /// Resolves which <see cref="SerializedFile"/> the path actually lives in.
    /// FileId == 0 means the same file; FileId &gt; 0 is a 1-based index into
    /// <see cref="SerializedFile.m_Externals"/>.
    /// </summary>
    private SerializedFile ResolveFile(SerializedFile file, UnityAssetPath path)
    {
        if (path.FileId == 0) return file;

        int extIndex = path.FileId - 1;
        if (extIndex < 0 || extIndex >= file.m_Externals.Count)
        {
            HpDebugLog.AudioWarning($"External index {extIndex} out of range for {file.fileName}");
            return null;
        }

        string extFileName = file.m_Externals[extIndex].fileName;
        var resolved = file.assetsManager.assetsFileList
            .FirstOrDefault(f => string.Equals(f.fileName, extFileName, StringComparison.OrdinalIgnoreCase));

        if (resolved == null)
        {
            HpDebugLog.AudioWarning($"Could not resolve external file: {extFileName}");
        }

        return resolved;
    }
}