namespace Hp2BaseMod;
/// <summary>
/// The <see cref="PhotoDefinition"/> class is a data-driven blueprint that defines the visual assets and metadata for collectible character photos. 
/// It acts as the primary configuration for both the thumbnail previews and the full-resolution "Big Photos" displayed in the character album 
/// or triggered during narrative events [1].
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Asset Configuration</b>
/// A <see cref="PhotoDefinition"/> functions as a container for sprite references. Each definition typically includes a thumbnail sprite used for 
/// the album grid and a collection of full-sized images. These high-resolution assets are intended for full-screen display during specific 
/// gameplay milestones, such as completing a date or reaching a relationship threshold [1, 2].
/// </para>
/// 
/// <para>
/// <b>Functional Logic: GetBigPhotoImage(int index)</b>
/// The core functional method of this class, <c>GetBigPhotoImage(int index)</c>, resolves which full-sized sprite to render. 
/// The engine typically uses index 0 for the standard version of a photo and index 1 for the "Alt" version. 
/// The result is determined by checking the global game state to see if alternate versions are currently enabled or unlocked for 
/// the character [2].
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="PhotoData"/>: The global data repository used by the engine to resolve and retrieve photo 
///     definitions at runtime [2].</description>
///   </item>
///   <item>
///     <description><see cref="UiWindowPhotos"/>: The primary UI controller for the Photo Album. It uses the definition to 
///     populate the grid and handle full-screen transitions via the <c>RefreshBigPhoto</c> method [3-5].</description>
///   </item>
///   <item>
///     <description><see cref="UiPhotoSlot"/>: The UI component representing a single thumbnail entry in the album. It maps 
///     the <see cref="PhotoDefinition"/> metadata to a clickable button [6].</description>
///   </item>
///   <item>
///     <description><see cref="WindowManager"/>: Coordinates with the photo system to programmatically toggle or refresh 
///     the album view during gameplay [3, 7].</description>
///   </item>
///   <item>
///     <description><see cref="CutsceneManager"/>: Invokes photo displays through specialized steps like 
///     <see cref="ShowDatePhotoCutsceneStep"/> and <see cref="ShowSexPhotoCutsceneStep"/> [3, 7, 8].</description>
///   </item>
///   <item>
///     <description><c>SaveData</c>: Persists the "unlocked" state of photos, ensuring that collected art remains 
///     accessible across game sessions [1, 2].</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedPhotoDefinition Functionality</b>
/// The <see cref="ExpandedPhotoDefinition"/> provides the infrastructure required to introduce new collectible art into 
/// the game without overwriting original assets. This expansion is critical for mods that add new characters, outfits, 
/// or narrative scenes [2].
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Dynamic Photo Injection:</b> Developers can use <see cref="PhotoDataMod"/> to register entirely 
///     new photo definitions into the engine. This allows for modular art additions that do not conflict with the 
///     base game's static data [2, 9].</description>
///   </item>
///   <item>
///     <description><b>RelativeId Integration:</b> Decouples photos from the base game's fixed integer-based 
///     lookups. By utilizing <see cref="RelativeId"/> namespaces, multiple mods can introduce "Photo 0" 
///     simultaneously without data collisions [2].</description>
///   </item>
///   <item>
///     <description><b>Alt Image Overrides:</b> The expansion allows for procedural redirection of the 
///     <c>GetBigPhotoImage</c> logic. This enables modders to implement complex "Alt" version requirements, 
///     such as photos that change based on specific modded flags (e.g., the "Female Jizz" toggle) [2, 10].</description>
///   </item>
///   <item>
///     <description><b>Custom Resolution Support:</b> Works alongside <see cref="PhotoRegistrar"/> to ensure that 
///     modded thumbnails and big photos are processed with the correct aspect ratios and dimensions for 
///     consistent UI layout [11].</description>
///   </item>
/// </list>
/// To apply these expansions, a <see cref="PhotoDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(PhotoDefinition), HasModId = true)]
public partial class ExpandedPhotoDefinition
{
    
}