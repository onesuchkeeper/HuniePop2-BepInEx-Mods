using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiCellphoneAppStatus))]
public static class UiCellphoneAppStatusPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(UiCellphoneAppStatus __instance)
    {
        ExpandedUiCellphoneAppStatus.Get(__instance).Start();
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(UiCellphoneAppStatus __instance)
    {
        ExpandedUiCellphoneAppStatus.Get(__instance).OnDestroy();
    }
}

public class ExpandedUiCellphoneAppStatus
{
    private static Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus> _expansions
        = new Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus>();

    public static ExpandedUiCellphoneAppStatus Get(UiCellphoneAppStatus uiCellphoneAppPair)
    {
        if (!_expansions.TryGetValue(uiCellphoneAppPair, out var expansion))
        {
            expansion = new ExpandedUiCellphoneAppStatus(uiCellphoneAppPair);
            _expansions[uiCellphoneAppPair] = expansion;
        }

        return expansion;
    }

    private UiCellphoneAppStatus _uiCellphoneAppPair;

    public ExpandedUiCellphoneAppStatus(UiCellphoneAppStatus uiCellphoneAppPair)
    {
        _uiCellphoneAppPair = uiCellphoneAppPair;
    }

    public void Start()
    {
        if (State.IsLocationPairSingle())
        {
            _uiCellphoneAppPair.canvasGroupLeft.transform.SetParent(null);
            _uiCellphoneAppPair.statusPortraitLeft.transform.SetParent(null);
            _uiCellphoneAppPair.staminaMeterRight.transform.SetParent(null);

            var staminaRectTransform = _uiCellphoneAppPair.staminaMeterRight.GetComponent<RectTransform>();
            _uiCellphoneAppPair.statusPortraitRight.transform.position = new Vector3(
                _uiCellphoneAppPair.statusPortraitRight.transform.position.x - (staminaRectTransform.sizeDelta.x / 2),
                _uiCellphoneAppPair.statusPortraitRight.transform.position.y,
                _uiCellphoneAppPair.statusPortraitRight.transform.position.z);

            _uiCellphoneAppPair.sentimentRollerRight.transform.position = _uiCellphoneAppPair.passionRollerLeft.transform.position;
        }
    }

    public void OnDestroy()
    {
        _expansions.Remove(_uiCellphoneAppPair);
    }
}