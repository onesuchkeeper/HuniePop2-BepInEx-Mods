using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(AilmentManager))]
public static class AilmentManagerPatch
{
    [HarmonyPatch(nameof(AilmentManager.OnAilmentEnable))]
    [HarmonyPrefix]
    public static bool OnAilmentEnable(AilmentManager __instance, Ailment ailment, bool fromTrigger)
    {
        var id = ModInterface.Data.GetDataId(GameDataType.Ailment, ailment.definition.id);

        if (!ModInterface.Data.TryGetFunctionalAilment(id, out var mods))
        {
            return true;
        }

        foreach (var mod in mods)
        {
            mod.Enable(ailment, fromTrigger);
        }

        return false;
    }

    [HarmonyPatch(nameof(AilmentManager.OnAilmentDisable))]
    [HarmonyPrefix]
    public static bool OnAilmentDisable(AilmentManager __instance, Ailment ailment, bool fromTrigger)
    {
        var id = ModInterface.Data.GetDataId(GameDataType.Ailment, ailment.definition.id);

        if (!ModInterface.Data.TryGetFunctionalAilment(id, out var mods))
        {
            return true;
        }

        foreach (var mod in mods)
        {
            mod.Disable(ailment, fromTrigger);
        }

        return false;
    }

    // [HarmonyPatch(nameof(AilmentManager.Trigger), [typeof(AilmentTriggerType), typeof(Ailment)])]
    // [HarmonyPrefix]
    // public static bool Trigger(AilmentManager __instance, AilmentTriggerType triggerType, Ailment onlyAilment)
    // {
    //     return true;
    // }

    // [HarmonyPatch(nameof(AilmentManager.Trigger), [typeof(PuzzleSet)])]
    // [HarmonyPrefix]
    // public static bool Trigger(AilmentManager __instance, PuzzleSet move, ref MoveModifier __result)
    // {
    //     return true;
    // }

    // [HarmonyPatch(nameof(AilmentManager.Trigger), [typeof(PuzzleMatch)])]
    // [HarmonyPrefix]
    // public static bool Trigger(AilmentManager __instance, PuzzleMatch match, ref MoveModifier __result)
    // {
    //     return true;
    // }

    // [HarmonyPatch(nameof(AilmentManager.Trigger), [typeof(ItemDefinition)])]
    // [HarmonyPrefix]
    // public static bool Trigger(AilmentManager __instance, ItemDefinition gift, ref GiftModifier __result)
    // {
    //     return true;
    // }
}