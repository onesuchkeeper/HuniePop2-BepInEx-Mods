﻿// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make an AudioClip
    /// </summary>
    public class AudioClipInfo : IGameDefinitionInfo<AudioClip>
    {
        public string Path;

        public bool IsExternal;

        /// <summary>
        /// Constructor
        /// </summary>
        public AudioClipInfo() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assest referenced by the definition.</param>
        public AudioClipInfo(AudioClip def, AssetProvider assetProvider)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (assetProvider == null) { throw new ArgumentNullException(nameof(assetProvider)); }

            assetProvider.NameAndAddAsset(ref Path, def);

            IsExternal = false;
        }

        /// <inheritdoc/>
        public void SetData(ref AudioClip def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (Path == null)
            {
                if (insertStyle == InsertStyle.assignNull)
                {
                    def = null;
                }
            }
            else
            {
                if (IsExternal)
                {
                    if (File.Exists(Path))
                    {
                        byte[] fileBytes = File.ReadAllBytes(Path);
                        def = WAVUtility.LoadAudioClip(fileBytes);

                        ModInterface.Log.LogInfo($"{(def == null ? "Failed to load" : "Loaded")} external {nameof(AudioClip)} {Path ?? "null"}");
                    }
                    else
                    {
                        ModInterface.Log.LogInfo($"Could not find {Path} to load {nameof(AudioClip)} from");
                    }
                }
                else
                {
                    def = (AudioClip)assetProvider.GetAsset(Path);
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests() => null;

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests()
        {
            if (!IsExternal)
            {
                yield return Path;
            }
        }
    }
}
