using System.Collections.Generic;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(LocationDefinition), HasModId = true)]
public partial class ExpandedLocationDefinition
{
    /// <summary>
    /// Maps girl id to the greeting used at this location
    /// </summary>
    public Dictionary<RelativeId, RelativeId> GirlIdToLocationGreetingLineId = new();

    /// <summary>
    /// Times when this location can be used
    /// </summary>
    public List<ClockDaytimeType> DateTimes;

    /// <summary>
    /// If this location can be used to be with a pair outside of a date: the finder offers it as a place to go.
    /// </summary>
    public bool AllowSim;

    /// <summary>
    /// If the location's backgrounds are the two of a date (the normal one, then the blurred one) and not one for each time of day
    /// </summary>
    public bool StaticBackground;

    /// <summary>
    /// If this location can be used for non-stop dates
    /// </summary>
    public bool AllowNonStop;

    /// <summary>
    /// If this location can be used for standard dates
    /// </summary>
    public bool AllowNormal;

    /// <summary>
    /// If this location is only available after defeating the nymphojinn
    /// </summary>
    public bool PostBoss;

    /// <summary>
    /// If the location can be used for a normal date at the current time with the current story progress
    /// </summary>
    public bool IsValidForNormalDate() => IsValidForNormalDate((ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4));

    /// <summary>
    /// If the location can be used for a normal date at the specified time with the current story progress
    /// </summary>
    public bool IsValidForNormalDate(ClockDaytimeType time)
        => AllowNormal
            && (!PostBoss || Game.Persistence.playerFile.IsProgressPoolsideEnding())
            && DateTimes?.Contains(time) == true;
    
    public RelativeId? DefaultStyle;

    [HarmonyPatch(typeof(LocationDefinition))]
    private static class Patch
    {
        [HarmonyPatch(nameof(LocationDefinition.GetDaytimeBackground))]
        [HarmonyPrefix]
        private static bool GetDaytimeBackground(LocationDefinition __instance, int daytimeElapsed, bool blur, ref Sprite __result)
        {
            __result = ExpandedLocationDefinition.Get(__instance).GetDaytimeBackground_Prefix(daytimeElapsed, blur);
            return false;
        }
    }

    /// <summary>
    /// Replaces the base game's GetDaytimeBackground, which decided between the two from the location's type.
    /// </summary>
    private Sprite GetDaytimeBackground_Prefix(int daytimeElapsed, bool blur)
        => StaticBackground
            ? _core.backgrounds[blur ? 1 : 0]
            : _core.backgrounds[Mathf.Clamp(daytimeElapsed % 4, 0, _core.backgrounds.Count - 1)];
}

/// <summary>
/// There are no location types to ask for. A caller that still does fails here instead of being handed a list of the wrong
/// locations (the location type of a location a mod made is whatever the enum's default is).
/// </summary>
[HarmonyPatch(typeof(LocationData), nameof(LocationData.GetAllByLocationType))]
internal static class LocationData_GetAllByLocationType
{
    private static bool Prefix()
        => throw new System.NotSupportedException("LocationData.GetAllByLocationType must not be used, there is no location type. "
            + "Use Game.Data.Locations.GetAll() and what the location's expansion says (AllowSim, AllowNormal, AllowNonStop).");
}