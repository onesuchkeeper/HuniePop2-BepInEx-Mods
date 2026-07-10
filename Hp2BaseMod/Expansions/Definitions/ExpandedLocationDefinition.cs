using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// The <see cref="LocationDefinition"/> class is a data-driven blueprint used to define environment properties, 
/// visual assets, and behavioral logic for the various settings in the game world. It acts as the primary 
/// configuration for SIM (story), HUB (hotel), and DATE environments.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Background Logic</b>
/// A <see cref="LocationDefinition"/> functions as a container for environmental metadata. The most critical 
/// functional component is the <c>backgrounds</c> sprite collection. The engine uses this list to provide 
/// dynamic time-of-day visuals. 
/// </para>
/// 
/// <para>
/// <b>Visual Pipeline: GetDaytimeBackground()</b>
/// The <c>GetDaytimeBackground()</c> method resolves which sprite to display by mapping the current 
/// <see cref="ClockDaytimeType"/> to an index in the <c>backgrounds</c> list. Under standard implementation:
/// <list type="bullet">
///   <item><description>Index 0: Morning</description></item>
///   <item><description>Index 1: Afternoon</description></item>
///   <item><description>Index 2: Evening</description></item>
///   <item><description>Index 3: Night</description></item>
/// </list>
/// If the current time index exceeds the number of available sprites, the engine clamps to the nearest 
/// valid background or defaults to a specific fallback defined in the manager.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="LocationManager"/>: The core processor that tracks the current location, 
///     manages <see cref="LocationTransitionNormal"/> sequences, and provides the global time state 
///     required for background resolution.</description>
///   </item>
///   <item>
///     <description><see cref="UiBackgroundArt"/>: The UI component responsible for rendering the 
///     actual sprite returned by the definition.</description>
///   </item>
///   <item>
///     <description><see cref="UiBackgroundBar"/>: Consumes definition data to display the location 
///     name and character labels in the HUD.</description>
///   </item>
///   <item>
///     <description><see cref="LocationType"/>: An enum within the definition that dictates 
///     gameplay rules, such as whether the phone is accessible or if girls appear in pairs.</description>
///   </item>
///   <item>
///     <description><see cref="GirlHairstyleSubDefinition"/> and <see cref="GirlHairstyleSubDefinition"/>: 
///     Referenced by the definition to dictate the required attire for characters appearing 
///     at that location.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedLocationDefinition Functionality</b>
/// The <see cref="ExpandedLocationDefinition"/> overcomes the base game's rigid categorization and hardcoded 
/// style mapping. It introduces more granular control over character presentation and environment 
/// registration.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Modular Style Defaults:</b> Replaces the original <c>dateGirlStyleType</c> 
///     enum with the <see cref="DefaultHairstyle"/> and <see cref="DefaultOutfit"/> fields. This 
///     allows modded locations to specify custom style categories without being restricted to 
///     the base game's "Activity," "Romantic," or "Party" presets.</description>
///   </item>
///   <item>
///     <description><b>Dynamic Environment Injection:</b> Through <see cref="LocationDataMod"/>, 
///     developers can register entirely new environments into the <see cref="LocationManager"/> 
///     without overwriting existing assets. This is essential for mods like "Extra Locations" 
///     or "HuniePop Ultimate."</description>
///   </item>
///   <item>
///     <description><b>RelativeId Integration:</b> Locations are identified via <see cref="RelativeId"/> 
///     rather than integer-based database indexes. This prevents ID collisions between multiple mods 
///     that add new travel destinations or date spots.</description>
///   </item>
///   <item>
///     <description><b>Sequence Hooking:</b> Works alongside the <see cref="LocationArriveSequenceArgs"/> 
///     system to allow modded locations to play unique transition animations or procedural 
///     narrative events upon arrival.</description>
///   </item>
/// </list>
/// To apply these expansions, a <see cref="LocationDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(LocationDefinition), HasModId = true)]
public partial class ExpandedLocationDefinition
{
    /// <summary>
    /// Maps girl id to the greeting used at this location
    /// </summary>
    public Dictionary<RelativeId, RelativeId> GirlIdToLocationGreetingLineId = new();

    /// <summary>
    /// Times when this location can be used
    /// </summary>
    public List<ClockDaytimeType> DateTimes;

    /// <summary>
    /// If this location can be used for non-stop dates
    /// </summary>
    public bool AllowNonStop;

    /// <summary>
    /// If this location can be used for standard dates
    /// </summary>
    public bool AllowNormal;

    /// <summary>
    /// If this location is only available after defeating the nymphojinn
    /// </summary>
    public bool PostBoss;

    /// <summary>
    /// If the location can be used for a normal date at the current time with the current story progress
    /// </summary>
    public bool IsValidForNormalDate() => IsValidForNormalDate((ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4));

    /// <summary>
    /// If the location can be used for a normal date at the specified time with the current story progress
    /// </summary>
    public bool IsValidForNormalDate(ClockDaytimeType time)
        => AllowNormal
            && (!PostBoss || Game.Persistence.playerFile.storyProgress >= 12)
            && DateTimes.Contains(time);

    public RelativeId? DefaultStyle;
}
