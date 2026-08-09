using UnityEngine;

namespace Hp2BaseMod;

public class TokenPuzzleReward : IPuzzleReward
{
    public ExpandedTokenDefinition TokenDefinition { get; }

    public int ResourceValue { get; set; }

    public PuzzleStatusGirl Girl { get; }

    public bool ZeroedValue { get; set; }

    public bool Negative => ResourceValue < 0;

    public TokenDefinition ReplaceDefinition { get; set; }

    public bool ReplaceUpgraded { get; set; }

    public ItemDefinition FruitDefinition { get; set; }

    public int DisplayValue => ResourceValue;

    public TokenPuzzleReward(
        ExpandedTokenDefinition tokenDefinition,
        int resourceValue,
        PuzzleStatusGirl girl)
    {
        TokenDefinition = tokenDefinition;
        ResourceValue = resourceValue;
        Girl = girl;
    }

    public virtual void Apply(ExpandedUiPuzzleGrid puzzleGrid, ExpandedPuzzleStatus puzzleStatus) 
        => TokenDefinition.PuzzleResource.AddResourceValue(ResourceValue, puzzleStatus, Girl);

    public virtual (string splashText, string burstText) GetLabelText(bool bonusRound = false, bool shortVersion = false)
        => TokenDefinition.PuzzleResource.GetLabelText(ResourceValue, bonusRound);
}
