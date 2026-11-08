using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// The relationship icon pulses when the sex date is available at the slot's time of day, unless the slot is set to wait
/// (<c>hornyDelayed</c>) for the puzzle of a date to start.
/// </summary>
/// <remarks>
/// Runs after any other mod's replacement (low priority) and does nothing when one has already handled the slot
/// (<c>__runOriginal</c> is false), which is how SingleDate takes the slots of single pairs.
/// </remarks>
[HarmonyPatch(typeof(UiAppRelationshipSlot))]
internal static class UiAppRelationshipSlotStatePatch
{
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, Sequence> HornySequence = AccessTools.FieldRefAccess<UiAppRelationshipSlot, Sequence>("_hornySequence");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, PlayerFileGirlPair> PairFileOf = AccessTools.FieldRefAccess<UiAppRelationshipSlot, PlayerFileGirlPair>("_playerFileGirlPair");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, int> DaytimeOffset = AccessTools.FieldRefAccess<UiAppRelationshipSlot, int>("_daytimeOffset");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, GirlDefinition> MainGirlDefinition = AccessTools.FieldRefAccess<UiAppRelationshipSlot, GirlDefinition>("_mainGirlDefinition");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, UiTooltipSimple> Tooltip = AccessTools.FieldRefAccess<UiAppRelationshipSlot, UiTooltipSimple>("_tooltip");

    /// <summary>
    /// Whether the pair is attracted, it is the pair's sex time of day (at the slot's offset) and, for a slot that waits, a
    /// date's puzzle has started (or no date is being played).
    /// </summary>
    private static bool ShowsHornyIcon(UiAppRelationshipSlot slot, PlayerFileGirlPair pair)
        => pair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && (Game.Persistence.playerFile.daytimeElapsed + DaytimeOffset(slot)) % 4 == (int)pair.girlPairDefinition.sexDaytime
            && (!slot.hornyDelayed
                || !(ModInterface.GameState.CurrentState is IPuzzleState)
                || Game.Session.Puzzle.isPuzzleActive);

    [HarmonyPatch(nameof(UiAppRelationshipSlot.Refresh))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool Refresh(UiAppRelationshipSlot __instance, bool __runOriginal)
    {
        if (!__runOriginal) return false;

        var slot = __instance;
        var pair = PairFileOf(slot);

        Game.Manager.Time.KillTween(HornySequence(slot), false, true);
        slot.itemIcon.rectTransform.localScale = Vector3.one;

        if (pair != null)
        {
            if (ShowsHornyIcon(slot, pair))
            {
                slot.itemIcon.sprite = slot.hornyIcon;

                var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
                HornySequence(slot) = sequence;
                sequence.Insert(0f, slot.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.2f, slot.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.4f, slot.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.6f, slot.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                sequence.AppendInterval(0.25f);
                sequence.PrependInterval(0.25f);
                Game.Manager.Time.Play(sequence, slot.pauseDefinition, 0f);
            }
            else
            {
                slot.itemIcon.sprite = slot.relationshipIcons[(int)pair.relationshipType];
            }

            slot.itemIcon.SetNativeSize();

            if (slot.headIcon != null)
            {
                if (MainGirlDefinition(slot) != null)
                {
                    slot.headIcon.sprite = pair.girlPairDefinition.GetOtherGirlDef(MainGirlDefinition(slot)).GetHead(false);
                    slot.headIcon.SetNativeSize();
                }
                else
                {
                    slot.headIcon.sprite = null;
                    slot.headIcon.rectTransform.sizeDelta = Vector2.zero;
                }
            }

            slot.canvasGroup.alpha = 1f;
            slot.canvasGroup.blocksRaycasts = true;
            return false;
        }

        slot.itemIcon.sprite = null;
        slot.itemIcon.rectTransform.sizeDelta = Vector2.zero;

        if (slot.headIcon != null)
        {
            slot.headIcon.sprite = null;
            slot.headIcon.rectTransform.sizeDelta = Vector2.zero;
        }

        slot.canvasGroup.alpha = slot.multiSlot ? 0.4f : 0f;
        slot.canvasGroup.blocksRaycasts = false;
        return false;
    }

    [HarmonyPatch(nameof(UiAppRelationshipSlot.ShowTooltip))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool ShowTooltip(UiAppRelationshipSlot __instance, bool __runOriginal)
    {
        if (!__runOriginal) return false;

        var slot = __instance;
        var pair = PairFileOf(slot);

        if (pair == null) return false;

        var text = "Relationship";

        if (MainGirlDefinition(slot) != null)
        {
            text += " w/ " + pair.girlPairDefinition.GetOtherGirlDef(MainGirlDefinition(slot)).girlName;
        }
        else
        {
            text += " Status";
        }

        text += ":\n" + pair.relationshipType.ToString();

        if (ShowsHornyIcon(slot, pair))
        {
            text += slot.hornyText;
        }

        Tooltip(slot).Populate(text, 0, 1f, 1920f);
        Tooltip(slot).Show(Vector2.up * 75f, false);
        return false;
    }
}