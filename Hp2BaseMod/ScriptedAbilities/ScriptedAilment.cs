using System;

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

    /// <param name="onEnable">Called after the data-driven enable effect. May be null.</param>
    /// <param name="onDisable">Called after the data-driven disable effect. May be null.</param>
    /// <param name="onTrigger">Called after data-driven triggers. Return true for verbalization. May be null.</param>
    public ScriptedAilment(
        Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> onEnable = null,
        Action<Ailment, PuzzleStatusGirl, PuzzleStatusGirl> onDisable = null,
        Func<AilmentTriggerType, Ailment, PuzzleStatusGirl, bool, MoveModifier, MatchModifier, GiftModifier, bool> onTrigger = null)
    {
        _onEnable = onEnable;
        _onDisable = onDisable;
        _onTrigger = onTrigger;
    }

    ///<inheritdoc/>
    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
        => _onEnable?.Invoke(ailment, girl, otherGirl);

    ///<inheritdoc/>
    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
        => _onDisable?.Invoke(ailment, girl, otherGirl);

    ///<inheritdoc/>
    public bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier)
        => _onTrigger?.Invoke(triggerType, ailment, girl, unfocused, moveModifier, matchModifier, giftModifier) ?? false;
}