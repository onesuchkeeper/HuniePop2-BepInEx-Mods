// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.IO;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.Networking;

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
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
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
                        // byte[] fileBytes = File.ReadAllBytes(Path);
                        // def = WAVUtility.LoadAudioClip(fileBytes);
                        //def = Resources.Load<AudioClip>("file://" + System.IO.Path.GetFullPath(Path));

                        using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + System.IO.Path.GetFullPath(Path), AudioType.WAV))
                        {
                            request.SendWebRequest();

                            //with the way the resource pipeline works atm there's no great way to await this
                            while (!request.isDone) { }

                            if (request.isNetworkError)
                            {
                                ModInterface.Log.Error($"Error while loading external {nameof(AudioClip)}: {request.error}");
                            }
                            else
                            {
                                def = DownloadHandlerAudioClip.GetContent(request);
                            }
                        }
                    }
                    else
                    {
                        ModInterface.Log.Error($"Could not find {Path} to load {nameof(AudioClip)} from");
                    }
                }
                else
                {
                    def = assetProvider.GetInternalAsset<AudioClip>(Path);
                }
            }
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            if (!IsExternal)
            {
                assetProvider.RequestInternal(typeof(AudioClip), Path);
            }
        }
    }
}
