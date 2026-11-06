using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;
/*
    In the base game PuzzleStatus is a member of PuzzleManager
    Instantiated in Awake.
*/

[Expansion(typeof(PuzzleStatus))]
[Deprecates(nameof(PuzzleStatus.AddResourceValue), $"Use {nameof(ExpandedPuzzleStatus)}.{nameof(ExpandedPuzzleStatus.AddResourceValue)} instead")]
[Deprecates(nameof(PuzzleStatus.GetResourceValue), $"Use {nameof(ExpandedPuzzleStatus)}.{nameof(ExpandedPuzzleStatus.GetResourceValue)} instead")]
[Deprecates(nameof(PuzzleStatus.statusType), $"What kind of puzzle is running is the current {nameof(IPuzzleState)}'s business. Ask it for the rule you need (see {nameof(IPuzzleStatusRules)}, AllowsForfeit...) instead of switching on a kind.")]
[Deprecates(nameof(PuzzleStatus.IsTutorial), $"What kind of puzzle is running is the current {nameof(IPuzzleState)}'s business. Ask it for the rule you need (AllowsForfeit, AllowsAction, SuppressesDollDialog...) instead of asking whether it is the tutorial.")]
[Deprecates(nameof(PuzzleStatus.Reset), $"Use {nameof(ExpandedPuzzleStatus)}.{nameof(ExpandedPuzzleStatus.Reset)}. The kind of puzzle comes from the current {nameof(IPuzzleState)}, not from arguments.")]
public partial class ExpandedPuzzleStatus
{
    [HarmonyPatch(typeof(PuzzleStatus))]
    private static class Patch
    {
        [HarmonyPatch(nameof(PuzzleStatus.Reset), [typeof(List<GirlDefinition>), typeof(bool)])]
        [HarmonyPrefix]
        private static bool Reset(PuzzleStatus __instance, List<GirlDefinition> girlList)
        {
            ExpandedPuzzleStatus.Get(__instance).Reset(girlList);
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.NextRound))]
        [HarmonyPrefix]
        private static bool NextRound(PuzzleStatus __instance, int staminaOverride, bool checkStaminaFreeze)
        {
            ExpandedPuzzleStatus.Get(__instance).NextRound(staminaOverride, checkStaminaFreeze);
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.PopulateAilments))]
        [HarmonyPrefix]
        private static bool PopulateAilments(PuzzleStatus __instance)
        {
            var random = ExpandedPuzzleStatus.Get(__instance).Rules.PopulatesRandomAilments;
            __instance.girlStatusLeft.PopulateAilments(random);
            __instance.girlStatusRight.PopulateAilments(random);
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.initialMovesRemaining), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool InitialMovesRemaining(PuzzleStatus __instance, ref int __result)
        {
            __result = ExpandedPuzzleStatus.Get(__instance).Rules.GetInitialMoves(Game.Persistence.playerFile.settingDifficulty);
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.maxMovesRemaining), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool MaxMovesRemaining(PuzzleStatus __instance, ref int __result)
        {
            __result = ExpandedPuzzleStatus.Get(__instance).Rules.GetMaxMoves(Game.Persistence.playerFile.settingDifficulty);
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.movesRemaining), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool MovesRemaining(PuzzleStatus __instance, int value)
        {
            var statusExp = ExpandedPuzzleStatus.Get(__instance);
            statusExp._movesRemaining = Mathf.Clamp(value, statusExp.Rules.MinMovesRemaining, __instance.maxMovesRemaining);
            return false;
        }

        // The deprecated members keep answering for base game code and for mods that still ask. They answer from the puzzle state.
        [HarmonyPatch(nameof(PuzzleStatus.statusType), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool StatusType(ref PuzzleStatusType __result)
        {
            __result = (ModInterface.GameState.CurrentState as IPuzzleState)?.ConditionDateType ?? PuzzleStatusType.NORMAL;
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.IsTutorial))]
        [HarmonyPrefix]
        private static bool IsTutorial(bool full, ref bool __result)
        {
            __result = (ModInterface.GameState.CurrentState as IPuzzleState)?.MatchesTutorialCondition(full) ?? false;
            return false;
        }

        [HarmonyPatch(nameof(PuzzleStatus.Clear))]
        [HarmonyPrefix]
        private static void Clear(PuzzleStatus __instance) 
            => ExpandedPuzzleStatus.Get(__instance).Clear();

        [HarmonyPatch(nameof(PuzzleStatus.EnableAilments))]
        [HarmonyPrefix]
        private static bool EnableAilments(PuzzleStatus __instance) 
            => ExpandedPuzzleStatus.Get(__instance).EnableAilments_Prefix();

        [HarmonyPatch(nameof(PuzzleStatus.AddResourceValue))]
        [HarmonyPrefix]
        private static bool AddResourceValue(PuzzleStatus __instance, PuzzleResourceType resourceType, int value, bool altGirl)
            => ExpandedPuzzleStatus.Get(__instance).AddResourceValue_Prefix(resourceType, value, altGirl);

        [HarmonyPatch(nameof(PuzzleStatus.GetResourceValue))]
        [HarmonyPrefix]
        private static bool GetResourceValue(PuzzleStatus __instance, PuzzleResourceType resourceType, bool altGirl, bool maxVal, ref int __result)
            => ExpandedPuzzleStatus.Get(__instance).GetResourceValue_Prefix(resourceType, altGirl, maxVal, ref __result);

        [HarmonyPatch(nameof(PuzzleStatus.ReviveGirls))]
        [HarmonyPrefix]
        private static void ReviveGirls_Pre(PuzzleStatus __instance)
            => ExpandedPuzzleStatus.Get(__instance).ReviveGirls_Pre();

        [HarmonyPatch(nameof(PuzzleStatus.ReviveGirls))]
        [HarmonyPostfix]
        private static void ReviveGirls_Post(PuzzleStatus __instance)
            => ExpandedPuzzleStatus.Get(__instance).ReviveGirls_Post();

        [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocus))]
        [HarmonyPrefix]
        private static void SetGirlFocus(PuzzleStatus __instance, ref bool altGirl)
            => altGirl = ExpandedPuzzleStatus.Get(__instance).RulesOrNull?.LockedAltGirlFocus ?? altGirl;

        // SetGirlFocusByStamina writes the field directly, it doesn't go through SetGirlFocus
        [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocusByStamina))]
        [HarmonyPostfix]
        private static void SetGirlFocusByStamina(PuzzleStatus __instance)
            => ExpandedPuzzleStatus.Get(__instance).ApplyFocusLock();
    }

    private IPuzzleStatusRules RulesOrNull => _rulesOverride ?? ModInterface.GameState.CurrentState as IPuzzleStatusRules;

    private void ApplyFocusLock()
    {
        if (RulesOrNull?.LockedAltGirlFocus is bool locked) _altGirlFocused = locked;
    }

    /// <summary>
    /// The rules of the puzzle that is running, from the current puzzle state.
    /// </summary>
    internal IPuzzleStatusRules Rules
        => _rulesOverride
        ?? ModInterface.GameState.CurrentState as IPuzzleStatusRules
        ?? throw new InvalidOperationException("The puzzle status needs a puzzle state to know the rules of the puzzle.");

    // Only for a status built outside of a puzzle state, which has no puzzle state to ask.
    private IPuzzleStatusRules _rulesOverride;

    /// <summary>
    /// Builds the status for a puzzle of the current puzzle state's kind.
    /// </summary>
    public void Reset(GirlDefinition girlDefLeft, GirlDefinition girlDefRight)
        => Reset(new List<GirlDefinition> { girlDefLeft, girlDefRight });

    /// <summary>
    /// Builds the status with the given rules, for the states that are not puzzle states but still need the girls' status
    /// (the sim state builds a normal date's status for the pair the player is with).
    /// </summary>
    public void Reset(IPuzzleStatusRules rules, GirlDefinition girlDefLeft, GirlDefinition girlDefRight)
    {
        Reset(new List<GirlDefinition> { girlDefLeft, girlDefRight }, rules);
    }

    /// <summary>
    /// Builds the status for a puzzle of the current puzzle state's kind, played by <paramref name="girlList"/>.
    /// </summary>
    public void Reset(List<GirlDefinition> girlList) => Reset(girlList, null);

    private void Reset(List<GirlDefinition> girlList, IPuzzleStatusRules rulesOverride)
    {
        _core.Clear();
        _rulesOverride = rulesOverride;
        var rules = Rules;

        var tokens = Game.Data.Tokens.GetAll();
        for (int i = 0; i < tokens.Count; i++)
        {
            _tokenStatus.Add(new PuzzleStatusToken(_core, tokens[i]));
        }

        _girlList.AddRange(girlList);
        _alphaDateIndex = Game.Persistence.playerFile.alphaDateCount;
        _movesRemaining = _core.initialMovesRemaining;
        _sentimentWeightOffset = 0;
        NextRound(4, rules.ChecksStaminaFreezeOnStart);
    }

    /// <summary>
    /// Starts the next round: picks the girls, rebuilds their status, and sets what the round asks for.
    /// </summary>
    public void NextRound(int staminaOverride = -1, bool checkStaminaFreeze = false)
    {
        var rules = Rules;

        _roundIndex++;
        _bonusRound = rules.IsBonusRound(_roundIndex, _girlList.Count);

        if (_girlList.Count >= 2)
        {
            var roundGirls = new GirlDefinition[2];
            rules.TakeRoundGirls(_girlList, roundGirls);

            var left = _playerFile.GetPlayerFileGirl(roundGirls[0]);
            var right = _playerFile.GetPlayerFileGirl(roundGirls[1]);
            _girlStatusLeft = new PuzzleStatusGirl(left, false, GetStartStamina(left, staminaOverride, checkStaminaFreeze));
            _girlStatusRight = new PuzzleStatusGirl(right, true, GetStartStamina(right, staminaOverride, checkStaminaFreeze));
            _altGirlFocused = rules.LockedAltGirlFocus ?? MathUtils.RandomBool();
        }
        else
        {
            _girlStatusLeft.Reset(GetStartStamina(_girlStatusLeft.playerFileGirl, staminaOverride, checkStaminaFreeze));
            _girlStatusRight.Reset(GetStartStamina(_girlStatusRight.playerFileGirl, staminaOverride, checkStaminaFreeze));
        }

        rules.GetRoundGoal(_roundIndex, _bonusRound, out var affectionGoal, out var bonusDrainDelay);
        _affectionGoal = affectionGoal;
        _bonusDrainDelay = bonusDrainDelay;
        _affection = 0;
    }

    // Bonus rounds always start fresh. Other rounds start with the stamina asked for, or the one the girl has frozen.
    private int GetStartStamina(PlayerFileGirl girl, int staminaOverride, bool checkStaminaFreeze)
        => _bonusRound
            ? 6
            : (checkStaminaFreeze && girl.staminaFreeze >= 0) ? girl.staminaFreeze : staminaOverride;

    public bool SuppressStateDialog {get;set;}

    private void ReviveGirls_Pre()
    {
        SuppressStateDialog = true;
    }

    private void ReviveGirls_Post()
    {
        _core.girlStatusLeft?.GetExpansion().OnRevived();
        _core.girlStatusRight?.GetExpansion().OnRevived();

        SuppressStateDialog = false;
    }

    private void Clear()
    {
        _rulesOverride = null;
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }

    private bool EnableAilments_Prefix()
    {
        var ailmentManagerExp = Game.Session.Ailment.GetExpansion();
        foreach (var ailment in _girlStatusLeft.ailments.Concat(_girlStatusRight.ailments))
        {
            ailmentManagerExp.Enable(ailment, _girlStatusLeft);
        }

        return false;
    }

    public void AddResourceValue(RelativeId resourceId, int value, bool altGirl)
    {
        if (value == 0) return;
        var statusGirl = _core.GetStatusGirl(altGirl);
        var resource = ModInterface.GameData.GetPuzzleResource(resourceId);
        if (resource != null && resource.AddResourceValue(value, this, statusGirl))
        {
            _core.resourceChanged = true;
        }
    }

    public int GetResourceValue(RelativeId resourceId, bool altGirl, bool maxVal = false)
    {
        var statusGirl = _core.GetStatusGirl(altGirl);

        if (resourceId == PuzzleResourceId.AffectionTalent ||
            resourceId == PuzzleResourceId.AffectionFlirtation ||
            resourceId == PuzzleResourceId.AffectionRomance ||
            resourceId == PuzzleResourceId.AffectionSexuality)
        {
            return maxVal ? _core.affectionGoal : _core.affection;
        }
        if (resourceId == PuzzleResourceId.Moves)
        {
            return maxVal ? _core.maxMovesRemaining : _core.movesRemaining;
        }
        if (resourceId == PuzzleResourceId.Stamina)
        {
            return maxVal ? 6 : statusGirl.stamina;
        }
        if (resourceId == PuzzleResourceId.Passion)
        {
            return maxVal ? 100 : statusGirl.passion;
        }
        if (resourceId == PuzzleResourceId.Sentiment)
        {
            return maxVal ? 40 : statusGirl.sentiment;
        }

        return 0;
    }

    private bool AddResourceValue_Prefix(PuzzleResourceType resourceType, int value, bool altGirl)
    {
        if (value == 0) return false;

        var statusGirl = _core.GetStatusGirl(altGirl);
        RelativeId resourceId = resourceType == PuzzleResourceType.AFFECTION
            ? PuzzleResourceId.AffectionTalent
            : PuzzleResourceId.From(resourceType);

        var customResource = ModInterface.GameData.GetPuzzleResource(resourceId);
        if (customResource != null && customResource.AddResourceValue(value, this, statusGirl))
        {
            _core.resourceChanged = true;
            return false;
        }

        return true;
    }

    private bool GetResourceValue_Prefix(PuzzleResourceType resourceType, bool altGirl, bool maxVal, ref int __result)
    {
        RelativeId resourceId = resourceType == PuzzleResourceType.AFFECTION
            ? PuzzleResourceId.AffectionTalent
            : PuzzleResourceId.From(resourceType);

        __result = GetResourceValue(resourceId, altGirl, maxVal);
        return false;
    }

    private void OnDestroy()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }
}