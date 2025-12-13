using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PuzzleManager))]
public static class PuzzleMangerPatch
{
    private static FieldInfo f_roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
    private static FieldInfo f_newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");
    private static FieldInfo f_gameOver = AccessTools.Field(typeof(PuzzleStatus), "_gameOver");
    private static FieldInfo f_roundState = AccessTools.Field(typeof(UiPuzzleGrid), "_roundState");
    private static MethodInfo m_handleCutscenes = AccessTools.Method(typeof(PuzzleMangerPatch), nameof(HandleCutscenes));
    private static MethodInfo m_checkRelationShip = AccessTools.Method(typeof(PuzzleMangerPatch), nameof(CheckRelationship));

    private static PuzzleRoundOverArgs _args;

    [HarmonyPatch("OnRoundOver")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OnRoundOver(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Call, m_checkRelationShip);

        int step = 0;
        foreach (var instruction in instructions)
        {
            switch (step)
            {
                case 0:
                    step = instruction.opcode == OpCodes.Ldarg_0 ? 1 : 0;
                    break;
                case 1:
                    step = instruction.opcode == OpCodes.Ldfld ? 2 : 0;
                    break;
                case 2:
                    step = instruction.opcode == OpCodes.Ldloc_0 ? 3 : 0;
                    break;
                case 3:
                    step = instruction.opcode == OpCodes.Callvirt && instruction.operand?.ToString() == "Void set_gameOver(Boolean)" ? 4 : 0;
                    break;
                case 4:
                    step = -1;
                    yield return new CodeInstruction(OpCodes.Call, m_handleCutscenes);
                    break;
            }

            yield return instruction;
        }

        if (step != -1)
        {
            ModInterface.Log.Error("Failed to transply hook into PuzzleManager.OnRoundOver");
        }
    }

    public static void CheckRelationship()
    {
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL) return;

        var currentGirlPair = Game.Session.Location.currentGirlPair;
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);

        _args = new PuzzleRoundOverArgs();
        _args.IsSexDate = playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Session.Location.currentLocation == currentGirlPair.sexLocationDefinition;

        switch (Game.Session.Puzzle.puzzleStatus.statusType)
        {
            case PuzzleStatusType.NORMAL:
                _args.LevelUpType = playerFileGirlPair.relationshipType == GirlPairRelationshipType.COMPATIBLE
                    ? PuzzleRoundOverArgs.CutsceneType.CompatToAttract
                    : PuzzleRoundOverArgs.CutsceneType.AttractToLovers;
                break;
            case PuzzleStatusType.NONSTOP:
            case PuzzleStatusType.BOSS:
                _args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
                break;
        }
    }

    public static void HandleCutscenes()
    {
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL) return;

        _args.IsGameOver = Game.Session.Puzzle.puzzleStatus.gameOver;
        _args.IsSuccess = Game.Session.Puzzle.puzzleGrid.roundState == PuzzleRoundState.SUCCESS;

        ModInterface.Events.NotifyPuzzleRoundOver(_args);

        ModInterface.Log.Message(_args.ToString());

        f_gameOver.SetValue(Game.Session.Puzzle.puzzleStatus, _args.IsGameOver);
        f_roundState.SetValue(Game.Session.Puzzle.puzzleGrid, _args.IsSuccess
            ? PuzzleRoundState.SUCCESS
            : PuzzleRoundState.FAILURE);

        //TODO, handle other puzzle types
        switch (Game.Session.Puzzle.puzzleStatus.statusType)
        {
            case PuzzleStatusType.NORMAL:
                ProcessNormalDate();
                break;
            case PuzzleStatusType.NONSTOP:
                break;
            case PuzzleStatusType.BOSS:
                break;
        }
    }

    private static void ProcessNormalDate()
    {
        var pairExpansion = Game.Session.Location.currentGirlPair.Expansion();

        if (_args.IsSuccess)
        {
            if (_args.IsSexDate)
            {
                if (Game.Session.Puzzle.puzzleStatus.bonusRound)
                {
                    f_newRoundCutscene.SetValue(Game.Session.Puzzle, null);

                    f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessBonus);
                }
                else
                {
                    f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalAttractedSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessAttracted);

                    f_newRoundCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusNewRoundId) ?? Game.Session.Puzzle.cutsceneNewroundBonus);
                }
            }
            else
            {
                switch (_args.LevelUpType)
                {
                    case PuzzleRoundOverArgs.CutsceneType.None:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalSuccessId) ?? Game.Session.Puzzle.cutsceneSuccess);
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.AttractToLovers:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalAttractedSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessAttracted);
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.CompatToAttract:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalCompatibleSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessCompatible);
                        break;
                }
            }
        }
        else
        {
            f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                    ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalFailureId) ?? Game.Session.Puzzle.cutsceneFailure);
        }
    }
}
