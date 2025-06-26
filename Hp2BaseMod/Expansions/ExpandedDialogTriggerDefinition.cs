using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod;

public static class DialogTriggerDefinitionExtension
{
    public static ExpandedDialogTriggerDefinition Expansion(this DialogTriggerDefinition def)
        => ExpandedDialogTriggerDefinition.Get(def);
}

public class ExpandedDialogTriggerDefinition
{
    private static Dictionary<RelativeId, ExpandedDialogTriggerDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedDialogTriggerDefinition>();

    public static ExpandedDialogTriggerDefinition Get(DialogTriggerDefinition def)
        => Get(def.id);

    public static ExpandedDialogTriggerDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.DialogTrigger, runtimeId));

    public static ExpandedDialogTriggerDefinition Get(RelativeId id)
    {
        if (!_expansions.TryGetValue(id, out var expansion))
        {
            expansion = new ExpandedDialogTriggerDefinition();
            _expansions[id] = expansion;
        }

        return expansion;
    }

    public Dictionary<RelativeId, Dictionary<RelativeId, int>> GirlIdToLineIdToLineIndex = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();
    public Dictionary<RelativeId, Dictionary<int, RelativeId>> GirlIdToLineIndexToLineId = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

    public bool TryGetLineSet(DialogTriggerDefinition def, RelativeId girlId, out DialogTriggerLineSet lineSet)
    {
        var girlExpansion = ExpandedGirlDefinition.Get(girlId);

        if (def.dialogLineSets[girlExpansion.DialogTriggerIndex].dialogLines.Count > 0)
        {
            lineSet = def.dialogLineSets[girlExpansion.DialogTriggerIndex];
            return true;
        }

        if (def.dialogLineSets.FirstOrDefault()?.dialogLines.Count > 0)
        {
            lineSet = def.dialogLineSets[0];
            return true;
        }

        lineSet = null;
        return false;
    }

    public bool TryGetLine(DialogTriggerDefinition def, RelativeId girlId, RelativeId id, out DialogLine line)
    {
        if (TryGetLineSet(def, girlId, out var lineSet))
        {
            line = lineSet.dialogLines[GirlIdToLineIdToLineIndex[girlId][id]];
            return true;
        }

        line = null;
        return false;
    }
}
