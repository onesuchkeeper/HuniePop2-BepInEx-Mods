namespace Hp2BaseMod;
/// <summary>
/// The <see cref="EnergyDefinition"/> class is a data-driven configuration container used by the game engine 
/// to define the visual assets and behavioral properties of energy particles. These particles, 
/// colloquially known as "energy trails," represent the physical manifestation of gameplay resources 
/// moving from matched tokens to their respective UI meters.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Visual Feedback</b>
/// An <see cref="EnergyDefinition"/> acts as a bridge between the match-three logic and the particle 
/// feedback system. When a player successfully matches tokens, the engine looks up the energy 
/// definition associated with that resource type (e.g., Affection, Stamina, or Sentiment) to 
/// instantiate the correct particle trail. These trails provide essential feedback by visually 
/// confirming that a match has been processed and and indicating which character resource is being replenished.
/// </para>
/// 
/// <para>
/// <b>Visual Pipeline</b>
/// When a <see cref="PuzzleSet"/> is consumed on the <see cref="UiPuzzleGrid"/>, the engine triggers 
/// a particle spawn. The <see cref="EnergyTrailBehavior"/> component is attached to these instances, 
/// using the definition's data to set properties such as the <c>TextMaterialName</c> (controlling 
/// shaders and colors) and pathing logic as they travel toward the character portraits.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="EnergyData"/>: The global data repository used to resolve 
///     particle definitions at runtime during puzzle consumption.</description>
///   </item>
///   <item>
///     <description><see cref="EnergyTrailBehavior"/>: The MonoBehaviour that manages the lifecycle, 
///     tweening, and collision detection of individual particle instances.</description>
///   </item>
///   <item>
///     <description><see cref="UiPuzzleGrid"/>: The origin point of the energy trails, where 
///     matches are detected and particle instantiation is invoked.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatus"/>: Receives the resource increment once the 
///     energy trail reaches its destination.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatusGirl"/>: Serves as the target destination for trails 
///     providing character-specific resources like stamina or traits.</description>
///   </item>
///   <item>
///     <description><see cref="TokenDefinition"/>: References energy types to determine 
///     which specific trail should be spawned when a token of a given type is matched.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedEnergyDefinition Functionality</b>
/// The <see cref="ExpandedEnergyDefinition"/> extends the system to support a more modular and 
/// stylistically varied feedback loop. 
/// 
/// Key features provided by the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Custom Resource Support:</b> Allows modders to define entirely new 
///     energy types (e.g., custom trait-based trails) that map to modded resource pools 
///     via the <see cref="EnergyTypes"/> system.</description>
///   </item>
///   <item>
///     <description><b>Enhanced UI Styling:</b> Through the <see cref="EnergyDataMod"/>, 
///     developers can modify the <c>TextMaterialName</c> to apply custom shaders or 
///     TextMeshPro materials to energy particles, enabling distinct visual identities 
///     for mod-specific effects.</description>
///   </item>
///   <item>
///     <description><b>Dynamic Injection:</b> New energy definitions can be registered 
///     into the engine without overwriting base game assets, preventing visual 
///     glitches when multiple mods attempt to modify the puzzle feedback system.</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
[Expansion(typeof(EnergyDefinition), HasModId = true)]
public partial class ExpandedEnergyDefinition
{
    
}