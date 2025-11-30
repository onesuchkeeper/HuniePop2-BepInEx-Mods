using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
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
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
internal class Plugin : Hp2BaseModPlugin
{
    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    [ConfigProperty(false, "If upset hints are shown on single dates.")]
    public static bool ShowSingleUpsetHunt
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If baggage is active on single dates.")]
    public static bool SingleDateBaggage
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(false, "If both characters must reach lovers on single dates before a threesome can occur.")]
    public static bool RequireLoversBeforeThreesome
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(3, "Maximum relationship level for single dates. Maximum level must be reached for lovers status.")]
    public static int MaxSingleGirlRelationshipLevel
    {
        get => _instance.GetConfigProperty<int>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(4, "Maximum level for sensitivity.")]
    public static int MaxSensitivityLevel
    {
        get => _instance.GetConfigProperty<int>();
        set => _instance.SetConfigProperty(value);
    }

    private Dictionary<RelativeId, SingleDateGirl> _singleDateGirls = new();
    private static Plugin _instance;
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    private void Awake()
    {
        _instance = this;

        State.On_Plugin_Awake();

        GirlNobody.AddDataMods();
        ItemSensitivitySmoothie.AddDataMods();

        SingleDateMeetingCutscene.AddDataMods();
        SingleDateAttractCutscene.AddDataMods();
        SingleDatePreSexCutscene.AddDataMods();
        SingleDatePostSexCutscene.AddDataMods();
        SingleDateSuccessCutscene.AddDataMods();

        PhotoDefault.AddDataMods();
        PhotoAbia.AddDataMods();
        PhotoBrooke.AddDataMods();
        PhotoCandace.AddDataMods();
        PhotoLailani.AddDataMods();
        PhotoLillian.AddDataMods();
        PhotoNora.AddDataMods();
        PhotoPolly.AddDataMods();
        PhotoSarah.AddDataMods();
        PhotoZoey.AddDataMods();

        AddGirlSexPhotos(Girls.AbiaId, [(PhotoAbia.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.BrookeId, [(PhotoBrooke.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.CandaceId, [(PhotoCandace.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.LailaniId, [(PhotoLailani.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.LillianId, [(PhotoLillian.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.NoraId, [(PhotoNora.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.PollyId, [(PhotoPolly.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.SarahId, [(PhotoSarah.Id, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Girls.ZoeyId, [(PhotoZoey.Id, Locations.RoyalSuite)]);

        UiPrefabs.InitExternals();

        ModInterface.AddExp(new SensitivityExp());

        ModInterface.Events.PreDataMods += On_PreDataMods;

        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;

        ModInterface.Events.PuzzleRoundOver += ModEventHandles.On_PuzzleRoundOver;
        ModInterface.Events.FinderSlotsPopulate += ModEventHandles.On_FinderSlotsPopulate;
        ModInterface.Events.LocationArriveSequence += ModEventHandles.On_LocationArriveSequence;
        ModInterface.Events.RandomDollSelected += ModEventHandles.On_RandomDollSelected;
        ModInterface.Events.DateLocationSelected += ModEventHandles.On_DateLocationSelected;
        ModInterface.Events.SinglePhotoDisplayed += ModEventHandles.On_SinglePhotoDisplayed;
        ModInterface.Events.RequestUnlockedPhotos += ModEventHandles.On_RequestUnlockedPhotos;
        ModInterface.Events.PreDateDollReset += ModEventHandles.On_PreDateDollsRefresh;

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
    public static bool IsSexDateValid(RelativeId girlId) => State.SaveFile.GetGirl(girlId).RelationshipLevel >= (MaxSensitivityLevel - 1);

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

        var questions = new List<IGameDefinitionInfo<GirlPairFavQuestionSubDefinition>>();

        for (int i = 1; i <= 20; i++)
        {
            questions.Add(new GirlPairFavQuestionInfo()
            {
                GirlResponseIndexOne = 1,
                GirlResponseIndexTwo = 1,
                QuestionDefinitionID = new RelativeId(-1, i)
            });
        }

        //pairs
        for (int i = 1; i < 13; i++)
        {
            ModInterface.AddDataMod(new GirlPairDataMod(new RelativeId(State.ModId, i), InsertStyle.replace)
            {
                GirlDefinitionOneID = GirlNobody.Id,
                GirlDefinitionTwoID = new RelativeId(-1, i),
                SpecialPair = false,
                PhotoDefinitionID = PhotoDefault.Id,
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (i % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = new RelativeId(-1, 20),//royal suite
                IntroRelationshipCutsceneDefinitionID = CutsceneIds.Meeting,
                AttractRelationshipCutsceneDefinitionID = CutsceneIds.Attract,
                PreSexRelationshipCutsceneDefinitionID = CutsceneIds.PreSex,
                PostSexRelationshipCutsceneDefinitionID = CutsceneIds.PostSex,
                Styles = defaultPairStyle,
                FavQuestions = questions
            });
        }
    }
}
