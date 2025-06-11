using System;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;
using UnityEngine;

public class AudioClipInfoVorbis : IGameDefinitionInfo<AudioClip>
{
    private VorbisReader _reader;

    public AudioClipInfoVorbis(VorbisReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException();
    }

    public void SetData(ref AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = AudioClip.Create("", (int)_reader.TotalSamples, _reader.Channels, _reader.SampleRate, true, On_PCMReaderCallback, On_PCMSetPositionCallback);
    }

    private void On_PCMSetPositionCallback(int position) => _reader.SamplePosition = position;

    private void On_PCMReaderCallback(float[] data) => _reader.ReadSamples(data);

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}