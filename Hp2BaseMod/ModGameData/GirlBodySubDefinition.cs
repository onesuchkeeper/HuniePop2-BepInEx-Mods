using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod.ModGameData;

/// <summary>
/// Visual details for a girl
/// </summary>
public class GirlBodySubDefinition : SubDefinition
{
    public string BodyName;

    public Vector2 BreathEmitterPos;
    public Vector2 UpsetEmitterPos;
    public Vector2 BackPos;
    public Vector2 HeadPos;
    public Vector2 SpecialEffectOffset;

    public UiDollSpecialEffect SpecialEffectPrefab;

    public int PartIndexBody = -1;
    public int PartIndexNipples = -1;
    public int PartIndexBlushLight = -1;
    public int PartIndexBlushHeavy = -1;
    public int PartIndexBlink = -1;
    public int PartIndexMouthNeutral = -1;
    public List<int> PartIndexesPhonemes = new() { -1, -1, -1, -1, -1 };
    public List<int> PartIndexesPhonemesTeeth = new() { -1, -1, -1, -1, -1 };
    public int DefaultExpressionIndex = -1;
    public int FailureExpressionIndex = -1;
    public int DefaultHairstyleIndex = -1;
    public int DefaultOutfitIndex = -1;

    public List<GirlExpressionSubDefinition> Expressions = new();
    public List<GirlHairstyleSubDefinition> Hairstyles = new();
    public List<GirlOutfitSubDefinition> Outfits = new();
    public List<GirlSpecialPartSubDefinition> SpecialParts = new();
    public List<GirlPartSubDefinition> Parts = new();

    /// <summary>
    /// The scale of the girl's parts and outline layers
    /// </summary>
    public float Scale = 1;

    public GirlBodySubDefinition() { }

    internal GirlBodySubDefinition(GirlDefinition def)
    {
        BreathEmitterPos = def.breathEmitterPos;
        UpsetEmitterPos = def.upsetEmitterPos;

        SpecialEffectOffset = def.specialEffectOffset;
        SpecialEffectPrefab = def.specialEffectPrefab;

        PartIndexBody = def.partIndexBody;
        PartIndexNipples = def.partIndexNipples;
        PartIndexBlushLight = def.partIndexBlushLight;
        PartIndexBlushHeavy = def.partIndexBlushHeavy;
        PartIndexBlink = def.partIndexBlink;
        PartIndexMouthNeutral = def.partIndexMouthNeutral;

        PartIndexesPhonemes = def.partIndexesPhonemes;
        PartIndexesPhonemesTeeth = def.partIndexesPhonemesTeeth;
        DefaultExpressionIndex = def.defaultExpressionIndex;
        FailureExpressionIndex = def.failureExpressionIndex;
        DefaultHairstyleIndex = def.defaultHairstyleIndex;
        DefaultOutfitIndex = def.defaultOutfitIndex;

        Expressions = def.expressions;
        Hairstyles = def.hairstyles;
        Outfits = def.outfits;
        SpecialParts = def.specialParts;
        Parts = def.parts;
    }

    public void Apply(GirlDefinition def)
    {
        def.breathEmitterPos = BreathEmitterPos;
        def.upsetEmitterPos = UpsetEmitterPos;
        def.specialEffectOffset = SpecialEffectOffset;

        def.specialEffectPrefab = SpecialEffectPrefab;

        def.partIndexBody = PartIndexBody;
        def.partIndexNipples = PartIndexNipples;
        def.partIndexBlushLight = PartIndexBlushLight;
        def.partIndexBlushHeavy = PartIndexBlushHeavy;
        def.partIndexBlink = PartIndexBlink;
        def.partIndexMouthNeutral = PartIndexMouthNeutral;
        def.partIndexesPhonemes = PartIndexesPhonemes;
        def.partIndexesPhonemesTeeth = PartIndexesPhonemesTeeth;
        def.defaultExpressionIndex = DefaultExpressionIndex;
        def.failureExpressionIndex = FailureExpressionIndex;
        def.defaultHairstyleIndex = DefaultHairstyleIndex;
        def.defaultOutfitIndex = DefaultOutfitIndex;

        def.expressions = Expressions;
        def.hairstyles = Hairstyles;
        def.outfits = Outfits;
        def.specialParts = SpecialParts;
        def.parts = Parts;
    }

    public IdIndexMap PartLookup => _partLookup;
    private IdIndexMap _partLookup = new();

    /// <summary>
    /// Given an id, returns the associated part.
    /// </summary>
    public GirlPartSubDefinition GetPart(RelativeId id)
    {
        var index = _partLookup[id];

        return index < 0
            ? null
            : Parts[index];
    }

    internal GirlPartSubDefinition GetOrNewPart(RelativeId id) => Parts.GetOrNew(_partLookup[id]);

    public IdIndexMap SpecialPartLookup => _specialPartLookup;
    private IdIndexMap _specialPartLookup = new();

    /// <summary>
    /// Given an id, returns the associated hairstyle.
    /// </summary>
    public GirlSpecialPartSubDefinition GetSpecialPart(RelativeId id)
    {
        var index = _specialPartLookup[id];

        return index < 0
            ? null
            : SpecialParts[index];
    }

    internal GirlSpecialPartSubDefinition GetOrNewSpecialPart(RelativeId id) => SpecialParts.GetOrNew(_specialPartLookup[id]);

    /// <summary>
    /// Maps location ids to the default outfit
    /// </summary>
    public Dictionary<RelativeId, GirlStyleInfo> LocationIdToOutfitId = new();
}