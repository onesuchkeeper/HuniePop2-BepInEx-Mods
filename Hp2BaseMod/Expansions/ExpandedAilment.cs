using HarmonyLib;

namespace Hp2BaseMod;

/*
    An Ailment is a context instance for an AilmentDefinition that lives for the duration
    of a puzzle. It is owned by a PuzzleStatusGirl.
*/

[HarmonyPatch(typeof(Ailment))]
internal static class AilmentPatch
{
    [HarmonyPatch(nameof(Ailment.Enable))]
    [HarmonyPrefix]
    public static bool Enable(Ailment __instance) 
        => ExpandedAilment.Get(__instance).Enable();
}

/// <summary>
/// Holds additional runtime state for an <see cref="Ailment"/> instance.
/// Populated at construction time by <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/>
/// when the definition has one set.
/// </summary>
[Expansion(typeof(Ailment))]
public partial class ExpandedAilment
{
    /// <summary>
    /// The scripted behaviour attached to this ailment instance.
    /// Null if this is a purely data-driven ailment.
    /// </summary>
    public IScriptedAilment ScriptedAilment;

    public bool Enable()
    {
        var grid = Game.Session.Puzzle?.puzzleGrid;
        if (grid == null) return true;

        var expanded = ExpandedUiPuzzleGrid.Get(grid);
        var status = Game.Session.Puzzle.puzzleStatus;
        if (status == null || status.isEmpty) return true;

        // Find which girl owns this ailment.
        PuzzleStatusGirl girl = null;
        PuzzleStatusGirl otherGirl = null;
        if (status.girlStatusLeft.ailments.Contains(_core)) 
        {
            girl = status.girlStatusLeft;
            otherGirl = status.girlStatusRight;
        } 
        else if (status.girlStatusRight.ailments.Contains(_core)) 
        {
            girl = status.girlStatusRight;
            otherGirl = status.girlStatusLeft;
        }

        if (girl == null) return true;

        return expanded.CanEnableAilment(_core, girl, otherGirl);
    }
}