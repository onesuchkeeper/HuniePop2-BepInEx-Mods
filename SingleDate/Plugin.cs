using System.Collections.Generic;
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
        State.ModId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

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

        ModInterface.Events.PreDataMods += On_PreDataMods;

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

        //favorite questions (I can't look these up because mods haven't processed yet...)
        //I need some kind of meta step, I could technically do that here...
        //just afterwards I go and correct everything?
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
            //I only want non-special girls, no kyu/moxie/jewn
            //that may be what's throwing this off
            //but I don't have a way to check if a girl is special until after 
            //data mods have been applied
            //test hacky fix
            if (girlId.SourceId == -1 && girlId.LocalId > 12)
            {
                continue;
            }

            //each girl needs her own 'nobody' to pair with,
            //otherwise only one single date can be selected

            ModInterface.AddDataMod(new GirlPairDataMod(new RelativeId(State.ModId, pairCount), InsertStyle.replace)
            {
                GirlDefinitionOneID = new RelativeId(State.ModId, 0),
                GirlDefinitionTwoID = girlId,
                SpecialPair = false,
                PhotoDefinitionID = new RelativeId(-1, 1),
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (pairCount % 8)),
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = new RelativeId(-1, 20),
                IntroRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 0),
                AttractRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 1),
                PreSexRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 2),
                PostSexRelationshipCutsceneDefinitionID = new RelativeId(State.ModId, 1),
                Styles = defaultPairStyle,
                FavQuestions = questions
            });

            pairCount++;
        }
    }
}