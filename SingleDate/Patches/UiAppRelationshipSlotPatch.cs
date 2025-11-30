using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppRelationshipSlot))]
internal static class UiAppRelationshipSlotPatch
{
    private static readonly FieldInfo f_hornySequence = AccessTools.Field(typeof(UiAppRelationshipSlot), "_hornySequence");
    private static readonly FieldInfo f_playerFileGirlPair = AccessTools.Field(typeof(UiAppRelationshipSlot), "_playerFileGirlPair");
    private static readonly FieldInfo f_daytimeOffset = AccessTools.Field(typeof(UiAppRelationshipSlot), "_daytimeOffset");
    private static readonly FieldInfo f_mainGirlDefinition = AccessTools.Field(typeof(UiAppRelationshipSlot), "_mainGirlDefinition");
    private static readonly FieldInfo f_tooltip = AccessTools.Field(typeof(UiAppRelationshipSlot), "_tooltip");

    [HarmonyPatch(nameof(UiAppRelationshipSlot.Refresh))]
    [HarmonyPrefix]
    public static bool Refresh(UiAppRelationshipSlot __instance)
    {
        var playerFileGirlPair = f_playerFileGirlPair.GetValue<PlayerFileGirlPair>(__instance);

        if (!State.IsSingle(playerFileGirlPair?.girlPairDefinition))
        {
            return true;
        }

        var hornySequence = f_hornySequence.GetValue<Sequence>(__instance);
        var daytimeOffset = f_daytimeOffset.GetValue<int>(__instance);
        //var mainGirlDefinition = _mainGirlDefinition.GetValue<GirlDefinition>(__instance);

        var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

        Game.Manager.Time.KillTween(hornySequence);
        __instance.itemIcon.rectTransform.localScale = Vector3.one;

        if (playerFileGirlPair != null)
        {
            var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;

            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && girlSave?.RelationshipLevel == (maxSingleGirlRelationshipLevel - 1)
                && (Game.Persistence.playerFile.daytimeElapsed + daytimeOffset) % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime
                && (!__instance.hornyDelayed || (__instance.hornyDelayed && (!Game.Session.Location.AtLocationType(LocationType.DATE) || Game.Session.Puzzle.isPuzzleActive))))
            {
                __instance.itemIcon.sprite = __instance.hornyIcon;

                hornySequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
                f_hornySequence.SetValue(__instance, hornySequence);

                hornySequence.Insert(0f, __instance.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                hornySequence.Insert(0.2f, __instance.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                hornySequence.Insert(0.4f, __instance.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                hornySequence.Insert(0.6f, __instance.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                hornySequence.AppendInterval(0.25f);
                hornySequence.PrependInterval(0.25f);
                Game.Manager.Time.Play(hornySequence, __instance.pauseDefinition);
            }
            else
            {
                __instance.itemIcon.sprite = __instance.relationshipIcons[(int)playerFileGirlPair.relationshipType];
            }

            __instance.itemIcon.SetNativeSize();
            if (__instance.headIcon != null)
            {
                // if (_mainGirlDefinition != null)
                // {
                //     // this code is copied straight from the decomp, but GetOtherGirlDef returns null here and throws where it doesn't
                //     // without this patch. I don't know why, and I also don't think I've ever even seen this head icon
                //     // on the relationship icon in game before. So I'mma just comment this bit out for now. Weird.
                //     __instance.headIcon.sprite = playerFileGirlPair.girlPairDefinition.GetOtherGirlDef(mainGirlDefinition).GetHead(mini: false);

                //     __instance.headIcon.SetNativeSize();
                // }
                // else
                // {
                __instance.headIcon.sprite = null;
                __instance.headIcon.rectTransform.sizeDelta = Vector2.zero;
                //}
            }
            __instance.canvasGroup.alpha = 1f;
            __instance.canvasGroup.blocksRaycasts = true;
        }
        else
        {
            __instance.itemIcon.sprite = null;
            __instance.itemIcon.rectTransform.sizeDelta = Vector2.zero;

            if (__instance.headIcon != null)
            {
                __instance.headIcon.sprite = null;
                __instance.headIcon.rectTransform.sizeDelta = Vector2.zero;
            }

            __instance.canvasGroup.alpha = (!__instance.multiSlot)
                ? 0f
                : 0.4f;

            __instance.canvasGroup.blocksRaycasts = false;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiAppRelationshipSlot.ShowTooltip))]
    [HarmonyPostfix]
    public static void ShowTooltip(UiAppRelationshipSlot __instance)
    {
        var playerFileGirlPair = f_playerFileGirlPair.GetValue<PlayerFileGirlPair>(__instance);

        if (!State.IsSingle(playerFileGirlPair.girlPairDefinition))
        {
            return;
        }

        var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
        var daytimeOffset = f_daytimeOffset.GetValue<int>(__instance);

        //when not horny
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;
        if (!(girlSave.RelationshipLevel == maxSingleGirlRelationshipLevel - 1
                && (Game.Persistence.playerFile.daytimeElapsed + daytimeOffset) % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
        //and attracted or compatible
            && (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                || playerFileGirlPair.relationshipType == GirlPairRelationshipType.COMPATIBLE))
        {
            var tooltip = f_tooltip.GetValue<UiTooltipSimple>(__instance);
            tooltip.Populate($"Relationship Status:\n{playerFileGirlPair.relationshipType} {girlSave.RelationshipLevel}/{maxSingleGirlRelationshipLevel}", 0, 1f, 1920f);
        }
    }
}
