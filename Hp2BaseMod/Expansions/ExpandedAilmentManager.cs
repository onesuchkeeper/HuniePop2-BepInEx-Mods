using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod
{
    /// <summary>
    /// Exposes events called when ailment triggers occur.
    /// Forces all enable/disabling of ailments to happen through
    /// this manager so they may be notified and canceled if needed.
    /// </summary>
    [Expansion(typeof(AilmentManager))]
    public partial class ExpandedAilmentManager
    {
        [HarmonyPatch(typeof(AilmentManager))]
        private static class Patch
        {
            [HarmonyPatch("TriggerAilment")]
            [HarmonyPostfix]
            private static void TriggerAilment(AilmentManager __instance, AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
                => ExpandedAilmentManager.Get(__instance).TriggerAilment_Postfix(triggerType, ailment, girlStatus, unfocused);

            [HarmonyPatch("Execute")]
            [HarmonyPrefix]
            private static bool Execute(AilmentManager __instance, Ailment ailment, AilmentTrigger ailmentTrigger, PuzzleStatusGirl girlStatus)
                => ExpandedAilmentManager.Get(__instance).Execute_Prefix(ailment, ailmentTrigger, girlStatus);

            [HarmonyPatch("IsMatchConditionListMet")]
            [HarmonyPrefix]
            private static bool IsMatchConditionListMet(AilmentManager __instance, List<MatchCondition> matchConditions, bool orCheck, PuzzleStatusGirl girlStatus, ref bool __result)
                => ExpandedAilmentManager.Get(__instance).IsMatchConditionListMet_Prefix(matchConditions, orCheck, girlStatus, ref __result);

            [HarmonyPatch("IsMoveConditionListMet")]
            [HarmonyPrefix]
            private static bool IsMoveConditionListMet(AilmentManager __instance, List<MoveCondition> moveConditions, bool orCheck, PuzzleStatusGirl girlStatus, ref bool __result)
                => ExpandedAilmentManager.Get(__instance).IsMoveConditionListMet_Prefix(moveConditions, orCheck, girlStatus, ref __result);

            [HarmonyPatch("IsGirlConditionListMet")]
            [HarmonyPrefix]
            private static bool IsGirlConditionListMet(AilmentManager __instance, Ailment ailment, List<GirlCondition> girlConditions, bool orCheck, ref bool __result)
                => ExpandedAilmentManager.Get(__instance).IsGirlConditionListMet_Prefix(ailment, girlConditions, orCheck, ref __result);
        }

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

        public event Action<AilmentTriggerArgs.MoveWarningArgs> QueryMoveWarning;

        private readonly List<AilmentDefinition> _pendingGlobals = new List<AilmentDefinition>();
        private readonly Dictionary<AilmentDefinition, Ailment> _activeGlobals = new Dictionary<AilmentDefinition, Ailment>();

        public void AddGlobal(RelativeId ailmentId)
            => AddGlobal(ModInterface.GameData.GetAilment(ailmentId));
        public void AddGlobal(AilmentDefinition ailmentDefinition)
        {
            if (ailmentDefinition == null)
            {
                ModInterface.Log.Error("Null modifier added.");
                return;
            }

            if (_pendingGlobals.Contains(ailmentDefinition))
            {
                ModInterface.Log.Warning($"Duplicate modifier {ailmentDefinition.ModId()} added. Discarding.");
                return;
            }

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

        public void RemoveGlobal(AilmentDefinition ailmentDefinition)
        {
            if (_pendingGlobals.Remove(ailmentDefinition)) return;

            if (!_activeGlobals.TryGetValue(ailmentDefinition, out var ailment))
            {
                ModInterface.Log.Warning($"Attempted to remove global ailment, {ailmentDefinition.ModId()}, but that ailment wasn't added as a global.");
                return;
            }

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

        private void TriggerAilment_Postfix(AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
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
            foreach (var pendingGlobal in _pendingGlobals)
            {
                ApplyGlobal(pendingGlobal);
            }
            _pendingGlobals.Clear();

            RoundStarted?.Invoke(new AilmentTriggerArgs.RoundStart(_puzzleGrid.GetExpansion(), null));
        }

        internal bool OnPreFocusSwitch()
        {
            var args = new AilmentTriggerArgs.PreFocusSwitch(_puzzleGrid.GetExpansion(), null);
            PreFocusSwitch?.Invoke(args);
            return !args.Canceled;
        }

        internal void OnCheckRoundOver(AilmentTriggerArgs.CheckRoundOver args)
            => CheckRoundOver?.Invoke(args);

        internal void OnEndPuzzle()
        {
            PuzzleEnded?.Invoke(new AilmentTriggerArgs.PuzzleEnded(_puzzleGrid.GetExpansion(), null));

            foreach (var global in _activeGlobals.Keys.ToList())
            {
                RemoveGlobal(global);
            }
        }

        internal void OnPreMatchReward(AilmentTriggerArgs.PreMatchReward args) => PreMatchReward?.Invoke(args);

        public void OnPreAffectionMatchReward(AilmentTriggerArgs.PreAffectionMatchReward args) => PreAffectionMatchReward?.Invoke(args);

        internal void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args) => PostMatchReward?.Invoke(args);
        internal void NotifyQueryMoveWarning(AilmentTriggerArgs.MoveWarningArgs args) => QueryMoveWarning?.Invoke(args);

        private bool IsGirlConditionListMet_Prefix(Ailment ailment, List<GirlCondition> girlConditions, bool orCheck, ref bool __result)
        {
            PuzzleStatusGirl puzzleStatusGirl = (_puzzleStatus.girlStatusLeft.ailments.Contains(ailment) ? _puzzleStatus.girlStatusLeft : _puzzleStatus.girlStatusRight);
            PuzzleStatusGirl statusGirl = _puzzleStatus.GetStatusGirl(!puzzleStatusGirl.altGirl);
            int num = 0;
            for (int i = 0; i < girlConditions.Count; i++)
            {
                GirlCondition girlCondition = girlConditions[i];
                bool isConditionMet = false;
                var conditionStatusGirl = ((!girlCondition.otherGirl) ? puzzleStatusGirl : statusGirl);
                var conditionStatusGirlExp = conditionStatusGirl.GetExpansion();

                switch (girlCondition.type)
                {
                    case GirlConditionType.FOCUSED:
                        isConditionMet = conditionStatusGirl == _puzzleStatus.girlStatusFocused;
                        break;
                    case GirlConditionType.EXHAUSTED:
                    case GirlConditionType.UPSET:
                    case GirlConditionType.EXHAUSTED_OR_UPSET:
                        isConditionMet = conditionStatusGirlExp.State.SatisfiesCondition(girlCondition.type);
                        break;
                    case GirlConditionType.HAS_AILMENT:
                        isConditionMet = conditionStatusGirl.HasAilment(girlCondition.ailmentDefinition);
                        break;
                }

                if (girlCondition.inverse)
                {
                    isConditionMet = !isConditionMet;
                }

                if (isConditionMet)
                {
                    num++;
                }
            }

            if ((!orCheck && num == girlConditions.Count) || (orCheck && num >= Mathf.Min(girlConditions.Count, 1)))
            {
                __result = true;
            }

            __result = false;

            return false;
        }

        private bool IsMatchConditionListMet_Prefix(
            List<MatchCondition> matchConditions, 
            bool orCheck, 
            PuzzleStatusGirl girlStatus, 
            ref bool __result)
        {
            var match = _match;
            var matchModifier = _matchModifier;

            if (match == null)
            {
                __result = false;
                return false;
            }

            var conditionMetCount = 0;

            foreach (var matchCondition in matchConditions)
            {
                var isConditionMet = false;
                var tokenDefinition = match.tokenDefinition;
                if (matchModifier != null && matchModifier.tokenDefinition != null)
                {
                    tokenDefinition = matchModifier.tokenDefinition;
                }

                var tokenExp = tokenDefinition.GetExpansion();

                switch (matchCondition.type)
                {
                    case MatchConditionType.TOKEN:
                        switch (matchCondition.tokenType)
                        {
                            case MatchConditionTokenType.DEFINITION:
                                isConditionMet = tokenDefinition == matchCondition.tokenDefinition;
                                break;

                            case MatchConditionTokenType.MOST_FAV:
                                isConditionMet = tokenExp.PuzzleResource != null && tokenExp.PuzzleResource.IsMostFav(girlStatus);
                                break;

                            case MatchConditionTokenType.LEAST_FAV:
                                isConditionMet = tokenExp.PuzzleResource != null && tokenExp.PuzzleResource.IsLeastFav(girlStatus);
                                break;

                            case MatchConditionTokenType.NO_SPAWN_MATCH:
                                isConditionMet = tokenDefinition == girlStatus.noSpawnMatchTokenDef
                                    || (matchCondition.boolValue && girlStatus.extraNoSpawnMatchTokenDefs.Contains(tokenDefinition));
                                break;
                        }
                        break;

                    case MatchConditionType.COUNT:
                        isConditionMet = MathUtils.CompareInts(matchCondition.comparison, match.slots.Count, matchCondition.val) && !match.flatMatch;
                        break;

                    case MatchConditionType.POWER:
                        isConditionMet = match.HasPowerToken();
                        break;

                    case MatchConditionType.FOCUSED:
                        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
                        isConditionMet = (puzzleStatus.altGirlFocused == girlStatus.altGirl && (matchModifier == null || !matchModifier.absorb || matchModifier.absorbAltGirl == girlStatus.altGirl))
                            || (puzzleStatus.altGirlFocused != girlStatus.altGirl && matchModifier != null && matchModifier.absorb && matchModifier.absorbAltGirl == girlStatus.altGirl);
                        break;

                    case MatchConditionType.FLAT:
                        isConditionMet = match.flatMatch;
                        break;

                    case MatchConditionType.RESOURCE:
                        var targetTokenDef = Game.Data.Tokens.GetByResourceType(matchCondition.resourceType);
                        var targetResource = targetTokenDef?.GetExpansion().PuzzleResource;
                        isConditionMet = tokenExp.PuzzleResource != null && tokenExp.PuzzleResource == targetResource;
                        break;
                }

                if (matchCondition.inverse)
                {
                    isConditionMet = !isConditionMet;
                }

                if (isConditionMet)
                {
                    conditionMetCount++;
                }
            }

            __result = (!orCheck && conditionMetCount == matchConditions.Count)
                || (orCheck && conditionMetCount >= Mathf.Min(matchConditions.Count, 1));

            return false;
        }

        private bool IsMoveConditionListMet_Prefix(
            List<MoveCondition> moveConditions, 
            bool orCheck, 
            PuzzleStatusGirl girlStatus, 
            ref bool __result)
        {
            var move = _move;

            if (move == null)
            {
                __result = false;
                return false;
            }

            var conditionMetCount = 0;

            foreach (var moveCondition in moveConditions)
            {
                var isConditionMet = false;

                switch (moveCondition.type)
                {
                    case MoveConditionType.HAS_TOKEN:
                        switch (moveCondition.tokenType)
                        {
                            case MoveConditionTokenType.DEFINITION:
                                isConditionMet = move.HasMatchWithTokenDef(moveCondition.tokenDefinition);
                                break;

                            case MoveConditionTokenType.MOST_FAV:
                                isConditionMet = move.matches.Any(m => {
                                    var res = m.tokenDefinition.GetExpansion().PuzzleResource;
                                    return res != null && res.IsMostFav(girlStatus);
                                });
                                break;

                            case MoveConditionTokenType.LEAST_FAV:
                                isConditionMet = move.matches.Any(m => {
                                    var res = m.tokenDefinition.GetExpansion().PuzzleResource;
                                    return res != null && res.IsLeastFav(girlStatus);
                                });
                                break;

                            case MoveConditionTokenType.NO_SPAWN_MATCH:
                                isConditionMet = move.HasMatchWithTokenDef(girlStatus.noSpawnMatchTokenDef)
                                    || (moveCondition.boolValue && move.HasMatchWithTokenDef(girlStatus.extraNoSpawnMatchTokenDefs));
                                break;
                        }
                        break;

                    case MoveConditionType.HAS_LENGTH:
                        isConditionMet = move.HasMatchWithLength(moveCondition.comparison, moveCondition.val);
                        break;

                    case MoveConditionType.MATCH_COUNT:
                        isConditionMet = MathUtils.CompareInts(moveCondition.comparison, move.matches.Count, moveCondition.val);
                        break;

                    case MoveConditionType.STAMINA_COST:
                        isConditionMet = MathUtils.CompareInts(moveCondition.comparison, move.GetStaminaCost(false, moveCondition.boolValue), moveCondition.val);
                        break;

                    case MoveConditionType.HAS_RESOURCE:
                        var targetTokenDef = Game.Data.Tokens.GetByResourceType(moveCondition.resourceType);
                        var targetResource = targetTokenDef?.GetExpansion().PuzzleResource;
                        isConditionMet = move.matches.Any(m => {
                            var res = m.tokenDefinition.GetExpansion().PuzzleResource;
                            return res != null && res == targetResource;
                        });
                        break;
                }

                if (moveCondition.inverse)
                {
                    isConditionMet = !isConditionMet;
                }

                if (isConditionMet)
                {
                    conditionMetCount++;
                }
            }

            __result = (!orCheck && conditionMetCount == moveConditions.Count)
                || (orCheck && conditionMetCount >= Mathf.Min(moveConditions.Count, 1));

            return false;
        }
    }
}
