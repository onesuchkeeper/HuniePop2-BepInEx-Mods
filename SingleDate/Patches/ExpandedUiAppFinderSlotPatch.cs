using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

/// <summary>Hooks the finder slot's Populate so single pairs can be displayed with one head.</summary>
[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotPatch
{
    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppFinderSlot __instance, bool settled)
        => ExpandedUiAppFinderSlot.Get(__instance).Populate();
}

/// <summary>
/// Extends the cellphone's finder slot (a pair's entry in the finder app) to show a single pair: the girl's head is
/// centered-left and the relationship icon moves right, and the Nobody head is detached. Everything is restored
/// when the slot is next populated with a double pair.
/// </summary>
[Expansion(typeof(UiAppFinderSlot))]
internal partial class ExpandedUiAppFinderSlot
{
    // Horizontal offset of the girl's head from the slot's center on a single pair (the relationship icon is the same distance the other way), in UI pixels.
    private static readonly float SINGLE_SPACING = 48f;
    // Horizontal offset of each head from the center on a double pair (the base game's layout), in UI pixels.
    private static readonly float DOUBLE_SPACING = 96f;
    // UiAppHeadSlot._girlDefinition (private), used to tell which head slot holds Nobody.
    private static readonly FieldInfo f_girlDefinition = AccessTools.Field(typeof(UiAppHeadSlot), "_girlDefinition");

    // The head slot that shows the real girl, and the one that is empty (Nobody) and is detached while single.
    private UiAppHeadSlot _girlSlot;
    private UiAppHeadSlot _nobodySlot;
    // If the slot is currently laid out for a single pair, so layout is changed only when it switches.
    private bool _populatedAsSingle;

    /// <summary>
    /// Lays the slot out for single or double depending on its pair. For a single pair the real girl's head slot is
    /// used for her and the Nobody slot is detached; going back to a double pair re-attaches it and restores the layout.
    /// </summary>
    internal void Populate()
    {
        // The pair for this slot: the current pair if this is the slot for the location the player is at, otherwise the slot's own pair.
        var pairDef = _core.locationDefinition == Game.Session.Location.currentLocation
            ? Game.Session.Location.currentGirlPair
            : f_playerFileFinderSlot.GetValue<PlayerFileFinderSlot>(_core)?.girlPairDefinition;

        if (State.IsSingle(pairDef))
        {
            // First time as single, rearrange the slot.
            if (!_populatedAsSingle)
            {
                var leftDef = f_girlDefinition.GetValue<GirlDefinition>(_core.headSlotLeft);
                var leftId = ModInterface.Data.GetDataId(GameDataType.Girl, leftDef.id);

                // Nobody could be in either head slot (sides can be flipped), the Nobody girl's source is this mod.
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

        // Empty slots (no pair) are blanked, except at the hub.
        if (_core.locationDefinition.ModId() != Locations.HotelRoom
            && pairDef == null)
        {
            _core.headSlotLeft.Populate(null);
            _core.headSlotRight.Populate(null);
            _core.relationshipSlot.Populate(null);
        }
    }
}