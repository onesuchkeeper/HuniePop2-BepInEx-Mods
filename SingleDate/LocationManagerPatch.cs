using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(LocationManager))]
public static class LocationManagerPatch
{
    private static FieldInfo _isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static FieldInfo _arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static FieldInfo _currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake(LocationManager __instance)
    {
        UiPrefabs.InitActionBubbles(__instance.actionBubblesWindow);
    }

    [HarmonyPatch(nameof(LocationManager.Depart))]
    [HarmonyPrefix]
    public static void Depart(LocationManager __instance, LocationDefinition locationDef, GirlPairDefinition girlPairDef, ref bool sidesFlipped)
    {
        if (State.IsSingle(girlPairDef))
        {
            sidesFlipped = false;
        }
    }

    // [HarmonyPatch(nameof(LocationManager.Arrive))]
    // [HarmonyPrefix]
    // public static void PreArrive(LocationManager __instance,
    //     LocationDefinition locationDef,
    //     GirlPairDefinition girlPairDef,
    //     ref bool sidesFlipped,
    //     bool initialArrive = false)
    // {
    //     State.ArrivedPair = girlPairDef;
    // }

    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPostfix]
    public static void PostArrive(LocationManager __instance,
    LocationDefinition locationDef,
    GirlPairDefinition girlPairDef,
    bool sidesFlipped,
    bool initialArrive = false)
    {
        if (__instance.currentLocation.locationType == LocationType.SPECIAL
            || __instance.currentLocation.locationType == LocationType.HUB
            || !State.IsSingle(__instance.currentGirlPair))
        {
            return;
        }

        if (Game.Session.gameCanvas.dollRight.girlDefinition != __instance.currentGirlPair.girlDefinitionTwo)
        {
            Game.Session.gameCanvas.dollRight.LoadGirl(__instance.currentGirlPair.girlDefinitionTwo);
        }

        Game.Session.gameCanvas.dollLeft.UnloadGirl();
        Game.Session.gameCanvas.dollLeft.dropZone.Disable();

        Game.Session.gameCanvas.header.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.header.xValues.y,
            Game.Session.gameCanvas.header.rectTransform.anchoredPosition.y);

        Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.cellphone.xValues.y,
            Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition.y);
    }

    [HarmonyPatch("OnLocationSettled")]
    [HarmonyPrefix]
    private static bool OnLocationSettled(LocationManager __instance)
    {
        if (__instance.currentLocation.locationType != LocationType.SIM)
        {
            __instance.actionBubblesWindow = UiPrefabs.DefaultDateBubbles;
            return true;
        }

        if (!State.IsSingle(__instance.currentGirlPair))
        {
            __instance.actionBubblesWindow = UiPrefabs.DefaultDateBubbles;
            return true;
        }
        __instance.actionBubblesWindow = UiPrefabs.SingleDateBubbles;

        var arrivalCutscene = (CutsceneDefinition)_arrivalCutscene.GetValue(__instance);

        _isLocked.SetValue(__instance, false);
        Game.Session.Logic.ProcessBundleList(__instance.currentLocation.departBundleList, false);

        Game.Manager.Windows.ShowWindow(__instance.actionBubblesWindow, false);

        if (arrivalCutscene == null)
        {
            ModInterface.Log.LogInfo("Forcing greeting to girl on right for single date");

            var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, __instance.dtGreetings.Length - 1);

            Game.Session.gameCanvas.dollRight.ReadDialogTrigger(__instance.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);
        }

        _arrivalCutscene.SetValue(__instance, null);

        return false;
    }
}
