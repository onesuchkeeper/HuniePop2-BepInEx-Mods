using System.Collections.Generic;
using System.Linq;
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
    private static Dictionary<RelativeId, ExpandedGirlDefinition> _expansions = new();

    public static ExpandedGirlDefinition Get(GirlDefinition def)
        => Get(def.id);

    public static ExpandedGirlDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Girl, runtimeId));

    public static ExpandedGirlDefinition Get(RelativeId id)
    {
        if (!_expansions.TryGetValue(id, out var expansion))
        {
            expansion = new(id, ModInterface.GameData.GetGirl(id));
            _expansions[id] = expansion;
        }

        return expansion;
    }

    private RelativeId _id;
    private GirlDefinition _def;

    public ExpandedGirlDefinition(RelativeId id, GirlDefinition def)
    {
        _id = id;
        _def = def;
    }

    public Dictionary<RelativeId, GirlBodySubDefinition> Bodies = new();

    public GirlBodySubDefinition GetBody()
        => Bodies.TryGetValue(ModInterface.Save.GetCurrentFile().GetGirl(_id).BodyId, out var body)
            ? body
            : Bodies.Values.FirstOrDefault();

    /// <summary>
    /// The girl's index within a <see cref="DialogTriggerDefinition"/>.
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
    public GirlOutfitSubDefinition GetOutfit(RelativeId id)
    {
        var index = OutfitIdToIndex[id];

        if (index == -1)
        {
            index = _def.defaultOutfitIndex;
        }

        return _def.outfits[index];
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
    public GirlHairstyleSubDefinition GetHairstyle(RelativeId id)
    {
        var index = HairstyleIdToIndex[id];

        if (index == -1)
        {
            index = _def.defaultHairstyleIndex;
        }

        return _def.hairstyles[index];
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
    public GirlPartSubDefinition GetPart(RelativeId id)
        => _def.parts[PartIdToIndex[id]];

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
    public GirlExpressionSubDefinition GetExpression(RelativeId id)
        => _def.expressions[ExpressionIdToIndex[id]];

    /// <summary>
    /// Maps a hairstyle id to its index within the def. 
    /// Use <see cref="GetHairstyle"/> unless you must access the full collection.
    /// </summary>
    public Dictionary<RelativeId, int> SpecialPartIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    /// <summary>
    /// Maps a hairstyle index to its id within the def. 
    /// Use <see cref="GetHairstyle"/> unless you must access the full collection
    /// </summary>
    public Dictionary<int, RelativeId> SpecialPartIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    /// <summary>
    /// Given an id, returns the associated hairstyle.
    /// </summary>
    public GirlSpecialPartSubDefinition GetSpecialPart(RelativeId id)
        => _def.specialParts[SpecialPartIdToIndex[id]];
}
