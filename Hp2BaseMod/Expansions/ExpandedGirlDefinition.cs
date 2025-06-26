using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public static class GirlDefinition_Ext
{
    public static ExpandedGirlDefinition Expansion(this GirlDefinition def)
        => ExpandedGirlDefinition.Get(def);
}

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

    public Vector2 BackPosition;
    public Vector2 HeadPosition;
    public int DialogTriggerIndex = -1;

    public Dictionary<RelativeId, int> OutfitIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    public Dictionary<int, RelativeId> OutfitIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    public GirlOutfitSubDefinition GetOutfit(GirlDefinition def, RelativeId id)
    {
        var index = OutfitIdToIndex[id];

        if (index == -1)
        {
            index = def.defaultOutfitIndex;
        }

        return def.outfits[index];
    }

    public Dictionary<RelativeId, int> HairstyleIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    public Dictionary<int, RelativeId> HairstyleIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    public GirlHairstyleSubDefinition GetHairstyle(GirlDefinition def, RelativeId id)
    {
        var index = HairstyleIdToIndex[id];

        if (index == -1)
        {
            index = def.defaultHairstyleIndex;
        }

        return def.hairstyles[index];
    }

    public Dictionary<RelativeId, int> PartIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    public Dictionary<int, RelativeId> PartIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    public GirlPartSubDefinition GetPart(GirlDefinition def, RelativeId id)
        => def.parts[PartIdToIndex[id]];

    public Dictionary<RelativeId, int> ExpressionIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    public Dictionary<int, RelativeId> ExpressionIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    public GirlExpressionSubDefinition GetExpression(GirlDefinition def, RelativeId id)
        => def.expressions[ExpressionIdToIndex[id]];
}
