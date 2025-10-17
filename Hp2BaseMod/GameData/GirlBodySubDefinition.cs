using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

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

    public GirlBodySubDefinition()
    {

    }

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
    {
        var index = PartIdToIndex[id];

        return index == -1
            ? null
            : Parts[index];
    }

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
        => SpecialParts[SpecialPartIdToIndex[id]];
}