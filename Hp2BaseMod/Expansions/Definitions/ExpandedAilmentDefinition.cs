using System;

namespace Hp2BaseMod;

[Expansion(typeof(AilmentDefinition), HasModId = true)]
public partial class ExpandedAilmentDefinition
{
    /// <summary>
    /// Factory that produces the IScriptedAilment for a new Ailment instance built from this definition.
    /// Null if this is a purely data-driven ailment.
    /// Set this via <see cref="ScriptedAilmentDataMod"/>.
    /// </summary>
    public Func<Ailment, IScriptedAilment> ScriptedAilmentFactory;
}