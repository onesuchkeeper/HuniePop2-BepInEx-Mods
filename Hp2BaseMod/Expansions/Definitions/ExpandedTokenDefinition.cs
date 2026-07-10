namespace Hp2BaseMod;
/// <summary>
/// The <see cref="TokenDefinition"/> class is a data-driven configuration container that serves as the 
/// primary blueprint for all puzzle tokens used within the match-three gameplay engine. These definitions 
/// dictate token categorization, spawn probabilities, and the specific resources rewarded upon matching.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics: Spawn Logic and Weighting</b>
/// A core function of the <see cref="TokenDefinition"/> is determining board population through 
/// the <c>GetBaseWeight()</c> method. This logic calculates the final spawn probability of a token 
/// by combining its serialized base weight with a difficulty-specific offset. This allows the 
/// engine to dynamically adjust the board's composition (e.g., reducing the frequency of stamina 
/// tokens) based on the current puzzle's difficulty level.
/// </para>
/// 
/// <para>
/// <b>Resource Mapping</b>
/// Each definition maps the token to a specific <c>PuzzleResourceType</c> (such as Affection, 
/// Stamina, Sentiment, or Passion). When a <see cref="PuzzleMatch"/> is processed on the grid, 
/// the engine references the token's definition to determine which global or character-specific 
/// resource meter should be incremented.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="UiPuzzleGrid"/>: The primary environment where tokens are instantiated, 
///     moved, and matched. It uses the definition's weight data to populate empty slots [1].</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatus"/>: Receives the resource rewards derived from the 
///     token's categorization during a successful match [2, 3].</description>
///   </item>
///   <item>
///     <description><see cref="EnergyDefinition"/>: Linked to the token to provide visual feedback. 
///     When a token is matched, the engine spawns the corresponding "energy trail" particle 
///     defined for that token type [4, 5].</description>
///   </item>
///   <item>
///     <description><see cref="TokenCondition"/>: Used by the <see cref="AbilityManager"/> to filter 
///     specific subsets of tokens on the grid for manipulation (e.g., destroying all tokens of 
///     a specific <see cref="TokenDefinition"/> type) [6, 7].</description>
///   </item>
///   <item>
///     <description><c>TokenData</c>: The global repository used by the engine to resolve token 
///     blueprints at runtime [8].</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedTokenDefinition Functionality</b>
/// The <see cref="ExpandedTokenDefinition"/> integrates tokens into the modular data-modding 
/// pipeline, allowing for the introduction of custom token types with unique behaviors.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>TokenName Metadata:</b> Provides a dedicated field for human-readable 
///     identifiers used in debug logs and potential UI labels [9].</description>
///   </item>
///   <item>
///     <description><b>Modular Weighting Overrides:</b> Through <see cref="TokenDataMod"/>, 
///     developers can modify the base weights of existing tokens or define custom difficulty 
///     offsets to rebalance puzzle gameplay for specific mods [8].</description>
///   </item>
///   <item>
///     <description><b>RelativeId Integration:</b> Decouples tokens from the base game's 
///     integer-based lookup system. This ensures that custom tokens added by different mods 
///     can coexist on the same board without triggering ID collisions or registry errors.</description>
///   </item>
/// </list>
/// To apply these expansions, a <see cref="TokenDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(TokenDefinition), HasModId = true)]
public partial class ExpandedTokenDefinition
{
}