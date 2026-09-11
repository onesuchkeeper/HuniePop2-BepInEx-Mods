using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod
{
    [Expansion(typeof(TokenDefinition), HasModId = true)]
#pragma warning disable HP001 // Deprecated member usage
    [Deprecates(nameof(TokenDefinition.resourceType), $"Uses {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)} instead")]
    [Deprecates(nameof(TokenDefinition.affectionType), $"Responsibility moved to {nameof(PuzzleResourceAffection)}")]
    [Deprecates(nameof(TokenDefinition.resourceName), $"Responsibility moved to {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)}")]
    [Deprecates(nameof(TokenDefinition.resourceSign), $"Responsibility moved to {nameof(ExpandedTokenDefinition)}.{nameof(ExpandedTokenDefinition.PuzzleResource)}")]
#pragma warning restore HP001 // Deprecated member usage
    public partial class ExpandedTokenDefinition
    {
        public IPuzzleResource PuzzleResource;
        public ITokenHandler TokenHandler;

        public bool BypassesStaminaCost(PuzzleStatus status)
        {
            var statusExp = status.GetExpansion();
            var focusedGirl = status.girlStatusFocused;
            return TokenHandler?.BypassesStaminaCost(statusExp, focusedGirl, _core) 
                ?? focusedGirl.noStaminaCostTokenDefs.Contains(_core);
        }

        public bool BypassesMovesCost(PuzzleStatus status)
        {
            var statusExp = status.GetExpansion();
            var focusedGirl = status.girlStatusFocused;
            return TokenHandler?.BypassesMovesCost(statusExp, focusedGirl, _core) 
                ?? focusedGirl.noMoveCostTokenDefs.Contains(_core);
        }

        public int GetStaminaCost(PuzzleMatch match) => TokenHandler?.GetStaminaCost(match) ?? Mathf.Max(match.slots.Count - 2, 1);

        public int GetMovesCost(PuzzleMatch match) => TokenHandler?.GetMovesCost(match) ?? 1;

        public bool CanSpawn(PuzzleStatus puzzleStatus) => TokenHandler.CanSpawn(puzzleStatus);

        internal void InitInternalDefinition(Dictionary<RelativeId, IPuzzleResource> puzzleResources, Dictionary<RelativeId, ITokenHandler> tokenHandlers)
        {
            if (_core.resourceType == PuzzleResourceType.AFFECTION)
            {
                switch (_core.affectionType)
                {
                    case PuzzleAffectionType.TALENT:
                        PuzzleResource = puzzleResources[PuzzleResourceId.AffectionTalent];
                        break;
                    case PuzzleAffectionType.FLIRTATION:
                        PuzzleResource = puzzleResources[PuzzleResourceId.AffectionFlirtation];
                        break;
                    case PuzzleAffectionType.ROMANCE:
                        PuzzleResource = puzzleResources[PuzzleResourceId.AffectionRomance];
                        break;
                    case PuzzleAffectionType.SEXUALITY:
                        PuzzleResource = puzzleResources[PuzzleResourceId.AffectionSexuality];
                        break;
                }
            }
            else
            {
                if (puzzleResources.TryGetValue(PuzzleResourceId.From(_core.resourceType), out var puzzleResource))
                {
                    PuzzleResource = puzzleResource;
                }
            }

            switch (_core.resourceType)
            {
                case PuzzleResourceType.STAMINA:
                    TokenHandler = tokenHandlers[TokenHandlerId.Stamina];
                    break;
                case PuzzleResourceType.MOVES:
                    TokenHandler = tokenHandlers[TokenHandlerId.Moves];
                    break;
                default:
                    TokenHandler = tokenHandlers[TokenHandlerId.Default];
                    break;
            }
        }
    }
}
