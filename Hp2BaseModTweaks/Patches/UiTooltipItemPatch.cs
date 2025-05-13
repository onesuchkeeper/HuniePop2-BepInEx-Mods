using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseModTweaks;

/// <summary>
/// Even in the base game the item tooltip is too narrow for some items
/// This just makes it 24 pixels wider
/// </summary>
[HarmonyPatch(typeof(UiTooltipItem))]
public static class UiTooltipItemPatch
{
    private static float _northDescriptionWidth;
    private static readonly float _tooltipWidthAdd = 24;
    private static readonly FieldInfo _direction = AccessTools.Field(typeof(UiTooltipItem), "_direction");

    [HarmonyPatch("OnStart")]
    [HarmonyPostfix]
    public static void OnStart(UiTooltipItem __instance)
    {
        var bgSizeDelta = __instance.styleInfos[(int)CardinalDirection.NORTH].bgSizeDelta;
        __instance.styleInfos[(int)CardinalDirection.NORTH].bgSizeDelta = new Vector2(bgSizeDelta.x + _tooltipWidthAdd, bgSizeDelta.y);
        _northDescriptionWidth = __instance.descriptionLabel.GetComponent<RectTransform>().sizeDelta.x + _tooltipWidthAdd;
    }

    [HarmonyPatch("Resize")]
    [HarmonyPrefix]
    public static void Resize(UiTooltipItem __instance)
    {
        if (_direction.GetValue<CardinalDirection>(__instance) == CardinalDirection.NORTH)
        {
            var descriptionRect = __instance.descriptionLabel.GetComponent<RectTransform>();
            descriptionRect.sizeDelta = new Vector2(_northDescriptionWidth, descriptionRect.sizeDelta.y);
        }
    }
}