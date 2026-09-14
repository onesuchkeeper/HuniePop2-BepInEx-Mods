using System.Linq;
using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// Power tokens have a 50% chance to not be scored and remain on
/// the grid while she waits for a better opportunity to use them.
/// </summary>
public class PerfectionistAilment : IScriptedAilment
{
    private bool _triggered;

    private readonly RelativeId _dtId;
    private readonly float _triggerChance;
    public PerfectionistAilment(RelativeId dtId, float triggerChance)
    {
        _dtId = dtId;
        _triggerChance = triggerChance;
    }
    
    private PuzzleStatusGirl _owner;
    private ExpandedAilmentManager _ailmentManager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _owner = owner as PuzzleStatusGirl;
        _ailmentManager = ailmentManager;
        _ailmentManager.PreMatchReward += On_AilmentManager_PreMatchReward;
        _ailmentManager.PostMatchReward += On_AilmentManager_PostMatchReward;
    }

    public void Disable()
    {
        _ailmentManager.PreMatchReward -= On_AilmentManager_PreMatchReward;
        _ailmentManager.PostMatchReward -= On_AilmentManager_PostMatchReward;
    }

    private void On_AilmentManager_PreMatchReward(AilmentTriggerArgs.PreMatchReward args)
    {
        if (_owner != null && _owner != args.StatusGirl) return;

        //Safeguard, if all the tokens are upgraded just let it match normally
        //This way there will always be some change on the board and the board will
        //be re-checked via cascade
        if (args.Match.slots.All(x => x.token.upgraded)) return;

        foreach (var slot in args.Match.slots)
        {
            if (slot.token == null || !slot.token.upgraded) continue;

            if (UnityEngine.Random.Range(0f, 1f) < _triggerChance)
            {
                _triggered = true;
                args.ExcludedSlots.Add(slot);
            }
        }
    }

    private void On_AilmentManager_PostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (_owner != null && _owner != args.StatusGirl) return;
        
        if (_triggered)
        {
            _triggered = false;
            ModInterface.Log.Message("Perfectionist removed power token(s) from match");
            var doll = args.StatusGirl.altGirl ? args.UiPuzzleGrid._dollRight : args.UiPuzzleGrid._dollLeft;
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(_dtId), DialogLineFormat.PASSIVE);
        }
    }
}