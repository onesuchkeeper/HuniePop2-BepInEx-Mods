using System;
using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;
using UnityEngine;

/// <summary>
/// Simple lazy-loading audio clip with aggressive unloading.
/// Loads data on first play, unloads after a short idle period.
/// </summary>
public class AudioClipInfoVorbisLazy : IGameDefinitionInfo<UnityEngine.AudioClip>
{
    // Keep instances alive but make data management simple
    private static readonly List<AudioClipInfoVorbisLazy> _allInstances = new List<AudioClipInfoVorbisLazy>();
    private static float _lastCleanupTime = 0f;
    private static readonly float _cleanupInterval = 1f; // Check every second
    private static readonly float _unloadDelay = 3f; // Unload after 3 seconds of no reads
    private static readonly int _maxLoadedClips = 10; // Only keep 10 clips loaded max

    private readonly AssetStudio.ResourceReader _resourceReader;
    private readonly string _debugName;
    private int _channels;
    private int _sampleRate;
    private int _totalSamples;
    private long _startPos;
    private long _endPos;
    private long _durationSamples;

    // Transient data
    private MemoryStream _memoryStream;
    private VorbisReader _reader;
    private long _currentSamplePosition; // Position in samples (NOT per-channel)
    private bool _hasReachedEnd; // Safeguard against infinite loops
    private int _consecutiveZeroReads; // Track failed reads
    private float _lastAccessTime;
    private int _readCallsSinceLastCleanup;

    public AudioClipInfoVorbisLazy(AssetStudio.ResourceReader resourceReader, double startSeconds = 0.0, double durationSeconds = -1.0)
    {
        if (resourceReader == null)
            throw new ArgumentNullException(nameof(resourceReader));

        _resourceReader = resourceReader;
        _debugName = $"Clip_{_allInstances.Count}";

        // Load only metadata
        byte[] fullData = _resourceReader.GetData();
        using (var stream = new MemoryStream(fullData, writable: false))
        using (var tempReader = new VorbisReader(stream, true))
        {
            _channels = tempReader.Channels;
            _sampleRate = tempReader.SampleRate;
            _totalSamples = (int)tempReader.TotalSamples;
        }

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

        _lastAccessTime = -999f;
        _readCallsSinceLastCleanup = 0;
        _currentSamplePosition = 0; // Relative to clip start
        _hasReachedEnd = false;
        _consecutiveZeroReads = 0;

        _allInstances.Add(this);
    }

    public void SetData(ref UnityEngine.AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = UnityEngine.AudioClip.Create(
            _debugName,
            (int)_durationSamples,
            _channels,
            _sampleRate,
            true,
            this.OnPCMReaderCallback,
            this.OnPCMSetPositionCallback
        );
    }

    private void LoadAudioData()
    {
        if (_reader != null)
            return;

        try
        {
            byte[] data = _resourceReader.GetData();
            _memoryStream = new MemoryStream(data, writable: false);
            _reader = new VorbisReader(_memoryStream, true);

            // Reset safeguards
            _hasReachedEnd = false;
            _consecutiveZeroReads = 0;

            // Seek to the correct position based on current sample position
            SeekToPosition(_currentSamplePosition);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load audio data for {_debugName}: {ex.Message}");
            // Clean up partial state
            _reader?.Dispose();
            _memoryStream?.Dispose();
            _reader = null;
            _memoryStream = null;
        }
    }

    private void SeekToPosition(long clipPosition)
    {
        if (_reader == null)
            return;

        try
        {
            // Convert clip-relative position to absolute position in file
            long absolutePosition = _startPos + clipPosition;

            // Clamp to valid range
            if (absolutePosition < _startPos)
                absolutePosition = _startPos;
            if (absolutePosition >= _endPos)
                absolutePosition = _endPos - 1;

            // Validate before seeking
            if (absolutePosition < 0 || absolutePosition >= _totalSamples)
            {
                Debug.LogWarning($"Invalid seek position for {_debugName}: {absolutePosition} (total: {_totalSamples})");
                return;
            }

            // Try to seek
            _reader.SamplePosition = absolutePosition;
            _hasReachedEnd = false;
            _consecutiveZeroReads = 0;
        }
        catch (Exception ex)
        {
            // NVorbis can throw ArgumentOutOfRangeException even with valid positions
            // Log but continue - reader will start from wherever it is
            if (clipPosition != 0) Debug.LogWarning($"Seek failed for {_debugName} to position {clipPosition}: {ex.Message}");
        }
    }

    private void UnloadAudioData()
    {
        if (_reader == null)
            return;

        try
        {
            _reader?.Dispose();
            _memoryStream?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error unloading {_debugName}: {ex.Message}");
        }
        finally
        {
            _reader = null;
            _memoryStream = null;
            _hasReachedEnd = false;
            _consecutiveZeroReads = 0;
        }
    }

    private void OnPCMReaderCallback(float[] data)
    {
        // Safeguard: If we've reached the end, just return silence
        if (_hasReachedEnd)
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        // Load on first read if not already loaded
        if (_reader == null)
        {
            LoadAudioData();

            // If loading failed, return silence
            if (_reader == null)
            {
                Array.Clear(data, 0, data.Length);
                _hasReachedEnd = true;
                return;
            }
        }

        _lastAccessTime = Time.realtimeSinceStartup;
        _readCallsSinceLastCleanup++;

        // Calculate how many samples we should read
        long remainingSamples = _durationSamples - _currentSamplePosition;
        if (remainingSamples <= 0)
        {
            // Past the end - mark as ended and fill with silence
            _hasReachedEnd = true;
            Array.Clear(data, 0, data.Length);
            return;
        }

        // Read samples (data.Length is in individual samples, accounting for all channels)
        int samplesToRead = (int)Math.Min(data.Length, remainingSamples * _channels);
        int samplesRead = 0;

        try
        {
            samplesRead = _reader.ReadSamples(data, 0, samplesToRead);

            // Track zero reads as a safeguard
            if (samplesRead == 0)
            {
                _consecutiveZeroReads++;

                // If we get too many zero reads, mark as ended to prevent infinite loop
                if (_consecutiveZeroReads > 10)
                {
                    Debug.LogWarning($"Too many consecutive zero reads for {_debugName}, marking as ended");
                    _hasReachedEnd = true;
                    Array.Clear(data, 0, data.Length);
                    return;
                }
            }
            else
            {
                _consecutiveZeroReads = 0; // Reset on successful read
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to read samples from {_debugName}: {ex.Message}");
            _hasReachedEnd = true;
            Array.Clear(data, 0, data.Length);
            return;
        }

        // Update our position (ReadSamples returns total samples across all channels)
        _currentSamplePosition += samplesRead / _channels;

        // Check if we've reached the end
        if (_currentSamplePosition >= _durationSamples)
        {
            _hasReachedEnd = true;
        }

        // Pad remaining with silence if we didn't fill the buffer
        if (samplesRead < data.Length)
        {
            Array.Clear(data, samplesRead, data.Length - samplesRead);
        }

        // Periodic cleanup check (lightweight)
        if (Time.realtimeSinceStartup - _lastCleanupTime > _cleanupInterval)
        {
            PerformCleanup();
        }
    }

    private void OnPCMSetPositionCallback(int position)
    {
        _lastAccessTime = Time.realtimeSinceStartup;

        // Clamp position to valid range
        if (position < 0)
            position = 0;
        if (position >= _durationSamples)
            position = (int)_durationSamples - 1;

        // Reset end flag when seeking
        _hasReachedEnd = false;
        _consecutiveZeroReads = 0;

        // Update our current position
        _currentSamplePosition = position;

        // If reader is already loaded, seek immediately
        if (_reader != null)
        {
            SeekToPosition(position);
        }
        // If not loaded, position will be used when LoadAudioData is called
    }

    private static void PerformCleanup()
    {
        _lastCleanupTime = Time.realtimeSinceStartup;

        // Find clips to unload (idle ones)
        var idleClips = new List<AudioClipInfoVorbisLazy>();
        var activeClips = new List<AudioClipInfoVorbisLazy>();

        foreach (var instance in _allInstances)
        {
            if (instance._reader == null)
                continue; // Already unloaded

            float idleTime = _lastCleanupTime - instance._lastAccessTime;

            // If has recent read activity, it's active
            if (instance._readCallsSinceLastCleanup > 0)
            {
                activeClips.Add(instance);
                instance._readCallsSinceLastCleanup = 0; // Reset counter
            }
            // If idle for longer than delay, mark for unload
            else if (idleTime > _unloadDelay)
            {
                idleClips.Add(instance);
            }
        }

        // Unload idle clips
        foreach (var clip in idleClips)
        {
            clip.UnloadAudioData();
        }

        // If we still have too many loaded, unload the oldest active ones
        if (activeClips.Count > _maxLoadedClips)
        {
            activeClips.Sort((a, b) => a._lastAccessTime.CompareTo(b._lastAccessTime));
            int toUnload = activeClips.Count - _maxLoadedClips;
            for (int i = 0; i < toUnload; i++)
            {
                activeClips[i].UnloadAudioData();
            }
        }
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
    }

    // Utility methods
    public static int GetTotalInstanceCount()
    {
        return _allInstances.Count;
    }

    public static int GetLoadedInstanceCount()
    {
        int count = 0;
        foreach (var instance in _allInstances)
        {
            if (instance._reader != null)
                count++;
        }
        return count;
    }

    public static long GetTotalLoadedMemoryBytes()
    {
        long total = 0;
        foreach (var instance in _allInstances)
        {
            if (instance._memoryStream != null)
            {
                total += instance._memoryStream.Length;
            }
        }
        return total;
    }

    public static void LogMemoryStats()
    {
        int total = GetTotalInstanceCount();
        int loaded = GetLoadedInstanceCount();
        long memoryBytes = GetTotalLoadedMemoryBytes();
        float memoryMB = memoryBytes / (1024f * 1024f);

        Debug.Log($"AudioClip Stats - Total: {total}, Loaded: {loaded}, Memory: {memoryMB:F2} MB");
    }

    public static void ForceUnloadAll()
    {
        foreach (var instance in _allInstances)
        {
            instance.UnloadAudioData();
        }
    }
}

/// <summary>
/// Optional: Add to a GameObject to periodically log stats
/// </summary>
public class AudioMemoryMonitor : MonoBehaviour
{
    [SerializeField] private float _logInterval = 5f;
    private float _lastLogTime;

    private void Update()
    {
        if (Time.realtimeSinceStartup - _lastLogTime > _logInterval)
        {
            _lastLogTime = Time.realtimeSinceStartup;
            AudioClipInfoVorbisLazy.LogMemoryStats();
        }
    }
}