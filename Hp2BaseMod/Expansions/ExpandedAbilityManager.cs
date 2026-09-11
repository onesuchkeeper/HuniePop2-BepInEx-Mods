using System.Collections.Generic;
using HarmonyLib;

namespace Hp2BaseMod;

[Expansion(typeof(AbilityManager))]
public partial class ExpandedAbilityManager
{
    [HarmonyPatch(typeof(AbilityManager))]
    private static class Patch
    {
        [HarmonyPatch("PerformAbility")]
        [HarmonyPrefix]
        private static bool PerformAbility_Prefix(
            AbilityManager __instance, 
            AbilityDefinition abilityDef, 
            bool altGirl, 
            Dictionary<string, int> insertValues, 
            ref bool __result, 
            out (IScriptedAbility Scripted, Ability Ability) __state) 
            => ExpandedAbilityManager.Get(__instance).PerformAbility_Prefix(abilityDef, altGirl, insertValues, ref __result, out __state);

        [HarmonyPatch("PerformAbility")]
        [HarmonyPostfix]
        private static void PerformAbility_Postfix(
            AbilityManager __instance, 
            AbilityDefinition abilityDef, 
            bool altGirl, 
            ref bool __result, 
            (IScriptedAbility Scripted, Ability Ability) __state) 
            => ExpandedAbilityManager.Get(__instance).PerformAbility_Postfix(abilityDef, altGirl, ref __result, __state);

        [HarmonyPatch("SlotMeetsCondition")]
        [HarmonyPrefix]
        private static bool SlotMeetsCondition(AbilityManager __instance, Ability ability, UiPuzzleSlot checkSlot, TokenCondition condition, ref bool __result)
            => ExpandedAbilityManager.Get(__instance).SlotMeetsCondition_Prefix(ability, checkSlot, condition, ref __result);

        [HarmonyPatch("IsDefineValueConditionMet")]
        [HarmonyPrefix]
        private static bool IsDefineValueConditionMet(AbilityManager __instance, Ability ability, AbilityStepSubDefinition step, ref bool __result)
            => ExpandedAbilityManager.Get(__instance).IsDefineValueConditionMet_Prefix(ability, step, ref __result);
    }

    private bool PerformAbility_Prefix(
        AbilityDefinition abilityDef, 
        bool altGirl, 
        Dictionary<string, int> insertValues, 
        ref bool __result, 
        out (IScriptedAbility Scripted, Ability Ability) __state)
    {
        __state = (null, null);

        var factory = abilityDef.GetExpansion().ScriptedAbilityFactory;
        if (factory == null) return true;

        var ability = new Ability(abilityDef, altGirl);
        if (insertValues != null)
        {
            ListUtils.DictionaryAddRangeUnique(ability.values, insertValues);
        }

        var scripted = factory(ability);

        __state = (scripted, ability);

        if (!scripted.PrePerform(ability, altGirl))
        {
            __result = false;
            return false;
        }

        var replaced = scripted.ReplacePerform(ability, altGirl);
        if (replaced.HasValue)
        {
            __result = replaced.Value;
            return false;
        }

        return true;
    }

    private void PerformAbility_Postfix(AbilityDefinition abilityDef, bool altGirl, ref bool __result, (IScriptedAbility Scripted, Ability Ability) __state)
    {
        if (__state.Scripted == null) return;
        __result = __state.Scripted.PostPerform(__state.Ability, altGirl, __result);
    }

    private bool SlotMeetsCondition_Prefix(Ability ability, UiPuzzleSlot checkSlot, TokenCondition condition, ref bool __result)
    {
        if (checkSlot.state != PuzzleSlotState.SETTLED)
        {
            __result = false;
            return false;
        }

        var conditionMet = false;
        var comparisonValue = ability.ParseIntValue(condition.val);
        var status = Game.Session.Puzzle.puzzleStatus;

        switch (condition.type)
        {
            case TokenConditionType.TOKEN_TYPE:
                switch (condition.tokenType)
                {
                    case TokenConditionTokenType.DEFINITION:
                        conditionMet = checkSlot.token.definition == condition.tokenDefinition;
                        break;

                    case TokenConditionTokenType.MOST_FAV:
                        {
                            var targetGirl = status.GetStatusGirl(!condition.oppositeGirl ? ability.altGirl : !ability.altGirl);
                            var tokenExp = checkSlot.token.definition.GetExpansion();
                            conditionMet = tokenExp.PuzzleResource != null && tokenExp.PuzzleResource.IsMostFav(targetGirl);
                            break;
                        }

                    case TokenConditionTokenType.LEAST_FAV:
                        {
                            var targetGirl = status.GetStatusGirl(!condition.oppositeGirl ? ability.altGirl : !ability.altGirl);
                            var tokenExp = checkSlot.token.definition.GetExpansion();
                            conditionMet = tokenExp.PuzzleResource != null && tokenExp.PuzzleResource.IsLeastFav(targetGirl);
                            break;
                        }
                }
                break;

            case TokenConditionType.ROW:
                conditionMet = MathUtils.CompareInts(condition.comparison, checkSlot.row, comparisonValue);
                break;

            case TokenConditionType.COL:
                conditionMet = MathUtils.CompareInts(condition.comparison, checkSlot.col, comparisonValue);
                break;

            case TokenConditionType.POWER:
                conditionMet = checkSlot.token.upgraded;
                break;
        }

        if (condition.inverse)
        {
            conditionMet = !conditionMet;
        }

        __result = conditionMet;
        return false;
    }

    private bool IsDefineValueConditionMet_Prefix(Ability ability, AbilityStepSubDefinition step, ref bool __result)
    {
        var status = Game.Session.Puzzle.puzzleStatus;
        var statusGirl = status.GetStatusGirl(!step.oppositeGirl ? ability.altGirl : !ability.altGirl);
        var statusGirlExp = statusGirl.GetExpansion();
        var girlExp = statusGirl.girlDefinition.GetExpansion();

        bool flipped = Game.Session.Puzzle.isPuzzleActive 
            && Game.Session.Puzzle.IsPuzzleOffset(PuzzleOffsetId.FlipMostLeastFavs);

        switch (step.conditionType)
        {
            case AbilityStepConditionType.MOST_FAV_TRAIT:
                {
                    var mostFav = flipped ? girlExp.GetLeastFavAffectionType() : girlExp.GetMostFavAffectionType();
                    __result = mostFav != null && IsAffectionTypeMatch(mostFav.Id, step.affectionType);
                    return false;
                }

            case AbilityStepConditionType.LEAST_FAV_TRAIT:
                {
                    var leastFav = flipped ? girlExp.GetMostFavAffectionType() : girlExp.GetLeastFavAffectionType();
                    __result = leastFav != null && IsAffectionTypeMatch(leastFav.Id, step.affectionType);
                    return false;
                }

            case AbilityStepConditionType.IS_EXHAUSTED:
            case AbilityStepConditionType.IS_UPSET:
                return statusGirlExp.State.SatisfiesCondition(step.conditionType);

            default:
                __result = false;
                return false;
        }
    }

    private static bool IsAffectionTypeMatch(RelativeId affectionId, PuzzleAffectionType type)
    {
        if (type == PuzzleAffectionType.TALENT) return affectionId == PuzzleAffectionId.Talent;
        if (type == PuzzleAffectionType.FLIRTATION) return affectionId == PuzzleAffectionId.Flirtation;
        if (type == PuzzleAffectionType.ROMANCE) return affectionId == PuzzleAffectionId.Romance;
        if (type == PuzzleAffectionType.SEXUALITY) return affectionId == PuzzleAffectionId.Sexuality;
        return false;
    }
}
