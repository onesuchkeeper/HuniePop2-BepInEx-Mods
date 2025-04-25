// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.U2D;

namespace Hp2BaseMod
{
    /// <summary>
    /// Holding space for unity objects like audio files, prefabs and images from the game while setting up data mods
    /// </summary>
    public class AssetProvider
    {
        public Sprite EmptySprite => _emptySprite;
        private readonly Sprite _emptySprite = Sprite.Create(TextureUtility.Empty(), new Rect(0, 0, 0, 0), Vector2.zero);

        public IReadOnlyDictionary<string, Texture2D> ExternalTextures => _externalTextures;
        private readonly Dictionary<string, Texture2D> _externalTextures = new Dictionary<string, Texture2D>();

        public readonly Dictionary<string, SpriteAtlas> ExternalAtlases = new Dictionary<string, SpriteAtlas>();

        private readonly Dictionary<string, UnityEngine.Object> Assets;

        private readonly HashSet<string> _internalSpriteRequests = new HashSet<string>();
        private readonly HashSet<string> _internalAudioRequests = new HashSet<string>();

        public AssetProvider()
        {
            Assets = new Dictionary<string, Object>()
            {
                {nameof(EmptySprite), _emptySprite}
            };
        }

        #region Internal Asset Requests

        public void RequestInternalSprite(string path) => _internalSpriteRequests.Add(path);
        public void RequestInternalSprite(params IEnumerable<string> paths) => paths.ForEach(x => _internalSpriteRequests.Add(x));

        public void RequestInternalAudio(string path) => _internalAudioRequests.Add(path);
        public void RequestInternalAudio(params IEnumerable<string> paths) => paths.ForEach(x => _internalAudioRequests.Add(x));

        internal void RequestInternals<D>(IGameDataMod<D> mod)
        {
            if (mod == null) { return; }
            mod.GetInternalSpriteRequests()?.ForEach(x => _internalSpriteRequests.Add(x));
            mod.GetInternalAudioRequests()?.ForEach(x => _internalAudioRequests.Add(x));
        }

        internal void FulfilInternalAssetRequests()
        {
            //remove assets we already have
            foreach (var key in Assets.Keys)
            {
                _internalSpriteRequests.Remove(key);
                _internalAudioRequests.Remove(key);
            }

            //sprites
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (_internalSpriteRequests.Contains(sprite.name))
                {
                    _internalSpriteRequests.Remove(sprite.name);
                    Assets[sprite.name] = sprite;
                }
            }

            foreach (var path in _internalSpriteRequests)
            {
                ModInterface.Log.LogWarning($"Unable to find internal sprite: {path}");
            }

            //audioClips
            foreach (var audio in Resources.FindObjectsOfTypeAll<AudioClip>())
            {
                if (_internalAudioRequests.Contains(audio.name))
                {
                    _internalAudioRequests.Remove(audio.name);
                    Assets[audio.name] = audio;
                }
            }

            foreach (var path in _internalAudioRequests)
            {
                ModInterface.Log.LogWarning($"Unable to find internal audioClip: {path}");
            }
        }

        internal void AddAsset(string identifier, UnityEngine.Object asset)
        {
            if (identifier == null) { return; }
            if (!Assets.ContainsKey(identifier))
            {
                Assets.Add(identifier, asset);
            }
        }

        internal void AddExternalTexture(string identifier, Texture2D sprite) => _externalTextures[identifier] = sprite;

        #endregion

        public T GetAsset<T>(string identifier) => (T)GetAsset(identifier);

        public object GetAsset(string identifier)
        {
            if (identifier == null) { return null; }
            if (Assets.ContainsKey(identifier))
            {
                return Assets[identifier];
            }
            else
            {
                ModInterface.Log.LogInfo($"Failed to find internal asset {identifier}");
            }
            return null;
        }

        internal void NameAndAddAsset(ref string target, UnityEngine.Object unityObj)
        {
            if (unityObj != null)
            {
                target = unityObj.name;
                AddAsset(target, unityObj);
            }
        }

        #region Load
        internal void Load(AbilityDefinition def) => def?.steps?.ForEach(x => Load(x));

        internal void Load(AbilityStepSubDefinition def) => Load(def?.audioKlip);

        internal void Load(AudioKlip def) => Load(def?.clip);

        internal void Load(AudioClip def) => AddAsset(def?.name, def);

        internal void Load(CutsceneDefinition def) => def?.steps?.ForEach(x => Load(x));

        internal void Load(CutsceneStepSubDefinition def)
        {
            AddAsset(def?.specialStepPrefab?.name, def?.specialStepPrefab);
            AddAsset(def?.windowPrefab?.name, def?.windowPrefab);
            AddAsset(def?.emitterBehavior?.name, def?.emitterBehavior);
            AddAsset(def?.bannerTextPrefab?.name, def?.bannerTextPrefab);

            Load(def?.audioKlip);
            Load(def?.dialogLine);
            Load(def?.logicAction);

            def?.dialogOptions?.ForEach(x => Load(x));
            def?.branches?.ForEach(x => Load(x));
        }

        internal void Load(DialogLine def)
        {
            Load(def?.yuriAudioClip);
            Load(def?.audioClip);
        }

        internal void Load(LogicAction def) => Load(def?.backgroundMusic);

        internal void Load(CutsceneDialogOptionSubDefinition def) => def?.steps?.ForEach(x => Load(x));

        internal void Load(CutsceneBranchSubDefinition def) => def?.steps?.ForEach(x => Load(x));

        internal void Load(DialogTriggerDefinition def) => def?.dialogLineSets?.ForEach(x => Load(x));

        internal void Load(DialogTriggerLineSet def) => def?.dialogLines?.ForEach(x => Load(x));

        internal void Load(GirlDefinition def)
        {
            AddAsset(def?.specialEffectPrefab?.name, def?.specialEffectPrefab);

            Load(def?.cellphonePortrait);
            Load(def?.cellphonePortraitAlt);
            Load(def?.cellphoneHead);
            Load(def?.cellphoneHeadAlt);
            Load(def?.cellphoneMiniHead);
            Load(def?.cellphoneMiniHeadAlt);
            def?.parts?.ForEach(x => Load(x, def?.id));
        }

        internal void Load(Sprite def) => AddAsset(def?.name, def);

        internal void Load(GirlPartSubDefinition def, int? girlId)
        {
            if (def?.sprite != null && girlId.HasValue)
            {
                AddAsset($"{girlId.Value}_{(def.sprite.name)}", def?.sprite);
            }
        }

        internal void Load(EnergyDefinition def)
        {
            def?.burstSprites?.ForEach(x => Load(x));
            def?.trailSprites?.ForEach(x => Load(x));
            def?.splashSprites?.ForEach(x => Load(x));
            def?.surgeSprites?.ForEach(x => Load(x));

            AddAsset(def?.textMaterial?.name, def?.textMaterial);
        }

        internal void Load(ItemDefinition def) => Load(def?.itemSprite);

        internal void Load(LocationDefinition def)
        {
            Load(def?.bgMusic);
            Load(def?.finderLocationIcon);
            def?.backgrounds?.ForEach(x => Load(x));
            def?.arriveBundleList?.ForEach(x => Load(x));
            def?.departBundleList?.ForEach(x => Load(x));
        }

        internal void Load(LogicBundle def) => def?.actions?.ForEach(x => Load(x));

        internal void Load(PhotoDefinition def)
        {
            def?.bigPhotoImages?.ForEach(x => Load(x));
            def?.thumbnailImages?.ForEach(x => Load(x));
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

        internal void SaveToFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var group in Assets.Where(x => x.Value != null).GroupBy(x => x.Value.GetType()))
            {
                var result = string.Empty;

                foreach (var asset in group)
                {
                    result += $"\"{asset.Key}\",";
                }

                var filePath = Path.Combine(folderPath, $"{group.Key.Name}.csv");

                ModInterface.Log.LogInfo($"Dev: Saving asset file {filePath}");

                File.WriteAllText(filePath, result);
            }
        }
    }
}
