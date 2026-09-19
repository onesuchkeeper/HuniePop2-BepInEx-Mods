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

        [HarmonyPatch(typeof(HubManager), nameof(HubManager.StartHub))]
        [HarmonyPrefix]
        public static bool StartHub_Prefix(HubManager __instance)
            => ExpandedHubManager.Get(__instance).StartHub_Prefix();
    }

    /// <summary>
    /// Raised when the Hub pipeline determines the next GameState to transition into upon departure.
    /// </summary>
    public event Action<RelativeId> StateDeparting;

    private LocationDefinition[] _nonStopLocs;

    private bool HubStep_Prefix()
    {
        if (_hubBailed) return true;

        var nextHubStepIndex = _hubStepIndex;
        var stepType = _hubStepType;

        if (stepType == HubStepType.WINGS && nextHubStepIndex == 2)
        {
            _hubStepIndex = nextHubStepIndex;
            Game.Manager.Time.KillTween(_hubSequence, true, true);
            _hubSequence = DOTween.Sequence();

            if (Game.Persistence.playerFile.IsProgressVolcano())
            {
                Game.Persistence.playerFile.PushDaytimeTo(ClockDaytimeType.NIGHT);

                var isPreDate = Game.Persistence.playerFile.IsProgressNymphojinnPreDate();
                var targetLoc = isPreDate ? _core.spaceLocationDefinition : _core.volcanoLocationDefinition;
                var targetPair = isPreDate ? Game.Session.Puzzle.bossGirlPairDefinition : null;

                StateDeparting?.Invoke(isPreDate ? GameStateId.Special : GameStateId.Puzzle);

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

                    var locs = Game.Data.Locations.GetAllByLocationType(LocationType.DATE)
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
                        StateDeparting?.Invoke(GameStateId.Puzzle);

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

    private bool StartHub_Prefix()
    {
        if (Game.Persistence.playerFile.GetFlagValue(_core.firstLocationFlag) > 0)
        {
            // First location from Hub is always a Sim location
            StateDeparting?.Invoke(GameStateId.Sim);
        }
        return true;
    }

    private void OnDialogSelected_Hook()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogSelected_Hook;
        m_OnDialogSelected.Invoke(_core, null);
    }
}
