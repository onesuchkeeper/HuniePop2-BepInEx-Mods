namespace Hp2BaseMod;

public interface IPuzzleReward
{
    ExpandedTokenDefinition TokenDefinition { get; }

    PuzzleStatusGirl Girl { get; }

    bool ZeroedValue { get; }

    int ResourceValue { get; set; }

    /// <summary>
    /// If this reward is considered a negative
    /// </summary>
    bool Negative { get; }

    int DisplayValue { get; }

    TokenDefinition ReplaceDefinition { get; set; }

    bool ReplaceUpgraded { get; set; }

    ItemDefinition FruitDefinition { get; }

    void Apply(ExpandedUiPuzzleGrid puzzleGrid, ExpandedPuzzleStatus puzzleStatus);

    (string splashText, string burstText) GetLabelText(bool bonusRound = false, bool shortVersion = false);
}