using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="GirlPairDefinition"/> class is a data-driven blueprint that defines the logical pairing of two characters 
/// for double dates. These definitions contain relationship metadata, progression requirements, and designated environments 
/// for post-date narrative events.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Architecture</b>
/// A <see cref="GirlPairDefinition"/> serves as the bridge between individual character data and the double-date gameplay 
/// loop. While most pairings are randomized for the player's "Finder" app, the engine uses these static definitions to 
/// validate valid combinations and to handle unique narrative encounters. The most significant logical branch is the 
/// <b>specialPair</b> flag; when enabled, the pair is treated as a unique/boss encounter (such as The Nymphojinn) 
/// rather than a standard progression-based combination.
/// </para>
/// 
/// <para>
/// <b>Serialized Fields</b>
/// <list type="bullet">
///   <item>
///     <description><b>girlOneDefinition</b> (<see cref="GirlDefinition"/>): Reference to the primary character 
///     in the pair.</description>
///   </item>
///   <item>
///     <description><b>girlTwoDefinition</b> (<see cref="GirlDefinition"/>): Reference to the secondary character 
///     in the pair.</description>
///   </item>
///   <item>
///     <description><b>sexLocationDefinition</b> (<see cref="LocationDefinition"/>): Defines the specific environment 
///     where the threesome or bonus round occurs upon a successful date.</description>
///   </item>
///   <item>
///     <description><b>specialPair</b> (bool): A flag indicating if the pair is a unique boss fight, exempting 
///     it from standard randomized population logic.</description>
///   </item>
///   <item>
///     <description><b>favQuestions</b> (List): <b>[DEPRECATED]</b> A base-game list of preferred conversation 
///     topics. In modded environments, this field is ignored in favor of ID-based lookups to prevent index 
///     collisions between plugins.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="GirlDefinition"/>: The individual data containers for the characters that 
///     make up the pair.</description>
///   </item>
///   <item>
///     <description><see cref="LocationDefinition"/>: Used to configure both the date environment and 
///     the specific <c>sexLocationDefinition</c>.</description>
///   </item>
///   <item>
///     <description><see cref="TalkManager"/>: Processes the favorite question data during the date 
///     dialogue phase.</description>
///   </item>
///   <item>
///     <description><see cref="PuzzleStatus"/>: Tracks the state of the girls (stamina, passion) 
///     relative to their pairing during active gameplay.</description>
///   </item>
///   <item>
///     <description><see cref="GirlPairData"/>: The global data repository used to resolve 
///     pair definitions during population and scene transitions.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedGirlPairDefinition Functionality</b>
/// The <see cref="ExpandedGirlPairDefinition"/> is designed to solve the base game's heavy reliance on fixed collection 
/// indexes and hardcoded enums. In the original engine, favorite questions were tracked by their position in a 
/// static list, making it impossible for multiple mods to add new questions without overwriting each other.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Decoupled Question Logic:</b> Replaces the indexed <c>favQuestions</c> list with the 
///     <see cref="FavQuestionIdToAnswerId"/> dictionary. This allows the engine to map specific 
///     <see cref="QuestionDefinition"/> IDs to their intended answers using <see cref="RelativeId"/> 
///     namespaces, ensuring that modded questions never conflict with base game data.</description>
///   </item>
///   <item>
///     <description><b>Dynamic Pair Injection:</b> Through <see cref="GirlPairDataMod"/>, developers can 
///     inject new valid pairings into the engine (e.g., adding a custom girl to a base game pair) 
///     or override the <c>sexLocationDefinition</c> for existing pairs.</description>
///   </item>
///   <item>
///     <description><b>Custom Progression Hooks:</b> The expansion allows for the injection of 
///     procedural logic during relationship level-ups, enabling modders to trigger custom 
///     cutscenes or reward structures when a specific pair reaches a new milestone.</description>
///   </item>
/// </list>
/// To apply these expansions, a <see cref="GirlPairDataMod"/> must be registered via 
/// <see cref="ModInterface.DataMod.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(GirlPairDefinition), HasModId = true)]
public partial class ExpandedGirlPairDefinition
{
    /// <summary>
    /// Maps a pair's id to its style info.
    /// </summary>
    public PairStyleInfo PairStyle;

    /// <summary>
    /// Custom default failure cutscene
    /// </summary>
    public RelativeId CutsceneNormalFailureId = RelativeId.Default;

    /// <summary>
    /// Custom default success cutscene
    /// </summary>
    public RelativeId CutsceneNormalSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom compatible success cutscene
    /// </summary>
    public RelativeId CutsceneNormalCompatibleSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom attracted success cutscene
    /// </summary>
    public RelativeId CutsceneNormalAttractedSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom bonus new round cutscene
    /// </summary>
    public RelativeId CutsceneNormalBonusNewRoundId = RelativeId.Default;

    /// <summary>
    /// Custom bonus new round cutscene
    /// </summary>
    public RelativeId CutsceneNormalBonusSuccessId = RelativeId.Default;
}
