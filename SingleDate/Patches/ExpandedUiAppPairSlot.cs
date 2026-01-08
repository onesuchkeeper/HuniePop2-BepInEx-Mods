using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppPairSlot))]
internal static class UiAppPairSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppPairSlot __instance)
    => ExpandedUiAppPairSlot.Get(__instance).Start();

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

internal class ExpandedUiAppPairSlot
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

    private static readonly FieldInfo f_playerFileGirlPair = AccessTools.Field(typeof(UiAppPairSlot), "_playerFileGirlPair");
    private static readonly FieldInfo f_tooltip = AccessTools.Field(typeof(UiAppPairSlot), "_tooltip");

    private static readonly FieldInfo f_transitionDef = AccessTools.Field(typeof(ButtonStateTransition), "_transitionDef");
    private static readonly FieldInfo f_origSprite = AccessTools.Field(typeof(ButtonStateTransition), "_origSprite");

    private static readonly FieldInfo f_overTransitions = AccessTools.Field(typeof(ButtonBehavior), "_overTransitions");

    private ButtonStateTransitionDef _singleOverDef;
    private ButtonStateTransitionDef _defaultOverDef;
    private ButtonStateTransition _overSpriteTransition;
    private bool _defaultProfileLinked;
    private Sprite _defaultBackgroundSprite;
    private Image _background;
    private Transform _headWrapper;
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

    internal void Start()
    {
        _overSpriteTransition = f_overTransitions.GetValue<List<ButtonStateTransition>>(_core.button)
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

        var headWrapperGO = new GameObject();
        _headWrapper = headWrapperGO.transform;

        _headWrapper.transform.SetParent(_core.girlHeadTwo.transform.parent, false);
        _headWrapper.localPosition = Vector3.zero;
        _headWrapper.localRotation = Quaternion.identity;
        _headWrapper.localScale = Vector3.one;

        _core.girlHeadTwo.transform.SetParent(_headWrapper, false);

        _started = true;

        Refresh();
    }

    public void Refresh()
    {
        if (!_started) return;

        if (!State.IsSingle(f_playerFileGirlPair.GetValue<PlayerFileGirlPair>(_core)?.girlPairDefinition))
        {
            if (_overSpriteTransition?.transitionDef != _defaultOverDef)
            {
                f_transitionDef.SetValue(_overSpriteTransition, _defaultOverDef);
                f_origSprite.SetValue(_overSpriteTransition, _defaultBackgroundSprite);
            }

            _background.sprite = _defaultBackgroundSprite;
            _headWrapper.localPosition = Vector3.zero;
            _core.profileLinked = _defaultProfileLinked;

            return;
        }

        if (_overSpriteTransition?.transitionDef != _singleOverDef)
        {
            f_transitionDef.SetValue(_overSpriteTransition, _singleOverDef);
            f_origSprite.SetValue(_overSpriteTransition, UiPrefabs.SingleUiAppPairSlotBg);
        }

        _background.sprite = UiPrefabs.SingleUiAppPairSlotBg;
        _headWrapper.localPosition = new Vector3(-32f, 0f, 0f);
        _core.profileLinked = false;
    }

    public void ShowTooltip()
    {
        var playerFileGirlPair = f_playerFileGirlPair.GetValue<PlayerFileGirlPair>(_core);

        if (!State.IsSingle(playerFileGirlPair?.girlPairDefinition))
        {
            return;
        }

        var text = $"{playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.girlName}:\n{playerFileGirlPair.relationshipType}";

        if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
            || playerFileGirlPair.relationshipType == GirlPairRelationshipType.COMPATIBLE)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
            text += $" {girlSave.RelationshipLevel}/{Plugin.MaxSingleGirlRelationshipLevel.Value}";
        }

        f_tooltip.GetValue<UiTooltipSimple>(_core)?
            .Populate(text, 0, 1f, 1920f);
    }
}
