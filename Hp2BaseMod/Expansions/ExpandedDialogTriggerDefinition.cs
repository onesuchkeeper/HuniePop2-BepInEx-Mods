using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

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

    public DialogTriggerLineSet GetLineSet(DialogTriggerDefinition def, RelativeId girlId)
    {
        var girlExpansion = ExpandedGirlDefinition.Get(girlId);

        if (def.dialogLineSets[girlExpansion.DialogTriggerIndex].dialogLines.Count > 0)
        {
            return def.dialogLineSets[girlExpansion.DialogTriggerIndex];
        }

        if (def.dialogLineSets.FirstOrDefault()?.dialogLines.Count > 0)
        {
            return def.dialogLineSets[0];
        }

        return null;
    }

    public DialogLine GetLine(DialogTriggerDefinition def, RelativeId girlId, RelativeId id)
        => GetLineSet(def, girlId).dialogLines[GirlIdToLineIdToLineIndex[girlId][id]];
}
