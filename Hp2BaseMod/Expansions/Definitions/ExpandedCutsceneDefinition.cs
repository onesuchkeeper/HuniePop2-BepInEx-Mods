namespace Hp2BaseMod;

/// <summary>
/// The <see cref="CutsceneDefinition"/> class serves as the master blueprint for scripted narrative sequences, 
/// transitions, and round-based events. It acts as a static configuration container for a collection of 
/// modular steps that dictate the visual and logical flow of the game's non-interactive segments.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Step Architecture</b>
/// A <see cref="CutsceneDefinition"/> is primarily a collection of <see cref="CutsceneStepSubDefinition"/> objects 
/// stored in a linear list. While narrative sequences are generally linear, modularity is achieved through 
/// specialized step types—such as <see cref="CutsceneBranchSubDefinition"/>—that allow for conditional logic 
/// based on player choices or game state. The definition also includes a <see cref="CutsceneCleanUpType"/> 
/// field, which determines how the engine resets UI elements, character dolls, and menus once the 
/// sequence concludes.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="CutsceneManager"/>: The core runtime processor. it maintains the execution 
///     state, tracks the current step index, and manages standby/proceed logic for the active 
///     definition.</description>
///   </item>
///   <item>
///     <description><see cref="CutsceneStepSubDefinition"/>: The abstract building block for all actions 
///     within a cutscene, ranging from dialogue triggers and camera shakes to logic branches.</description>
///   </item>
///   <item>
///     <description><see cref="UiDoll"/>: The primary target for visual steps. The definition dictates 
///     when dolls change expressions, visibility, or play specific dialogue triggers.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleManager"/>: Coordinates with cutscenes to handle gameplay lifecycle 
///     transitions, such as the sequences played upon round success, failure, or the start of a new 
///     round.</description>
///   </item>
///   <item>
///     <description><see cref="WindowManager"/>: Interacted with by specialized steps to programmatically 
///     toggle or refresh UI windows, such as the photo album or item notification screens, mid-sequence.</description>
///   </item>
///   <item>
///     <description><see cref="CutsceneData"/>: The global data repository used by the engine to resolve 
///     and retrieve cutscene definitions at runtime.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedCutsceneDefinition Functionality</b>
/// The <see cref="ExpandedCutsceneDefinition"/> introduces the ability to inject procedural C# logic into 
/// previously static narrative pipelines. This expansion is essential for complex modded behaviors 
/// that require real-time context or interaction with custom game systems.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Step Manipulation:</b> Developers can use <see cref="CutsceneDataMod"/> to 
///     dynamically append, prepend, or replace steps in existing base game cutscenes without 
///     altering the original assets.</description>
///   </item>
///   <item>
///     <description><b>Functional Steps:</b> By implementing the <see cref="IFunctionalCutsceneStep"/> 
///     interface, developers can create steps that execute arbitrary code. Unlike standard data-driven 
///     steps, a functional step must manually signal the <see cref="CutsceneManager"/> that it has 
///     finished by invoking its completion event.</description>
///   </item>
///   <item>
///     <description><b>Utility Integration:</b> The expansion works alongside <see cref="CutsceneStepUtility"/> 
///     to facilitate the creation of complex step sequences—such as centering dolls, refocusing 
///     puzzle targets, or displaying custom banner text—via a simplified API.</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
[Expansion(typeof(CutsceneDefinition), HasModId = true)]
public partial class ExpandedCutsceneDefinition
{
    
}