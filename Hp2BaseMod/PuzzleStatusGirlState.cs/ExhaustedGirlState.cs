using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

public class ExhaustedGirlState : IPuzzleStatusGirlState
{
    public RelativeId Id => PuzzleStatusGirlStateId.Exhausted;

    public bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType) => abilityStepConditionType == AbilityStepConditionType.IS_EXHAUSTED;
    public bool SatisfiesCondition(GirlConditionType girlConditionType) => girlConditionType == GirlConditionType.EXHAUSTED || girlConditionType == GirlConditionType.EXHAUSTED_OR_UPSET;

    public void OnEnter(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        UiDoll doll = Game.Session.gameCanvas.GetDoll(statusGirl.altGirl);
        doll.SetMood(GirlExpressionType.EXHAUSTED, 4f);
        doll.ReadDialogTrigger(Game.Session.Puzzle.dtStaminaExhausted, DialogLineFormat.UNCHECKED, -1);
        Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Puzzle.sfxStaminaExhausted, doll.pauseDefinition);
    }

    public void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        UiDoll doll = Game.Session.gameCanvas.GetDoll(statusGirl.altGirl);
        doll.ClearMood();

        var puzzleStatusExp = Game.Session.Puzzle.puzzleStatus.GetExpansion();

        if (!puzzleStatusExp.SuppressStateDialog)
        {
            doll.ReadDialogTrigger(Game.Session.Puzzle.dtStaminaRecovered, DialogLineFormat.UNCHECKED, -1);
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Puzzle.sfxStaminaRecovered, doll.pauseDefinition);

            ModInterface.Log.Message("Exhaust Exit Played");
        }
        else
        {
            ModInterface.Log.Message("Exhaust Exit Supressed");
        }
    }

    public RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina)
    {
        if (newStamina >= 4) // GIRL_RECOVER_STAMINA
        {
            return PuzzleStatusGirlStateId.Normal;
        }
        return Id;
    }

    public RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount)
    {
        return PuzzleStatusGirlStateId.Upset;
    }

    public RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) => Id;
}