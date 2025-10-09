using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiDollPart))]
public class UiDollPartPatch
{
    private static readonly FieldInfo _currentGirlPart = AccessTools.Field(typeof(UiDollPart), "_currentGirlPart");

    [HarmonyPatch(nameof(UiDollPart.LoadPart))]
    [HarmonyPostfix]
    public static void Postfix(UiDollPart __instance, GirlPartSubDefinition girlPart, float durationOverride)
    {
        if (__instance.image != null)
        {
            __instance.image.useSpriteMesh = true;
        }

        if (__instance.backImage != null)
        {
            __instance.backImage.useSpriteMesh = true;
        }

        if (__instance.frontImage != null)
        {
            __instance.frontImage.useSpriteMesh = true;
        }

        if (__instance.innerImage != null)
        {
            __instance.innerImage.useSpriteMesh = true;
        }
    }
}