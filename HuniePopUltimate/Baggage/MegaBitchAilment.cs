using Hp2BaseMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuniePopUltimate;

/// <summary>
/// 
/// </summary>
public class MegaBitchAilment : IScriptedAilment
{
    private const int PERCENTAGE_LOST = 5;

    private PuzzleStatusGirl _owner;
    private ExpandedAilmentManager _ailmentManager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _owner = owner as PuzzleStatusGirl;
        _ailmentManager.PostMatchReward += On_AilmentManager_PostMatchReward;
    }

    public void Disable()
    {
        _ailmentManager.PostMatchReward -= On_AilmentManager_PostMatchReward;
    }

    private void On_AilmentManager_PostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (args.Rewards.Any(x => x.Item2.TokenDefinition.Id == PuzzleResourceId.Sentiment))
        {
            //is already bounded to range [0,100] internally
            _owner.passion -= PERCENTAGE_LOST;
            ModInterface.Log.Message("Mega Bitch lowering passion due to sentiment match");
        }
    }
}