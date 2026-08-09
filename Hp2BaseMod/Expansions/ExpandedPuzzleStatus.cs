using System.Linq;
using HarmonyLib;

namespace Hp2BaseMod;
/*
    In the base game PuzzleStatus is a member of PuzzleManager
    Instantiated in Awake.
*/

[HarmonyPatch(typeof(PuzzleStatus))]
internal static class PuzzleStatusPatch
{
    [HarmonyPatch(nameof(PuzzleStatus.Clear))]
    [HarmonyPrefix]
    private static void Clear(PuzzleStatus __instance) 
        => ExpandedPuzzleStatus.Get(__instance).Clear();

    [HarmonyPatch(nameof(PuzzleStatus.EnableAilments))]
    [HarmonyPrefix]
    private static bool EnableAilments(PuzzleStatus __instance) 
        => ExpandedPuzzleStatus.Get(__instance).EnableAilments_Prefix();
}

[Expansion(typeof(PuzzleStatus))]
public partial class ExpandedPuzzleStatus
{
    internal void Clear()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }

    internal bool EnableAilments_Prefix()
    {
        foreach (var ailment in _girlStatusLeft.ailments.Concat(_girlStatusRight.ailments))
        {
            Game.Session.Ailment.GetExpansion().Enable(ailment, _girlStatusLeft);
        }

        foreach (var ailment in _girlStatusRight.ailments.Concat(_girlStatusRight.ailments))
        {
            Game.Session.Ailment.GetExpansion().Enable(ailment, _girlStatusRight);
        }

        return false;
    }

    private void OnDestroy()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }
}