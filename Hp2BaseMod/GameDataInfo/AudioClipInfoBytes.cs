﻿using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    public class AudioClipInfoBytes : IGameDefinitionInfo<AudioClip>
    {
        public byte[] Data;

        public void SetData(ref AudioClip def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (Data == null)
            {
                if (insertStyle == InsertStyle.assignNull)
                {
                    def = null;
                }
            }
            else
            {
                def = WAVUtility.LoadAudioClip(Data);
                ModInterface.Log.LogInfo($"{(def == null ? "Failed to load" : "Loaded")} byte {nameof(AudioClip)}");
            }

            Data = null;
        }

        public IEnumerable<string> GetInternalAudioRequests() => null;

        public IEnumerable<string> GetInternalSpriteRequests() => null;
    }
}
