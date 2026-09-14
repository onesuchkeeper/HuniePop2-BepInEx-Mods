namespace Hp2BaseMod;

public class UpsetGirlState : IPuzzleStatusGirlState
{
    public RelativeId Id => PuzzleStatusGirlStateId.Upset;

    public bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType) => abilityStepConditionType == AbilityStepConditionType.IS_UPSET;
    public bool SatisfiesCondition(GirlConditionType girlConditionType) => girlConditionType == GirlConditionType.UPSET || girlConditionType == GirlConditionType.EXHAUSTED_OR_UPSET;

    public void OnEnter(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        statusGirl.stamina = 0;
        UiDoll doll = Game.Session.gameCanvas.GetDoll(statusGirl.altGirl);
        doll.SetMood(GirlExpressionType.UPSET, 4f);
        doll.upsetEmitter.Init();
        doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenExhausted, DialogLineFormat.UNCHECKED, -1);
        Game.Session.Ailment.Trigger(AilmentTriggerType.ON_EXHAUSTION);
    }

    public void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        UiDoll doll = Game.Session.gameCanvas.GetDoll(statusGirl.altGirl);
        doll.ClearMood();
        doll.upsetEmitter.Stop();

        var puzzleStatusExp = Game.Session.Puzzle.puzzleStatus.GetExpansion();

        if (!puzzleStatusExp.SuppressStateDialog)
        {
            doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenRecovered, DialogLineFormat.UNCHECKED, -1);
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Puzzle.sfxStaminaRecovered, doll.pauseDefinition);

            ModInterface.Log.Message("Upset Exit Played");
        }
        else
        {
            ModInterface.Log.Message("Upset Exit Supressed");
        }
    }

    public RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina)
    {
        if (newStamina >= 4)
        {
            return PuzzleStatusGirlStateId.Normal;
        }
        return Id;
    }

    public RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount) => Id;
    public RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) => Id;

    public RelativeId OnRevive(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        return PuzzleStatusGirlStateId.Normal;
    }
}