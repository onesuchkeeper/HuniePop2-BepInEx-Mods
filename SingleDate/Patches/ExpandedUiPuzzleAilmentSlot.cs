using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleAilmentSlot))]
public static class UiPuzzleAilmentSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleAilmentSlot __instance)
        => ExpandedUiPuzzleAilmentSlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleAilmentSlot __instance)
        => ExpandedUiPuzzleAilmentSlot.Get(__instance).OnDestroy();
}

public class ExpandedUiPuzzleAilmentSlot
{
    private static Dictionary<UiPuzzleAilmentSlot, ExpandedUiPuzzleAilmentSlot> _expansions
        = new Dictionary<UiPuzzleAilmentSlot, ExpandedUiPuzzleAilmentSlot>();

    public static ExpandedUiPuzzleAilmentSlot Get(UiPuzzleAilmentSlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiPuzzleAilmentSlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiPuzzleAilmentSlot), "OnTooltipPreShow");

    protected UiPuzzleAilmentSlot _core;
    private ExpandedUiPuzzleAilmentSlot(UiPuzzleAilmentSlot core)
    {
        _core = core;
    }

    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    public void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }

    private void OnTooltipPreShow() => m_onTooltipPreShow.Invoke(_core, null);
}
