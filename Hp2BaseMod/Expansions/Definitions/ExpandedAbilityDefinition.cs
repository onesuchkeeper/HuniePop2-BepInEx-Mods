using System;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="AbilityDefinition"/> class is a data-driven blueprint used to define specialized puzzle actions. 
/// These definitions dictate the behavior of active abilities used during puzzle gameplay, detailing the 
/// sequence of effects to be applied to the game board or character states.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Architecture</b>
/// An <see cref="AbilityDefinition"/> acts as a static configuration file containing a list of 
/// <see cref="AbilityStepSubDefinition"/> objects. These steps are processed sequentially by the 
/// <see cref="AbilityManager"/> to execute complex actions. The logic is primarily data-driven, 
/// allowing the engine to calculate target values dynamically—such as random ranges, character-trait 
/// metrics (e.g., current Passion), or counts of specific tokens on the board—without requiring 
/// unique C# classes for every ability type.
/// </para>
/// 
/// <para>
/// <b>Execution Pipeline</b>
/// When an ability is triggered, the engine replicates the definition into an active <see cref="Ability"/> 
/// instance. The <see cref="AbilityManager"/> then iterates through the defined steps, applying 
/// <see cref="TokenCondition"/> filters to find valid subsets of tokens for manipulation. These operations 
/// include consuming, destroying, powering up, or spawning tokens in specific rows, columns, or patterns.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="AbilityManager"/>: The core processor that parses the definition and 
///     orchestrates the execution pipeline.</description>
///   </item>
///   <item>
///     <description><see cref="UiPuzzleGrid"/>: The primary target for board-based effects. The 
///     definition provides the parameters used to mutate token states or grid layout.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatus"/>: Used to modify global puzzle resources such as moves, 
///     affection, or sentiment based on ability outcomes.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatusGirl"/>: The target for character-specific effects, 
///     including stamina recovery or the application of status <see cref="Ailment"/>s.</description>
///   </item>
///   <item>
///     <description><see cref="AbilityStepSubDefinition"/>: The individual building blocks that make up 
///      the sequence of actions within the definition.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedAbilityDefinition Functionality</b>
/// The <see cref="ExpandedAbilityDefinition"/> introduces a procedural C# layer into the previously 
/// restricted data-driven pipeline. By utilizing the <see cref="ScriptedAbilityFactory"/>, 
/// developers can attach an <see cref="IScriptedAbility"/> to an ability instance. 
/// 
/// This allows for three specific points of intervention:
/// <list type="number">
///   <item>
///     <description><b>Pre-Perform:</b> Logic that executes before any steps run, providing a 
///     mechanism to validate conditions or abort the ability entirely.</description>
///   </item>
///   <item>
///     <description><b>Replace-Perform:</b> A powerful override that allows a script to entirely 
///     short-circuit the native step-based switch block, replacing it with complex, code-driven 
///     behaviors that the base game's data steps cannot achieve.</description>
///   </item>
///   <item>
///     <description><b>Post-Perform:</b> Logic that executes after the standard steps are 
///     complete, used to manipulate final results or trigger secondary game events.</description>
///   </item>
/// </list>
/// To apply these expansions, an <see cref="AbilityDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>

[Expansion(typeof(AbilityDefinition), HasModId = true)]
public partial class ExpandedAbilityDefinition
{
    /// <summary>
    /// Factory invoked once per <see cref="Ability"/> construction to produce scripted behaviour.
    /// Receives the newly constructed Ability so the factory can capture instance-specific context.
    /// Null if this is a purely data-driven ability.
    /// </summary>
    public Func<Ability, IScriptedAbility> ScriptedAbilityFactory;
}