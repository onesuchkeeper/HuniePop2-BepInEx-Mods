using System.Collections.Generic;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

public static class GirlStyleSubDefinition_Ext
{
    public static ExpandedStyleDefinition Expansion(this GirlHairstyleSubDefinition def)
        => ExpandedStyleDefinition.Get(def);

    public static ExpandedStyleDefinition Expansion(this GirlOutfitSubDefinition def)
        => ExpandedStyleDefinition.Get(def);
}

/// <summary>
/// Holds additional fields for a <see cref="GirlHairstyleSubDefinition"/> or a <see cref="GirlOutfitSubDefinition"/> .
/// Consider this readonly and do not modify these fields unless you know what your doing, instead
/// register a <see cref="GirlDataMod"/> with a <see cref="HairstyleDataMod"/> or <see cref="OutfitDataMod"/> using <see cref="ModInterface.AddDataMod"/>
/// </summary>
public class ExpandedStyleDefinition
{
    private static Dictionary<GirlHairstyleSubDefinition, ExpandedStyleDefinition> _hairstyleExpansions
        = new Dictionary<GirlHairstyleSubDefinition, ExpandedStyleDefinition>();

    private static Dictionary<GirlOutfitSubDefinition, ExpandedStyleDefinition> _outfitExpansions
        = new Dictionary<GirlOutfitSubDefinition, ExpandedStyleDefinition>();

    public static ExpandedStyleDefinition Get(GirlHairstyleSubDefinition core) => _hairstyleExpansions.GetOrNew(core);

    public static ExpandedStyleDefinition Get(GirlOutfitSubDefinition core) => _outfitExpansions.GetOrNew(core);

    /// <summary>
    /// If style is unavailable when censored.
    /// </summary>
    public bool IsNSFW;

    /// <summary>
    /// Is the style is unlocked via a code.
    /// </summary>
    public bool IsCodeUnlocked;

    /// <summary>
    /// If the style is unlocked via purchase.
    /// </summary>
    public bool IsPurchased;

    /// <summary>
    /// If the style should hide special effects.
    /// </summary>
    public bool HideSpecial;
}
