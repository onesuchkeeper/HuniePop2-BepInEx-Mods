using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PuzzleManager))]
internal static class PuzzleManagerPatch
{
    private static readonly MethodInfo m_handleCutscenes = AccessTools.Method(typeof(PuzzleManagerPatch), nameof(HandleCutscenes));
    private static readonly MethodInfo m_checkRelationship = AccessTools.Method(typeof(PuzzleManagerPatch), nameof(CheckRelationship));

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    private static void OnDestroy(PuzzleManager __instance) 
        => ExpandedPuzzleManager.Destroy(__instance);

    [HarmonyPatch("OnRoundOver")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OnRoundOver(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Call, m_checkRelationship);

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
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
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

    public static void CheckRelationship(PuzzleManager manager)
    {
        ModInterface.Log.Message("CHECK RELATIONSHIP");
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL) return;

        var currentGirlPair = Game.Session.Location.currentGirlPair;
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);
        var managerExp = manager.GetExpansion();

        var args = new PuzzleRoundOverArgs();
        args.IsSexDate = playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Session.Location.currentLocation == currentGirlPair.sexLocationDefinition;

        switch (Game.Session.Puzzle.puzzleStatus.statusType)
        {
            case PuzzleStatusType.NORMAL:
                switch (playerFileGirlPair.relationshipType)
                {
                    case GirlPairRelationshipType.UNKNOWN:
                        args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
                        break;
                    case GirlPairRelationshipType.COMPATIBLE:
                        args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.CompatToAttract;
                        break;
                    case GirlPairRelationshipType.ATTRACTED:
                        args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.AttractToLovers;
                        break;
                    case GirlPairRelationshipType.LOVERS:
                        args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
                        break;
                }
                break;
            case PuzzleStatusType.NONSTOP:
            case PuzzleStatusType.BOSS:
                args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
                break;
        }

        managerExp.Args = args;
    }

    public static void HandleCutscenes(PuzzleManager manager)
    {
        ModInterface.Log.Message("HANDLE CUTSCENES");
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL) return;

        var managerExp = manager.GetExpansion();
        var args = manager.GetExpansion().Args;
        args.IsGameOver = Game.Session.Puzzle.puzzleStatus.gameOver;
        args.IsSuccess = Game.Session.Puzzle.puzzleGrid.roundState == PuzzleRoundState.SUCCESS;

        ModInterface.Events.NotifyPuzzleRoundOver(args);

        ModInterface.Log.Message(args.ToString());

        var puzzleStatusExp = Game.Session.Puzzle.puzzleStatus.GetExpansion();
        puzzleStatusExp.GameOver = args.IsGameOver;

        var puzzleGridExp = Game.Session.Puzzle.puzzleGrid.GetExpansion();
        puzzleGridExp.RoundState = args.IsSuccess
            ? PuzzleRoundState.SUCCESS
            : PuzzleRoundState.FAILURE;

        //TODO, handle other puzzle types
        switch (Game.Session.Puzzle.puzzleStatus.statusType)
        {
            case PuzzleStatusType.NORMAL:
                ProcessNormalDate(managerExp);
                break;
            case PuzzleStatusType.NONSTOP:
                break;
            case PuzzleStatusType.BOSS:
                break;
        }
    }

    private static void ProcessNormalDate(ExpandedPuzzleManager managerExp)
    {
        ModInterface.Log.Message("PROCESS NORMAL DATE");
        var args = managerExp.Args;
        var pairExpansion = Game.Session.Location.currentGirlPair.GetExpansion();

        if (args.IsSuccess)
        {
            if (args.IsSexDate)
            {
                if (Game.Session.Puzzle.puzzleStatus.bonusRound)
                {
                    managerExp.NewRoundCutscene = null;

                    managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusSuccessId) 
                        ?? Game.Session.Puzzle.cutsceneSuccessBonus;
                }
                else
                {
                    managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalAttractedSuccessId) 
                        ?? Game.Session.Puzzle.cutsceneSuccessAttracted;

                    managerExp.NewRoundCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusNewRoundId) 
                        ?? Game.Session.Puzzle.cutsceneNewroundBonus;
                }
            }
            else
            {
                switch (args.LevelUpType)
                {
                    case PuzzleRoundOverArgs.CutsceneType.None:
                        managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalSuccessId) ?? Game.Session.Puzzle.cutsceneSuccess;
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.AttractToLovers:
                        managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalAttractedSuccessId) 
                            ?? Game.Session.Puzzle.cutsceneSuccessAttracted;
                        break;
                    case PuzzleRoundOverArgs.CutsceneType.CompatToAttract:
                        managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalCompatibleSuccessId) 
                            ?? Game.Session.Puzzle.cutsceneSuccessCompatible;
                        break;
                }
            }
        }
        else
        {
            managerExp.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalFailureId) 
                ?? Game.Session.Puzzle.cutsceneFailure;
        }
    }
}

[Expansion(typeof(PuzzleManager), 
    Fields = new[]{"_roundOverCutscene", "_newRoundCutscene"})]
public partial class ExpandedPuzzleManager
{
    public PuzzleRoundOverArgs Args;

    public CutsceneDefinition RoundOverCutscene
    {
        get => _roundOverCutscene;
        set => _roundOverCutscene = value;
    }

    public CutsceneDefinition NewRoundCutscene
    {
        get => _newRoundCutscene;
        set => _newRoundCutscene = value;
    }

    private void OnDestroy()
    {
        _core.puzzleStatus?.GetExpansion().Destroy();
    }
}