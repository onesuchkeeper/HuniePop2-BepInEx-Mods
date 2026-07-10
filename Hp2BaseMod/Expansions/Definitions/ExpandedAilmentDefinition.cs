using System;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="AilmentDefinition"/> class serves as the master blueprint for status effects encountered during puzzle gameplay. 
/// It acts as a primary configuration container that dictates the lifecycle and behavioral logic of status effects, 
/// which are then processed by the game engine's management systems.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Logic</b>
/// While an <see cref="AilmentDefinition"/> provides the static data, the engine utilizes it to determine the 
/// conditions under which a status effect is enabled, disabled, or triggered. The base game logic is 
/// primarily data-driven, using these definitions to execute specific sequences—such as triggering 
/// specialized <see cref="AbilityDefinition"/>s when specific puzzle conditions are met.
/// </para>
/// 
/// <para>
/// <b>Evaluation Pipeline</b>
/// The game engine evaluates active ailments across several distinct gameplay phases. This "execution pipeline" 
/// allows status effects to intercept and modify the board or character state during:
/// <list type="bullet">
///   <item><description><b>Pre-Move:</b> Before the player swaps tokens.</description></item>
///   <item><description><b>Pre-Match:</b> Before a match is processed for rewards.</description></item>
///   <item><description><b>Pre-Gift:</b> When the player attempts to give an item to a girl.</description></item>
///   <item><description><b>Focus Change:</b> When the focused character swaps.</description></item>
///   <item><description><b>Board Settling:</b> After tokens have fallen and the grid is stable.</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="AilmentManager"/>: The central processor responsible for activation, 
///     sorting, and evaluation of all status effects during a puzzle session.</description>
///   </item>
///   <item>
///     <description><see cref="Ailment"/>: The runtime instance created from this definition. It exists 
///     only for the duration of a puzzle and tracks persistent state like turn counters or trigger flags.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatusGirl"/>: The character-state container that "owns" active 
///     ailments and serves as the target for resource-based effects (stamina, passion, etc.).</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatus"/>: The global state holder used by the management system 
///     to track moves, affection goals, and round-over conditions.</description>
///   </item>
///   <item>
///     <description><see cref="UiPuzzleGrid"/>: The target system for ailments that manipulate token 
///     placement, matching logic, or board settling behaviors.</description>
///   </item>
///   <item>
///     <description><see cref="AbilityDefinition"/>: Often referenced within the ailment for 
///     automatic "enable/disable" effects that execute specific ability steps when the ailment starts or ends.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedAilmentDefinition Functionality</b>
/// The <see cref="ExpandedAilmentDefinition"/> introduces an extensible C# scripting layer to the status effect system. 
/// In the base game, ailments are restricted to pre-defined data steps. The expansion allows developers 
/// to attach a procedural <see cref="IScriptedAilment"/> to the definition.
/// 
/// This procedural layer allows for:
/// <list type="number">
///   <item>
///     <description><b>Custom Lifecycle Callbacks:</b> Executing complex C# code via 
///     <see cref="IScriptedAilment.OnEnable"/> and <see cref="IScriptedAilment.OnDisable"/>.</description>
///   </item>
///   <item>
///     <description><b>Logic Bypassing:</b> The ability to augment or entirely replace the hardcoded 
///     evaluation loops in the <see cref="AilmentManager"/>.</description>
///   </item>
///   <item>
///     <description><b>Contextual Access:</b> Direct access to runtime puzzle context (such as match reward 
///     modifiers or move-set properties) that the base game's data-driven steps cannot reach.</description>
///   </item>
/// </list>
/// These behaviors are applied by setting the <see cref="ScriptedAilmentFactory"/> within a 
/// <see cref="AilmentDataMod"/> and registering it via <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(AilmentDefinition), HasModId = true)]
public partial class ExpandedAilmentDefinition
{
    /// <summary>
    /// Factory that produces the IScriptedAilment for a new Ailment instance built from this definition.
    /// Null if this is a purely data-driven ailment.
    /// Set this via <see cref="ScriptedAilmentDataMod"/>.
    /// </summary>
    public Func<Ailment, IScriptedAilment> ScriptedAilmentFactory;
}