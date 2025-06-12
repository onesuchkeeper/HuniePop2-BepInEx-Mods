
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

[HarmonyPatch(typeof(HubManager), "HubStep")]
internal static class HubManager_HubStep
{
    [HarmonyPatch("HubStep")]
    [HarmonyPrefix]
    public static bool Prefix(HubManager __instance)
        => ExpandedHubManager.Get(__instance).HubStep();
}

public class ExpandedHubManager
{
    private static Dictionary<HubManager, ExpandedHubManager> _expansions
        = new Dictionary<HubManager, ExpandedHubManager>();

    public static ExpandedHubManager Get(HubManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedHubManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }
    private static readonly FieldInfo _hubStepType = AccessTools.Field(typeof(HubManager), "_hubStepType");
    private static readonly FieldInfo _hubStepIndex = AccessTools.Field(typeof(HubManager), "_hubStepIndex");
    private static readonly FieldInfo _hubBailed = AccessTools.Field(typeof(HubManager), "_hubBailed");
    private static readonly FieldInfo _hubSequence = AccessTools.Field(typeof(HubManager), "_hubSequence");
    private static readonly MethodInfo m_onDialogSelected = AccessTools.Method(typeof(HubManager), "OnDialogSelected");
    private static readonly MethodInfo m_changeStepType = AccessTools.Method(typeof(HubManager), "ChangeStepType");

    protected HubManager _core;
    private LocationDefinition[] _nonStopLocs;

    private ExpandedHubManager(HubManager core)
    {
        _core = core;
    }

    public bool HubStep()
    {
        //replace nonstop step 1 and 4 to expand nonstop location options

        //presteps
        var hubStepIndex = _hubStepIndex.GetValue<int>(_core) + 1;

        if (_hubBailed.GetValue<bool>(_core)
            || _hubStepType.GetValue<HubStepType>(_core) != HubStepType.NONSTOP
            || (hubStepIndex != 1 && hubStepIndex != 4))
        {
            return true;
        }

        _hubStepIndex.SetValue(_core, hubStepIndex);
        List<DialogOptionInfo> list = new List<DialogOptionInfo>();

        var hubSequence = _hubSequence.GetValue<Sequence>(_core);
        Game.Manager.Time.KillTween(hubSequence, true, true);
        hubSequence = DOTween.Sequence();
        _hubSequence.SetValue(_core, hubSequence);

        switch (hubStepIndex)
        {
            case 1:
                var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

                var locs = Game.Data.Locations.GetAllByLocationType(LocationType.DATE)
                    .Where(x =>
                    {
                        var expansion = x.Expansion();

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
                Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogSelected;
                break;
            case 4:
                if (Game.Session.Dialog.selectedDialogOptionIndex < 3)
                {
                    Game.Session.Location.Depart(_nonStopLocs[Game.Session.Dialog.selectedDialogOptionIndex], null, false);
                }
                else
                {
                    m_changeStepType.Invoke(_core, [HubStepType.ROOT]);
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

    private void OnDialogSelected()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogSelected;
        m_onDialogSelected.Invoke(_core, null);
    }
}