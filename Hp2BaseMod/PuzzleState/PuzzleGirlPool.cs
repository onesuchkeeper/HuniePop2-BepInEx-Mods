// Hp2BaseMod 2026, By OneSuchKeeper

using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// The girls that puzzles made of many girls (the boss date and non-stop) draw from.
/// </summary>
internal static class PuzzleGirlPool
{
    /// <summary>
    /// Regular (non-special) girls the player has met. The base game takes every regular girl because it has no others to be
    /// unmet by the point in the story where these puzzles are available, but modded girls can be.
    /// </summary>
    public static List<GirlDefinition> GetMetRegularGirls()
    {
        var girls = new List<GirlDefinition>();
        foreach (var girl in Game.Data.Girls.GetAllBySpecial(special: false))
        {
            var file = Game.Persistence.playerFile.GetPlayerFileGirl(girl);
            if (file != null && file.playerMet)
            {
                girls.Add(girl);
            }
        }

        return girls;
    }

    /// <summary>
    /// PuzzleStatus.Reset only builds a BOSS or NONSTOP status for more than 2 girls; with fewer it silently becomes a normal date.
    /// If the player has met too few girls, pad with unmet ones so the puzzle is still the right kind.
    /// </summary>
    public static void PadToAtLeast(List<GirlDefinition> girls, int minimumCount, string puzzleName)
    {
        if (girls.Count >= minimumCount) return;

        ModInterface.Log.Warning($"{puzzleName}: fewer than {minimumCount} regular girls have been met; padding with unmet girls so the status is still the right kind of puzzle.");
        foreach (var girl in Game.Data.Girls.GetAllBySpecial(special: false))
        {
            if (girls.Count >= minimumCount) break;
            if (!girls.Contains(girl)) girls.Add(girl);
        }
    }
}