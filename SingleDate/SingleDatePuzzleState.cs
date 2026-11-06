using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// A date with a single pair: <see cref="SingleDateRules"/> (a normal date with the girl alone on the right, focus
/// locked to her) plus what the single date changes about play. Replaces <c>SingleDateAilment</c> and the single date
/// parts of <c>PuzzleStatusPatch</c>, <c>UiPuzzleGridPatch</c>, <c>CutsceneStepSpecialPostRewardsPatch</c> and
/// <c>PlayerFileGirlPairPatch</c>, and of the global round over, doll style and girl state handlers.
/// </summary>
/// <remarks>
/// Needs the three <c>protected virtual</c> hooks in <see cref="PuzzleStateNormal"/> (<c>LevelUpPair</c>,
/// <c>AdjustRoundOver</c> and <c>AdjustDollStyle</c>), see CHANGES.md.
/// </remarks>
public sealed class SingleDatePuzzleState : SingleDateRules
{
    public new class Factory : IPuzzleStateFactory
    {
        public IFullPuzzleState Create(IGameStateLocation gameState) => new SingleDatePuzzleState(gameState);
    }

    private readonly SingleDateLocationBehavior _behavior;
    private bool _disposed;

    public SingleDatePuzzleState(IGameStateLocation owner) : base(owner)
    {
        _behavior = new SingleDateLocationBehavior();

        var ailmentManager = Game.Session.Ailment.GetExpansion();
        ailmentManager.PreAilmentEnable += On_PreAilmentEnable;
        ailmentManager.PostMatchReward += On_PostMatchReward;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            _behavior.Dispose();

            var ailmentManager = Game.Session.Ailment.GetExpansion();
            ailmentManager.PreAilmentEnable -= On_PreAilmentEnable;
            ailmentManager.PostMatchReward -= On_PostMatchReward;

            SetStaminaSuppressed(false);
        }

        base.Dispose(disposing);
    }

    // ---- stamina isn't a thing on a single date ----

    public override void OnPuzzleStarting()
    {
        base.OnPuzzleStarting();

        SetStaminaSuppressed(true);
        MakeStaminaTokensInvalid();
    }

    public override void OnNextRound()
    {
        base.OnNextRound();

        // A round that brings new girls has new girl status objects.
        MakeStaminaTokensInvalid();
    }

    /// <summary>
    /// Turns the stamina cost, and the stamina, exhaustion and upset warnings, off (or back on when the state ends).
    /// </summary>
    private static void SetStaminaSuppressed(bool suppressed)
    {
        if (Game.Session == null || Game.Session.Puzzle == null || Game.Session.Puzzle.puzzleGrid == null) return;

        var grid = ExpandedUiPuzzleGrid.Get();
        grid.SuppressStaminaCost = suppressed;
        grid.SuppressStaminaWarning = suppressed;
        grid.SuppressExhaustionWarning = suppressed;
        grid.SuppressUpsetWarning = suppressed;
    }

    /// <summary>Stamina tokens can't be matched for the girl, there is no stamina to give her.</summary>
    private static void MakeStaminaTokensInvalid()
    {
        var staminaToken = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA);
        var rightGirl = Game.Session.Puzzle.puzzleStatus.girlStatusRight;

        if (staminaToken != null && !rightGirl.invalidTokenDefs.Contains(staminaToken))
        {
            rightGirl.invalidTokenDefs.Add(staminaToken);
        }
    }

    /// <summary>
    /// Turns broken heart matches into an affection loss (as in HuniePop 1, the base game takes nothing). A broken heart
    /// token takes away a fraction of the girl's current affection (<see cref="State.GetBrokenMult"/>, which sensitivity
    /// lowers), with a minimum loss of 1. The reward values are set to that loss, and the affection is removed from the girl.
    /// </summary>
    private void On_PostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (args?.Rewards == null) return;

        var brokenRewards = args.Rewards
            .Where(x => x.Reward?.TokenDefinition?.PuzzleResource?.Id == PuzzleResourceId.Broken)
            .ToList();

        if (brokenRewards.Count == 0) return;

        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
        var allotment = -Mathf.Max(1, Mathf.FloorToInt(puzzleStatus.affection * State.GetBrokenMult()));

        foreach (var entry in brokenRewards)
        {
            entry.Reward.ResourceValue = allotment;
        }

        puzzleStatus.GetExpansion().AddResourceValue(PuzzleResourceId.AffectionTalent, allotment * brokenRewards.Count, true);
    }

    // ---- ailments ----

    /// <summary>
    /// Cancels enabling of ailments listed in <see cref="Plugin.AddSingleDateDisabledAilment"/>, mostly ones that need
    /// the player to switch focus between two girls (which focus lock doesn't allow).
    /// </summary>
    private void On_PreAilmentEnable(AilmentTriggerArgs.PreAilmentEnable args)
    {
        var id = args.Ailment.definition.ModId();
        if (!Plugin._singleDateDisabledAilments.Contains(id)) return;

        ModInterface.Log.Message($"Preventing ailment {id} \"{args.Ailment.definition.name}\" from being enabled during single date");
        args.Cancel = true;
    }

    // ---- what a round ending means ----

    /// <summary>The pair's level up is single date bookkeeping, and single pairs aren't counted as completed.</summary>
    protected override void LevelUpPair(PlayerFileGirlPair pairFile) => SingleDateRelationship.LevelUp(pairFile);

    /// <summary>
    /// Decides what a successful date round means for single date progress, and whether the bonus (sex) round occurs.
    /// Runs after the normal rules and before mods are notified.
    /// </summary>
    /// <remarks>
    /// Only acts on successful rounds for an attracted pair (the date that makes a compatible pair attracted is a normal
    /// round over, and a pair of lovers is done). If the girl is at (or above) one level below max, the date is at the
    /// pair's sex location (or the pair has none) and it is the pair's sex time of day, her level is set to max and the
    /// lovers cutscene and bonus round are enabled. The game ends after the bonus round. Otherwise the bonus round and any
    /// attract-to-lovers cutscene are denied, the date ends, and her level goes up by one (until it is one below max).
    /// </remarks>
    protected override void AdjustRoundOver(PuzzleRoundOverArgs args)
    {
        if (!args.IsSuccess) return;

        var pair = Game.Session.Location.currentGirlPair;
        var pairFile = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);
        if (pairFile == null || pairFile.relationshipType != GirlPairRelationshipType.ATTRACTED) return;

        // Single pairs always have the real girl as girl two (girl one is Nobody).
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, pair.girlDefinitionTwo.id);
        var girlSave = State.SaveFile.GetGirl(girlId);
        if (girlSave == null) return;

        var maxLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        // True when she is at the level where the sex date unlocks (max - 1) or beyond.
        var oneUnderMax = girlSave.RelationshipLevel >= maxLevel - 1;

        // Sex date requires: the right place (any place if the pair has no sex location), the right level and the right time of day.
        if ((pair.sexLocationDefinition == null || pair.sexLocationDefinition == Game.Session.Location.currentLocation)
            && oneUnderMax
            && pair.sexDaytime == (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4))
        {
            // Reaching the sex date promotes her to max level (lovers).
            girlSave.RelationshipLevel = maxLevel;

            ModInterface.Log.Message("Enable single date bonus round");
            args.IsSexDate = true;
            args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.AttractToLovers;
            // The date is over once the bonus round has been played, not before.
            args.IsGameOver = Game.Session.Puzzle.puzzleStatus.bonusRound;
            return;
        }

        // Not ready for the sex date yet, each successful date moves her up one level.
        if (!oneUnderMax)
        {
            girlSave.RelationshipLevel++;
        }

        ModInterface.Log.Message("Single date deny bonus round");
        // The normal rules want to play attract-to-lovers, but single dates don't level up that way.
        if (args.LevelUpType == PuzzleRoundOverArgs.CutsceneType.AttractToLovers)
        {
            args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
        }

        args.IsSexDate = false;
        args.IsGameOver = true;
    }

    // ---- rewards: seeds for the right girl only ----

    // The rewards step makes one reward for every two fruit, each reward one fruit for each girl. Only one girl gets fruit
    // here, so twice as many rewards keeps the seeds a single date has always given.
    public override int GetPostRewardFruitCount(int baseFruitCount, out bool failed)
        => base.GetPostRewardFruitCount(baseFruitCount, out failed) * 2;

    public override void GetPostRewardFruits(int rewardIndex, out ItemDefinition forFocused, out ItemDefinition forUnfocused)
    {
        // The right girl is always the focused one, and Nobody gets nothing.
        forFocused = Game.Session.Puzzle.puzzleStatus.girlStatusRight.girlDefinition.GetExpansion().GetRandomFruit();
        forUnfocused = null;
    }

    // ---- doll styles ----

    /// <summary>
    /// Picks the sex outfit/hairstyle set once the sex date is available: when the girl is one level below max and the pair
    /// defines a non default sex outfit.
    /// </summary>
    protected override void AdjustDollStyle(PreDateDollResetArgs args)
    {
        var pair = Game.Session.Location.currentGirlPair;
        var girlSave = State.SaveFile.GetGirl(pair.girlDefinitionTwo.id);

        // must be non default and at pre max level
        var pairStyle = pair.GetExpansion().PairStyle;
        if (pairStyle != null
            && pairStyle.SexGirlTwo.OutfitId != RelativeId.Default
            && girlSave?.RelationshipLevel == Plugin.MaxSingleGirlRelationshipLevel.Value - 1)
        {
            args.Style = PreDateDollResetArgs.StyleType.Sex;
        }
    }
}