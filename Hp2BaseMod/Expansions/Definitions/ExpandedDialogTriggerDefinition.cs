using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="DialogTriggerDefinition"/> class is a data-driven blueprint used to define dialogue events 
/// and triggers. It acts as a primary configuration container for character speech patterns, 
/// determining which specific lines of text are selected and played in response to gameplay events.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Logic</b>
/// A <see cref="DialogTriggerDefinition"/> functions as a filterable repository of text. Instead of 
/// hardcoding dialogue into gameplay scripts, the engine uses these definitions to resolve which 
/// lines are appropriate for a given <see cref="GirlDefinition"/> at any specific moment. This 
/// allows for highly varied character reactions to universal events, such as accepting a gift, 
/// winning a round, or being greeted in the hub.
/// </para>
/// 
/// <para>
/// <b>Dialogue Resolution Pipeline</b>
/// The core logic resides within the <see cref="UiDoll"/> component. During a <c>ReadDialogTrigger</c> 
/// sequence, the engine passes a trigger instance to the doll. The definition then evaluates its internal 
/// collection of <c>DialogTriggerLineSet</c> objects to find a match for the doll's current character. 
/// Once matched, a line is randomly or sequentially selected and added to the dialogue queue for display.
/// </para>
/// 
/// <para>
/// <b>Serialized Fields</b>
/// <list type="bullet">
///   <item>
///     <description><b>forceType</b> (<c>DialogTriggerForceType</c>): Determines how the engine 
///     prioritizes this dialogue. It can dictate whether the dialogue forces a UI state change 
///     or overrides current doll animations.</description>
///   </item>
///   <item>
///     <description><b>LineSets</b> (List): A collection of character-specific data containers 
///     that map a <see cref="GirlDefinition"/> to their respective dialogue lines.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="UiDoll"/>: The primary runtime consumer. It invokes the resolution 
///     logic and handles the visual presentation of the resulting text.</description>
///   </item>
///   <item>
///     <description><see cref="GirlDefinition"/>: Used as a key during the resolution process to 
///     ensure the dialogue retrieved matches the character currently on screen.</description>
///   </item>
///   <item>
///     <description><c>DialogTriggerData</c>: The global repository used by the engine to 
///     resolve and retrieve these definitions at runtime.</description>
///   </item>
///   <item>
///     <description><c>DialogTriggerLineSet</c>: A sub-container within the definition that 
///     holds the actual string arrays for a specific girl.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedDialogTriggerDefinition Functionality</b>
/// The <see cref="ExpandedDialogTriggerDefinition"/> extends the system to support the mod's 
/// modular architecture. Its primary purpose is to decouple dialogue from the base game's 
/// fixed collection indexes, which are prone to collisions when multiple mods are present.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>RelativeId Integration:</b> Allows dialogue triggers to be defined 
///     within a mod's own namespace, preventing conflicts with base game triggers or other 
///     plugins.</description>
///   </item>
///   <item>
///     <description><b>Dynamic Injection:</b> Developers can use <see cref="DialogTriggerDataMod"/> 
///     to inject new lines into existing base game triggers (e.g., adding unique reactions 
///     for a custom girl to a standard "Date Success" event).</description>
///   </item>
///   <item>
///     <description><b>Character Mapping Overrides:</b> Through the 
///     <c>DialogTriggerDefinitionPatch</c>, the expansion overrides the native 
///     <c>GetLineSetByGirl</c> method. This ensures that even if a character appears in 
///     multiple pairings or custom mods, their dialogue is resolved via the modded 
///     <see cref="RelativeId"/> system rather than hardcoded enums.</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
[Expansion(typeof(DialogTriggerDefinition), HasModId = true)]
public partial class ExpandedDialogTriggerDefinition
{
    private Dictionary<RelativeId, IdIndexMap> _girlToLineIndexes = new();

    /// <summary>
    /// Given a girl, attempts to get their <see cref="DialogTriggerLineSet"/>
    /// </summary>
    public bool TryGetLineSet(DialogTriggerDefinition def, RelativeId girlId, out DialogTriggerLineSet lineSet)
    {
        var girlIndex = ExpandedGirlDefinition.DialogTriggerIndexes[girlId];
        lineSet = def.dialogLineSets.GetOrNew(girlIndex);
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
