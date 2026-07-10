namespace Hp2BaseMod;

/// <summary>
/// The <see cref="CodeDefinition"/> class is a data-driven container used by the game engine to define 
/// secret unlockable codes and system-level toggles. These definitions represent the "cheat" or "bonus" 
/// inputs entered via the cellphone's Code app to alter gameplay or unlock hidden features.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Configuration</b>
/// A <see cref="CodeDefinition"/> defines the specific sequence of characters a player must input 
/// and the category of the reward. The core engine uses these to perform string-matching 
/// validation when the "Submit" button is pressed in the UI.
/// </para>
/// 
/// <para>
/// <b>Serialized Fields</b>
/// <list type="bullet">
///   <item>
///     <description><b>codeType</b> (<see cref="CodeType"/>): Categorizes the code, usually as a 
///     Cheat or a Bonus, which dictates how the engine prioritizes or displays the result.</description>
///   </item>
///   <item>
///     <description><b>codeString</b> (string): The actual text (case-insensitive) the player must 
///     type to trigger the effect.</description>
///   </item>
///   <item>
///     <description><b>description</b> (string): The text displayed to the player after a 
///     successful activation, explaining what has been unlocked or changed.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="UiCellphoneAppCode"/>: The primary user interface. It handles input 
///     buffering and invokes the validation logic when a code is submitted.</description>
///   </item>
///   <item>
///     <description><see cref="CodeData"/>: The global data repository used to resolve code 
///     instances during the validation phase.</description>
///   </item>
///   <item>
///     <description><see cref="SaveData"/>: Contains the <c>unlockedCodes</c> collection, which 
///     persists the "unlocked" state of non-volatile codes across game sessions.</description>
///   </item>
///   <item>
///     <description><see cref="CodeUtility"/>: A utility class used to programmatically validate, 
///     lock, or unlock specific codes within a save file.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedCodeDefinition and Command Support</b>
/// The <see cref="ExpandedCodeDefinition"/> functionality, provided by the base mod, expands the 
/// code system into a functional developer console. 
/// 
/// While base game codes are limited to static unlocks, the expansion allows the 
/// <see cref="UiCellphoneAppCode"/> to intercept strings starting with the <c>/</c> prefix. 
/// These are processed as <b>Commands</b> rather than standard secret codes. 
/// 
/// Developers can register custom logic by implementing the <see cref="ICommand"/> interface 
/// and adding it via <see cref="ModInterface.AddCommand(ICommand)"/>. This allows the 
/// Code app to trigger complex C# methods (like spawning items, modifying affection, or 
/// clearing board states) with optional arguments passed directly through the text field.
/// </para>
/// </remarks>
[Expansion(typeof(CodeDefinition), HasModId = true)]
public partial class ExpandedCodeDefinition
{
    
}