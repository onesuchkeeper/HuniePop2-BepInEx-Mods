using System.Collections.Generic;

namespace Hp2BaseMod;

[Expansion(typeof(GirlSpecialPartSubDefinition))]
public partial class ExpandedGirlSpecialPartSubDefinition
{
    /// <summary>
    /// Hairstyles required to show the special part, or empty if
    /// no required hairstyles
    /// </summary>
    public List<RelativeId> RequiredHairstyles;
}