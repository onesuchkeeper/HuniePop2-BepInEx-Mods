using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiAppStoreSlot))]
internal static class UiAppStoreSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppStoreSlot __instance)
        => ExpandedUiAppStoreSlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiAppStoreSlot __instance)
        => ExpandedUiAppStoreSlot.Get(__instance).OnDestroy();
}

internal class ExpandedUiAppStoreSlot
{
    private static Dictionary<UiAppStoreSlot, ExpandedUiAppStoreSlot> _expansions
        = new Dictionary<UiAppStoreSlot, ExpandedUiAppStoreSlot>();

    public static ExpandedUiAppStoreSlot Get(UiAppStoreSlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiAppStoreSlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiAppStoreSlot), "OnTooltipPreShow");

    protected UiAppStoreSlot _core;
    private ExpandedUiAppStoreSlot(UiAppStoreSlot core)
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
