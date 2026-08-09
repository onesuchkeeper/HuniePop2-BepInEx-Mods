using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Exposes events called when ailment triggers occur.
/// Forces all enable/disabling of ailments to happen through
/// this manager so they may be notified and canceled if needed.
/// </summary>
[Expansion(typeof(AilmentManager))]
public partial class ExpandedAilmentManager
{
    [HarmonyPatch(typeof(AilmentManager))]
    private static class AilmentManagerPatch
    {
        [HarmonyPatch("TriggerAilment")]
        [HarmonyPostfix]
        public static void TriggerAilment(AilmentManager __instance, AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
            => ExpandedAilmentManager.Get(__instance).TriggerAilment_Postfix(triggerType, ailment, girlStatus, unfocused);

        [HarmonyPatch("Execute")]
        [HarmonyPrefix]
        public static bool Execute(AilmentManager __instance, Ailment ailment, AilmentTrigger ailmentTrigger, PuzzleStatusGirl girlStatus)
            => ExpandedAilmentManager.Get(__instance).Execute_Prefix(ailment, ailmentTrigger, girlStatus);
    }

    /// <summary>
    /// Called before a match is made on the puzzle grid
    /// </summary>
    public event Action<AilmentTriggerArgs.PreMatch> PreMatch;
    public event Action<AilmentTriggerArgs.PreMatch> PostMatch;

    public event Action<AilmentTriggerArgs.PreMove> PreMove;
    public event Action<AilmentTriggerArgs.PostMove> PostMove;

    public event Action<AilmentTriggerArgs.PreGift> PreGift;
    public event Action<AilmentTriggerArgs.PostGift> PostGift;

    public event Action<AilmentTriggerArgs.PreFocusSwitch> PreFocusSwitch;
    public event Action<AilmentTriggerArgs.PostFocusSwitch> PostFocusSwitch;

    public event Action<AilmentTriggerArgs.PreExhaust> PreExhaust;
    public event Action<AilmentTriggerArgs.PostExhaust> PostExhaust;
    
    public event Action<AilmentTriggerArgs.PostSettled> Settled;
    public event Action<AilmentTriggerArgs.SettledPure> SettledPure;
    public event Action<AilmentTriggerArgs.RoundSettled> RoundSettled;
    public event Action<AilmentTriggerArgs.RoundStart> RoundStarted;
    public event Action<AilmentTriggerArgs.PuzzleEnded> PuzzleEnded;

    public event Action<AilmentTriggerArgs.ResourceChanged> ResourceChanged;

    public event Action<AilmentTriggerArgs.PreConsume> PreConsume;
    public event Action<AilmentTriggerArgs.PostConsume> PostConsume;

    public event Action<AilmentTriggerArgs.PreMatchReward> PreMatchReward;
    public event Action<AilmentTriggerArgs.PreAffectionMatchReward> PreAffectionMatchReward;
    public event Action<AilmentTriggerArgs.PostMatchReward> PostMatchReward;

    public event Action<AilmentTriggerArgs.PreAilmentEnable> PreAilmentEnable;
    public event Action<AilmentTriggerArgs.PostAilmentEnable> PostAilmentEnable;

    public event Action<AilmentTriggerArgs.PreAilmentDisable> PreAilmentDisable;
    public event Action<AilmentTriggerArgs.PostAilmentDisable> PostAilmentDisable;
    public event Action<AilmentTriggerArgs.CheckRoundOver> CheckRoundOver;

    // Modifiers registered before StartPuzzle and applied/removed automatically.
    private readonly List<AilmentDefinition> _pendingGlobals
        = new List<AilmentDefinition>();

    // Global ailments that are not owned by a girl status. Used for custom
    // puzzle behaviour
    private readonly Dictionary<AilmentDefinition, Ailment> _activeGlobals
        = new Dictionary<AilmentDefinition, Ailment>();

    public void AddGlobal(RelativeId ailmentId) 
        => AddGlobal(ModInterface.GameData.GetAilment(ailmentId));

    /// <summary>
    /// Registers a global ailment to be applied when the next puzzle starts.
    /// If called after StartPuzzle has already run, the modifier is applied immediately.
    /// Safe to call multiple times with the same instance, duplicates are ignored.
    /// </summary>
    public void AddGlobal(AilmentDefinition ailmentDefinition)
    {
        if (ailmentDefinition == null)
        {
            ModInterface.Log.Error($"Null modifier added.");
            return;
        }

        if (_pendingGlobals.Contains(ailmentDefinition))
        {
            ModInterface.Log.Warning($"Duplicate modifier {ailmentDefinition.ModId()} added. Discarding.");
            return;
        }

        // If the puzzle is already running, apply immediately.
        var status = _puzzleStatus;
        if (status != null && !status.isEmpty && Game.Session.Puzzle.isPuzzleActive)
        {
            ApplyGlobal(ailmentDefinition);
        }
        else
        {
            _pendingGlobals.Add(ailmentDefinition);
        }
    }

    private void ApplyGlobal(AilmentDefinition ailmentDefinition)
    {
        if (_activeGlobals.ContainsKey(ailmentDefinition))
        {
            ModInterface.Log.Warning($"Attempted to apply duplicate global ailment {ailmentDefinition.ModId()}. Discarding.");
            return;
        }

        var newAilment = new Ailment(ailmentDefinition);
        _activeGlobals[ailmentDefinition] = newAilment;

        Enable(newAilment, this);
    }

    /// <summary>
    /// Removes a modifier. If the puzzle is active, OnRemove is called immediately.
    /// If the modifier is still pending (puzzle not yet started), it is simply discarded.
    /// </summary>
    public void RemoveGlobal(AilmentDefinition ailmentDefinition)
    {
        if (_pendingGlobals.Remove(ailmentDefinition)) return;

        if (!_activeGlobals.TryGetValue(ailmentDefinition, out var ailment))
        {
            ModInterface.Log.Warning($"Attempted to remove global ailment, {ailmentDefinition.ModId()}, but that ailment wasn't added as a global.");
            return;
        }

        // we treat remove global as an absolute removal that isn't able to be canceled
        // so we don't call PreAilmentDisable
        ailment.GetExpansion().Disable();
        PostAilmentDisable?.Invoke(new AilmentTriggerArgs.PostAilmentDisable(ailment, _puzzleGrid.GetExpansion(), null));
        _activeGlobals.Remove(ailmentDefinition);
    }

    public bool Enable(Ailment ailment, object owner = null)
    {
        var args = new AilmentTriggerArgs.PreAilmentEnable(ailment, _puzzleGrid.GetExpansion(), owner as PuzzleStatusGirl);
        PreAilmentEnable?.Invoke(args);
        if (args.Cancel) return false;
        ailment.GetExpansion().Enable(this, owner);
        return true;
    }

    public bool Disable(Ailment ailment)
    {
        var args = new AilmentTriggerArgs.PreAilmentDisable(ailment, _puzzleGrid.GetExpansion(), null);
        PreAilmentDisable?.Invoke(args);
        if (args.Cancel) return false;
        ailment.GetExpansion().Disable();
        PostAilmentDisable?.Invoke(new AilmentTriggerArgs.PostAilmentDisable(ailment, _puzzleGrid.GetExpansion(), null));
        return true;
    }

    /// <summary>
    /// Fully overridden to prevent the use of Ailment.Enable and Ailment.Disable
    /// All enables and disables must go through this manager's Enable and Disable instead
    /// so they may be canceled if needed.
    /// </summary>
    /// <param name="ailment"></param>
    /// <param name="ailmentTrigger"></param>
    /// <param name="girlStatus"></param>
    /// <returns></returns>
    private bool Execute_Prefix(Ailment ailment, AilmentTrigger ailmentTrigger, PuzzleStatusGirl girlStatus)
	{
		var subDefinition = ailmentTrigger.subDefinition; 
		bool executeSucceeded = false;
        int i = 0;
		foreach (var ailmentStepSubDefinition in subDefinition.steps)
		{
			bool stepSucceeded = true;
			switch (ailmentStepSubDefinition.stepType)
			{
			case AilmentStepType.ABILITY:
				if (!Game.Session.Ability.PerformAbility(ailmentStepSubDefinition.abilityDefinition, girlStatus.altGirl, ailmentStepSubDefinition.boolValue ? ailment.flags : null))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.CHECK_MOVE:
				if (!IsMoveConditionListMet(ailmentStepSubDefinition.moveConditions, ailmentStepSubDefinition.boolValue, girlStatus))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.MODIFY_MOVE:
				if (ailmentStepSubDefinition.moveModifier.blockMoveCost)
				{
					_moveModifier.blockMoveCost = ailmentStepSubDefinition.moveModifier.blockMoveCost;
				}
				if (ailmentStepSubDefinition.moveModifier.blockStaminaCost)
				{
					_moveModifier.blockStaminaCost = ailmentStepSubDefinition.moveModifier.blockStaminaCost;
				}
				if (ailmentStepSubDefinition.moveModifier.blockStaminaRecover)
				{
					_moveModifier.blockStaminaRecover = ailmentStepSubDefinition.moveModifier.blockStaminaRecover;
				}
				if (ailmentStepSubDefinition.moveModifier.postSwitchGirlFocus)
				{
					_moveModifier.postSwitchGirlFocus = ailmentStepSubDefinition.moveModifier.postSwitchGirlFocus;
				}
				break;
			case AilmentStepType.CHECK_MATCH:
				if (!IsMatchConditionListMet(ailmentStepSubDefinition.matchConditions, ailmentStepSubDefinition.boolValue, girlStatus))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.MODIFY_MATCH:
				if (ailmentStepSubDefinition.matchModifier.absorb)
				{
					if (!_matchModifier.absorb)
					{
						_matchModifier.absorb = true;
						_matchModifier.absorbAltGirl = (ailmentStepSubDefinition.matchModifier.absorbAltGirl ? (!girlStatus.altGirl) : girlStatus.altGirl);
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.tokenDefinition != null)
				{
					if (_matchModifier.tokenDefinition == null)
					{
						_matchModifier.tokenDefinition = ailmentStepSubDefinition.matchModifier.tokenDefinition;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.replaceDefinition != null)
				{
					if (_matchModifier.replaceDefinition == null)
					{
						_matchModifier.replaceDefinition = ailmentStepSubDefinition.matchModifier.replaceDefinition;
						_matchModifier.replacePriority = ailmentStepSubDefinition.matchModifier.replacePriority;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.skipMostFavFactor)
				{
					if (!_matchModifier.skipMostFavFactor)
					{
						_matchModifier.skipMostFavFactor = true;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.skipLeastFavFactor)
				{
					if (!_matchModifier.skipLeastFavFactor)
					{
						_matchModifier.skipLeastFavFactor = true;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.pointsOp)
				{
					if (!_matchModifier.pointsOp)
					{
						_matchModifier.pointsOp = true;
						_matchModifier.pointsOperation = ailmentStepSubDefinition.matchModifier.pointsOperation;
						_matchModifier.pointsFactor = ailmentStepSubDefinition.matchModifier.pointsFactor;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				else if (ailmentStepSubDefinition.matchModifier.pointsOp2)
				{
					if (!_matchModifier.pointsOp2)
					{
						_matchModifier.pointsOp2 = true;
						_matchModifier.pointsOperation2 = ailmentStepSubDefinition.matchModifier.pointsOperation2;
						_matchModifier.pointsFactor2 = ailmentStepSubDefinition.matchModifier.pointsFactor2;
					}
					else
					{
						stepSucceeded = false;
					}
				}
				break;
			case AilmentStepType.CHECK_GIFT:
				if (!IsGiftConditionListMet(ailmentStepSubDefinition.giftConditions, ailmentStepSubDefinition.boolValue))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.MODIFY_GIFT:
				if (ailmentStepSubDefinition.giftModifier.blockGift)
				{
					_giftModifier.blockGift = ailmentStepSubDefinition.giftModifier.blockGift;
				}
				break;
			case AilmentStepType.DISABLE_TRIGGER:
				ailment.triggers[ailmentStepSubDefinition.intValue].Disable();
				break;
			case AilmentStepType.ENABLE_TRIGGER:
				ailment.triggers[ailmentStepSubDefinition.intValue].Enable();
				break;
			case AilmentStepType.RESET_TRIGGER:
				ailment.triggers[ailmentStepSubDefinition.intValue].Reset();
				break;
			case AilmentStepType.DISABLE_AILMENT:
                Disable(ailment);
				break;
			case AilmentStepType.ENABLE_AILMENT:
                Enable(ailment);
				break;
			case AilmentStepType.RESET_AILMENT:
				ailment.ResetTriggers();
				break;
			case AilmentStepType.SWITCH_FOCUS:
				_puzzleGrid.AttemptGirlFocusSwitch();
				break;
			case AilmentStepType.CHECK_GIRL:
				if (!IsGirlConditionListMet(ailment, ailmentStepSubDefinition.girlConditions, ailmentStepSubDefinition.boolValue))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.CHECK_FLAG:
				if (!IsFlagCheckMet(ailment, ailmentStepSubDefinition))
				{
					stepSucceeded = false;
				}
				break;
			case AilmentStepType.SET_FLAG:
				SetFlag(ailment, ailmentStepSubDefinition);
				break;
			case AilmentStepType.REMOVE_AILMENT:
				ailment.GetExpansion().Disable();
                PostAilmentDisable?.Invoke(new AilmentTriggerArgs.PostAilmentDisable(ailment, _puzzleGrid.GetExpansion(), girlStatus));
				girlStatus.ailments.Remove(ailment);
				break;
			}
            
			if (subDefinition.stepsProcessType == AilmentTriggerStepsProcessType.ALL_FORCE)
			{
				stepSucceeded = true;
			}

			if (stepSucceeded)
			{
				executeSucceeded = true;
				if (subDefinition.stepsProcessType == AilmentTriggerStepsProcessType.FIRST_SUCCESS)
				{
					break;
				}

				if (subDefinition.stepsProcessType == AilmentTriggerStepsProcessType.BAIL_EARLY_ON_SUCCESS 
                    && i < subDefinition.steps.Count - 1)
				{
					executeSucceeded = false;
					break;
				}
			}
			else if (subDefinition.stepsProcessType == AilmentTriggerStepsProcessType.BAIL_ON_FAIL 
                || (subDefinition.stepsProcessType == AilmentTriggerStepsProcessType.BAIL_ON_FAIL_FIRST_ONLY && i == 0))
			{
				executeSucceeded = false;
				break;
			}
            i++;
		}

		if (executeSucceeded)
		{
			int executeCount = ailmentTrigger.executeCount;
			ailmentTrigger.executeCount = executeCount + 1;
			if (subDefinition.triggerType == AilmentTriggerType.ON_SETTLED || subDefinition.triggerType == AilmentTriggerType.ON_FOCUS)
			{
				_puzzleStatus.CheckChanges();
			}
			if (girlStatus.girlDefinition.baggageItemDefs.Contains(ailment.definition.itemDefinition) && subDefinition.verbalized)
			{
				UiDoll doll = Game.Session.gameCanvas.GetDoll(girlStatus.altGirl);
				if (!doll.soulGirlDefinition.specialCharacter)
				{
					doll.ReadDialogTrigger(Game.Session.Puzzle.dtBaggages[Mathf.Clamp(girlStatus.girlDefinition.baggageItemDefs.IndexOf(ailment.definition.itemDefinition), 0, girlStatus.girlDefinition.baggageItemDefs.Count - 1)], DialogLineFormat.UNCHECKED, (subDefinition.verbalizedIndex >= 0) ? subDefinition.verbalizedIndex : (-1));
					return false;
				}
				doll.ReadDialogTrigger(Game.Session.Puzzle.dtBaggages[0], DialogLineFormat.UNCHECKED, -1);
			}
		}

        return false;
	}

    internal void TriggerAilment_Postfix(AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
    {
        switch (triggerType)
        {
            case AilmentTriggerType.PRE_MATCH:
                PreMatch?.Invoke(new AilmentTriggerArgs.PreMatch(_match, _matchModifier, _puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.PRE_GIFT:
                PreGift?.Invoke(new AilmentTriggerArgs.PreGift(_giftModifier, _puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.POST_GIFT:
                PostGift?.Invoke(new AilmentTriggerArgs.PostGift(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.PRE_MOVE:
                PreMove?.Invoke(new AilmentTriggerArgs.PreMove(_moveModifier, _puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.POST_MOVE:
                PostMove?.Invoke(new AilmentTriggerArgs.PostMove(_puzzleGrid.GetExpansion(), _moveModifier, girlStatus));
                break;
            case AilmentTriggerType.ON_SETTLED:
                Settled?.Invoke(new AilmentTriggerArgs.PostSettled(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_FOCUS:
                PostFocusSwitch?.Invoke(new AilmentTriggerArgs.PostFocusSwitch(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_RESOURCE_CHANGED:
                ResourceChanged?.Invoke(new AilmentTriggerArgs.ResourceChanged(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_ROUND_SETTLED:
                RoundSettled?.Invoke(new AilmentTriggerArgs.RoundSettled(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_SETTLED_PURE:
                SettledPure?.Invoke(new AilmentTriggerArgs.SettledPure(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_EXHAUSTION:
                PostExhaust?.Invoke(new AilmentTriggerArgs.PostExhaust(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.PRE_CONSUME:
                PreConsume?.Invoke(new AilmentTriggerArgs.PreConsume(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.POST_CONSUME:
                PostConsume?.Invoke(new AilmentTriggerArgs.PostConsume(_puzzleGrid.GetExpansion(), girlStatus));
                break;
            case AilmentTriggerType.ON_ROUND_START:
                OnRoundStart();
                break;
            case AilmentTriggerType.ON_AILMENT_ENABLED:
                PostAilmentEnable?.Invoke(new AilmentTriggerArgs.PostAilmentEnable(ailment, _puzzleGrid.GetExpansion(), girlStatus));
                break;
        }
    }

    private void OnRoundStart()
    {
        foreach(var pendingGlobal in _pendingGlobals)
        {
            ApplyGlobal(pendingGlobal);
        }
        _pendingGlobals.Clear();

        RoundStarted?.Invoke(new AilmentTriggerArgs.RoundStart(_puzzleGrid.GetExpansion(), null));
    }

    /// <summary>
    /// Called by the UiPuzzleGrid when attempting to switch focus
    /// </summary>
    /// <returns>True if switching focus is allowed, false otherwise.</returns>
    internal bool OnPreFocusSwitch()
    {
        var args = new AilmentTriggerArgs.PreFocusSwitch(_puzzleGrid.GetExpansion(), null);
        PreFocusSwitch?.Invoke(args);
        return !args.Canceled;
    }

    /// <summary>
    /// Called by the UiPuzzleGrid when seeing if a round is over.
    /// </summary>
    /// <returns>True if round over is allowed, false otherwise.</returns>
    internal void OnCheckRoundOver(AilmentTriggerArgs.CheckRoundOver args)
        => CheckRoundOver?.Invoke(args);

    /// <summary>
    /// Called by the PuzzleManager when the puzzle ends
    /// </summary>
    internal void OnEndPuzzle()
    {
        PuzzleEnded?.Invoke(new AilmentTriggerArgs.PuzzleEnded(_puzzleGrid.GetExpansion(), null));

        foreach (var global in _activeGlobals.Keys.ToList())
        {
            RemoveGlobal(global);
        }
    }

    internal void OnPreMatchReward(AilmentTriggerArgs.PreMatchReward args)
    {
        PreMatchReward?.Invoke(args);
    }

    internal void OnPreAffectionMatchReward(AilmentTriggerArgs.PreAffectionMatchReward args)
    {
        PreAffectionMatchReward?.Invoke(args);
    }

    internal void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        PostMatchReward?.Invoke(args);
    }
}