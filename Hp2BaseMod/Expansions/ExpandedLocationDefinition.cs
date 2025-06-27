using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class LocationDefinition_Ext
{
    public static ExpandedLocationDefinition Expansion(this LocationDefinition def)
        => ExpandedLocationDefinition.Get(def);
}

/// <summary>
/// Holds additional fields for a <see cref="LocationDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what your doing, instead
/// register a <see cref="LocationDataMod"/> using <see cref="ModInterface.AddDataMod"/>
/// </summary>
public class ExpandedLocationDefinition
{
    private static Dictionary<RelativeId, ExpandedLocationDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedLocationDefinition>();

    public static ExpandedLocationDefinition Get(LocationDefinition def)
        => Get(def.id);

    public static ExpandedLocationDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Location, runtimeId));

    public static ExpandedLocationDefinition Get(RelativeId id)
    {
        if (!_expansions.TryGetValue(id, out var expansion))
        {
            expansion = new ExpandedLocationDefinition();
            _expansions[id] = expansion;
        }

        return expansion;
    }

    /// <summary>
    /// Maps girl id to her style at the location.
    /// </summary>
    public Dictionary<RelativeId, GirlStyleInfo> GirlIdToLocationStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>();

    /// <summary>
    /// Times when this location can be used
    /// </summary>
    public List<ClockDaytimeType> DateTimes;

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
}
