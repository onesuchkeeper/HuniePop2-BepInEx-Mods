using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(UiPuzzleStaminaHint))]
public partial class ExpandedUiPuzzleStaminaHint
{
    [HarmonyPatch(typeof(UiPuzzleStaminaHint))]
    private static class Patch
    {
        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiPuzzleStaminaHint __instance)
            => ExpandedUiPuzzleStaminaHint.Destroy(__instance);

        [HarmonyPatch("Refresh")]
        [HarmonyPrefix]
        private static bool Refresh_Prefix(UiPuzzleStaminaHint __instance)
            => ExpandedUiPuzzleStaminaHint.Get(__instance).Refresh_Prefix();
    }

    private bool Refresh_Prefix()
    {
        var state = ModInterface.GameState.CurrentState;
        var puzzle = Game.Session.Puzzle;

        var stamina = puzzle.puzzleStatus.GetStatusGirl(_core.altGirl).stamina;
        var cost = 0;

        if (state != null && state.HintsSimStaminaCost)
        {
            var talk = Game.Session.Talk;

            if (talk.staminaHintCost != 0
                && (talk.staminaHintDollIndex < 0
                    || (!_core.altGirl && talk.staminaHintDollIndex == 0)
                    || (_core.altGirl && talk.staminaHintDollIndex > 0)))
            {
                cost = talk.staminaHintCost;
            }
            else if (Game.Manager.DragDrop.IsDropZone(Game.Session.gameCanvas.GetDoll(_core.altGirl).dropZone))
            {
                var item = Game.Manager.DragDrop.currentDraggable.GetItemDefinition();

                cost = !item.noStaminaCost
                    && Game.Session.Gift.CheckGiveCondition(Game.Session.gameCanvas.GetDoll(_core.altGirl).girlDefinition, item)
                    && (item.itemType != ItemType.FOOD || puzzle.puzzleStatus.foodGiftCount < 2)
                    ? 1
                    : 0;
            }
        }
        else if (state is IPuzzleState)
        {
            stamina = puzzle.puzzleStatus.girlStatusFocused.stamina;

            if (puzzle.puzzleGrid.state == PuzzleGameState.MOVING
                && puzzle.puzzleGrid.moveMatchSet != null
                && puzzle.puzzleStatus.altGirlFocused == _core.altGirl)
            {
                cost = puzzle.puzzleGrid.moveMatchSet.GetStaminaCost(false, false);
            }
        }

        var gain = cost < 0;
        cost = Mathf.Abs(cost);

        if (cost == _currentCost) return false;

        _currentCost = cost;
        TweenUtils.KillTween(_hintSequence, false, true);

        var sequence = DOTween.Sequence();
        _hintSequence = sequence;

        if (cost > 0)
        {
            _core.rectTransform.localScale = Vector2.one * 0.25f;
            _core.canvasGroup.alpha = 0f;

            if (cost <= stamina || gain)
            {
                _core.image.sprite = _defaultBgSprite;
                _core.textLabel.color = _defaultTextColor;
                _core.textLabel.fontSharedMaterial = _defaultTextMaterial;
            }
            else
            {
                _core.image.sprite = _core.errorBgSprite;
                _core.textLabel.color = _core.errorTextColor;
                _core.textLabel.fontSharedMaterial = _core.errorTextMaterial;
            }

            _core.rectTransform.sizeDelta = _origSizeDelta + new Vector2((cost - 1) * 22, (cost - 1) * 4);
            _core.textLabel.fontSize = _origFontSize + (cost - 1) * 3;
            _core.textLabel.text = (gain ? "+" : "-") + cost.ToString() + " Stamina";

            sequence.Insert(0f, _core.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
            sequence.Insert(0f, _core.canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear));
        }
        else
        {
            sequence.Insert(0f, _core.rectTransform.DOScale(Vector3.one * 0.5f, 0.1f).SetEase(Ease.Linear));
            sequence.Insert(0f, _core.canvasGroup.DOFade(0f, 0.1f).SetEase(Ease.Linear));
        }

        sequence.Play();
        return false;
    }
}