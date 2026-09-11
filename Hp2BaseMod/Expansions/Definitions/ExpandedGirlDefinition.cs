using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;
using UnityEngine;

namespace Hp2BaseMod;

#pragma warning disable HP001 // Deprecated member usage
[Expansion(typeof(GirlDefinition), HasModId = true)]
[Deprecates(nameof(GirlDefinition.GetMostFavAffectionType), $"Use {nameof(ExpandedGirlDefinition)}.{nameof(ExpandedGirlDefinition.GetMostFavAffectionType)} instead.")]
[Deprecates(nameof(GirlDefinition.GetLeastFavAffectionType), $"Use {nameof(ExpandedGirlDefinition)}.{nameof(ExpandedGirlDefinition.GetMostFavAffectionType)} instead.")]
[Deprecates(nameof(GirlDefinition.specialEffectOffset), $"Use {nameof(GirlBodySubDefinition)}.{nameof(GirlBodySubDefinition.BackPos)} or {nameof(GirlBodySubDefinition)}.{nameof(GirlBodySubDefinition.HeadPos)} instead.")]
#pragma warning restore HP001 // Deprecated member usage
public partial class ExpandedGirlDefinition
{
    /// <summary>
    /// The girl's index within a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new(1);

    public IAffection FavAffection;
    public IAffection LeastFavAffection;

    public Dictionary<RelativeId, GirlBodySubDefinition> Bodies = new();

    public IGirlTalkHandler TalkHandler;

    public ItemDefinition GetRandomFruit()
    {
        // Fall back to general pool if FavAffection is null
        if (FavAffection != null && Random.Range(0f, 1f) >= 0.5f)
        {
            return FavAffection.GetRandomFruit();
        }

        var pool = ModInterface.GameData.Affections.Values.Where(x => x.HasFruit).ToList();
        if (FavAffection != null) pool.Remove(FavAffection);
        if (LeastFavAffection != null) pool.Remove(LeastFavAffection);

        if (pool.Count == 0)
        {
            return ModInterface.GameData.GetRandomFruit();
        }

        return pool.GetRandom().GetRandomFruit();
    }

    public IAffection GetMostFavAffectionType()
    {
        if (!Game.Session.Puzzle.isPuzzleActive 
            || !Game.Session.Puzzle.IsPuzzleOffset(PuzzleOffsetId.FlipMostLeastFavs))
        {
            return FavAffection;
        }

        return LeastFavAffection;
    }

    public IAffection GetLeastFavAffectionType()
    {
        if (!Game.Session.Puzzle.isPuzzleActive 
            || !Game.Session.Puzzle.IsPuzzleOffset(PuzzleOffsetId.FlipMostLeastFavs))
        {
            return LeastFavAffection;
        }

        return FavAffection;
    }

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

            body.Apply(_core);

            if (oldId != id)
            {
                var baseFile = Game.Persistence.playerFile.GetPlayerFileGirl(_core);
                baseFile.outfitIndex = _core.defaultOutfitIndex;
                baseFile.hairstyleIndex = _core.defaultHairstyleIndex;
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

    public IdIndexMap OutfitLookup = new();

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

    public IdIndexMap HairstyleLookup = new();

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

    public IdIndexMap DateGreetingLocIdToDtIndex => _dateGreetingLocIdToDtIndex;
    private IdIndexMap _dateGreetingLocIdToDtIndex = new();
}
