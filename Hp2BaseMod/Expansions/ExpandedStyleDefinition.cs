using System.Collections.Generic;

namespace Hp2BaseMod;

public static class GirlStyleSubDefinition_Ext
{
    public static ExpandedStyleDefinition Expansion(this GirlHairstyleSubDefinition def)
        => ExpandedStyleDefinition.Get(def);

    public static ExpandedStyleDefinition Expansion(this GirlOutfitSubDefinition def)
        => ExpandedStyleDefinition.Get(def);
}

public class ExpandedStyleDefinition
{
    private static Dictionary<GirlHairstyleSubDefinition, ExpandedStyleDefinition> _hairstyleExpansions
        = new Dictionary<GirlHairstyleSubDefinition, ExpandedStyleDefinition>();

    private static Dictionary<GirlOutfitSubDefinition, ExpandedStyleDefinition> _outfitExpansions
        = new Dictionary<GirlOutfitSubDefinition, ExpandedStyleDefinition>();

    public static ExpandedStyleDefinition Get(GirlHairstyleSubDefinition core)
    {
        if (!_hairstyleExpansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedStyleDefinition();
            _hairstyleExpansions[core] = expansion;
        }

        return expansion;
    }

    public static ExpandedStyleDefinition Get(GirlOutfitSubDefinition core)
    {
        if (!_outfitExpansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedStyleDefinition();
            _outfitExpansions[core] = expansion;
        }

        return expansion;
    }

    public bool IsNSFW;
    public bool IsCodeUnlocked;
    public bool IsPurchased;
    public bool HideSpecial;
}
