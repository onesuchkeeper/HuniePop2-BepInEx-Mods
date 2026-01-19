using System;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;
using UnityEngine;

namespace HuniePopUltimate
{
    /// <summary>
    /// Streams audio from a WAV file on disk using PCMReaderCallback.
    /// If the file doesn't exist, converts the OGG data from ResourceReader to WAV first.
    /// Never holds the full clip in memory.
    /// </summary>
    public class AudioClipInfoPCMStreamed : IGameDefinitionInfo<AudioClip>
    {
        private readonly string _debugName;
        private readonly string _filePath;

        private int _channels;
        private int _sampleRate;
        private long _totalSamples;

        private AudioClip _clip;
        private bool _isStreamingStarted;

        private FileStream _fileStream;

        public AudioClipInfoPCMStreamed(AssetStudio.ResourceReader resourceReader, string filePath, string debugName = null)
        {
            if (resourceReader == null) throw new ArgumentNullException(nameof(resourceReader));
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
            _debugName = debugName ?? $"Clip_{Guid.NewGuid()}";

            // Ensure directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
            {
                // Convert OGG → WAV
                using (var oggStream = new MemoryStream(resourceReader.GetData()))
                using (var vorbis = new VorbisReader(oggStream, false))
                {
                    _channels = vorbis.Channels;
                    _sampleRate = vorbis.SampleRate;
                    _totalSamples = vorbis.TotalSamples;

                    using (var fs = File.Create(_filePath))
                    using (var bw = new BinaryWriter(fs))
                    {
                        WriteWavHeader(bw, _channels, _sampleRate, (int)_totalSamples);

                        float[] buffer = new float[4096 * _channels];
                        int read;
                        while ((read = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < read; i++)
                            {
                                short sample = (short)Mathf.Clamp(buffer[i] * 32767f, short.MinValue, short.MaxValue);
                                bw.Write(sample);
                            }
                        }

                        UpdateWavHeaderSizes(fs);
                    }
                }
            }
            else
            {
                // WAV already exists — read metadata properly from header
                using (var fs = File.OpenRead(_filePath))
                using (var br = new BinaryReader(fs))
                {
                    fs.Seek(22, SeekOrigin.Begin); // channels at offset 22
                    _channels = br.ReadInt16();

                    _sampleRate = br.ReadInt32(); // sample rate at offset 24

                    fs.Seek(40, SeekOrigin.Begin); // data chunk size at offset 40
                    int dataSize = br.ReadInt32();
                    _totalSamples = dataSize / (_channels * 2); // 16-bit PCM
                }
            }
        }

        public void SetData(ref AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            def = _clip ?? CreateStreamingClip();
        }

        private AudioClip CreateStreamingClip()
        {
            if (_isStreamingStarted)
                return _clip;

            _isStreamingStarted = true;

            _clip = AudioClip.Create(_debugName, (int)_totalSamples, _channels, _sampleRate, true, OnPCMRead, OnPCMSetPos);

            // Open file stream for reading PCM
            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _fileStream.Seek(44, SeekOrigin.Begin); // skip WAV header

            return _clip;
        }

        private void OnPCMRead(float[] data)
        {
            if (_fileStream == null)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            byte[] buffer = new byte[data.Length * 2]; // 16-bit PCM
            int bytesRead = _fileStream.Read(buffer, 0, buffer.Length);

            for (int i = 0; i < bytesRead / 2; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                data[i] = sample / 32768f;
            }

            if (bytesRead < buffer.Length)
            {
                for (int i = bytesRead / 2; i < data.Length; i++)
                    data[i] = 0f;
            }
        }

        private void OnPCMSetPos(int pos)
        {
            if (_fileStream != null)
            {
                long bytePos = 44 + pos * 2;
                _fileStream.Seek(bytePos, SeekOrigin.Begin);
            }
        }

        public void RequestInternals(AssetProvider assetProvider)
        {
            // PCM streaming only
        }

        #region WAV Helpers
        private void WriteWavHeader(BinaryWriter bw, int channels, int sampleRate, int totalSamples)
        {
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + totalSamples * 2);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1); // PCM
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(sampleRate * channels * 2);
            bw.Write((short)(channels * 2));
            bw.Write((short)16); // bits per sample
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(totalSamples * 2);
        }

        private void UpdateWavHeaderSizes(FileStream fs)
        {
            fs.Seek(4, SeekOrigin.Begin);
            int fileSize = (int)(fs.Length - 8);
            fs.Write(BitConverter.GetBytes(fileSize), 0, 4);

            fs.Seek(40, SeekOrigin.Begin);
            int dataSize = (int)(fs.Length - 44);
            fs.Write(BitConverter.GetBytes(dataSize), 0, 4);
        }
        #endregion
    }
}
