using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotPatch
{
    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppFinderSlot __instance, bool settled)
        => ExpandedUiAppFinderSlot.Get(__instance).Populate();
}

[Expansion(typeof(UiAppFinderSlot), 
    Fields = new[]{"_playerFileFinderSlot", })]
public partial class ExpandedUiAppFinderSlot
{
    private static readonly float SINGLE_SPACING = 48f;
    private static readonly float DOUBLE_SPACING = 96f;
    private static readonly FieldInfo f_girlDefinition = AccessTools.Field(typeof(UiAppHeadSlot), "_girlDefinition");

    private UiAppHeadSlot _girlSlot;
    private UiAppHeadSlot _nobodySlot;
    private bool _populatedAsSingle;

    internal void Populate()
    {
        var pairDef = _core.locationDefinition.locationType == LocationType.SIM && _core.locationDefinition == Game.Session.Location.currentLocation
            ? Game.Session.Location.currentGirlPair
            : f_playerFileFinderSlot.GetValue<PlayerFileFinderSlot>(_core)?.girlPairDefinition;

        if (State.IsSingle(pairDef))
        {
            if (!_populatedAsSingle)
            {
                var leftDef = f_girlDefinition.GetValue<GirlDefinition>(_core.headSlotLeft);
                var leftId = ModInterface.Data.GetDataId(GameDataType.Girl, leftDef.id);

                _girlSlot = leftId.SourceId == State.ModId
                    ? _core.headSlotRight
                    : _core.headSlotLeft;

                _nobodySlot = leftId.SourceId == State.ModId
                    ? _core.headSlotLeft
                    : _core.headSlotRight;

                _girlSlot.rectTransform.anchoredPosition = new Vector2(-SINGLE_SPACING, _core.headSlotLeft.rectTransform.anchoredPosition.y);

                _nobodySlot.rectTransform.SetParent(null);

                var relationshipTransform = _core.relationshipSlot.GetComponent<RectTransform>();
                relationshipTransform.anchoredPosition = new Vector2(SINGLE_SPACING, relationshipTransform.anchoredPosition.y);

                _populatedAsSingle = true;
            }

            _girlSlot.Populate(pairDef.girlDefinitionTwo);
        }
        else if (_populatedAsSingle)
        {
            _nobodySlot.rectTransform.SetParent(_girlSlot.transform.parent);

            _core.headSlotLeft.rectTransform.anchoredPosition = new Vector2(-DOUBLE_SPACING, _girlSlot.rectTransform.anchoredPosition.y);
            _core.headSlotRight.rectTransform.anchoredPosition = new Vector2(DOUBLE_SPACING, _girlSlot.rectTransform.anchoredPosition.y);

            var relationshipTransform = _core.relationshipSlot.GetComponent<RectTransform>();
            relationshipTransform.anchoredPosition = new Vector2(0, relationshipTransform.anchoredPosition.y);

            _populatedAsSingle = false;
        }

        if (_core.locationDefinition.locationType != LocationType.HUB
            && pairDef == null)
        {
            _core.headSlotLeft.Populate(null);
            _core.headSlotRight.Populate(null);
            _core.relationshipSlot.Populate(null);
        }
    }
}
