using Hp2BaseMod;

namespace Cheat;
/// <summary>
/// Scripted ailment that listens for match rewards and applies configured cheat values.
/// </summary>
public class CheatAilment : IScriptedAilment
{
    private ExpandedAilmentManager _ailmentManager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _ailmentManager.PostMatchReward += OnPostMatchReward;
    }

    public void Disable()
    {
        if (_ailmentManager != null)
        {
            _ailmentManager.PostMatchReward -= OnPostMatchReward;
            _ailmentManager = null;
        }
    }

    private void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        var status = Game.Session.Puzzle.puzzleStatus;
        if (status == null || status.isEmpty) return;

        var altGirl = status.altGirlFocused;
        var statusExp = status.GetExpansion();

        if (status.bonusRound)
        {
            if (Plugin.BonusRoundAffection.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.AffectionTalent, Plugin.BonusRoundAffection.Value, altGirl);
            }
        }
        else
        {
            if (Plugin.DateMoves.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.Moves, Plugin.DateMoves.Value, altGirl);
            }
            if (Plugin.DateAffection.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.AffectionTalent, Plugin.DateAffection.Value, altGirl);
            }
            if (Plugin.DateStamina.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.Stamina, Plugin.DateStamina.Value, altGirl);
            }
            if (Plugin.DatePassion.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.Passion, Plugin.DatePassion.Value, altGirl);
            }
            if (Plugin.DateSentiment.Value != 0)
            {
                statusExp.AddResourceValue(PuzzleResourceId.Sentiment, Plugin.DateSentiment.Value, altGirl);
            }
        }
    }
}