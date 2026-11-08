using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// A finder slot is for a location the finder offers (<see cref="ExpandedLocationDefinition.AllowSim"/>) or for the hub. The base game
/// decided both from the location's type.
/// </summary>
[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotStatePatch
{
    private static readonly AccessTools.FieldRef<UiAppFinderSlot, PlayerFileFinderSlot> PlayerFileFinderSlotOf
        = AccessTools.FieldRefAccess<UiAppFinderSlot, PlayerFileFinderSlot>("_playerFileFinderSlot");

    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPrefix]
    private static bool Populate(UiAppFinderSlot __instance, bool settled)
    {
        var slot = __instance;
        var location = slot.locationDefinition;
        var locationManager = Game.Session.Location;
        var isFinderLocation = location.GetExpansion().AllowSim;

        PlayerFileFinderSlotOf(slot) = Game.Persistence.playerFile.GetPlayerFileFinderSlot(location);
        var playerFileFinderSlot = PlayerFileFinderSlotOf(slot);

        var enabled = true;
        bool dimmed;

        if (isFinderLocation && location == locationManager.currentLocation)
        {
            if (locationManager.currentGirlPair)
            {
                slot.headSlotLeft.Populate(locationManager.currentGirlLeft);
                slot.headSlotRight.Populate(locationManager.currentGirlRight);
                slot.relationshipSlot.Populate(locationManager.currentGirlPair, 0, null, false);
            }

            dimmed = true;
        }
        else if (isFinderLocation && playerFileFinderSlot != null && playerFileFinderSlot.girlPairDefinition != null)
        {
            if (!playerFileFinderSlot.sidesFlipped)
            {
                slot.headSlotLeft.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionOne);
                slot.headSlotRight.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionTwo);
            }
            else
            {
                slot.headSlotLeft.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionTwo);
                slot.headSlotRight.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionOne);
            }

            slot.relationshipSlot.Populate(playerFileFinderSlot.girlPairDefinition, 1, null, false);
            dimmed = !settled;
        }
        else if (location == Game.Session.Hub.hubLocationDefinition)
        {
            slot.headSlotLeft.Populate(Game.Session.Hub.hubGirlDefinition);
            slot.headSlotLeft.rectTransform.anchoredPosition = new Vector2(0f, slot.headSlotLeft.rectTransform.anchoredPosition.y);
            slot.headSlotRight.canvasGroup.alpha = 0f;
            slot.relationshipSlot.canvasGroup.alpha = 0f;
            slot.headSlotRight.canvasGroup.blocksRaycasts = false;
            slot.relationshipSlot.canvasGroup.blocksRaycasts = false;

            // the hub can't be picked from the hub
            dimmed = !settled || ModInterface.GameState.CurrentState.ShowsHubMenu;
        }
        else
        {
            dimmed = true;
            enabled = false;
        }

        if (dimmed)
        {
            if (slot.headSlotLeft.canvasGroup.alpha > 0f)
            {
                slot.headSlotLeft.canvasGroup.alpha = 0.25f;
            }

            if (slot.headSlotRight.canvasGroup.alpha > 0f)
            {
                slot.headSlotRight.canvasGroup.alpha = 0.25f;
            }

            if (slot.relationshipSlot.canvasGroup.alpha > 0f)
            {
                slot.relationshipSlot.canvasGroup.alpha = 0.25f;
            }

            if (slot.headSlotLeft.canvasGroup.blocksRaycasts)
            {
                slot.headSlotLeft.canvasGroup.blocksRaycasts = enabled;
            }

            if (slot.headSlotRight.canvasGroup.blocksRaycasts)
            {
                slot.headSlotRight.canvasGroup.blocksRaycasts = enabled;
            }

            if (slot.relationshipSlot.canvasGroup.blocksRaycasts)
            {
                slot.relationshipSlot.canvasGroup.blocksRaycasts = enabled;
            }

            slot.locationIcon.color = ColorUtils.ColorAlpha(slot.locationIcon.color, 0.5f);
            slot.locationLabel.color = ColorUtils.ColorAlpha(slot.locationLabel.color, 0.4f);
            slot.locationButton.Disable();
        }

        slot.locationIcon.sprite = location.finderLocationIcon;
        slot.locationLabel.text = location.locationName.ToUpper();

        var width = Mathf.RoundToInt(slot.locationLabel.preferredWidth);
        if (width % 2 != 0)
        {
            width++;
        }

        slot.locationRectTransform.sizeDelta = new Vector2(90 + width, slot.locationRectTransform.sizeDelta.y);
        return false;
    }
}