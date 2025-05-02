using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotPatch
{
    private static readonly float _spacing = 48f;
    private static readonly FieldInfo _playerFileFinderSlot = AccessTools.Field(typeof(UiAppFinderSlot), "_playerFileFinderSlot");
    private static readonly FieldInfo _girlDefinition = AccessTools.Field(typeof(UiAppHeadSlot), "_girlDefinition");

    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppFinderSlot __instance, bool settled)
    {
        var pairDef = __instance.locationDefinition.locationType == LocationType.SIM && __instance.locationDefinition == Game.Session.Location.currentLocation
            ? Game.Session.Location.currentGirlPair
            : ((PlayerFileFinderSlot)_playerFileFinderSlot.GetValue(__instance))?.girlPairDefinition;

        if (!State.IsSingle(pairDef))
        {
            return;
        }

        //The positions of the left and right heads on some of the slots are reversed and tbh I have no idea where or why
        //I guess they're just like that in the prefab? Maybe?
        //even though there populated according to if the sides are swapped? Idk, I give up
        //instead find the slot with the non-'nobody' girl and position that

        var leftDef = (GirlDefinition)_girlDefinition.GetValue(__instance.headSlotLeft);
        var leftId = ModInterface.Data.GetDataId(GameDataType.Girl, leftDef.id);

        var usedSlot = leftId.SourceId == State.ModId
            ? __instance.headSlotRight
            : __instance.headSlotLeft;

        var unusedSlot = leftId.SourceId == State.ModId
            ? __instance.headSlotLeft
            : __instance.headSlotRight;

        usedSlot.Populate(pairDef.girlDefinitionTwo);
        usedSlot.rectTransform.anchoredPosition = new Vector2(-_spacing, __instance.headSlotLeft.rectTransform.anchoredPosition.y);

        //I don't like having to destroy this, but setting the alpha to 0 keeps it around and visible
        //somewhere after this it must be re-populated or refreshed but atm I have no clue ¯\_(ツ)_/¯
        unusedSlot.rectTransform.SetParent(null);
        UnityEngine.Object.Destroy(unusedSlot.gameObject);
        // unusedSlot.Populate(null);
        // unusedSlot.canvasGroup.alpha = 0;
        // unusedSlot.canvasGroup.blocksRaycasts = false;

        var relationshipTransform = __instance.relationshipSlot.GetComponent<RectTransform>();
        relationshipTransform.anchoredPosition = new Vector2(_spacing, relationshipTransform.anchoredPosition.y);
    }
}