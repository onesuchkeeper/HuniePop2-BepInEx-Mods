namespace Hp2BaseMod;

/// <summary>
/// The <see cref="ItemDefinition"/> class is a data-driven blueprint that defines the properties, 
/// categories, and functional metadata for every collectible object in the game world. This includes 
/// player gifts, consumables, baggage items, and special quest-related objects.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <b>System Mechanics and Categorization</b>
/// An <see cref="ItemDefinition"/> acts as the primary configuration container for items used across 
/// puzzle and hub gameplay. Items are categorized into several distinct functional groups, such as 
/// Gifts (Unique, Date, and Food), Smoothies (resource-specific boosters), and Baggage (character-specific 
/// obstacles or requirements). The engine uses these definitions to determine an item's monetary value, 
/// its effects on character resources (e.g., stamina or affection), and its visual representation in 
/// various UI contexts.
/// </para>
/// 
/// <para>
/// <b>Functional Logic: Category Resolution</b>
/// The core engine provides the <c>GetCategoryName()</c> method to generate human-readable labels for 
/// the UI (e.g., "Food • Japanese"). In the base game implementation, this logic is strictly bound 
/// to hardcoded enums, which limits the engine's ability to describe custom item types. The 
/// <see cref="ItemDefinition_Ext"/> and associated patches modernize this by prioritizing the 
/// <c>categoryDescription</c> field, allowing for procedural or mod-defined category names that 
/// bypass the original enum restrictions.
/// </para>
/// 
/// <para>
/// <b>Interacting Classes</b>
/// <list type="bullet">
///   <item>
///     <description><see cref="GiftManager"/>: The primary gameplay processor that executes the 
///     logic for giving items to dolls, calculating resource gains or rejections based on 
///     the definition's traits.</description>
///   </item>
///   <item>
///     <description><see cref="ItemSlotBehavior"/>: The MonoBehaviour responsible for the 
///     visual state of item slots. It uses the definition to load icons and trigger 
///     context-sensitive tooltips.</description>
///   </item>
///   <item>
///     <description><see cref="UiTooltipItem"/>: Consumes definition data to render detailed 
///     descriptions and resource modifiers when the player hovers over an item.</description>
///   </item>
///   <item>
///     <description><see cref="UiAppStoreSlot"/> and <see cref="UiAppDisplaySlot"/>: UI components 
///     used to represent items within the store or character-specific collection menus.</description>
///   </item>
///   <item>
///     <description><see cref="UiCellphoneInventorySlot"/>: Manages the display and selection 
///     of items within the player's mobile inventory.</description>
///   </item>
///   <item>
///     <description><c>PlayerFile</c>: Persists the "owned" or "unlocked" state of items 
///     derived from these definitions across save sessions.</description>
///   </item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>ExpandedItemDefinition Functionality</b>
/// The <see cref="ExpandedItemDefinition"/> facilitates a modular approach to item expansion, 
/// ensuring that multiple plugins can introduce new items without colliding with base game data.
/// 
/// Key features of the expansion include:
/// <list type="bullet">
///   <item>
///     <description><b>Dynamic Category Injection:</b> By overriding the native enum-based 
///     category logic, the expansion allows developers to use the <c>categoryDescription</c> 
///     string to define entirely new item classifications that the base UI can still 
///     render correctly.</description>
///   </item>
///   <item>
///     <description><b>RelativeId Integration:</b> Decouples items from the base game's 
///     integer-based lookup system via the <c>ModId()</c> extension. This ensures that 
///     modded items are identified via their unique source namespace, preventing 
///     conflicts in the store or inventory.</description>
///   </item>
///   <item>
///     <description><b>Priority-Based Store Population:</b> Works alongside the 
///     modded <c>PopulateStoreProducts</c> logic to allow modded items to be inserted 
///     into the store with specific priority levels (e.g., placing custom smoothies 
///     above standard food items).</description>
///   </item>
/// </list>
/// To apply these expansions, an <see cref="ItemDataMod"/> must be registered via 
/// <see cref="ModInterface.AddDataMod(IGameDataMod)"/>.
/// </para>
/// </remarks>
[Expansion(typeof(ItemDefinition), HasModId = true)]
public partial class ExpandedItemDefinition
{
    
}