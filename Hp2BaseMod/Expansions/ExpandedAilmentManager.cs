using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(AilmentManager))]
internal static class AilmentManagerPatch
{
    [HarmonyPatch("OnAilmentEnable")]
    [HarmonyPostfix]
    public static void OnAilmentEnable(AilmentManager __instance, Ailment ailment, bool fromTrigger)
        => ExpandedAilmentManager.Get(__instance).OnAilmentEnable(ailment, fromTrigger);

    [HarmonyPatch("OnAilmentDisable")]
    [HarmonyPostfix]
    public static void OnAilmentDisable(AilmentManager __instance, Ailment ailment, bool fromTrigger)
        => ExpandedAilmentManager.Get(__instance).OnAilmentDisable(ailment, fromTrigger);

    [HarmonyPatch("TriggerAilment")]
    [HarmonyPostfix]
    public static void TriggerAilment(AilmentManager __instance, AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
        => ExpandedAilmentManager.Get(__instance).TriggerAilment(triggerType, ailment, girlStatus, unfocused);
}

[Expansion(typeof(AilmentManager), 
    Fields = new[]{"_move", "_match", "_puzzleStatus", "_moveModifier", "_matchModifier", "_giftModifier"})]
public partial class ExpandedAilmentManager
{
    public PuzzleSet Move => f_move.GetValue<PuzzleSet>(_core);
    public MoveModifier MoveModifier => f_moveModifier.GetValue<MoveModifier>(_core);
    public PuzzleMatch Match => f_match.GetValue<PuzzleMatch>(_core);
    public MatchModifier MatchModifier => f_matchModifier.GetValue<MatchModifier>(_core);

    public void OnAilmentEnable(Ailment ailment, bool fromTrigger)
    {
        var scripted = ailment.GetExpansion().ScriptedAilment;
        if (scripted == null) return;

        // Mirror the original guard: only apply when enableTriggerIndex < 0 or called from a trigger.
        if (ailment.definition.enableTriggerIndex >= 0 && !fromTrigger)
        {
            return;
        }

        ResolveGirls(ailment, out var girl, out var otherGirl);
        scripted.OnEnable(ailment, girl, otherGirl);
    }

    public void OnAilmentDisable(Ailment ailment, bool fromTrigger)
    {
        var scripted = ailment.GetExpansion().ScriptedAilment;
        if (scripted == null) return;

        if (ailment.definition.enableTriggerIndex >= 0 && !fromTrigger)
        {
            return;
        }

        ResolveGirls(ailment, out var girl, out var otherGirl);
        scripted.OnDisable(ailment, girl, otherGirl);
    }

    public void TriggerAilment(AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
    {
        ExpandedUiPuzzleGrid.Get().OnTrigger(triggerType);

        var scripted = ailment.GetExpansion().ScriptedAilment;
        if (scripted == null || !ailment.isEnabled) return;

        bool success = scripted.OnTrigger(
            triggerType,
            ailment,
            girlStatus,
            unfocused,
            f_moveModifier.GetValue<MoveModifier>(_core),
            f_matchModifier.GetValue<MatchModifier>(_core),
            f_giftModifier.GetValue<GiftModifier>(_core));

        if (success && girlStatus.girlDefinition.baggageItemDefs.Contains(ailment.definition.itemDefinition))
        {
            var doll = Game.Session.gameCanvas.GetDoll(girlStatus.altGirl);
            if (!doll.soulGirlDefinition.specialCharacter)
            {
                int baggageIndex = girlStatus.girlDefinition.baggageItemDefs.IndexOf(ailment.definition.itemDefinition);
                doll.ReadDialogTrigger(
                    Game.Session.Puzzle.dtBaggages[UnityEngine.Mathf.Clamp(baggageIndex, 0, girlStatus.girlDefinition.baggageItemDefs.Count - 1)],
                    DialogLineFormat.UNCHECKED,
                    -1);
            }
        }
    }

    private void ResolveGirls(Ailment ailment, out PuzzleStatusGirl girl, out PuzzleStatusGirl otherGirl)
    {
        var puzzleStatus = f_puzzleStatus.GetValue(_core) as PuzzleStatus;

        girl = puzzleStatus.girlStatusLeft.ailments.Contains(ailment)
            ? puzzleStatus.girlStatusLeft
            : puzzleStatus.girlStatusRight;

        otherGirl = puzzleStatus.GetStatusGirl(!girl.altGirl);
    }
}