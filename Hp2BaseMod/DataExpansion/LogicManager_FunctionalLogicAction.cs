using HarmonyLib;

namespace Hp2BaseMod;

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
}