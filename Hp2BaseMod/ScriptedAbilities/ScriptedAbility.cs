using System;

namespace Hp2BaseMod;

/// <summary>
/// A delegate-based implementation of <see cref="IScriptedAbility"/>.
/// Use this when your scripted behaviour is simple enough to express as lambdas.
///
/// For complex behaviour with significant shared state, implement <see cref="IScriptedAbility"/> directly.
/// </summary>
public class ScriptedAbility : IScriptedAbility
{
    private readonly Func<Ability, bool, bool> _prePerform;
    private readonly Func<Ability, bool, bool?> _replacePerform;
    private readonly Func<Ability, bool, bool, bool> _postPerform;

    public ScriptedAbility(
        Func<Ability, bool, bool> prePerform = null,
        Func<Ability, bool, bool?> replacePerform = null,
        Func<Ability, bool, bool, bool> postPerform = null)
    {
        _prePerform = prePerform;
        _replacePerform = replacePerform;
        _postPerform = postPerform;
    }

    ///<inheritdoc/>
    public bool PrePerform(Ability ability, bool altGirl)
        => _prePerform?.Invoke(ability, altGirl) ?? true;

    ///<inheritdoc/>
    public bool? ReplacePerform(Ability ability, bool altGirl)
        => _replacePerform?.Invoke(ability, altGirl);

    ///<inheritdoc/>
    public bool PostPerform(Ability ability, bool altGirl, bool pipelineResult)
        => _postPerform?.Invoke(ability, altGirl, pipelineResult) ?? pipelineResult;
}