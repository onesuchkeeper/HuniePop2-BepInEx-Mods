using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiCellphoneAppPair))]
internal static class UiCellphoneAppPairPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(UiCellphoneAppPair __instance)
        => ExpandedUiCellphoneAppPair.Get(__instance).Start();
}

internal class ExpandedUiCellphoneAppPair
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

    private static readonly Vector3 _meterVertOffset = new Vector3(0, 96, 0);

    protected UiCellphoneAppPair _core;
    private ExpandedUiCellphoneAppPair(UiCellphoneAppPair core)
    {
        _core = core;
    }

    public void Start()
    {
        if (!ModInterface.ExpDisplays.Any())
        {
            return;
        }

        var scroll_GO = new GameObject();
        var scroll_Rect = scroll_GO.AddComponent<RectTransform>();
        var scroll_scroll = scroll_GO.AddComponent<ScrollRect>();
        var scroll_Image = scroll_GO.AddComponent<Image>();
        var scroll_Mask = scroll_GO.AddComponent<Mask>();
        scroll_Rect.SetParent(_core.transform);
        scroll_Rect.localPosition = new Vector3(528f, -284.1f, 0f);
        scroll_Rect.sizeDelta = new Vector2(1056, 304);

        var padding_GO = new GameObject();
        var padding_Rect = padding_GO.AddComponent<RectTransform>();
        padding_Rect.pivot = new Vector2(0.5f, 1f);
        padding_Rect.position = scroll_Rect.position;
        padding_Rect.sizeDelta = new Vector2(1056, ((2 + ((ModInterface.ExpDisplays.Count + 1) / 2)) * 96f) + 4);

        scroll_scroll.scrollSensitivity = 18;
        scroll_scroll.horizontal = false;
        scroll_scroll.content = padding_Rect;
        scroll_scroll.verticalNormalizedPosition = 1f;
        scroll_Mask.showMaskGraphic = false;
        scroll_scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll_scroll.elasticity = 0.15f;

        if (_core.transform.Find("ActiveContainer/SectionBottom/Background") is Transform backgroundTransform
                    && backgroundTransform.gameObject.TryGetComponent<RectTransform>(out var backgroundRectTransform)
                    && backgroundTransform.gameObject.TryGetComponent<Image>(out var backgroundImage))
        {
            backgroundImage.sprite = UiPrefabs.PairBG;
            backgroundImage.preserveAspect = true;
            backgroundRectTransform.anchoredPosition = backgroundRectTransform.anchoredPosition - new Vector2(0, 96);
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to find {nameof(UiCellphoneAppPair)}'s background image");
        }

        if (_core.transform.Find("ActiveContainer/SectionBottom/TitleBar") is Transform titleBarTransform)
        {
            titleBarTransform.SetParent(null);
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to find {nameof(UiCellphoneAppPair)}'s title bar");
        }

        foreach (var meter in _core.levelMeters.Take(4))
        {
            meter.transform.SetParent(padding_Rect, true);
            meter.transform.localPosition += _meterVertOffset;

            meter.levelPlate.transform.SetParent(padding_Rect, true);
            meter.levelPlate.transform.localPosition += _meterVertOffset;

            meter.smoothieSlot.transform.SetParent(padding_Rect, true);
            meter.smoothieSlot.transform.localPosition += _meterVertOffset;
        }

        var meters = new List<UiAppLevelMeter>();
        foreach (var expDisplay in ModInterface.ExpDisplays)
        {
            var isLeft = meters.Count % 2 == 0;

            var templateLevelMeter = isLeft ? _core.levelMeters[1] : _core.levelMeters[3];

            var meter = UnityEngine.Object.Instantiate(templateLevelMeter);
            meter.meterFront.sprite = expDisplay.MeterFront;
            meter.meterBack.sprite = expDisplay.MeterFront;
            var meterExpansion = ExpandedUiAppLevelMeter.Get(meter);
            meterExpansion.ExpDisplay = expDisplay;

            meters.Add(meter);
            var localOffset = new Vector3(0, -96 * ((meters.Count + 1) / 2));

            var levelPlate = UnityEngine.Object.Instantiate(meter.levelPlate);
            var plateExpansion = ExpandedUiAppLevelPlate.Get(levelPlate);
            plateExpansion.ExpDisplay = expDisplay;

            levelPlate.transform.SetParent(meter.levelPlate.transform.parent);
            levelPlate.transform.localPosition = meter.levelPlate.transform.localPosition + localOffset;
            meter.levelPlate = levelPlate;

            var sensitivitySmoothieSlot = UnityEngine.Object.Instantiate(meter.smoothieSlot);
            sensitivitySmoothieSlot.transform.SetParent(meter.smoothieSlot.transform.parent);
            sensitivitySmoothieSlot.transform.localPosition = meter.smoothieSlot.transform.localPosition + localOffset;

            meter.smoothieSlot = sensitivitySmoothieSlot;
            meter.smoothieItemDefinition = expDisplay.ExpItemDef;
            meter.transform.SetParent(templateLevelMeter.transform.parent);
            meter.transform.localPosition = templateLevelMeter.transform.localPosition + localOffset;
        }

        padding_Rect.SetParent(scroll_Rect, true);
        padding_Rect.localPosition -= new Vector3(0, padding_Rect.sizeDelta.y, 0);
        _core.levelMeters = _core.levelMeters.Concat(meters).ToArray();
    }
}
