using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    private static FieldInfo _isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static FieldInfo _arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");

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

    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPrefix]
    public static void PreArrive(LocationManager __instance,
    LocationDefinition locationDef,
    GirlPairDefinition girlPairDef,
    bool sidesFlipped,
    bool initialArrive = false)
    {
        State.On_LocationManger_Arrive(girlPairDef);
    }

    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPostfix]
    public static void PostArrive(LocationManager __instance,
    LocationDefinition locationDef,
    GirlPairDefinition girlPairDef,
    bool sidesFlipped,
    bool initialArrive = false)
    {
        if (__instance.currentLocation.locationType == LocationType.SPECIAL
            || __instance.currentLocation.locationType == LocationType.HUB)
        {
            return;
        }

        if (!State.IsSingleDate)
        {
            __instance.actionBubblesWindow = UiPrefabs.DefaultDateBubbles;
            return;
        }
        __instance.actionBubblesWindow = UiPrefabs.SingleDateBubbles;

        if (Game.Session.gameCanvas.dollRight.girlDefinition != __instance.currentGirlPair.girlDefinitionTwo)
        {
            Game.Session.gameCanvas.dollRight.LoadGirl(__instance.currentGirlPair.girlDefinitionTwo);
        }

        Game.Session.gameCanvas.header.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.header.xValues.y,
            Game.Session.gameCanvas.header.rectTransform.anchoredPosition.y);

        Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.cellphone.xValues.y,
            Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition.y);
    }
}
