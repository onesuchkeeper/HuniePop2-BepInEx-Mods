using HarmonyLib;
using Hp2BaseMod.Extension;

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
}

[Expansion(typeof(PuzzleStatus), Fields = new[]{"_gameOver"})]
public partial class ExpandedPuzzleStatus
{
    public bool GameOver
    {
        get => _gameOver;
        set => _gameOver = value;
    }

    public void Clear()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }

    private void OnDestroy()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }
}