using System;

namespace Hp2BaseMod;

[Expansion(typeof(AbilityDefinition), HasModId = true)]
public partial class ExpandedAbilityDefinition
{
    /// <summary>
    /// Factory invoked once per <see cref="Ability"/> construction to produce scripted behaviour.
    /// Receives the newly constructed Ability so the factory can capture instance-specific context.
    /// Null if this is a purely data-driven ability.
    /// </summary>
    public Func<Ability, IScriptedAbility> ScriptedAbilityFactory;
}