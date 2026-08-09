// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod;
public static class PuzzleAffectionId
{
    internal static readonly Dictionary<RelativeId, PuzzleAffectionType> _lookup = new()
    {
        {Talent, PuzzleAffectionType.TALENT},
        {Flirtation, PuzzleAffectionType.FLIRTATION},
        {Romance, PuzzleAffectionType.ROMANCE},
        {Sexuality, PuzzleAffectionType.SEXUALITY},
    };

    public static RelativeId Talent => new RelativeId(-1, 0);
    public static RelativeId Flirtation => new RelativeId(-1, 1);
    public static RelativeId Romance => new RelativeId(-1, 2);
    public static RelativeId Sexuality => new RelativeId(-1, 3);

    public static int GetAffectionLevelExp(this PlayerFile playerFile, RelativeId puzzleAffectionId, bool ofLevel = false)
    {
        if (_lookup.TryGetValue(puzzleAffectionId, out var puzzleAffectionType))
        {
            return playerFile.GetAffectionLevelExp(puzzleAffectionType, ofLevel);
        }

        throw new Exception($"Unhandled id {puzzleAffectionId}");
    }

    public static int GetAffectionLevel(this PlayerFile playerFile, RelativeId puzzleAffectionId, bool raw = false)
    {
        if (_lookup.TryGetValue(puzzleAffectionId, out var puzzleAffectionType))
        {
            return playerFile.GetAffectionLevel(puzzleAffectionType, raw);
        }

        throw new Exception($"Unhandled id {puzzleAffectionId}");
    }

    public static void AddAffectionLevelExp(this PlayerFile playerFile, RelativeId puzzleAffectionId, int amount)
    {
        if (!_lookup.TryGetValue(puzzleAffectionId, out var puzzleAffectionType))
        {
            throw new Exception($"Unhandled id {puzzleAffectionId}");
        }

        playerFile.AddAffectionLevelExp(puzzleAffectionType, amount);   
    }

    public static int GetBaggageCountByAffectionType(this PlayerFile playerFile, RelativeId puzzleAffectionId, bool mostFav)
    {
        if (!_lookup.TryGetValue(puzzleAffectionId, out var puzzleAffectionType))
        {
            throw new Exception($"Unhandled id {puzzleAffectionId}");
        }

        return playerFile.GetBaggageCountByAffectionType(puzzleAffectionType, mostFav);   
    }

    public static void AddFruitCount(this PlayerFile playerFile, RelativeId puzzleAffectionId, int addCount)
    {
        if (!_lookup.TryGetValue(puzzleAffectionId, out var puzzleAffectionType))
        {
            throw new Exception($"Unhandled id {puzzleAffectionId}");
        }

        playerFile.AddFruitCount(puzzleAffectionType, addCount);   
    }
}