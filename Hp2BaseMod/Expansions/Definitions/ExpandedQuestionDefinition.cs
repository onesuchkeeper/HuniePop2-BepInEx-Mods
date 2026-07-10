using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="QuestionDefinition"/> class is a data-driven blueprint used to define conversation topics 
/// and "favorite questions" encountered during date and hub gameplay. These definitions serve as the primary 
/// configuration for character preferences and drive the preference-learning progression mechanics.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Gameplay Logic</b>
/// A <see cref="QuestionDefinition"/> acts as a configuration key for the <see cref="TalkManager"/>. 
/// During a date or hub conversation, the engine selects valid questions based on the characters 
/// present. When a player selects a question, the resulting response (Good or Bad) dictates 
/// character resource changes—such as sentiment or passion—and allows the player to "learn" the girl's 
/// preference. Once learned, the correct answer is permanently highlighted in the UI for future encounters.
/// </para>
/// 
/// <para>
/// <b>Workflow and Tracking Changes</b>
/// In the base game, the engine identifies and tracks questions using their index within the 
/// <c>Game.Session.Talk.favQuestionDefinitions</c> collection. The modded environment deprecates 
/// this indexed approach in favor of runtime IDs and <see cref="RelativeId"/> namespaces. 
/// This shift ensures that "Learned Favorites" are tracked globally via unique identifiers, 
/// allowing multiple mods to introduce new questions without causing index collisions or data corruption.
/// </para>
/// 
/// <para>
/// <b>Serialized Fields</b>
/// <list type="bullet">
///   <item>
///     <description><b>QuestionName</b> (string): The label or identifier for the topic (e.g., 
///     "Favorite Drink" or "Birthday").</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="TalkManager"/>: The core runtime processor. It manages dialogue 
///     state and uses these definitions to populate the player's conversation options during 
///     active dates.</description>
///   </item>
///   <item>
///     <description><see cref="PlayerFileGirl"/>: The character-specific persistence container. 
///     It tracks discovered preferences by storing question IDs in the <c>learnedFavs</c> 
///     collection.</description>
///   </item>
///   <item>
///     <description><see cref="UiAppFavAnswer"/>: The UI controller within the cellphone's 
///     Profile app. It consumes definition metadata to display icons and labels for 
///     character likes and dislikes.</description>
///   </item>
///   <item>
///     <description><see cref="GirlPairDefinition"/>: References these definitions to 
///     determine the valid pool of questions for specific character pairings.</description>
///   </item>
///   <item>
///     <description><c>QuestionData</c>: The global data repository used by the engine to 
///     resolve and retrieve question definitions at runtime.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedQuestionDefinition Functionality</b>
/// The <see cref="ExpandedQuestionDefinition"/> provides the infrastructure required to 
/// scale the conversation system beyond the base game's fixed constraints. 
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Dialog Trigger Mapping:</b> Utilizes the <see cref="DialogTriggerIndexes"/> 
///     (an <see cref="IdIndexMap"/>) to dynamically associate specific questions with 
///     character-specific reaction dialogue, enabling procedural reactions without 
///     modifying the <see cref="TalkManager"/> core logic.</description>
///   </item>
///   <item>
///     <description><b>Modular Addition:</b> Developers can use <see cref="FavQuestionDataMod"/> 
///     to inject entirely new topics (e.g., custom hobbies or physical traits) into the game. 
///     These are automatically integrated into the player's profile and discovery systems.</description>
///   </item>
///   <item>
///     <description><b>Decoupled Question Logic:</b> By moving away from the hardcoded 
///     <c>favQuestionDefinitions</c> array, the expansion allows for an unlimited number 
///     of custom questions, uniquely identified by their mod source to prevent conflicts.</description>
///   </item>
/// </list>
/// To apply these expansions, a <see cref="FavQuestionDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(QuestionDefinition), HasModId = true)]
public partial class ExpandedQuestionDefinition
{
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new();

    public IdIndexMap AnswerLookup => _answerLookup;
    private IdIndexMap _answerLookup = new();

    public string GetAnswer(QuestionDefinition def, RelativeId answerId) => def.questionAnswers[_answerLookup[answerId]];
}
