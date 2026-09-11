using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// After matching a Power token on owner, become exhausted if a 3-match
/// without a power token is made on owner next player action.
/// </summary>
public class AddictAilment : IScriptedAilment
{
    private bool _isAddicted;
    private bool _makeUpset;
    private PuzzleStatusGirl _owner;
    private ExpandedAilmentManager _ailmentManager;

    private readonly RelativeId _dtId;
    public AddictAilment(RelativeId dtId)
    {
        _dtId = dtId;
    }

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _owner = owner as PuzzleStatusGirl;
        _isAddicted = false;

        _ailmentManager.PreMatchReward += On_PreMatchReward;
        _ailmentManager.PostMove += On_AilmentManger_PostMove;
    }

    public void Disable()
    {
        _ailmentManager.PreMatchReward -= On_PreMatchReward;
        _ailmentManager.PostMove -= On_AilmentManger_PostMove;
    }

    private void On_PreMatchReward(AilmentTriggerArgs.PreMatchReward args)
    {
        if (_owner != null 
            && _owner != args.StatusGirl)
        {
            _isAddicted = false;
            return;
        }

        if (args.Match.HasPowerToken())
        {
            _isAddicted = true;
            return;
        }

        if (_isAddicted && args.Match.slots.Count == 3)
        {
            _makeUpset = true;
        }
    }

    private void On_AilmentManger_PostMove(AilmentTriggerArgs.PostMove args)
    {
        if (_owner != null 
            && _owner != args.StatusGirl)
        {
            return;
        }

        if (_makeUpset)
        {
            _makeUpset = false;
            _isAddicted = false;

            _owner.upset = true;
            _owner.stamina = 0;
            var doll = _owner.altGirl ? args.UiPuzzleGrid._dollRight : args.UiPuzzleGrid._dollLeft;
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(_dtId), DialogLineFormat.PASSIVE);

            ModInterface.Log.Message($"Addict Ailment Making {doll.girlDefinition.girlName} upset");
        }
    }
}
