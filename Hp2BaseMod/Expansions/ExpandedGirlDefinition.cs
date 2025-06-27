using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public static class GirlDefinition_Ext
{
    public static ExpandedGirlDefinition Expansion(this GirlDefinition def)
        => ExpandedGirlDefinition.Get(def);
}

/// <summary>
/// Holds additional fields for a <see cref="GirlDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what your doing, instead
/// register a <see cref="GirlDataMod"/> using <see cref="ModInterface.AddDataMod"/>.
/// </summary>
public class ExpandedGirlDefinition
{
    private static Dictionary<RelativeId, ExpandedGirlDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedGirlDefinition>();

    public static ExpandedGirlDefinition Get(GirlDefinition def)
        => Get(def.id);

    public static ExpandedGirlDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Girl, runtimeId));

    public static ExpandedGirlDefinition Get(RelativeId id)
    {
        if (!_expansions.TryGetValue(id, out var expansion))
        {
            expansion = new ExpandedGirlDefinition();
            _expansions[id] = expansion;
        }

        return expansion;
    }

    /// <summary>
    /// Position for special parts centered on the head.
    /// </summary>
    public Vector2 BackPosition;

    /// <summary>
    /// Position for special parts centered on the back.
    /// </summary>
    public Vector2 HeadPosition;

    /// <summary>
    /// The girls index within a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public int DialogTriggerIndex = -1;

    /// <summary>
    /// Maps an outfit id to its index within the def. 
    /// Use <see cref="GetOutfit"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<RelativeId, int> OutfitIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    /// <summary>
    /// Maps an outfit index to its id within the def. 
    /// Use <see cref="GetOutfit"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<int, RelativeId> OutfitIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    /// <summary>
    /// Given an id, returns the associated outfit.
    /// </summary>
    public GirlOutfitSubDefinition GetOutfit(GirlDefinition def, RelativeId id)
    {
        var index = OutfitIdToIndex[id];

        if (index == -1)
        {
            index = def.defaultOutfitIndex;
        }

        return def.outfits[index];
    }

    /// <summary>
    /// Maps a hairstyle id to its index within the def. 
    /// Use <see cref="GetHairstyle"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<RelativeId, int> HairstyleIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    /// <summary>
    /// Maps a hairstyle index to its id within the def. 
    /// Use <see cref="GetHairstyle"/> unless you must access the full collection
    /// </summary>
    public Dictionary<int, RelativeId> HairstyleIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    /// <summary>
    /// Given an id, returns the associated hairstyle.
    /// </summary>
    public GirlHairstyleSubDefinition GetHairstyle(GirlDefinition def, RelativeId id)
    {
        var index = HairstyleIdToIndex[id];

        if (index == -1)
        {
            index = def.defaultHairstyleIndex;
        }

        return def.hairstyles[index];
    }

    /// <summary>
    /// Maps a part id to its index within the def. 
    /// Use <see cref="GetPart"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<RelativeId, int> PartIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    /// <summary>
    /// Maps a part index to its id within the def. 
    /// Use <see cref="GetPart"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<int, RelativeId> PartIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    /// <summary>
    /// Given an id, returns the associated part.
    /// </summary>
    public GirlPartSubDefinition GetPart(GirlDefinition def, RelativeId id)
        => def.parts[PartIdToIndex[id]];

    /// <summary>
    /// Maps an expression id to its index within the def. 
    /// Use <see cref="GetExpression"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<RelativeId, int> ExpressionIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    /// <summary>
    /// Maps an expression index to its id within the def. 
    /// Use <see cref="GetExpression"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<int, RelativeId> ExpressionIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    /// <summary>
    /// Given an id, returns the associated expression.
    /// </summary>
    public GirlExpressionSubDefinition GetExpression(GirlDefinition def, RelativeId id)
        => def.expressions[ExpressionIdToIndex[id]];
}
