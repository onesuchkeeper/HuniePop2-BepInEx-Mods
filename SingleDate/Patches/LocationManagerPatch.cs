using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    private static readonly FieldInfo f_isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static readonly FieldInfo f_arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static CutsceneDefinition _baseCutsceneMeeting;
    private static CutsceneDefinition _singleCutsceneMeeting;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake(LocationManager __instance)
    {
        UiPrefabs.InitActionBubbles(__instance.actionBubblesWindow);
        _baseCutsceneMeeting = __instance.cutsceneMeeting;

        _singleCutsceneMeeting = new CutsceneDefinition()
        {
            cleanUpType = CutsceneCleanUpType.NONE,
            steps = new(){
                CutsceneStepUtility.MakeDollMove(DollPositionType.INNER, DollOrientationType.RIGHT, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSubCutsceneGirlPair(GirlPairRelationshipType.UNKNOWN, CutsceneStepProceedType.AUTOMATIC),
                _baseCutsceneMeeting.steps[5],//compat banner
                CutsceneStepUtility.MakeTogglePhoneAndHeader(true, CutsceneStepProceedType.AUTOMATIC)
            }
        };
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
    bool initialArrive)
    {
        if (State.IsSingleDate)
        {
            __instance.actionBubblesWindow = UiPrefabs.SingleDateBubbles;
            __instance.cutsceneMeeting = _singleCutsceneMeeting;
        }
        else
        {
            __instance.actionBubblesWindow = UiPrefabs.DefaultDateBubbles;
            __instance.cutsceneMeeting = _baseCutsceneMeeting;
        }
    }
}
