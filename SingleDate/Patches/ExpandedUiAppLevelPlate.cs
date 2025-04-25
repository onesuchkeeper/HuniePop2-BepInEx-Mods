using System.Collections.Generic;
using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppLevelPlate))]
public static class UiAppLevelPlatePatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).Start();

    [HarmonyPatch("Populate", [])]
    [HarmonyPrefix]
    public static void Populate(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).Populate();
}

public class ExpandedUiAppLevelPlate
{
    private static Dictionary<UiAppLevelPlate, ExpandedUiAppLevelPlate> _expansions
        = new Dictionary<UiAppLevelPlate, ExpandedUiAppLevelPlate>();

    public static ExpandedUiAppLevelPlate Get(UiAppLevelPlate core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiAppLevelPlate(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    public RelativeId ExpTypeId;

    protected UiAppLevelPlate _core;
    private ExpandedUiAppLevelPlate(UiAppLevelPlate core)
    {
        _core = core;
    }

    public void Start()
    {

    }

    public void Populate()
    {

    }
}
