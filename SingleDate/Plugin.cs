using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Please note, any pair with <see cref="GirlNobody"/> (LocalId 0) as girl 1 will be treated as a single date pair.
/// The sex location will be chosen from the locations of the girls sex photos set by <see cref="AddGirlSexPhotos"/>.
/// If the location for the sex photo is <see cref="RelativeId.Default"/>, the sex location for the pair will be used instead.
/// If the pair's sex location is <see cref="RelativeId.Default"/> it will be randomized. 
/// Use <see cref="AddSexLocationBlackList"/> to limit the pool if needed. (maybe add multiple loc support to the base mod? Could be cool)
/// All Data mods are added with a priority of zero
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.1")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal partial class Plugin : Hp2BaseModPlugin
{
    private const string GENERAL_CONFIG_CAT = "general";
    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    public static ConfigEntry<bool> ShowSingleUpsetHint => _showSingleUpsetHint;
    private static ConfigEntry<bool> _showSingleUpsetHint;

    public static ConfigEntry<bool> SingleDateBaggage => _singleDateBaggage;
    private static ConfigEntry<bool> _singleDateBaggage;

    public static ConfigEntry<bool> RequireLoversBeforeThreesome => _requireLoversBeforeThreesome;
    private static ConfigEntry<bool> _requireLoversBeforeThreesome;

    public static ConfigEntry<int> MaxSingleGirlRelationshipLevel => _maxSingleGirlRelationshipLevel;
    private static ConfigEntry<int> _maxSingleGirlRelationshipLevel;

    public static ConfigEntry<int> MaxSensitivityLevel => _maxSensitivityLevel;
    private static ConfigEntry<int> _maxSensitivityLevel;

    public static AssetBundle AssetBundle => _instance._assetBundle;
    private AssetBundle _assetBundle;

    public static CharmAnimationRegistry CharmAnimationRegistry => _instance._charmAnimationRegistry;
    private CharmAnimationRegistry _charmAnimationRegistry;

    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    private Dictionary<RelativeId, SingleDateGirl> _singleDateGirls = new();
    private static Plugin _instance;
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        _instance = this;
        _charmAnimationRegistry = new();
        base.Awake();

        _assetBundle = AssetBundle.LoadFromFile(Path.Combine(ROOT_DIR, "singledate_assetbundle")); 

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
            out Action<Sprite, IEnumerable<(Sprite creditButtonPath, Sprite creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(_assetBundle.LoadAsset<Sprite>("CreditsLogo"), [
                (
                    _assetBundle.LoadAsset<Sprite>("onesuchkeeper_credits_art"),
                    _assetBundle.LoadAsset<Sprite>("onesuchkeeper_credits_art_over"),
                    "https://linktr.ee/onesuchkeeper"
                ),
            ]);
        }

        _showSingleUpsetHint = Config.Bind(GENERAL_CONFIG_CAT, nameof(ShowSingleUpsetHint), false, "If upset hints are shown on single dates.");
        _singleDateBaggage = Config.Bind(GENERAL_CONFIG_CAT, nameof(SingleDateBaggage), true, "If baggage is active on single dates.");
        _requireLoversBeforeThreesome = Config.Bind(GENERAL_CONFIG_CAT, nameof(RequireLoversBeforeThreesome), false, "If both characters must reach lovers on single dates before a threesome can occur.");
        _maxSingleGirlRelationshipLevel = Config.Bind(GENERAL_CONFIG_CAT, nameof(MaxSingleGirlRelationshipLevel), 3, "Maximum relationship level for single dates. Maximum level must be reached for lovers status.");
        _maxSensitivityLevel = Config.Bind(GENERAL_CONFIG_CAT, nameof(MaxSensitivityLevel), 4, "Maximum level for sensitivity.");

        State.On_Plugin_Awake();

        GirlNobody.AddDataMods();
        ItemSensitivitySmoothie.AddDataMods(_assetBundle);

        SingleDateMeetingCutscene.AddDataMods();
        SingleDateAttractCutscene.AddDataMods();
        SingleDatePreSexCutscene.AddDataMods();
        SingleDatePostSexCutscene.AddDataMods();
        SingleDateSuccessCutscene.AddDataMods();
        BonusRoundSuccessCutscene.AddDataMods();

        AddPhotoMod(Photos.DefaultSex, "default");
        AddPhotoMod(Photos.AbiaSex, "abia");
        AddPhotoMod(Photos.BrookeSex, "brooke");
        AddPhotoMod(Photos.CandaceSex, "candace");
        AddPhotoMod(Photos.LailaniSex, "lailani");
        AddPhotoMod(Photos.LillianSex, "lillian");
        AddPhotoMod(Photos.NoraSex, "nora");
        AddPhotoMod(Photos.PollySex, "polly", true);
        AddPhotoMod(Photos.SarahSex, "sarah");
        AddPhotoMod(Photos.ZoeySex, "zoey");

        AddGirlSexPhotos(Hp2BaseMod.Girls.Abia, [(Photos.AbiaSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Brooke, [(Photos.BrookeSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Candace, [(Photos.CandaceSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Lailani, [(Photos.LailaniSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Lillian, [(Photos.LillianSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Nora, [(Photos.NoraSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Polly, [(Photos.PollySex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Sarah, [(Photos.SarahSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Zoey, [(Photos.ZoeySex, Locations.RoyalSuite)]);

        UiPrefabs.InitExternals(_assetBundle);

        ModInterface.AddExp(new SensitivityExp());

        ModInterface.Events.PreDataMods += On_PreDataMods;

        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;

        ModInterface.Events.PuzzleRoundOver += ModEventHandles.On_PuzzleRoundOver;
        ModInterface.Events.FinderSlotsPopulate += ModEventHandles.On_FinderSlotsPopulate;
        ModInterface.Events.LocationArriveSequence += ModEventHandles.On_LocationArriveSequence;
        ModInterface.Events.RandomDollSelected += ModEventHandles.On_RandomDollSelected;
        ModInterface.Events.DateLocationSelected += ModEventHandles.On_DateLocationSelected;
        ModInterface.Events.RequestUnlockedPhotos += ModEventHandles.On_RequestUnlockedPhotos;
        ModInterface.Events.PreDateDollReset += ModEventHandles.On_PreDateDollsRefresh;
        ModInterface.Events.FavQuestionResponse += ModEventHandles.On_TalkFavQuestionResponse;
        ModInterface.Events.PreLocationArrive += ModEventHandles.On_PreLocationArrive;
        ModInterface.Events.PreLocationSettled += ModEventHandles.On_PreLocationSettled;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    [InteropMethod]
    public static void AddGirlSexPhotos(RelativeId girlId, IEnumerable<(RelativeId, RelativeId)> photoIds)
        => _instance._singleDateGirls.GetOrNew(girlId).SexPhotos.AddRange(photoIds.Select(x => new SexPhotoData() { PhotoId = x.Item1, LocationId = x.Item2 }));

    [InteropMethod]
    public static void AddGirlDatePhotos(RelativeId girlId, IEnumerable<(RelativeId, float)> photoIds)
            => _instance._singleDateGirls.GetOrNew(girlId).DatePhotos.AddRange(photoIds.Select(x => new DatePhotoData() { PhotoId = x.Item1, RelationshipPercentage = x.Item2 }));

    [InteropMethod]
    public static void AddSexLocationBlackList(RelativeId girlId, IEnumerable<RelativeId> photoIds)
    {
        var singleDateGirl = _instance._singleDateGirls.GetOrNew(girlId);
        singleDateGirl.SexLocBlackList.UnionWith(photoIds);
    }

    [InteropMethod]
    public static void SetCutsceneSuccessAttracted(RelativeId girlId, RelativeId CutsceneId)
        => _instance._singleDateGirls.GetOrNew(girlId).CutsceneSuccessAttracted = CutsceneId;

    [InteropMethod]
    public static void SetBonusRoundSuccessCutscene(RelativeId girlId, RelativeId CutsceneId)
        => _instance._singleDateGirls.GetOrNew(girlId).CutsceneSuccessBonus = CutsceneId;

    [InteropMethod]
    public static void SwapGirls(RelativeId girlIdA, RelativeId girlIdB)
    {
        var aData = _instance._singleDateGirls.GetOrNew(girlIdA);
        var bData = _instance._singleDateGirls.GetOrNew(girlIdB);

        _instance._singleDateGirls[girlIdA] = bData;
        _instance._singleDateGirls[girlIdB] = aData;
    }

    [InteropMethod]
    public static void SetGirlCharm(RelativeId girlId, Sprite charmSprite)
        => _instance._singleDateGirls.GetOrNew(girlId).CharmSprite = charmSprite;

    [InteropMethod]
    public static bool IsSexDateValid(RelativeId girlId) => State.SaveFile.GetGirl(girlId).RelationshipLevel >= (MaxSensitivityLevel.Value - 1);

    [InteropMethod]
    public static IGameDefinitionInfo<CutsceneStepSubDefinition> MakeSexPhotoCutsceneStep() => new ShowSexPhotoCutsceneStep.Info();

    [InteropMethod]
    public static IGameDefinitionInfo<CutsceneStepSubDefinition> MakeDatePhotoCutsceneStep() => new ShowDatePhotoCutsceneStep.Info();

    [InteropMethod]
        public void RegisterCharmAnimation(
        float weight, 
        float cost,
        Func<Sequence, (RectTransform transform, bool dir, RelativeId girlId), bool> build,
        HashSet<RelativeId> allowedCharacters) 
            => _charmAnimationRegistry.Register(weight, cost, build, allowedCharacters);

    public static SingleDateGirl GetSingleDateGirl(RelativeId girlId)
            => _instance._singleDateGirls.GetOrNew(girlId);

    public static bool TryGetSingleDateGirl(RelativeId girlId, out SingleDateGirl singleDateGirl)
        => _instance._singleDateGirls.TryGetValue(girlId, out singleDateGirl);

    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        //styles
        var defaultGirlStyle = new GirlStyleInfo()
        {
            HairstyleId = RelativeId.Default,
            OutfitId = RelativeId.Default,
        };

        var defaultPairStyle = new PairStyleInfo()
        {
            MeetingGirlOne = defaultGirlStyle,
            MeetingGirlTwo = defaultGirlStyle,
            SexGirlOne = defaultGirlStyle,
            SexGirlTwo = defaultGirlStyle,
        };

        // base game girl pairs
        for (int i = 1; i < 13; i++)
        {
            var mod = new GirlPairDataMod(new RelativeId(State.ModId, i), InsertStyle.replace)
            {
                GirlDefinitionOneID = Girls.Nobody,
                GirlDefinitionTwoID = new RelativeId(-1, i),
                SpecialPair = false,
                PhotoDefinitionID = Photos.DefaultSex,
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (i % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = Locations.RoyalSuite,
                IntroRelationshipCutsceneDefinitionID = CutsceneIds.Meeting,
                AttractRelationshipCutsceneDefinitionID = CutsceneIds.Attract,
                PreSexRelationshipCutsceneDefinitionID = CutsceneIds.PreSex,
                PostSexRelationshipCutsceneDefinitionID = CutsceneIds.PostSex,
                SuccessCutsceneDefinitionID = CutsceneIds.Success,
                BonusSuccessCutsceneDefinitionID = CutsceneIds.BonusSuccess,
                Styles = defaultPairStyle
            };

            ModInterface.AddDataMod(mod);
        }
    }

    private void AddPhotoMod(RelativeId id, string name, bool hasAlts = false)
    {
        if (!(
                TryGetSprite($"photo_{name}_1", out var photoSprite)
                && TryGetSprite($"photo_{name}_1_thumb", out var photoThumbSprite)
            ))
        {
            ModInterface.Log.Warning($"Failed to get photo for {name}");
            return;
        }

        var photoMod = new PhotoDataMod(id, InsertStyle.replace)
        {
            HasAlts = false,

            //BigPhotoCensored = photoInfo,
            BigPhotoUncensored = new SpriteInfoSprite(photoSprite),
            //BigPhotoWet = photoInfo,

            //ThumbnailCensored = thumbInfo,
            ThumbnailUncensored = new SpriteInfoSprite(photoThumbSprite),
            //ThumbnailWet = thumbInfo,
        };

        ModInterface.AddDataMod(photoMod);

        if (hasAlts)
        {
            if (!(
                    TryGetSprite($"photo_{name}_1", out var photoSpriteAlt)
                    && TryGetSprite($"photo_{name}_1_thumb", out var photoThumbSpriteAlt)
                ))
            {
                ModInterface.Log.Warning($"Failed to get alt photo for {name}");
                return;
            }

            photoMod.HasAlts = true;
            photoMod.BigPhotoUncensoredAlt = new SpriteInfoSprite(photoSpriteAlt);
            photoMod.ThumbnailUncensoredAlt = new SpriteInfoSprite(photoThumbSpriteAlt);
        }
    }

    private bool TryGetSprite(string assetName, out Sprite sprite)
    {
        sprite = _assetBundle.LoadAsset<Sprite>(assetName);
        return sprite != null;
    }
}
