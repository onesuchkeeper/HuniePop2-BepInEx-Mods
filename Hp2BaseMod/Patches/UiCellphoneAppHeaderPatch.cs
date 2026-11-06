// Hp2BaseMod 2026, By OneSuchKeeper

using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// The header's date label comes from the puzzle state (round counters, numbered post game dates...) instead of from the kind of puzzle.
/// Outside of a puzzle it keeps showing the time of day.
/// </summary>
[HarmonyPatch(typeof(UiCellphoneAppHeader))]
internal static class UiCellphoneAppHeaderPatch
{
    [HarmonyPatch(nameof(UiCellphoneAppHeader.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiCellphoneAppHeader __instance)
    {
        var playerFile = Game.Persistence.playerFile;

        __instance.wingRoller.Reset(playerFile.completedGirlPairs.Count, 24);
        __instance.talentRoller.Reset(playerFile.GetFruitCount(PuzzleAffectionType.TALENT), 999);
        __instance.flirtationRoller.Reset(playerFile.GetFruitCount(PuzzleAffectionType.FLIRTATION), 999);
        __instance.romanceRoller.Reset(playerFile.GetFruitCount(PuzzleAffectionType.ROMANCE), 999);
        __instance.sexualityRoller.Reset(playerFile.GetFruitCount(PuzzleAffectionType.SEXUALITY), 999);

        if (ModInterface.GameState.CurrentState is IPuzzleState puzzleState
            && puzzleState.TryGetHeaderLabel(out var label))
        {
            __instance.daytimeIcon.sprite = label.Icon switch
            {
                PuzzleHeaderIcon.Alpha => __instance.alphaIconSprite,
                PuzzleHeaderIcon.Round => __instance.roundIconSprite,
                _ => __instance.daytimeIconSprite
            };
            __instance.daytimeLabel.text = label.Text;
        }
        else
        {
            __instance.daytimeIcon.sprite = __instance.daytimeIconSprite;
            __instance.daytimeLabel.text = ((ClockDaytimeType)(playerFile.daytimeElapsed % 4)).ToString().ToUpper();
        }

        float num = Mathf.Abs(Mathf.Round(202f - __instance.daytimeLabel.preferredWidth));
        __instance.daytimeRectTransform.sizeDelta = new Vector2(280f - num, __instance.daytimeRectTransform.sizeDelta.y);
        __instance.daytimeRectTransform.anchoredPosition = new Vector2(-897f + Mathf.Round(num * 0.5f), __instance.daytimeRectTransform.anchoredPosition.y);

        return false;
    }
}