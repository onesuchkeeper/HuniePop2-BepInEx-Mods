using Hp2BaseMod;
using System;
using System.Collections.Generic;

namespace HuniePopUltimate;

/// <summary>
/// The provided affection type is marked as least favorite.
/// </summary>
public class AdditionalLeastFavoriteAilment : IScriptedAilment
{
    private readonly HashSet<RelativeId> _leastFavTypes;
    private ExpandedAilmentManager _ailmentManager;
    private PuzzleStatusGirl _owner;

    public AdditionalLeastFavoriteAilment(HashSet<RelativeId> leastFavTypes)
    {
        _leastFavTypes = leastFavTypes ?? throw new ArgumentNullException(nameof(leastFavTypes));
    }

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _owner = owner as PuzzleStatusGirl;
        _ailmentManager.PreAffectionMatchReward += On_AilmentManager_PreAffectionMatchReward;
    }

    public void Disable()
    {
        _ailmentManager.PreAffectionMatchReward -= On_AilmentManager_PreAffectionMatchReward;
    }

    private void On_AilmentManager_PreAffectionMatchReward(AilmentTriggerArgs.PreAffectionMatchReward args)
    {
        // Only when owner receives the match, or when applied globally
        if (_owner != null && args.StatusGirl != _owner) return;
        if (!_leastFavTypes.Contains(args.AffectionId)) return;
        
        ModInterface.Log.Message($"{nameof(AdditionalLeastFavoriteAilment)} triggered on {_owner?.girlDefinition.girlName ?? "null"}");
        args.IsLeastFav = true;
    }
}