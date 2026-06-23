using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

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

/// <summary>
/// Companion class for <see cref="AilmentManager"/> that injects <see cref="IScriptedAilment"/> calls
/// into the ailment enable/disable/trigger pipeline.
///
/// Scripted behaviour is additive — postfixes run after the original data-driven logic and
/// never replace it.
/// </summary>
public class ExpandedAilmentManager
{
    private static readonly Dictionary<AilmentManager, ExpandedAilmentManager> _expansions
        = new Dictionary<AilmentManager, ExpandedAilmentManager>();

    public static ExpandedAilmentManager Get(AilmentManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedAilmentManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_puzzleStatus  = AccessTools.Field(typeof(AilmentManager), "_puzzleStatus");
    private static readonly FieldInfo f_moveModifier  = AccessTools.Field(typeof(AilmentManager), "_moveModifier");
    private static readonly FieldInfo f_matchModifier = AccessTools.Field(typeof(AilmentManager), "_matchModifier");
    private static readonly FieldInfo f_giftModifier  = AccessTools.Field(typeof(AilmentManager), "_giftModifier");

    private readonly AilmentManager _core;

    private ExpandedAilmentManager(AilmentManager core)
    {
        _core = core;
    }

    public void OnAilmentEnable(Ailment ailment, bool fromTrigger)
    {
        var scripted = ailment.Expansion().ScriptedAilment;
        if (scripted == null)
        {
            return;
        }

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
        var scripted = ailment.Expansion().ScriptedAilment;
        if (scripted == null)
        {
            return;
        }

        if (ailment.definition.enableTriggerIndex >= 0 && !fromTrigger)
        {
            return;
        }

        ResolveGirls(ailment, out var girl, out var otherGirl);
        scripted.OnDisable(ailment, girl, otherGirl);
    }

    public void TriggerAilment(AilmentTriggerType triggerType, Ailment ailment, PuzzleStatusGirl girlStatus, bool unfocused)
    {
        var scripted = ailment.Expansion().ScriptedAilment;
        if (scripted == null || !ailment.isEnabled)
        {
            return;
        }

        var moveModifier  = f_moveModifier.GetValue(_core)  as MoveModifier;
        var matchModifier = f_matchModifier.GetValue(_core) as MatchModifier;
        var giftModifier  = f_giftModifier.GetValue(_core)  as GiftModifier;

        bool success = scripted.OnTrigger(
            triggerType,
            ailment,
            girlStatus,
            unfocused,
            moveModifier,
            matchModifier,
            giftModifier);

        if (success && girlStatus.girlDefinition.baggageItemDefs.Contains(ailment.definition.itemDefinition))
        {
            UiDoll doll = Game.Session.gameCanvas.GetDoll(girlStatus.altGirl);
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