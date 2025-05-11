using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiPuzzleDateGiftSlot))]
internal static class UiPuzzleDateGiftSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleDateGiftSlot __instance)
        => ExpandedUiPuzzleDateGiftSlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleDateGiftSlot __instance)
        => ExpandedUiPuzzleDateGiftSlot.Get(__instance).OnDestroy();
}

internal class ExpandedUiPuzzleDateGiftSlot
{
    private static Dictionary<UiPuzzleDateGiftSlot, ExpandedUiPuzzleDateGiftSlot> _expansions
        = new Dictionary<UiPuzzleDateGiftSlot, ExpandedUiPuzzleDateGiftSlot>();

    public static ExpandedUiPuzzleDateGiftSlot Get(UiPuzzleDateGiftSlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiPuzzleDateGiftSlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiPuzzleDateGiftSlot), "OnTooltipPreShow");

    protected UiPuzzleDateGiftSlot _core;
    private ExpandedUiPuzzleDateGiftSlot(UiPuzzleDateGiftSlot core)
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
