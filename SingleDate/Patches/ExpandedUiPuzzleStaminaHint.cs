using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleStaminaHint))]
public static class UiPuzzleStaminaHintPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleStaminaHint __instance)
        => ExpandedUiPuzzleStaminaHint.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleStaminaHint __instance)
        => ExpandedUiPuzzleStaminaHint.Get(__instance).OnDestroy();
}

public class ExpandedUiPuzzleStaminaHint
{
    private static Dictionary<UiPuzzleStaminaHint, ExpandedUiPuzzleStaminaHint> _expansions
         = new Dictionary<UiPuzzleStaminaHint, ExpandedUiPuzzleStaminaHint>();

    public static ExpandedUiPuzzleStaminaHint Get(UiPuzzleStaminaHint uiPuzzleStaminaHint)
    {
        if (!_expansions.TryGetValue(uiPuzzleStaminaHint, out var expansion))
        {
            expansion = new ExpandedUiPuzzleStaminaHint(uiPuzzleStaminaHint);
            _expansions[uiPuzzleStaminaHint] = expansion;
        }

        return expansion;
    }

    private static readonly MethodInfo m_onShouldCheck = AccessTools.Method(typeof(UiPuzzleStaminaHint), "OnShouldCheck");

    private readonly UiPuzzleStaminaHint _uiPuzzleStaminaHint;
    private ExpandedUiPuzzleStaminaHint(UiPuzzleStaminaHint uiPuzzleStaminaHint)
    {
        _uiPuzzleStaminaHint = uiPuzzleStaminaHint;
    }

    public void Start()
    {
        ExpandedUiPuzzleGrid.Get(Game.Session.Puzzle.puzzleGrid).MoveCompleteEvent += OnShouldCheck;
    }

    public void OnDestroy()
    {
        _expansions.Remove(_uiPuzzleStaminaHint);
        ExpandedUiPuzzleGrid.Get(Game.Session.Puzzle.puzzleGrid).MoveCompleteEvent -= OnShouldCheck;
    }

    public void OnShouldCheck() => m_onShouldCheck.Invoke(_uiPuzzleStaminaHint, null);
}