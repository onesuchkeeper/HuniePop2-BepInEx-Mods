using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

[Expansion(typeof(TokenDefinition), HasModId = true)]
#pragma warning disable HP001 // Deprecated member usage
[Deprecates(nameof(TokenDefinition.resourceType), $"Uses the {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)} instead")]
[Deprecates(nameof(TokenDefinition.affectionType), $"Responsibility moved to {nameof(PuzzleResourceAffection)}")]
[Deprecates(nameof(TokenDefinition.resourceName), $"Responsibility moved to {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)}")]
[Deprecates(nameof(TokenDefinition.resourceSign), $"Responsibility moved to {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)}")]
#pragma warning restore HP001 // Deprecated member usage
public partial class ExpandedTokenDefinition
{
    public IPuzzleResource PuzzleResource;

    internal void InitInternalDefinition(Dictionary<RelativeId, IPuzzleResource> puzzleResources)
    {
        if (_core.resourceType == PuzzleResourceType.AFFECTION)
        {
            switch (_core.affectionType)
            {
                case PuzzleAffectionType.TALENT:
                    PuzzleResource = puzzleResources[PuzzleResourceId.AffectionTalent];
                    return;
                case PuzzleAffectionType.FLIRTATION:
                    PuzzleResource = puzzleResources[PuzzleResourceId.AffectionFlirtation];
                    return;
                case PuzzleAffectionType.ROMANCE:
                    PuzzleResource = puzzleResources[PuzzleResourceId.AffectionRomance];
                    return;
                case PuzzleAffectionType.SEXUALITY:
                    PuzzleResource = puzzleResources[PuzzleResourceId.AffectionSexuality];
                    return;
            }
        }
        else
        {
            if (puzzleResources.TryGetValue(PuzzleResourceId.From(_core.resourceType), out var puzzleResource))
            {
                PuzzleResource = puzzleResource;    
                return;
            }
        }

        throw new Exception($"Failed to find {nameof(IPuzzleResource)} type");
    }
}