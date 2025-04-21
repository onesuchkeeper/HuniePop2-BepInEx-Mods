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
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        State.On_Plugin_Awake();

        var emptyPartId = new RelativeId(State.ModId, 0);
        var emptySpriteInfo = new SpriteInfoPath()
        {
            Path = "EmptySprite",
            IsExternal = false
        };

        ModInterface.AddDataMod(new GirlDataMod(new RelativeId(State.ModId, 0), InsertStyle.replace)
        {
            GirlName = "Nobody",
            SpecialCharacter = true,

            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(emptyPartId, InsertStyle.replace) {
                    X = 0,
                    Y = 0,
                    PartType = GirlPartType.BODY,
                    PartName = "Body",
                    SpriteInfo = emptySpriteInfo
                }
            },

            PartIdBody = emptyPartId,
            PartIdBlink = emptyPartId,
            PartIdBlushHeavy = emptyPartId,
            PartIdBlushLight = emptyPartId,
            PartIdMouthNeutral = emptyPartId,
            PartIdNipples = emptyPartId,
            HasAltStyles = false,

            CellphoneHead = emptySpriteInfo,
            CellphoneHeadAlt = emptySpriteInfo,
            CellphoneMiniHead = emptySpriteInfo,
            CellphoneMiniHeadAlt = emptySpriteInfo,
            CellphonePortrait = emptySpriteInfo,
            CellphonePortraitAlt = emptySpriteInfo,

            DefaultExpressionIndex = 0,
            DefaultHairstyleId = emptyPartId,
            DefaultOutfitId = emptyPartId,

            expressions = new List<IGirlSubDataMod<GirlExpressionSubDefinition>>(){
                new GirlExpressionDataMod(emptyPartId, InsertStyle.replace)
                {
                    ExpressionType = GirlExpressionType.NEUTRAL,
                    PartIdEyebrows = RelativeId.Default,
                    PartIdEyes = RelativeId.Default,
                    PartIdEyesGlow = RelativeId.Default,
                    PartIdMouthClosed = RelativeId.Default,
                    PartIdMouthOpen = RelativeId.Default
                }
            },

            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>(){
                new HairstyleDataMod(emptyPartId, InsertStyle.replace){
                    Name = string.Empty,
                    FrontHairPartId = RelativeId.Default,
                    BackHairPartId = RelativeId.Default
                }
            },

            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(emptyPartId, InsertStyle.replace)
                {
                    Name = string.Empty,
                    OutfitPartId = RelativeId.Default
                }
            }
        });

        var lailaniPhotoInfo = new SpriteInfoPath()
        {
            IsExternal = true,
            Path = Path.Combine(Paths.PluginPath, "SingleDate", "images", "photo_lailani_1.png")
        };

        var lailaniPhotoThumbInfo = new SpriteInfoPath()
        {
            IsExternal = true,
            Path = Path.Combine(Paths.PluginPath, "SingleDate", "images", "photo_lailani_1_thumb.png")
        };

        ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(State.ModId, 0), InsertStyle.replace)
        {
            HasAlts = false,

            BigPhotoCensored = lailaniPhotoInfo,
            BigPhotoUncensored = lailaniPhotoInfo,
            BigPhotoWet = lailaniPhotoInfo,

            ThumbnailCensored = lailaniPhotoThumbInfo,
            ThumbnailUncensored = lailaniPhotoThumbInfo,
            ThumbnailWet = lailaniPhotoThumbInfo,
        });

        ModInterface.Events.PreDataMods += On_PreDataMods;
        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        //meeting cutscene
        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(State.ModId, 0), InsertStyle.replace)
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

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(State.ModId, 1), InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 34)//big move
                }
            }
        });

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(State.ModId, 2), InsertStyle.replace)
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

        ModInterface.AddDataMod(new CutsceneDataMod(new RelativeId(State.ModId, 3), InsertStyle.replace)
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
        foreach (var girlId in ModInterface.Data.GetIds(GameDataType.Girl).Where(x => x.SourceId != State.ModId))
        {
            if (girlId.SourceId == -1 && girlId.LocalId > 12)
            {
                continue;
            }

            ModInterface.AddDataMod(new GirlPairDataMod(new RelativeId(State.ModId, pairCount), InsertStyle.replace)
            {
                GirlDefinitionOneID = new RelativeId(State.ModId, 0),
                GirlDefinitionTwoID = girlId,
                SpecialPair = false,
                PhotoDefinitionID = (girlId.SourceId == -1 && girlId.LocalId == 6)
                    ? new RelativeId(State.ModId, 0)
                    : new RelativeId(-1, 1),
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (pairCount % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = new RelativeId(-1, 20),//royal suite
                IntroRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 0),
                AttractRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 1),
                PreSexRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 2),
                PostSexRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 3),
                Styles = defaultPairStyle,
                FavQuestions = questions
            });

            pairCount++;
        }
    }
}
