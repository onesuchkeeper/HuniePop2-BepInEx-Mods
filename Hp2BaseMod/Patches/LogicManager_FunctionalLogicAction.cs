// Hp2BaseMod 2026, By OneSuchKeeper

using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Patches allowing for <see cref="IFunctionalLogicAction"/> triggers and 
/// updated conditions using <see cref="IGameState"/>
/// </summary>
[HarmonyPatch(typeof(LogicManager))]
internal static class LogicManagerPatch
{
    [HarmonyPatch(nameof(LogicManager.PerformAction))]
    [HarmonyPrefix]
    private static bool PerformAction_Prefix(LogicAction action)
    {
        if (action is IFunctionalLogicAction functionalLogicAction)
        {
            functionalLogicAction.Act();
            return false;
        }

        return true;
    }

    // The two conditions that ask about the kind of puzzle are answered by the puzzle state.
    [HarmonyPatch(nameof(LogicManager.IsConditionMet))]
    [HarmonyPrefix]
    private static bool IsConditionMet_Prefix(LogicCondition condition, ref bool __result)
    {
        var puzzleState = ModInterface.GameState.CurrentState as IPuzzleState;

        bool met;
        switch (condition.type)
        {
            case LogicConditionType.IS_TUTORIAL:
                met = puzzleState?.MatchesTutorialCondition(condition.boolValue) ?? false;
                break;
            case LogicConditionType.DATE_TYPE:
                met = Game.Session.Puzzle.isPuzzleActive
                    && puzzleState != null
                    && puzzleState.ConditionDateType == condition.dateType;
                break;
            default:
                return true;
        }

        __result = condition.inverse ? !met : met;
        return false;
    }
}