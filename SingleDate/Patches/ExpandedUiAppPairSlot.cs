using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppPairSlot))]
public static class UiAppPairSlotPatch
{
    [HarmonyPatch(nameof(UiAppPairSlot.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiAppPairSlot __instance)
        => ExpandedUiAppPairSlot.Get(__instance).Refresh();

    [HarmonyPatch(nameof(UiAppPairSlot.ShowTooltip))]
    [HarmonyPostfix]
    public static void ShowTooltip(UiAppPairSlot __instance)
        => ExpandedUiAppPairSlot.Get(__instance).ShowTooltip();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiAppPairSlot __instance)
        => ExpandedUiAppPairSlot.Get(__instance).OnDestroy();
}

public class ExpandedUiAppPairSlot
{
    private static Dictionary<UiAppPairSlot, ExpandedUiAppPairSlot> _expansions
        = new Dictionary<UiAppPairSlot, ExpandedUiAppPairSlot>();

    public static ExpandedUiAppPairSlot Get(UiAppPairSlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiAppPairSlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static FieldInfo _playerFileGirlPair = AccessTools.Field(typeof(UiAppPairSlot), "_playerFileGirlPair");
    private static FieldInfo _tooltip = AccessTools.Field(typeof(UiAppPairSlot), "_tooltip");

    private static FieldInfo _transitionDef = AccessTools.Field(typeof(ButtonStateTransition), "_transitionDef");
    private static FieldInfo _origSprite = AccessTools.Field(typeof(ButtonStateTransition), "_origSprite");

    private static FieldInfo _overTransitions = AccessTools.Field(typeof(ButtonBehavior), "_overTransitions");

    private ButtonStateTransitionDef _singleOverDef;
    private ButtonStateTransitionDef _defaultOverDef;
    private ButtonStateTransition _overSpriteTransition;
    private bool _defaultProfileLinked;
    private Sprite _defaultBackgroundSprite;
    private Image _background;
    private bool _started;

    protected UiAppPairSlot _core;
    private ExpandedUiAppPairSlot(UiAppPairSlot core)
    {
        _core = core;
    }

    public void OnDestroy()
    {
        _expansions.Remove(_core);
    }

    public void Refresh()
    {
        //slots are refreshed before they are started, and are not refreshed normally after starting.
        //so we do this pseudo-start thing here
        if (!_started)
        {
            _overSpriteTransition = _overTransitions.GetValue<List<ButtonStateTransition>>(_core.button)
                .FirstOrDefault(x => x.transitionDef.sprite != null);

            _defaultOverDef = _overSpriteTransition?.transitionDef;

            if (_defaultOverDef != null)
            {
                _started = true;

                _singleOverDef = new ButtonStateTransitionDef()
                {
                    duration = _defaultOverDef.duration,
                    imageTarget = _defaultOverDef.imageTarget,
                    rectTransformTarget = _defaultOverDef.rectTransformTarget,
                    type = _defaultOverDef.type,
                    val = _defaultOverDef.val,

                    sprite = UiPrefabs.SingleUiAppPairSlotBgOver
                };
            }

            _defaultProfileLinked = _core.profileLinked;
            _background = _core.transform.Find("Background").GetComponent<Image>();
            _defaultBackgroundSprite = _background.sprite;

            _started = true;
        }

        if (!State.IsSingle(_playerFileGirlPair.GetValue<PlayerFileGirlPair>(_core)?.girlPairDefinition))
        {
            if (_overSpriteTransition?.transitionDef != _defaultOverDef)
            {
                _transitionDef.SetValue(_overSpriteTransition, _defaultOverDef);
                _origSprite.SetValue(_overSpriteTransition, _defaultBackgroundSprite);
            }

            _background.sprite = _defaultBackgroundSprite;
            _core.girlHeadTwo.transform.localPosition = new Vector3(-16f, 0f, 0f);
            _core.profileLinked = _defaultProfileLinked;

            return;
        }

        if (_overSpriteTransition?.transitionDef != _singleOverDef)
        {
            _transitionDef.SetValue(_overSpriteTransition, _singleOverDef);
            _origSprite.SetValue(_overSpriteTransition, UiPrefabs.SingleUiAppPairSlotBg);
        }

        _background.sprite = UiPrefabs.SingleUiAppPairSlotBg;
        _core.girlHeadTwo.transform.localPosition = new Vector3(-48f, 0f, 0f);
        _core.profileLinked = false;
    }

    public void ShowTooltip()
    {
        var playerFileGirlPair = _playerFileGirlPair.GetValue<PlayerFileGirlPair>(_core);

        if (!State.IsSingle(playerFileGirlPair?.girlPairDefinition))
        {
            return;
        }

        _tooltip.GetValue<UiTooltipSimple>(_core)?
                    .Populate($"{playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.girlName}:\n{playerFileGirlPair.relationshipType}", 0, 1f, 1920f);
    }
}
