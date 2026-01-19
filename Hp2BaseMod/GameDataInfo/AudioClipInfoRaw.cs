using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

public class AudioClipInfoRaw : IGameDefinitionInfo<AudioClip>
{
    public float[] Data;
    public int Samples;
    public int Channels;
    public int SampleRate;

    public void SetData(ref AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = AudioClip.Create("", Samples, Channels, SampleRate, false);
        def.SetData(Data, 0);
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}