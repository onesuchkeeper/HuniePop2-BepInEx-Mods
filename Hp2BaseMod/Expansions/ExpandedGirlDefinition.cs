using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
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

    /// <summary>
    /// The girl's index within a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new();

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
            var girl = ModInterface.Save.GetCurrentFile().GetGirl(_id);
            var oldId = girl.BodyId;
            girl.BodyId = id;

            body.Apply(_def);

            if (oldId != id)
            {
                var baseFile = Game.Persistence.playerFile.GetPlayerFileGirl(_def);
                baseFile.outfitIndex = _def.defaultOutfitIndex;
                baseFile.hairstyleIndex = _def.defaultHairstyleIndex;
            }
        }
        else
        {
            ModInterface.Log.Warning($"Failed to set body of girl {_id} to {id}");
        }
    }

    public IdIndexMap ExpressionLookup => _expressionLookup;
    private IdIndexMap _expressionLookup = new();

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
        if (body.Expressions.TryGet(ExpressionLookup[expressionId], out var expression))
        {
            return expression;
        }

        return body.Expressions[body.DefaultExpressionIndex];
    }

    internal GirlExpressionSubDefinition GetOrNewExpression(GirlBodySubDefinition body, RelativeId expressionId)
        => body.Expressions.GetOrNew(ExpressionLookup[expressionId]);

    public IdIndexMap OutfitLookup => _outfitLookup;
    private IdIndexMap _outfitLookup = new();

    /// <summary>
    /// Given an id, returns the associated outfit.
    /// </summary>
    public GirlOutfitSubDefinition GetOutfit(RelativeId outfitId) => GetOutfit(GetCurrentBody(), outfitId);
    public GirlOutfitSubDefinition GetOutfit(RelativeId bodyId, RelativeId outfitId)
        => GetOutfit(Bodies[bodyId], outfitId);

    public int GetOutfitIndex(RelativeId outfitId) => OutfitLookup[outfitId];

    /// <summary>
    /// Given an id, returns the associated outfit.
    /// </summary>
    public GirlOutfitSubDefinition GetOutfit(GirlBodySubDefinition body, RelativeId id)
    {
        if (!body.Outfits.TryGet(OutfitLookup[id], out var outfit))
        {
            outfit = body.Outfits[body.DefaultOutfitIndex];
        }

        return outfit;
    }

    internal GirlOutfitSubDefinition GetOrNewOutfit(GirlBodySubDefinition body, RelativeId id) => body.Outfits.GetOrNew(OutfitLookup[id]);

    public IdIndexMap HairstyleLookup => _hairstyleLookup;
    private IdIndexMap _hairstyleLookup = new();

    /// <summary>
    /// Given an id, returns the associated hairstyle.
    /// </summary>
    public GirlHairstyleSubDefinition GetHairstyle(RelativeId hairstyleId) => GetHairstyle(GetCurrentBody(), hairstyleId);
    public GirlHairstyleSubDefinition GetHairstyle(RelativeId bodyId, RelativeId hairstyleId) => GetHairstyle(Bodies[bodyId], hairstyleId);

    public GirlHairstyleSubDefinition GetHairstyle(GirlBodySubDefinition body, RelativeId id)
    {
        if (!body.Hairstyles.TryGet(HairstyleLookup[id], out var hairstyle))
        {
            hairstyle = body.Hairstyles[body.DefaultHairstyleIndex];
        }

        return hairstyle;
    }

    internal GirlHairstyleSubDefinition GetOrNewHairstyle(GirlBodySubDefinition body, RelativeId id) => body.Hairstyles.GetOrNew(HairstyleLookup[id]);

    public Dictionary<RelativeId, RelativeId> FavQuestionIdToAnswerId = new();

    public IdIndexMap GetQuestionAnswerIndexMap(RelativeId questionId)
        => _questionAnswerIndexes.GetOrNew(questionId, () => new(1));//start at 1, 0 is hard coded correct answer
    private Dictionary<RelativeId, IdIndexMap> _questionAnswerIndexes = new();

    public IdIndexMap HerQuestionIdToIndex => _herQuestionIdToIndex;
    private IdIndexMap _herQuestionIdToIndex = new();

    public IdIndexMap HerQuestionGoodResponseIdToDtIndex => _herQuestionGoodResponseIdToIndex;
    private IdIndexMap _herQuestionGoodResponseIdToIndex = new();

    public IdIndexMap HerQuestionBadResponseIdToDtIndex => _herQuestionBadResponseIdToIndex;
    private IdIndexMap _herQuestionBadResponseIdToIndex = new();
}
