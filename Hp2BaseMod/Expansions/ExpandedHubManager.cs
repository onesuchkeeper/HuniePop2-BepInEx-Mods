using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(HubManager), "HubStep")]
internal static class HubManager_HubStep
{
    [HarmonyPatch("HubStep")]
    [HarmonyPrefix]
    public static bool Prefix(HubManager __instance)
        => ExpandedHubManager.Get(__instance).HubStep_Prefix();
}

/// <summary>
/// Allows for additional non-stop date locations
/// by overriding the HubStep where the are handled
/// </summary>
[Expansion(typeof(HubManager))]
public partial class ExpandedHubManager
{
    private LocationDefinition[] _nonStopLocs;

    internal bool HubStep_Prefix()
    {
        //replace nonstop step 1 and 4 to expand nonstop location options

        //presteps
        var nextHubStepIndex = f_hubStepIndex.GetValue<int>(_core) + 1;

        if (f_hubBailed.GetValue<bool>(_core)
            || f_hubStepType.GetValue<HubStepType>(_core) != HubStepType.NONSTOP
            || (nextHubStepIndex != 1 && nextHubStepIndex != 4))
        {
            return true;
        }

        _hubStepIndex = nextHubStepIndex;
        List<DialogOptionInfo> list = new List<DialogOptionInfo>();

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
                            && (Game.Persistence.playerFile.storyProgress >= 12 || !expansion.PostBoss)
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
                    Game.Session.Location.Depart(_nonStopLocs[Game.Session.Dialog.selectedDialogOptionIndex], null, false);
                }
                else
                {
                    m_ChangeStepType.Invoke(_core, [HubStepType.ROOT]);
                }
                break;
            default:
                throw new System.Exception("unhandled hubStepIndex");
        }

        //post steps (not currently overriding any steps that modify the sequence)
        // if (this._hubSequence.Duration(true) > 0f)
        // {
        //     this._hubSequence.OnComplete(new TweenCallback(this.OnHubSequenceComplete));
        //     Game.Manager.Time.Play(this._hubSequence, this.pauseDefinition, 0f);
        // }

        return false;
    }

    private void OnDialogSelected_Hook()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogSelected_Hook;
        m_OnDialogSelected.Invoke(_core, null);
    }
}