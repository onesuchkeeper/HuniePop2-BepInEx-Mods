using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    private static FieldInfo _isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static FieldInfo _arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");

    [HarmonyPatch("OnLocationSettled")]
    [HarmonyPrefix]
    private static bool OnLocationSettled(LocationManager __instance)
    {
        if (__instance.currentLocation.locationType != LocationType.SIM)
        {
            return true;
        }

        var arrivalCutscene = _arrivalCutscene.GetValue<CutsceneDefinition>(__instance);

        if (arrivalCutscene != null)
        {
            return true;
        }

        _isLocked.SetValue(__instance, false);

        Game.Session.Logic.ProcessBundleList(__instance.currentLocation.departBundleList, false);

        Game.Manager.Windows.ShowWindow(__instance.actionBubblesWindow, false);

        var args = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(args);
        var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

        var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, __instance.dtGreetings.Length - 1);

        uiDoll.ReadDialogTrigger(__instance.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);

        _arrivalCutscene.SetValue(__instance, null);
        return false;
    }
}
