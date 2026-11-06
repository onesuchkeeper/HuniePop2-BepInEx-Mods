using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// Allows for additional non-stop date locations
/// by overriding the HubStep where the are handled
/// </summary>
[Expansion(typeof(HubManager))]
public partial class ExpandedHubManager
{
    [HarmonyPatch(typeof(HubManager))]
    private static class Patch
    {
        [HarmonyPatch("HubStep")]
        [HarmonyPrefix]
        public static bool HubStep_Prefix(HubManager __instance)
            => ExpandedHubManager.Get(__instance).HubStep_Prefix();

        [HarmonyPatch(typeof(HubManager), nameof(HubManager.PrepHub))]
        [HarmonyPrefix]
        public static bool PrepHub_Prefix(HubManager __instance)
            => ExpandedHubManager.Get(__instance).PrepHub_Prefix();

        [HarmonyPatch(typeof(HubManager), nameof(HubManager.StartHub))]
        [HarmonyPrefix]
        public static void StartHub_Prefix(HubManager __instance)
            => ExpandedHubManager.Get(__instance).StartHub_Prefix();
    }

    /// <summary>
    /// Raised when the Hub pipeline determines the next GameState to transition into upon departure.
    /// </summary>
    public event Action<IGameStatePayload> StateDeparting;

    private LocationDefinition[] _nonStopLocs;

    // Temporary tracing of the hub steps this class intercepts. Set to false once the hub flows are verified.
    private const bool DebugHubSteps = true;

    /// <summary>
    /// Replaces the base game's PrepHub, which showed the hub's menu when the location was the hub's. The hub's menu is the
    /// hub state's (<see cref="IGameState.ShowsHubMenu"/>), wherever the player is.
    /// </summary>
    private bool PrepHub_Prefix()
    {
        var showsMenu = ModInterface.GameState.CurrentState.ShowsHubMenu;

        _core.hubMenuContainer.alpha = showsMenu ? 1f : 0f;
        _core.hubMenuContainer.blocksRaycasts = showsMenu;

        Game.Manager.Time.KillTween(_hubMenuTweener, false, true);
        _hubMenuShowing = false;
        _core.hubMenuCanvasGroup.alpha = 0.32f;
        _core.hubMenuCanvasGroup.blocksRaycasts = false;

        return false;
    }

    private bool HubStep_Prefix()
    {
        if (_hubBailed) return true;

        // HubManager.HubStep() does `_hubStepIndex++` as its first statement and its switch uses the incremented value.
        // A prefix runs before that increment, so the step the base is about to run is the current index + 1.
        // (Comparing the un-incremented index intercepted the wrong step: the boss date departed without StateDeparting,
        // and the non-stop options were rebuilt after the player had already chosen one.)
        var nextHubStepIndex = _hubStepIndex + 1;
        var stepType = _hubStepType;

        if (DebugHubSteps && (stepType == HubStepType.WINGS || stepType == HubStepType.NONSTOP))
        {
            ModInterface.Log.Message($"[HubDbg] HubStep {stepType} step {nextHubStepIndex}");
        }

        if (stepType == HubStepType.WINGS && nextHubStepIndex == 2)
        {
            _hubStepIndex = nextHubStepIndex;
            Game.Manager.Time.KillTween(_hubSequence, true, true);
            _hubSequence = DOTween.Sequence();

            if (Game.Persistence.playerFile.IsProgressVolcano())
            {
                Game.Persistence.playerFile.PushDaytimeTo(ClockDaytimeType.NIGHT);

                // Outer space with the boss pair is the boss DATE (a date location, so the Puzzle state), also when repeating it.
                // The volcano is the story scene that leads up to it: a special location, so the Special state. Its bundles send
                // the player on to outer space, which changes to the Puzzle state on that arrival.
                var isBossDate = Game.Persistence.playerFile.IsProgressNymphojinnPreDate();
                var targetLoc = isBossDate ? _core.spaceLocationDefinition : _core.volcanoLocationDefinition;
                var targetPair = isBossDate ? Game.Session.Puzzle.bossGirlPairDefinition : null;

                StateDeparting?.Invoke(isBossDate ? new GameStatePuzzle.Payload(PuzzleStateId.Boss) : new GameStateSpecial.Payload());

                Game.Session.Location.Depart(targetLoc, targetPair);
            }
            else
            {
                m_ChangeStepType.Invoke(_core, [HubStepType.ROOT]);
            }

            return false;
        }

        // 2. Handle Non-Stop Date Steps
        if (stepType == HubStepType.NONSTOP && (nextHubStepIndex == 1 || nextHubStepIndex == 4))
        {
            _hubStepIndex = nextHubStepIndex;
            Game.Manager.Time.KillTween(_hubSequence, true, true);
            _hubSequence = DOTween.Sequence();

            switch (nextHubStepIndex)
            {
                case 1:
                    var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

                    var locs = Game.Data.Locations.GetAll()
                        .Where(x =>
                        {
                            var expansion = x.GetExpansion();
                            return expansion.AllowNonStop
                                && (Game.Persistence.playerFile.IsProgressPoolsideEnding() || !expansion.PostBoss)
                                && (expansion.DateTimes?.Contains(time) ?? true);
                        });

                    var locPool = locs.ToList();

                    _nonStopLocs = [
                        locPool.PopRandom(),
                        locPool.PopRandom(),
                        locPool.PopRandom(),
                    ];

                    int i = 0;
                    Game.Session.Dialog.ShowDialogOptions(_nonStopLocs.Select(x => new DialogOptionInfo(x.nonStopOptionText, i++)).Append(new DialogOptionInfo(_core.optionNonStop, 3)).ToList(), false, false);
                    Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogSelected_Hook;
                    break;

                case 4:
                    if (Game.Session.Dialog.selectedDialogOptionIndex < 3)
                    {
                        // Communicate GameStateId.Puzzle for non-stop date departures
                        StateDeparting?.Invoke(new GameStatePuzzle.Payload(PuzzleStateId.Nonstop));

                        Game.Session.Location.Depart(_nonStopLocs[Game.Session.Dialog.selectedDialogOptionIndex], null, false);
                    }
                    else
                    {
                        m_ChangeStepType.Invoke(_core, [HubStepType.ROOT]);
                    }
                    break;
            }

            return false;
        }

        return true;
    }

    private void StartHub_Prefix()
    {
        //I think the state already handles this
        // if (Game.Persistence.playerFile.GetFlagValue(_core.firstLocationFlag) > 0)
        // {
        //     // first location from Hub is always a Sim location
        //     StateDeparting?.Invoke(new GameStateSim.Payload());
        // }
    }

    private void OnDialogSelected_Hook()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogSelected_Hook;
        m_OnDialogSelected.Invoke(_core, null);
    }
}