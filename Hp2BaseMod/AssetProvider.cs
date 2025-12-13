// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.IO;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod
{
    /// <summary>
    /// Holding space for unity objects like audio files, prefabs and images from the game while setting up mods
    /// </summary>
    public sealed class AssetProvider
    {
        /// <summary>
        /// A sprite with 0 width and 0 height
        /// </summary>
        public Sprite EmptySprite => _emptySprite;
        private readonly Sprite _emptySprite = Sprite.Create(TextureUtility.Empty(), new Rect(0, 0, 0, 0), Vector2.zero);

        private bool _internalsScraped = false;

        private readonly Dictionary<Type, Dictionary<string, UnityEngine.Object>> _assets;
        private readonly Dictionary<Type, HashSet<string>> _internalAssetRequests = new Dictionary<Type, HashSet<string>>();
        public AssetProvider()
        {
            _assets = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>()
            {
                {typeof(Sprite), new Dictionary<string, UnityEngine.Object>(){
                    {nameof(EmptySprite), _emptySprite}
                }}
            };
        }

        #region Internal Asset Requests

        /// <summary>
        /// Requests that the internal resource with the given name and type is scraped for 
        /// and accessible via <see cref="GetInternalAsset"/> after <see cref="ModEvents.PostDataMods"/> triggers
        /// </summary>
        public void RequestInternal<T>(string name) => RequestInternal(typeof(T), name);

        /// <summary>
        /// Requests that the internal resource with the given name and type is scraped for 
        /// and accessible via <see cref="GetInternalAsset"/> after <see cref="ModEvents.PostDataMods"/> triggers.
        /// </summary>
        public void RequestInternal(Type type, string name)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (_internalsScraped)
            {
                ModInterface.Log.Error("Internals have already been scraped for."
                    + $" Requests must take place on or before {nameof(ModEvents.PreDataMods)}."
                    + $" {type.Name} {name} will not be available via {nameof(AssetProvider)}.");
                return;
            }

            _internalAssetRequests.GetOrNew(type).Add(name);
        }

        /// <summary>
        /// Requests that the internal resources with the given names and type are scraped for 
        /// and accessible via <see cref="GetInternalAsset"/> after <see cref="ModEvents.PostDataMods"/> fires
        /// </summary>
        public void RequestInternal<T>(params IEnumerable<string> names) => RequestInternal(typeof(T), names);

        /// <summary>
        /// Requests that the internal resources with the given names and type are scraped for 
        /// and accessible via <see cref="GetInternalAsset"/> after <see cref="ModEvents.PostDataMods"/> fires
        /// </summary>
        public void RequestInternal(Type type, params IEnumerable<string> names)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            if (_internalsScraped)
            {
                ModInterface.Log.Error("Internals have already been scraped for."
                    + $" Requests must take place on or before {nameof(ModEvents.PreDataMods)}."
                    + $" The following paths of type {type.Name} will not be available via {nameof(AssetProvider)}: "
                    + string.Join(", ", names));
                return;
            }

            var set = _internalAssetRequests.GetOrNew(type);

            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(name));
                }

                set.Add(name);
            }
        }

        /// <summary>
        /// Scrapes resources for requested assets
        /// </summary>
        internal void FulfilInternalAssetRequests()
        {
            if (_internalsScraped)
            {
                throw new Exception($"{nameof(FulfilInternalAssetRequests)} called multiple times");
            }
            _internalsScraped = true;

            foreach (var type_paths in _internalAssetRequests)
            {
                var assets = _assets.GetOrNew(type_paths.Key);

                foreach (var key in assets.Keys)
                {
                    type_paths.Value.Remove(key);
                }

                foreach (var resource in Resources.FindObjectsOfTypeAll(type_paths.Key))
                {
                    if (type_paths.Value.Contains(resource.name))
                    {
                        type_paths.Value.Remove(resource.name);
                        assets[resource.name] = resource;
                    }
                }
            }

            foreach (var type_paths in _internalAssetRequests)
            {
                foreach (var path in type_paths.Value)
                {
                    ModInterface.Log.Warning($"Unable to find internal {type_paths.Key.Name}: {path}");
                }
            }
        }

        /// <summary>
        /// Manually adds an asset with the given identifier and type. 
        /// Useful for assigning unique identifiers to assets that share a type and name.
        /// </summary>
        internal void AddAsset(Type type, string identifier, UnityEngine.Object asset)
        {
            if (string.IsNullOrWhiteSpace(identifier)) { return; }
            _assets.GetOrNew(type)[identifier] = asset;
        }

        #endregion

        /// <summary>
        /// Returns an internal asset previously requested via <see cref="RequestInternal"/>.
        /// Can only be called after <see cref="ModEvents.PostDataMods"/> has fired.
        /// </summary>
        public T GetInternalAsset<T>(string identifier) => (T)GetInternalAsset(typeof(T), identifier);

        /// <summary>
        /// Returns an internal asset previously requested via <see cref="RequestInternal"/>.
        /// Can only be called after <see cref="ModEvents.PostDataMods"/> has fired.
        /// </summary>
        public object GetInternalAsset(Type type, string identifier)
        {
            if (!_internalsScraped)
            {
                throw new Exception("Attempted to get internal asset before they have been scraped for."
                    + $" {nameof(GetInternalAsset)} can only be called after {nameof(ModEvents.PostDataMods)} has fired.");
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            if (_assets.TryGetValue(type, out var typedAssets) && typedAssets.TryGetValue(identifier, out var asset))
            {
                return asset;
            }

            ModInterface.Log.Warning($"Unable to find internal Asset of type {type.Name} with identifier {identifier} - returning null");
            return null;
        }

        /// <summary>
        /// Registers an asset using its name as its identifier
        /// </summary>
        internal void NameAndAddAsset<T>(ref string target, T unityObj)
        where T : UnityEngine.Object
        {
            if (unityObj != null)
            {
                target = unityObj.name;
                AddAsset(typeof(T), target, unityObj);
            }
        }

        #region Load
        internal void Load(AbilityDefinition def) => def?.steps?.ForEach(Load);

        internal void Load(AbilityStepSubDefinition def) => Load(def?.audioKlip);

        internal void Load(AudioKlip def) => Load(def?.clip);

        internal void Load(AudioClip def) => AddAsset(typeof(AudioClip), def?.name, def);

        internal void Load(CutsceneDefinition def) => def?.steps?.ForEach(Load);

        internal void Load(CutsceneStepSubDefinition def)
        {
            AddAsset(typeof(CutsceneStepSpecial), def?.specialStepPrefab?.name, def?.specialStepPrefab);
            AddAsset(typeof(UiWindow), def?.windowPrefab?.name, def?.windowPrefab);
            AddAsset(typeof(EmitterBehavior), def?.emitterBehavior?.name, def?.emitterBehavior);
            AddAsset(typeof(BannerTextBehavior), def?.bannerTextPrefab?.name, def?.bannerTextPrefab);

            Load(def?.audioKlip);
            Load(def?.dialogLine);
            Load(def?.logicAction);

            def?.dialogOptions?.ForEach(Load);
            def?.branches?.ForEach(Load);
        }

        internal void Load(DialogLine def)
        {
            Load(def?.yuriAudioClip);
            Load(def?.audioClip);
        }

        internal void Load(LogicAction def) => Load(def?.backgroundMusic);

        internal void Load(CutsceneDialogOptionSubDefinition def) => def?.steps?.ForEach(Load);

        internal void Load(CutsceneBranchSubDefinition def) => def?.steps?.ForEach(Load);

        internal void Load(DialogTriggerDefinition def) => def?.dialogLineSets?.ForEach(Load);

        internal void Load(DialogTriggerLineSet def) => def?.dialogLines?.ForEach(Load);

        internal void Load(GirlDefinition def)
        {
            AddAsset(typeof(UiDollSpecialEffect), def?.specialEffectPrefab?.name, def?.specialEffectPrefab);

            Load(def?.cellphonePortrait);
            Load(def?.cellphonePortraitAlt);
            Load(def?.cellphoneHead);
            Load(def?.cellphoneHeadAlt);
            Load(def?.cellphoneMiniHead);
            Load(def?.cellphoneMiniHeadAlt);
            def?.parts?.ForEach(x => Load(x, def?.id));
        }

        internal void Load(Sprite def) => AddAsset(typeof(Sprite), def?.name, def);

        internal void Load(GirlPartSubDefinition def, int? girlId)
        {
            if (def?.sprite != null && girlId.HasValue)
            {
                AddAsset(typeof(Sprite), $"{girlId.Value}_{def.sprite.name}", def?.sprite);
            }
        }

        internal void Load(EnergyDefinition def)
        {
            def?.burstSprites?.ForEach(Load);
            def?.trailSprites?.ForEach(Load);
            def?.splashSprites?.ForEach(Load);
            def?.surgeSprites?.ForEach(Load);

            AddAsset(typeof(Material), def?.textMaterial?.name, def?.textMaterial);
        }

        internal void Load(ItemDefinition def) => Load(def?.itemSprite);

        internal void Load(LocationDefinition def)
        {
            Load(def?.bgMusic);
            Load(def?.finderLocationIcon);
            def?.backgrounds?.ForEach(Load);
            def?.arriveBundleList?.ForEach(Load);
            def?.departBundleList?.ForEach(Load);
        }

        internal void Load(LogicBundle def) => def?.actions?.ForEach(Load);

        internal void Load(PhotoDefinition def)
        {
            def?.bigPhotoImages?.ForEach(Load);
            def?.thumbnailImages?.ForEach(Load);
        }

        internal void Load(TokenDefinition def)
        {
            Load(def?.tokenSprite);
            Load(def?.overSprite);
            Load(def?.altTokenSprite);
            Load(def?.altOverSprite);
            Load(def?.sfxMatch);
        }

        #endregion Load

        /// <summary>
        /// Exports a csv file for each type of asset containing the identifiers registered
        /// </summary>
        /// <param name="folderPath"></param>
        internal void SaveToFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var type_assets in _assets)
            {
                var filePath = Path.Combine(folderPath, $"{type_assets.Key.Name}.csv");
                ModInterface.Log.Message($"Dev: Saving asset file {filePath}");
                File.WriteAllText(filePath, $"\"{string.Join("\",\"", type_assets.Value.Keys)}\"");
            }
        }
    }
}
