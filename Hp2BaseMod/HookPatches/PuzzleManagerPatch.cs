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
    private static MethodInfo m_checkRelationShip = AccessTools.Method(typeof(PuzzleMangerPatch), nameof(CheckRelationShip));

    private static PuzzleRoundOverArgs _args;

    public static void CheckRelationShip()
    {
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
        _args.IsGameOver = Game.Session.Puzzle.puzzleStatus.gameOver;
        _args.IsSuccess = Game.Session.Puzzle.puzzleGrid.roundState == PuzzleRoundState.SUCCESS;

        var pairExpansion = Game.Session.Location.currentGirlPair.Expansion();

        ModInterface.Events.NotifyPuzzleRoundOver(_args);

        ModInterface.Log.LogInfo(_args.ToString());

        f_gameOver.SetValue(Game.Session.Puzzle.puzzleStatus, _args.IsGameOver);
        f_roundState.SetValue(Game.Session.Puzzle.puzzleGrid, _args.IsSuccess
            ? PuzzleRoundState.SUCCESS
            : PuzzleRoundState.FAILURE);

        if (_args.IsSuccess)
        {
            if (_args.IsSexDate)
            {
                if (Game.Session.Puzzle.puzzleStatus.bonusRound)
                {
                    f_newRoundCutscene.SetValue(Game.Session.Puzzle, null);

                    f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneBonusSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessBonus);
                }
                else
                {
                    ModInterface.Log.LogInfo($"custom attract success cutscene id{pairExpansion.CutsceneAttractedSuccessId}");
                    f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneAttractedSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessAttracted);

                    f_newRoundCutscene.SetValue(Game.Session.Puzzle,
                        ModInterface.GameData.GetCutscene(pairExpansion.CutsceneBonusNewRoundId) ?? Game.Session.Puzzle.cutsceneNewroundBonus);
                }
            }
            else
            {
                switch (_args.LevelUpType)
                {
                    case PuzzleRoundOverArgs.CutsceneType.None:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneSuccessId) ?? Game.Session.Puzzle.cutsceneSuccess);
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.AttractToLovers:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneAttractedSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessAttracted);
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.CompatToAttract:
                        f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                            ModInterface.GameData.GetCutscene(pairExpansion.CutsceneCompatibleSuccessId) ?? Game.Session.Puzzle.cutsceneSuccessCompatible);
                        break;
                }
            }
        }
        else
        {
            f_roundOverCutscene.SetValue(Game.Session.Puzzle,
                    ModInterface.GameData.GetCutscene(pairExpansion.CutsceneFailureId) ?? Game.Session.Puzzle.cutsceneFailure);
        }
    }

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
            ModInterface.Log.LogError("Failed to transply hook into PuzzleManager.OnRoundOver");
        }
    }
}
