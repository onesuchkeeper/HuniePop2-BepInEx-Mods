using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static readonly string RootDir = Path.Combine(Paths.PluginPath, "SingleDate");
    public static readonly string ImagesDir = Path.Combine(RootDir, "images");

    private void Awake()
    {
        State.On_Plugin_Awake();

        GirlNobody.AddDataMods();
        ItemSensitivitySmoothie.AddDataMods();
        PhotoLailani.AddDataMods();

        UiPrefabs.InitExternals();

        ModInterface.AddExp(new SensitivityExp());

        ModInterface.Events.PreDataMods += On_PreDataMods;
        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;
        ModInterface.Events.PreRoundOverCutscene += On_PreRoundOverCutscene;
        ModInterface.Events.FinderSlotsPopulate += On_FinderSlotsPopulate;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel - 1);

        foreach (var id in args.SexPool)
        {
            ModInterface.Log.LogInfo(id.ToString());
        }

        if (State.RequireLoversBeforeThreesome)
        {
            args.SexPool?.RemoveAll(x => !State.IsSingle(x.girlPairDefinition)
                && (State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionOne.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel
                || State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel));
        }

        args.CompatiblePool.AddRange(args.AttractedPool.Where(IsIncompleteAttracted));
        args.AttractedPool.RemoveAll(IsIncompleteAttracted);
    }

    private static bool IsIncompleteAttracted(PlayerFileGirlPair filePair) => State.IsSingle(filePair.girlPairDefinition)
        && State.SaveFile.GetGirl(filePair.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel < (State.MaxSingleGirlRelationshipLevel - 1);

    private static FieldInfo _roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
    private static FieldInfo _newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");

    private void On_PreRoundOverCutscene()
    {
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL
            || Game.Session.Puzzle.puzzleGrid.roundState != PuzzleRoundState.SUCCESS)
        {
            return;
        }

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null
            || playerFileGirlPair.relationshipType != GirlPairRelationshipType.ATTRACTED)
        {
            return;
        }

        bool validSingleLevels = false;
        if (State.IsSingleDate)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            //single date relationship levels are handled post round over, so girl will be at max already for bonus round
            validSingleLevels = girlSave?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel;
        }
        else if (State.RequireLoversBeforeThreesome)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            validSingleLevels = (girlSaveOne == null || girlSaveOne.RelationshipLevel == State.MaxSingleGirlRelationshipLevel)
                && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == State.MaxSingleGirlRelationshipLevel);
        }

        //disable the bonus round for dates without the correct single relationship level
        if (!validSingleLevels && Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            ModInterface.Log.LogInfo("Invalid single date level, changing from bonus round to cutscene success");
            Game.Session.Puzzle.puzzleStatus.gameOver = true;
            _roundOverCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneSuccess);
            _newRoundCutscene.SetValue(Game.Session.Puzzle, null);
        }
    }

    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        //meeting cutscenes
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
        foreach (var girlId in ModInterface.Data.GetIds(GameDataType.Girl).Where(x => x != GirlNobody.Id || !(x.SourceId == -1 && x.LocalId > 12)))
        {
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
