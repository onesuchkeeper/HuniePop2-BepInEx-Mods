using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class DialogTriggerDefinition_Ext
{
    public static ExpandedDialogTriggerDefinition Expansion(this DialogTriggerDefinition def)
        => ExpandedDialogTriggerDefinition.Get(def);
}

/// <summary>
/// Holds additional fields for a <see cref="DialogTriggerDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what your doing, instead
/// register a <see cref="DialogTriggerDataMod"/> using <see cref="ModInterface.AddDataMod"/>.
/// </summary>
public class ExpandedDialogTriggerDefinition
{
    private static Dictionary<RelativeId, ExpandedDialogTriggerDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedDialogTriggerDefinition>();

    public static ExpandedDialogTriggerDefinition Get(DialogTriggerDefinition def)
        => Get(def.id);

    public static ExpandedDialogTriggerDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.DialogTrigger, runtimeId));

    public static ExpandedDialogTriggerDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    // /// <summary>
    // /// Maps a girl id to a map of line id to line index.
    // /// use <see cref="TryGetLine"/> unless you must access the full collection.
    // /// </summary>
    // public Dictionary<RelativeId, Dictionary<RelativeId, int>> GirlIdToLineIdToLineIndex = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();

    // /// <summary>
    // /// Maps a girl id to a map of line index to line id.
    // /// use <see cref="TryGetLineSet"/> or <see cref="TryGetLine"/> unless you must access the full collection.
    // /// </summary>
    // public Dictionary<RelativeId, Dictionary<int, RelativeId>> GirlIdToLineIndexToLineId = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

    internal IdIndexMap GetGirlLineMap(RelativeId girlId)
        => _girlToLineIndexes.GetOrNew(girlId, () => new(1));

    private Dictionary<RelativeId, IdIndexMap> _girlToLineIndexes = new();

    /// <summary>
    /// Given a girl, attempts to get their <see cref="DialogTriggerLineSet"/>
    /// </summary>
    public bool TryGetLineSet(DialogTriggerDefinition def, RelativeId girlId, out DialogTriggerLineSet lineSet)
    {
        lineSet = def.dialogLineSets[ExpandedGirlDefinition.DialogTriggerIndexes[girlId]];
        if (lineSet.dialogLines.Any(x => x != null))
        {
            return true;
        }

        lineSet = def.dialogLineSets.FirstOrDefault();
        if (lineSet?.dialogLines.Count > 0)
        {
            return true;
        }

        lineSet = null;
        return false;
    }

    public DialogTriggerLineSet GetLineSetOrNew(DialogTriggerDefinition def, RelativeId girlId)
        => def.dialogLineSets.GetOrNew(ExpandedGirlDefinition.DialogTriggerIndexes[girlId]);

    public DialogLine GetLineOrNew(DialogTriggerDefinition def, RelativeId girlId, RelativeId lineId)
    {
        var set = GetLineSetOrNew(def, girlId);
        var LineIndexes = _girlToLineIndexes.GetOrNew(girlId);
        var index = LineIndexes[lineId];
        return set.dialogLines.GetOrNew(index);
    }
}
