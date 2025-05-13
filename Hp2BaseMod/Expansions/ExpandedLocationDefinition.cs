using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

public static class LocationDefinition_Ext
{
    public static ExpandedLocationDefinition Expansion(this LocationDefinition def)
        => ExpandedLocationDefinition.Get(def);
}

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

    public Dictionary<RelativeId, GirlStyleInfo> GirlIdToLocationStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>();
}
