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
using MonoMod.Utils;

namespace SingleDate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
internal class Plugin : BaseUnityPlugin
{
    private readonly static string CONFIG_ERROR = "Failed to find config binding for ";
    internal static Plugin Instance => _instance;
    private static Plugin _instance;

    private static string ConfigGeneralCat = "General";

    internal bool ShowSingleUpsetHunt
    {
        get => this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(ShowSingleUpsetHunt), out var config)
            ? config.Value
            : false;
        set
        {
            if (this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(ShowSingleUpsetHunt), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning(CONFIG_ERROR + nameof(ShowSingleUpsetHunt));
            }
        }
    }

    internal bool SingleDateBaggage
    {
        get => this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(SingleDateBaggage), out var config)
            ? config.Value
            : false;
        set
        {
            if (this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(SingleDateBaggage), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning(CONFIG_ERROR + nameof(SingleDateBaggage));
            }
        }
    }

    internal bool RequireLoversBeforeThreesome
    {
        get => this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(RequireLoversBeforeThreesome), out var config)
            ? config.Value
            : false;
        set
        {
            if (this.Config.TryGetEntry<bool>(ConfigGeneralCat, nameof(RequireLoversBeforeThreesome), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning(CONFIG_ERROR + nameof(RequireLoversBeforeThreesome));
            }
        }
    }

    internal int MaxSingleGirlRelationshipLevel
    {
        get
        {
            if (this.Config.TryGetEntry<int>(ConfigGeneralCat, nameof(MaxSingleGirlRelationshipLevel), out var config)
                && config.Value > 0)
            {
                return config.Value;
            }

            return 3;
        }
        set
        {
            if (this.Config.TryGetEntry<int>(ConfigGeneralCat, nameof(MaxSingleGirlRelationshipLevel), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning(CONFIG_ERROR + nameof(MaxSingleGirlRelationshipLevel));
            }
        }
    }

    internal int MaxSensitivityLevel
    {
        get
        {
            if (this.Config.TryGetEntry<int>(ConfigGeneralCat, nameof(MaxSensitivityLevel), out var config)
                && config.Value > 0)
            {
                return config.Value;
            }

            return 4;
        }
        set
        {
            if (this.Config.TryGetEntry<int>(ConfigGeneralCat, nameof(MaxSensitivityLevel), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning(CONFIG_ERROR + nameof(MaxSensitivityLevel));
            }
        }
    }

    public static readonly string RootDir = Path.Combine(Paths.PluginPath, "SingleDate");
    public static readonly string ImagesDir = Path.Combine(RootDir, "images");

    internal Dictionary<RelativeId, List<RelativeId>> GirlIdToSexPhotoId = new();
    public void AddGirlSexPhotos(RelativeId girlId, IEnumerable<RelativeId> photoIds)
        => GirlIdToSexPhotoId.GetOrNew(girlId).AddRange(photoIds);

    internal Dictionary<RelativeId, List<(RelativeId, float)>> GirlIdToDatePhotoId = new();
    public void AddGirlDatePhotos(RelativeId girlId, IEnumerable<(RelativeId, float)> photoIds)
        => GirlIdToDatePhotoId.GetOrNew(girlId).AddRange(photoIds);

    public void SwapPhotos(RelativeId girlAId, RelativeId girlBId)
    {
        var gotADate = GirlIdToDatePhotoId.TryGetValue(girlAId, out var aDatePhotos);
        var gotBDate = GirlIdToDatePhotoId.TryGetValue(girlAId, out var bDatePhotos);

        if (gotADate)
        {
            GirlIdToDatePhotoId[girlBId] = aDatePhotos;
        }
        else if (gotBDate)
        {
            GirlIdToDatePhotoId.Remove(girlBId);
        }

        if (gotBDate)
        {
            GirlIdToDatePhotoId[girlAId] = bDatePhotos;
        }
        else if (gotADate)
        {
            GirlIdToDatePhotoId.Remove(girlAId);
        }

        var gotASex = GirlIdToSexPhotoId.TryGetValue(girlAId, out var aSexPhotos);
        var gotBSex = GirlIdToSexPhotoId.TryGetValue(girlAId, out var bSexPhotos);

        if (gotASex)
        {
            GirlIdToSexPhotoId[girlBId] = aSexPhotos;
        }
        else if (gotBSex)
        {
            GirlIdToSexPhotoId.Remove(girlBId);
        }

        if (gotBSex)
        {
            GirlIdToSexPhotoId[girlAId] = bSexPhotos;
        }
        else if (gotASex)
        {
            GirlIdToSexPhotoId.Remove(girlAId);
        }
    }

    private void Awake()
    {
        _instance = this;

        this.Config.Bind(ConfigGeneralCat, nameof(ShowSingleUpsetHunt), false, "If upset hints are shown on single dates.");
        this.Config.Bind(ConfigGeneralCat, nameof(SingleDateBaggage), true, "If baggage is active on single dates.");
        this.Config.Bind(ConfigGeneralCat, nameof(RequireLoversBeforeThreesome), true, "If both characters must reach lovers on single dates before a threesome can occur.");
        this.Config.Bind(ConfigGeneralCat, nameof(MaxSingleGirlRelationshipLevel), 3, "Maximum relationship level for single dates. Maximum level must be reached for lovers status.");
        this.Config.Bind(ConfigGeneralCat, nameof(MaxSensitivityLevel), 4, "Maximum level for sensitivity.");

        State.On_Plugin_Awake();

        Interop.RegisterInterModValues();//add interop for providing photos and charms

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

        AddGirlSexPhotos(Girls.AbiaId, [PhotoAbia.Id]);
        AddGirlSexPhotos(Girls.BrookeId, [PhotoBrooke.Id]);
        AddGirlSexPhotos(Girls.CandaceId, [PhotoCandace.Id]);
        AddGirlSexPhotos(Girls.LailaniId, [PhotoLailani.Id]);
        AddGirlSexPhotos(Girls.LillianId, [PhotoLillian.Id]);
        AddGirlSexPhotos(Girls.NoraId, [PhotoNora.Id]);
        AddGirlSexPhotos(Girls.PollyId, [PhotoPolly.Id]);
        AddGirlSexPhotos(Girls.SarahId, [PhotoSarah.Id]);
        AddGirlSexPhotos(Girls.ZoeyId, [PhotoZoey.Id]);

        UiPrefabs.InitExternals();

        ModInterface.AddExp(new SensitivityExp());

        ModInterface.Events.PreDataMods += On_PreDataMods;

        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;

        ModInterface.Events.PreRoundOverCutscene += EventHandles.On_PreRoundOverCutscene;
        ModInterface.Events.FinderSlotsPopulate += EventHandles.On_FinderSlotsPopulate;
        ModInterface.Events.LocationArriveSequence += EventHandles.On_LocationArriveSequence;
        ModInterface.Events.RandomDollSelected += EventHandles.On_RandomDollSelected;
        ModInterface.Events.DateLocationSelected += EventHandles.On_DateLocationSelected;
        ModInterface.Events.SinglePhotoDisplayed += EventHandles.On_SinglePhotoDisplayed;
        ModInterface.Events.RequestUnlockedPhotos += EventHandles.On_RequestUnlockedPhotos;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

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
        var pairCount = 0;
        foreach (var girlId in ModInterface.Data.GetIds(GameDataType.Girl).Where(x => x != GirlNobody.Id || !(x.SourceId == -1 && x.LocalId > 12)))
        {
            ModInterface.AddDataMod(new GirlPairDataMod(new RelativeId(State.ModId, pairCount), InsertStyle.replace)
            {
                GirlDefinitionOneID = GirlNobody.Id,
                GirlDefinitionTwoID = girlId,
                SpecialPair = false,
                PhotoDefinitionID = PhotoDefault.Id,
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (pairCount % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = new RelativeId(-1, 20),//royal suite
                IntroRelationshipCutsceneDefinitionID = CutsceneIds.Meeting,
                AttractRelationshipCutsceneDefinitionID = CutsceneIds.Attract,
                PreSexRelationshipCutsceneDefinitionID = CutsceneIds.PreSex,
                PostSexRelationshipCutsceneDefinitionID = CutsceneIds.PostSex,
                Styles = defaultPairStyle,
                FavQuestions = questions
            });

            pairCount++;
        }
    }
}
