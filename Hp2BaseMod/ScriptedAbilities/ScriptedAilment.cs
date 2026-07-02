using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// A delegate-based implementation of <see cref="IScriptedAilment"/>.
/// Use this when your scripted behaviour is simple enough to express as lambdas.
///
/// For complex behaviour with significant shared state, implement <see cref="IScriptedAilment"/> directly.
///
/// Pass an instance of this as the return value of <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/>.
/// </summary>
public class ScriptedAilment : IScriptedAilment
{
    private readonly Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> _onEnable;
    private readonly Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> _onDisable;
    private readonly Func<AilmentTriggerType, Ailment, PuzzleStatusGirl, bool, MoveModifier, MatchModifier, GiftModifier, bool> _onTrigger;
    private readonly Action<Ailment, PuzzleStatusGirl, PuzzleRewardContext> _onPreMatchReward;
    private readonly Action<Ailment, PuzzleStatusGirl, PuzzleRewardContext, Dictionary<UiPuzzleSlot, PuzzleReward>> _onPostMatchReward;
    private readonly Action<Ailment, PuzzleStatusGirl, PuzzleConsumeContext> _onPostSetReward;

    public ScriptedAilment(
        Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> onEnable = null,
        Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> onDisable = null,
        Func<AilmentTriggerType, Ailment, PuzzleStatusGirl, bool, MoveModifier, MatchModifier, GiftModifier, bool> onTrigger = null,
        Action<Ailment, PuzzleStatusGirl, PuzzleRewardContext> onPreMatchReward = null,
        Action<Ailment, PuzzleStatusGirl, PuzzleRewardContext, Dictionary<UiPuzzleSlot, PuzzleReward>> onPostMatchReward = null,
        Action<Ailment, PuzzleStatusGirl, PuzzleConsumeContext> onPostSetReward = null)
    {
        _onEnable = onEnable;
        _onDisable = onDisable;
        _onTrigger = onTrigger;
        _onPreMatchReward = onPreMatchReward;
        _onPostMatchReward = onPostMatchReward;
        _onPostSetReward = onPostSetReward;
    }

    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
        => _onEnable?.Invoke(ailment, girl, otherGirl);

    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
        => _onDisable?.Invoke(ailment, girl, otherGirl);

    public bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier)
        => _onTrigger?.Invoke(triggerType, ailment, girl, unfocused, moveModifier, matchModifier, giftModifier) ?? false;

    public void OnPreMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context)
        => _onPreMatchReward?.Invoke(ailment, girl, context);

    public void OnPostMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context,
        Dictionary<UiPuzzleSlot, PuzzleReward> rewards)
        => _onPostMatchReward?.Invoke(ailment, girl, context, rewards);

    public void OnPostSetReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleConsumeContext context)
        => _onPostSetReward?.Invoke(ailment, girl, context);
}