using System.Collections.Generic;

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
    /// If this location allows no pair to be present
    /// </summary>
    public bool AllowNoPair;

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
            && (!PostBoss || Game.Persistence.playerFile.storyProgress >= 12)
            && DateTimes.Contains(time);

    public RelativeId? DefaultStyle;
}
