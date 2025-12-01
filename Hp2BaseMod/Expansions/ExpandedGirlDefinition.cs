using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;

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

    public GirlBodySubDefinition GetCurrentBody()
    {
        var save = ModInterface.Save.GetCurrentFile().GetGirl(_id);
        if (!Bodies.TryGetValue(save.BodyId, out var body))
        {
            save.BodyId = Bodies.First().Key;
            body = Bodies[save.BodyId];
        }

        return body;
    }

    public void SetBody(RelativeId id)
    {
        if (Bodies.TryGetValue(id, out var body))
        {
            ModInterface.Save.GetCurrentFile().GetGirl(_id).BodyId = id;
            var baseFile = Game.Persistence.playerFile.GetPlayerFileGirl(_def);
            body.Apply(_def);

            baseFile.outfitIndex = _def.defaultOutfitIndex;
            baseFile.hairstyleIndex = _def.defaultHairstyleIndex;
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to set body of girl {_id} to {id}");
        }
    }

    /// <summary>
    /// The girl's index within a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public int DialogTriggerIndex = -1;

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
    public GirlExpressionSubDefinition GetExpression(RelativeId expressionId) => GetExpression(GetCurrentBody(), expressionId);

    /// <summary>
    /// Given an id, returns the associated expression.
    /// </summary>
    public GirlExpressionSubDefinition GetExpression(RelativeId bodyId, RelativeId expressionId) => GetExpression(Bodies[bodyId], expressionId);

    public GirlExpressionSubDefinition GetExpression(GirlBodySubDefinition body, RelativeId expressionId)
    {
        if (body.Expressions.TryGet(ExpressionIdToIndex[expressionId], out var expression))
        {
            return expression;
        }

        return body.Expressions[body.DefaultExpressionIndex];
    }

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
    public GirlOutfitSubDefinition GetOutfit(RelativeId outfitId) => GetOutfit(GetCurrentBody(), outfitId);
    public GirlOutfitSubDefinition GetOutfit(RelativeId bodyId, RelativeId outfitId) => GetOutfit(Bodies[bodyId], outfitId);

    public int GetOutfitIndex(RelativeId outfitId) => OutfitIdToIndex[outfitId];

    /// <summary>
    /// Given an id, returns the associated outfit.
    /// </summary>
    public GirlOutfitSubDefinition GetOutfit(GirlBodySubDefinition body, RelativeId id)
    {
        if (!body.Outfits.TryGet(OutfitIdToIndex[id], out var outfit))
        {
            outfit = body.Outfits[body.DefaultOutfitIndex];
        }

        return outfit;
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
    public GirlHairstyleSubDefinition GetHairstyle(RelativeId hairstyleId) => GetHairstyle(GetCurrentBody(), hairstyleId);
    public GirlHairstyleSubDefinition GetHairstyle(RelativeId bodyId, RelativeId hairstyleId) => GetHairstyle(Bodies[bodyId], hairstyleId);

    public GirlHairstyleSubDefinition GetHairstyle(GirlBodySubDefinition body, RelativeId id)
    {
        if (!body.Hairstyles.TryGet(HairstyleIdToIndex[id], out var hairstyle))
        {
            hairstyle = body.Hairstyles[body.DefaultHairstyleIndex];
        }

        return hairstyle;
    }
}
