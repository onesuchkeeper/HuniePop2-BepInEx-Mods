using System.Linq;
using HarmonyLib;

namespace Hp2BaseMod;
/*
    In the base game PuzzleStatus is a member of PuzzleManager
    Instantiated in Awake.
*/

[Expansion(typeof(PuzzleStatus))]
[Deprecates(nameof(PuzzleStatus.AddResourceValue), $"Use {nameof(ExpandedPuzzleStatus)}.{nameof(ExpandedPuzzleStatus.AddResourceValue)} instead")]
[Deprecates(nameof(PuzzleStatus.GetResourceValue), $"Use {nameof(ExpandedPuzzleStatus)}.{nameof(ExpandedPuzzleStatus.GetResourceValue)} instead")]
public partial class ExpandedPuzzleStatus
{
    [HarmonyPatch(typeof(PuzzleStatus))]
    private static class Patch
    {
        [HarmonyPatch(nameof(PuzzleStatus.Clear))]
        [HarmonyPrefix]
        private static void Clear(PuzzleStatus __instance) 
            => ExpandedPuzzleStatus.Get(__instance).Clear();

        [HarmonyPatch(nameof(PuzzleStatus.EnableAilments))]
        [HarmonyPrefix]
        private static bool EnableAilments(PuzzleStatus __instance) 
            => ExpandedPuzzleStatus.Get(__instance).EnableAilments_Prefix();

        [HarmonyPatch(nameof(PuzzleStatus.AddResourceValue))]
        [HarmonyPrefix]
        private static bool AddResourceValue(PuzzleStatus __instance, PuzzleResourceType resourceType, int value, bool altGirl)
            => ExpandedPuzzleStatus.Get(__instance).AddResourceValue_Prefix(resourceType, value, altGirl);

        [HarmonyPatch(nameof(PuzzleStatus.GetResourceValue))]
        [HarmonyPrefix]
        private static bool GetResourceValue(PuzzleStatus __instance, PuzzleResourceType resourceType, bool altGirl, bool maxVal, ref int __result)
            => ExpandedPuzzleStatus.Get(__instance).GetResourceValue_Prefix(resourceType, altGirl, maxVal, ref __result);

        [HarmonyPatch(nameof(PuzzleStatus.ReviveGirls))]
        [HarmonyPrefix]
        private static void ReviveGirls_Pre(PuzzleStatus __instance)
            => ExpandedPuzzleStatus.Get(__instance).ReviveGirls_Pre();

        [HarmonyPatch(nameof(PuzzleStatus.ReviveGirls))]
        [HarmonyPostfix]
        private static void ReviveGirls_Post(PuzzleStatus __instance)
            => ExpandedPuzzleStatus.Get(__instance).ReviveGirls_Post();
    }

    public bool SuppressStateDialog {get;set;}

    private void ReviveGirls_Pre()
    {
        SuppressStateDialog = true;
    }

    private void ReviveGirls_Post()
    {
        SuppressStateDialog = false;
    }

    private void Clear()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }

    private bool EnableAilments_Prefix()
    {
        var ailmentManagerExp = Game.Session.Ailment.GetExpansion();
        foreach (var ailment in _girlStatusLeft.ailments.Concat(_girlStatusRight.ailments))
        {
            ailmentManagerExp.Enable(ailment, _girlStatusLeft);
        }

        return false;
    }

    public void AddResourceValue(RelativeId resourceId, int value, bool altGirl)
    {
        if (value == 0) return;
        var statusGirl = _core.GetStatusGirl(altGirl);
        var resource = ModInterface.GameData.GetPuzzleResource(resourceId);
        if (resource != null && resource.AddResourceValue(value, this, statusGirl))
        {
            _core.resourceChanged = true;
        }
    }

    public int GetResourceValue(RelativeId resourceId, bool altGirl, bool maxVal = false)
    {
        var statusGirl = _core.GetStatusGirl(altGirl);

        if (resourceId == PuzzleResourceId.AffectionTalent ||
            resourceId == PuzzleResourceId.AffectionFlirtation ||
            resourceId == PuzzleResourceId.AffectionRomance ||
            resourceId == PuzzleResourceId.AffectionSexuality)
        {
            return maxVal ? _core.affectionGoal : _core.affection;
        }
        if (resourceId == PuzzleResourceId.Moves)
        {
            return maxVal ? _core.maxMovesRemaining : _core.movesRemaining;
        }
        if (resourceId == PuzzleResourceId.Stamina)
        {
            return maxVal ? 6 : statusGirl.stamina;
        }
        if (resourceId == PuzzleResourceId.Passion)
        {
            return maxVal ? 100 : statusGirl.passion;
        }
        if (resourceId == PuzzleResourceId.Sentiment)
        {
            return maxVal ? 40 : statusGirl.sentiment;
        }

        return 0;
    }

    private bool AddResourceValue_Prefix(PuzzleResourceType resourceType, int value, bool altGirl)
    {
        if (value == 0) return false;

        var statusGirl = _core.GetStatusGirl(altGirl);
        RelativeId resourceId = resourceType == PuzzleResourceType.AFFECTION
            ? PuzzleResourceId.AffectionTalent
            : PuzzleResourceId.From(resourceType);

        var customResource = ModInterface.GameData.GetPuzzleResource(resourceId);
        if (customResource != null && customResource.AddResourceValue(value, this, statusGirl))
        {
            _core.resourceChanged = true;
            return false;
        }

        return true;
    }

    private bool GetResourceValue_Prefix(PuzzleResourceType resourceType, bool altGirl, bool maxVal, ref int __result)
    {
        RelativeId resourceId = resourceType == PuzzleResourceType.AFFECTION
            ? PuzzleResourceId.AffectionTalent
            : PuzzleResourceId.From(resourceType);

        __result = GetResourceValue(resourceId, altGirl, maxVal);
        return false;
    }

    private void OnDestroy()
    {
        _core.girlStatusLeft?.DestroyExpansion();
        _core.girlStatusRight?.DestroyExpansion();
    }
}