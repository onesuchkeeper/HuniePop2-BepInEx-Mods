using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace SingleDate;

[HarmonyPatch(typeof(UiCellphoneAppPair))]
public static class UiCellphoneAppPairPatch
{
    private static readonly Vector3 _sensitivityOffset = new Vector3(0, 96, 0);

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(UiCellphoneAppPair __instance)
    {
        if (__instance.transform.Find("ActiveContainer/SectionBottom/Background") is Transform backgroundTransform
            && backgroundTransform.gameObject.TryGetComponent<RectTransform>(out var backgroundRectTransform)
            && backgroundTransform.gameObject.TryGetComponent<Image>(out var backgroundImage))
        {
            backgroundImage.sprite = UiPrefabs.SingleUiCellphoneAppPairBg;
            backgroundImage.preserveAspect = true;
            backgroundRectTransform.anchoredPosition = backgroundRectTransform.anchoredPosition - new Vector2(0, 96);
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to find {nameof(UiCellphoneAppPair)}'s background image");
        }

        if (__instance.transform.Find("ActiveContainer/SectionBottom/TitleBar") is Transform titleBarTransform)
        {
            titleBarTransform.SetParent(null);
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to find {nameof(UiCellphoneAppPair)}'s title bar");
        }

        var templateLevelMeter = __instance.levelMeters[0];
        var sensitivityMeter = UnityEngine.Object.Instantiate(templateLevelMeter);

        var sensitivityLevelPlate = UnityEngine.Object.Instantiate(sensitivityMeter.levelPlate);
        sensitivityLevelPlate.transform.SetParent(sensitivityMeter.levelPlate.transform.parent);
        sensitivityLevelPlate.transform.position = sensitivityMeter.levelPlate.transform.position + _sensitivityOffset;
        sensitivityMeter.levelPlate = sensitivityLevelPlate;
        sensitivityLevelPlate.iconImage.sprite = UiPrefabs.SensitivityIcon;
        sensitivityLevelPlate.nameLabel.text = "SENSTIV. LVL";
        sensitivityLevelPlate.levelPlateType = LevelPlateType.STYLE;

        if (sensitivityLevelPlate.gameObject.TryGetComponent<Image>(out var plateImage))
        {
            plateImage.sprite = UiPrefabs.SensitivityPlate;
        }

        UnityExplorationUtility.LogChildren(sensitivityLevelPlate.transform);

        var sensitivitySmoothieSlot = UnityEngine.Object.Instantiate(sensitivityMeter.smoothieSlot);
        sensitivitySmoothieSlot.transform.SetParent(sensitivityMeter.smoothieSlot.transform.parent);
        sensitivitySmoothieSlot.transform.position = sensitivityMeter.smoothieSlot.transform.position + _sensitivityOffset;
        sensitivityMeter.smoothieSlot = sensitivitySmoothieSlot;


        // var expansion = ExpandedUiAppLevelPlate.Get(sensitivityLevelPlate);
        // expansion.ExpTypeId = new RelativeId();

        sensitivityMeter.smoothieItemDefinition = ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Exp);

        sensitivityMeter.transform.SetParent(templateLevelMeter.transform.parent);
        sensitivityMeter.transform.position = templateLevelMeter.transform.position + _sensitivityOffset;

        __instance.levelMeters = __instance.levelMeters.Append(sensitivityMeter).ToArray();
    }
}

public class ExpandedUiCellphoneAppPair
{
    private static Dictionary<UiCellphoneAppPair, ExpandedUiCellphoneAppPair> _expansions
        = new Dictionary<UiCellphoneAppPair, ExpandedUiCellphoneAppPair>();

    public static ExpandedUiCellphoneAppPair Get(UiCellphoneAppPair core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiCellphoneAppPair(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    protected UiCellphoneAppPair _core;
    private ExpandedUiCellphoneAppPair(UiCellphoneAppPair core)
    {
        _core = core;
    }
}