using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="GirlDefinition"/> class is the fundamental data container used to define character identity, 
/// visual metadata, and gameplay categorization. These definitions serve as the primary configuration 
/// blueprint for standard dateable cast members, love fairies, and cosmic deities.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Categorization</b>
/// A <see cref="GirlDefinition"/> dictates how a character is populated within the game world. The most 
/// critical logical gate within this definition is the <b>specialCharacter</b> boolean. When set to 
/// <c>false</c>, the character is treated as a member of the standard dateable cast, making them eligible 
/// for randomized pairings in the Finder app and standard progression. When set to <c>true</c>, the 
/// character is flagged as a unique entity, such as a guide (<see cref="Girls.Kyu"/>) or a boss 
/// (<see cref="Girls.Moxie"/> and <see cref="Girls.Jewn"/>), which exempts them from standard population 
/// logic and requires specialized triggers for their appearances.
/// </para>
/// 
/// <para>
/// <b>Name Resolution Logic</b>
/// The class provides the <c>GetNickName()</c> method to ensure consistent UI rendering. This logic 
/// prioritizes the <c>girlNickName</c> field; if that field is null or empty, the engine falls back to 
/// the full <c>girlName</c>. This ensures that dialogue windows and cellphone apps always display 
/// the intended identifier without developer-side null checks.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="UiDoll"/>: The visual representation of the girl. It uses the definition 
///     to load specific character parts and resolve dialogue triggers.</description>
///   </item>
///   <item>
///     <description><see cref="GirlPairDefinition"/>: Defines the double-date logic. A character's 
///     "Boss" status is often a functional state derived from their presence in a pair marked as 
///     <c>specialPair</c>.</description>
///   </item>
///   <item>
///     <description><see cref="DialogTriggerDefinition"/>: Maps character-specific text arrays to 
///     universal triggers. The definition is used as a lookup key to retrieve the correct 
///     <c>DialogTriggerLineSet</c>.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatusGirl"/>: The runtime container for character state 
///     during gameplay, tracking stamina, traits, and active <see cref="Ailment"/>s based on 
///     definition metadata.</description>
///   </item>
///   <item>
///     <description><see cref="GirlData"/>: The global repository used by the engine to resolve 
///     character instances during scene transitions or UI population.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedGirlDefinition Functionality</b>
/// The <see cref="ExpandedGirlDefinition"/> enhances the character system to support a modular, 
/// multi-plugin architecture. Its primary goal is to bypass the base game's reliance on fixed 
/// collection indexes and hardcoded enums, which would otherwise cause collisions between mods.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>RelativeId Integration:</b> Decouples characters from integer-based database 
///     lookups, allowing custom girls to exist within their own mod-specific namespaces.</description>
///   </item>
///   <item>
///     <description><b>Dynamic Metadata Injection:</b> Through <see cref="GirlDataMod"/>, developers 
///     can modify or extend character properties—such as adding unique categories for items or 
///     overriding favorite questions—without altering original game assets.</description>
///   </item>
///   <item>
///     <description><b>Decoupled Favorite Questions:</b> Replaces the original index-based tracking 
///     of "Learned Favorites" with a system based on <see cref="RelativeId"/>. This allows 
///     for an infinite number of custom conversation topics and girl-specific questions.</description>
///   </item>
///   <item>
///     <description><b>Extended Style Sequences:</b> Provides a dedicated pipeline for defining 
///     complex outfit and hairstyle sequences used in narrative events, such as meeting 
///     cutscenes, which the base game previously handled through hardcoded logic.</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
[Expansion(typeof(GirlDefinition), HasModId = true)]
public partial class ExpandedGirlDefinition
{
    /// <summary>
    /// The girl's index within a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new(1);

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
