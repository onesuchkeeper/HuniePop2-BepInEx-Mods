using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine.UI;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppLevelPlate))]
internal static class UiAppLevelPlatePatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).Start();

    [HarmonyPatch("Populate", [])]
    [HarmonyPostfix]
    public static void Populate(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).Populate();

    [HarmonyPatch("Populate", [typeof(PlayerFileGirl)])]
    [HarmonyPostfix]
    public static void PopulateOverload(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).Populate();

    [HarmonyPatch("ShowFavArrow")]
    [HarmonyPrefix]
    public static bool ShowFavArrow(UiAppLevelPlate __instance, GirlDefinition girlDef)
        => ExpandedUiAppLevelPlate.Get(__instance).ShowFavArrow();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(UiAppLevelPlate __instance)
        => ExpandedUiAppLevelPlate.Get(__instance).OnDestroy();

    [HarmonyPatch("OnButtonEnter")]
    [HarmonyPostfix]
    public static void OnButtonEnter(UiAppLevelPlate __instance, ButtonBehavior buttonBehavior)
        => ExpandedUiAppLevelPlate.Get(__instance).OnButtonEnter();
}

internal class ExpandedUiAppLevelPlate
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

    private static readonly FieldInfo _tooltip = AccessTools.Field(typeof(UiAppLevelPlate), "_tooltip");
    private static readonly MethodInfo m_resize = AccessTools.Method(typeof(UiTooltipItem), "Resize");

    public RelativeId ExpTypeId;

    public IExpInfo _display;

    public IExpInfo ExpDisplay;
    protected UiAppLevelPlate _core;
    private ExpandedUiAppLevelPlate(UiAppLevelPlate core)
    {
        _core = core;
    }

    public void Start()
    {

    }

    internal void OnDestroy()
    {
        _expansions.Remove(_core);
    }

    public void Populate()
    {
        if (ExpDisplay == null)
        {
            return;
        }

        _core.iconImage.sprite = ExpDisplay.IconImage;
        _core.nameLabel.text = ExpDisplay.PlateTitle;

        _core.valueLabelPro.color = ExpDisplay.TextColor;
        _core.valueLabelPro.fontSharedMaterial = UnityEngine.Object.Instantiate(_core.valueLabelPro.fontSharedMaterial);
        _core.valueLabelPro.fontSharedMaterial.SetColor("_UnderlayColor", ExpDisplay.OutlineColor);

        _core.valueLabelPro.text = $"{ExpDisplay.CurrentLevel}/{ExpDisplay.MaxLevel}";

        _core.itemDefinition = ExpDisplay.LevelPlateItemDef;

        if (_core.TryGetComponent<Image>(out var plateImage))
        {
            plateImage.sprite = ExpDisplay.PlateImage;
        }
    }

    public bool ShowFavArrow()
    {
        if (ExpDisplay == null)
        {
            return true;
        }

        return false;
        //todo
    }

    public void OnButtonEnter()
    {
        if (ExpDisplay == null)
        {
            return;
        }

        var tooltip = _tooltip.GetValue<UiTooltipItem>(_core);

        tooltip.descriptionLabel.text = ExpDisplay.PlateDesc;
        tooltip.nameLabel.text = ExpDisplay.ExpTitle;
        m_resize.Invoke(tooltip, null);
    }
}
