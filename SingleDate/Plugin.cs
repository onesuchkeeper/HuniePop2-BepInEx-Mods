using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace SingleDate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
internal class Plugin : BaseUnityPlugin
{
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
                Logger.LogWarning($"Failed to find config binding for {nameof(ShowSingleUpsetHunt)}");
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
                Logger.LogWarning($"Failed to find config binding for {nameof(SingleDateBaggage)}");
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
                Logger.LogWarning($"Failed to find config binding for {nameof(RequireLoversBeforeThreesome)}");
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
                Logger.LogWarning($"Failed to find config binding for {nameof(MaxSingleGirlRelationshipLevel)}");
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
                Logger.LogWarning($"Failed to find config binding for {nameof(MaxSensitivityLevel)}");
            }
        }
    }

    public static readonly string RootDir = Path.Combine(Paths.PluginPath, "SingleDate");
    public static readonly string ImagesDir = Path.Combine(RootDir, "images");

    internal Dictionary<RelativeId, RelativeId> GirlIdToSexPhotoId;
    public void SetGirlSexPhoto(RelativeId girlId, RelativeId photoId) => GirlIdToSexPhotoId[girlId] = photoId;

    internal Dictionary<RelativeId, RelativeId> GirlIdToDatePhotoId;
    public void SetGirlDatePhoto(RelativeId girlId, RelativeId photoId) => GirlIdToDatePhotoId[girlId] = photoId;

    public static int ModId => _modId;
    private static int _modId;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
        SensitivityExp._id = new RelativeId(_modId, 0);
        _instance = this;

        this.Config.Bind(ConfigGeneralCat, nameof(ShowSingleUpsetHunt), false, "If upset hints are shown on single dates.");
        this.Config.Bind(ConfigGeneralCat, nameof(SingleDateBaggage), true, "If baggage is active on single dates.");
        this.Config.Bind(ConfigGeneralCat, nameof(RequireLoversBeforeThreesome), true, "If both characters must reach lovers on single dates before a threesome can occur.");
        this.Config.Bind(ConfigGeneralCat, nameof(MaxSingleGirlRelationshipLevel), 3, "Maximum relationship level for single dates. Maximum level must be reached for lovers status.");
        this.Config.Bind(ConfigGeneralCat, nameof(MaxSensitivityLevel), 4, "Maximum level for sensitivity.");

        Interop.RegisterInterModValues();//add interop for providing photos and charms

        GirlNobody.AddDataMods();
        ItemSensitivitySmoothie.AddDataMods();

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

        GirlIdToSexPhotoId = new Dictionary<RelativeId, RelativeId>(){
            {Girls.AbiaId, PhotoAbia.Id},
            {Girls.BrookeId, PhotoBrooke.Id},
            {Girls.CandaceId, PhotoCandace.Id},
            {Girls.LailaniId, PhotoLailani.Id},
            {Girls.LillianId, PhotoLillian.Id},
            {Girls.NoraId, PhotoNora.Id},
            {Girls.PollyId, PhotoPolly.Id},
            {Girls.SarahId, PhotoSarah.Id},
            {Girls.ZoeyId, PhotoZoey.Id},
        };

        GirlIdToDatePhotoId = new Dictionary<RelativeId, RelativeId>()
        {
        };

        UiPrefabs.InitExternals();

        ModInterface.Data.TryRegisterDataId(GameDataType.Affection, SensitivityExp.Id);
        ModInterface.AddExpInfo(SensitivityExp.Id, new SensitivityExp());

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

        //meeting cutscenes
        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(Plugin.ModId, 0), InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 10)//afternoon greeting
                }
            }
        });

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(ModId, 1), InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                //big move dialogue
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 34)//big move
                },

                //wait 0.5 sec
                new CutsceneStepInfo() {
                    StepType = CutsceneStepType.NOTHING,
                    ProceedFloat = 0.5f,
                    ProceedType = CutsceneStepProceedType.WAIT,
                },

                //show photo, shown photo is hard coded to the pair data
                // so instead this is handled in EventHandles.On_SinglePhotoDisplayed 
                new CutsceneStepInfo() {
                    StepType = CutsceneStepType.SHOW_WINDOW,
                    BoolValue = false,
                    WindowPrefabName = "PhotosWindow",
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                },
            }
        });

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(ModId, 2), InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 43)//moan 1
                }
            }
        });

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(ModId, 3), InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
        });

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
            if (!GirlIdToSexPhotoId.TryGetValue(girlId, out var photoId))
            {
                photoId = PhotoDefault.Id;
            }

            ModInterface.AddDataMod(new GirlPairDataMod(new RelativeId(ModId, pairCount), InsertStyle.replace)
            {
                GirlDefinitionOneID = GirlNobody.Id,
                GirlDefinitionTwoID = girlId,
                SpecialPair = false,
                PhotoDefinitionID = photoId,
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (pairCount % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = new RelativeId(-1, 20),//royal suite
                IntroRelationshipCutsceneDefinitionID = new RelativeId(ModId, 0),
                AttractRelationshipCutsceneDefinitionID = new RelativeId(ModId, 1),
                PreSexRelationshipCutsceneDefinitionID = new RelativeId(ModId, 2),
                PostSexRelationshipCutsceneDefinitionID = new RelativeId(ModId, 3),
                Styles = defaultPairStyle,
                FavQuestions = questions
            });

            pairCount++;
        }
    }
}
