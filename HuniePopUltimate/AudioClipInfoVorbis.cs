using System;
using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;
using UnityEngine;

/// <summary>
/// Lazy-loading audio clip that works around NVorbis seeking bugs.
/// Strategy: Decode entire audio on first play and cache PCM data.
/// Unload PCM cache when idle, but never recreate VorbisReader.
/// </summary>
public class AudioClipInfoVorbisLazy : IGameDefinitionInfo<UnityEngine.AudioClip>
{
    private static readonly List<AudioClipInfoVorbisLazy> _allInstances = new List<AudioClipInfoVorbisLazy>();
    private static float _lastCleanupTime = 0f;
    private static readonly float _cleanupInterval = 1f;
    private static readonly float _unloadDelay = 5f;

    private readonly AssetStudio.ResourceReader _resourceReader;
    private readonly string _debugName;
    private int _channels;
    private int _sampleRate;
    private int _totalSamples;
    private long _startPos;
    private long _durationSamples;

    // PCM cache - this is what we unload
    private float[] _pcmCache;
    private float _lastAccessTime;
    private int _readCallsSinceLastCleanup;
    private long _currentSamplePosition;
    private bool _hasReachedEnd;

    public AudioClipInfoVorbisLazy(AssetStudio.ResourceReader resourceReader, double startSeconds = 0.0, double durationSeconds = -1.0)
    {
        if (resourceReader == null)
            throw new ArgumentNullException(nameof(resourceReader));

        _resourceReader = resourceReader;
        _debugName = $"Clip_{_allInstances.Count}";

        // Load metadata only
        byte[] fullData = _resourceReader.GetData();
        using (var stream = new MemoryStream(fullData, writable: false))
        using (var tempReader = new VorbisReader(stream, true))
        {
            _channels = tempReader.Channels;
            _sampleRate = tempReader.SampleRate;
            _totalSamples = (int)tempReader.TotalSamples;
        }

        _startPos = (long)(startSeconds * _sampleRate);
        long endPos;
        if (durationSeconds > 0.0)
        {
            _durationSamples = (long)(durationSeconds * _sampleRate);
            endPos = _startPos + _durationSamples;
        }
        else
        {
            endPos = _totalSamples;
            _durationSamples = endPos - _startPos;
        }

        if (_startPos < 0)
            _startPos = 0;
        if (endPos > _totalSamples)
            endPos = _totalSamples;
        if (_durationSamples > endPos - _startPos)
            _durationSamples = endPos - _startPos;

        _lastAccessTime = -999f;
        _readCallsSinceLastCleanup = 0;
        _currentSamplePosition = 0;
        _hasReachedEnd = false;

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
            OnPCMReaderCallback,
            OnPCMSetPositionCallback
        );
    }

    private void LoadPCMCache()
    {
        if (_pcmCache != null)
            return;

        try
        {
            // Decode entire clip section into PCM cache
            byte[] data = _resourceReader.GetData();
            using (var stream = new MemoryStream(data, writable: false))
            using (var reader = new VorbisReader(stream, true))
            {
                // Seek to start position
                if (_startPos > 0)
                {
                    reader.SamplePosition = _startPos;
                }

                // Allocate cache for interleaved samples
                int totalSamples = (int)_durationSamples * _channels;
                _pcmCache = new float[totalSamples];

                // Read all samples at once
                int totalRead = 0;
                int remainingToRead = totalSamples;

                while (remainingToRead > 0)
                {
                    int samplesRead = reader.ReadSamples(_pcmCache, totalRead, remainingToRead);
                    if (samplesRead == 0)
                        break; // End of stream

                    totalRead += samplesRead;
                    remainingToRead -= samplesRead;

                    // Safety check: don't read beyond our duration
                    long currentFilePosition = reader.SamplePosition;
                    if (currentFilePosition >= _startPos + _durationSamples)
                        break;
                }

                // If we didn't read the full amount, clear the remainder
                if (totalRead < totalSamples)
                {
                    Array.Clear(_pcmCache, totalRead, totalSamples - totalRead);
                }
            }
        }
        catch (Exception ex)
        {
            _pcmCache = null;
        }
    }

    private void UnloadPCMCache()
    {
        _pcmCache = null;
    }

    private void OnPCMReaderCallback(float[] data)
    {
        if (_hasReachedEnd)
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        // Load cache on first read
        if (_pcmCache == null)
        {
            LoadPCMCache();

            if (_pcmCache == null)
            {
                Array.Clear(data, 0, data.Length);
                _hasReachedEnd = true;
                return;
            }
        }

        _lastAccessTime = Time.realtimeSinceStartup;
        _readCallsSinceLastCleanup++;

        // Calculate remaining samples
        long remainingSamples = _durationSamples - _currentSamplePosition;
        if (remainingSamples <= 0)
        {
            _hasReachedEnd = true;
            Array.Clear(data, 0, data.Length);
            return;
        }

        // Calculate how many samples to copy (interleaved)
        int samplesToRead = (int)Math.Min(data.Length, remainingSamples * _channels);
        int cacheOffset = (int)(_currentSamplePosition * _channels);

        // Copy from cache
        Array.Copy(_pcmCache, cacheOffset, data, 0, samplesToRead);

        // Update position (data.Length is interleaved samples, so divide by channels)
        _currentSamplePosition += samplesToRead / _channels;

        // Check for end
        if (_currentSamplePosition >= _durationSamples)
        {
            _hasReachedEnd = true;
        }

        // Pad with silence if needed
        if (samplesToRead < data.Length)
        {
            Array.Clear(data, samplesToRead, data.Length - samplesToRead);
        }

        // Periodic cleanup
        if (Time.realtimeSinceStartup - _lastCleanupTime > _cleanupInterval)
        {
            PerformCleanup();
        }
    }

    private void OnPCMSetPositionCallback(int position)
    {
        _lastAccessTime = Time.realtimeSinceStartup;

        // Clamp position
        if (position < 0) position = 0;
        if (position >= _durationSamples) position = (int)_durationSamples - 1;

        // Reset flags
        _hasReachedEnd = false;

        // Update position
        _currentSamplePosition = position;
    }

    private static void PerformCleanup()
    {
        _lastCleanupTime = Time.realtimeSinceStartup;

        var idleClips = new List<AudioClipInfoVorbisLazy>();
        var activeClips = new List<AudioClipInfoVorbisLazy>();

        foreach (var instance in _allInstances)
        {
            if (instance._pcmCache == null)
                continue; // Already unloaded

            float idleTime = _lastCleanupTime - instance._lastAccessTime;

            // If has recent read activity, it's active
            if (instance._readCallsSinceLastCleanup > 0)
            {
                activeClips.Add(instance);
                instance._readCallsSinceLastCleanup = 0;
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
            clip.UnloadPCMCache();
        }
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
    }
}