# Events and Game Lifecycle (`ModEvents`)

To make this all work, the Hp2BaseMod has had to patch and overwrite many base game methods. Because of this, it is encouraged that dependant mods avoid creating their own harmony patches when possible and instead use the Hp2BaseMod's events.

`Hp2BaseMod` provides a centralized event pipeline via `ModInterface.Events`. This allows dependant mods to subscribe to critical game lifecycle hooks, intercept or cancel transitions (such as location arrivals and departures), modify store products, adjust girl outfit styles on the fly, and react to date/puzzle milestones without writing manual Harmony patches.

---

## 1. Overview of `ModEvents`

All events are exposed through static access via `ModInterface.Events`. Subscriptions follow standard C# event patterns and should generally be set up during plugin initialization (`Awake`) and cleaned up on unload (`OnDestroy`).

```csharp
using BepInEx;
using Hp2BaseMod;

namespace MyCustomHp2Mod
{
    public class MyModPlugin : Hp2BaseModPlugin
    {
        public MyModPlugin() : base(MyPluginInfo.PLUGIN_GUID) { }

        protected override void Awake()
        {
            base.Awake();

            // Subscribe to events
            ModInterface.Events.PreLocationArrive += OnPreLocationArrive;
            ModInterface.Events.PopulateStoreProducts += OnPopulateStoreProducts;
        }

        private void OnDestroy()
        {
            // Clean up subscriptions
            ModInterface.Events.PreLocationArrive -= OnPreLocationArrive;
            ModInterface.Events.PopulateStoreProducts -= OnPopulateStoreProducts;
        }

        private void OnPreLocationArrive(LocationArriveArgs args)
        {
            ModInterface.Log.Message($"Traveling to: {args.locationDef.locationName}");
        }

        private void OnPopulateStoreProducts(StoreProductsPopulateArgs args)
        {
            // Custom store product logic
        }
    }
}
```

## 2. Initialization & Data Mods Lifecycle

These events signal key phases in `Hp2BaseMod`'s startup and save file initialization sequence.

| Event | Delegate Signature | Description |
| :--- | :--- | :--- |
| `PreDataMods` | `Action` | Triggered right before `DataModRegistry` compiles registered data mods into runtime game definitions. |
| `PostDataMods` | `Action` | Triggered after all data mods have compiled. **Do not access `ModInterface.GameData` or vanilla `Game.Data` before this fires.** |
| `PreLoadPlayerFile` | `Action<PlayerFile>` | Fired when a save file is about to be loaded from disk into memory. |
| `PrePersistenceReset` | `Action<SaveData>` | Fired before persistent data reset occurs (useful for initializing custom save fields). |
| `PostPersistenceReset` | `Action<SaveData>` | Fired after persistent data reset completes. |

---

## 3. Location & Travel Lifecycle Events

Location transitions move the player between Sim locations, Dates, and the Hub. `Hp2BaseMod` exposes hooks at every phase of travel, allowing you to inspect, modify, or completely cancel movement.

### Location Events Reference

| Event | Argument Type | Description |
| :--- | :--- | :--- |
| `PreLocationArrive` | `LocationArriveArgs` | Fired before arriving at a location. Set `args.Canceled = true` to abort arrival. |
| `LocationArriveSequence` | `LocationArriveSequenceArgs` | Fired during arrival animation assembly. Allows appending/modifying DOTween sequences. |
| `PreLocationSettled` | `LocationSettledArgs` | Fired after arrival transition finishes, just before location UI/logic initializes. |
| `PreLocationDepart` | `LocationDepartArgs` | Fired before departing the current location. Set `args.Canceled = true` to abort departure. |
| `LocationDepartSequence` | `LocationDepartSequenceArgs` | Fired during departure animation assembly. |

### Example: Canceling Travel & Overriding Arrival Arguments

```csharp
private void OnPreLocationArrive(LocationArriveArgs args)
{
    // Inspect arrival context
    ModInterface.Log.Message($"Player traveling from {args.previousLocationDef?.locationName} to {args.locationDef.locationName}");

    // Cancel arrival under specific conditions
    if (args.locationDef.locationName == "Secret Beach" && !PlayerHasKey())
    {
        args.Canceled = true; // Aborts location arrival completely
        return;
    }

    // Override cellphone position or meeting cutscene
    args.cellphoneOnLeft = true; // Shifts cellphone UI to the left side
}
```

## 4. Date & Location Selection Events

When initiating dates from meeting locations or selecting pairs in the finder app:

### Key Selection Events

* **`DateLocationSelected` (`DateLocationSelectedArgs`)**: Triggered when selecting the Date action bubble at a meeting location.
  * `args.Location`: Target `LocationDefinition` for the date (can be overridden).
  * `args.PlayerPoints`, `args.LeftPoints`, `args.RightPoints`: Relationship points awarded on date start.
  * `args.LeftStaminaGain`, `args.RightStaminaGain`: Stamina awarded to each girl upon starting the date.
  * `args.DenyDate`: Set to `true` to reject the date request and play a rejection sound.

* **`FinderSlotsPopulate` (`FinderSlotPopulateEventArgs`)**: Fired when populating girl pairs in the location finder app.
  * Exposes `SexPool`, `IntroPool`, `MeetingPool`, `CompatiblePool`, `LoversPool`, `AttractedPool`, and `LocationPool`.
  * Helper methods: `args.RemoveGirlFromAllPools(RelativeId)` and `args.RemovePairFromAllPools(RelativeId)`.

```csharp
private void OnDateLocationSelected(DateLocationSelectedArgs args)
{
    // Grant extra stamina when starting a date
    args.LeftStaminaGain += 1;
    args.RightStaminaGain += 1;

    // Deny date under custom rules
    if (IsGirlExhausted())
    {
        args.DenyDate = true;
    }
}
```

## 5. Store Restocking (`PopulateStoreProducts`)

Fired whenever store inventory is regenerated (or during daily restocks).

```csharp
private void OnPopulateStoreProducts(StoreProductsPopulateArgs args)
{
    // Access item categories dictionary keyed by RelativeId
    // Modifiers can inspect or inject custom item categories into the store selection pool
    foreach (var categoryPair in args.ItemCategories)
    {
        RelativeId categoryId = categoryPair.Key;
        Category<ExpandedItemDefinition> category = categoryPair.Value;

        ModInterface.Log.Message($"Store category {categoryId}: pool size {category.Pool.Count}");
    }
}
```

## 6. Style & Doll Reset Events

`Hp2BaseMod` dynamically controls girl outfits, hairstyles, and positions across different game states (Sim, Date, Hub).

* **`RequestStyleChange` (`RequestStyleChangeEventArgs`)**: Fired when the framework determines an outfit/hairstyle change for a girl.
  * `args.ApplyChance`: Chance (`0.0` to `1.0`) for the style change to apply.
  * `args.Style`: `GirlStyleInfo` containing target `OutfitId` and `HairstyleId`.
  * Modders can set `args.ApplyChance = 1f` or swap `args.Style` to force specific modded outfits.

* **`PreDateDollReset` (`PreDateDollResetArgs`)**: Fired before resetting dolls for a date. Indicates whether style resolution is defaulting to `Sex`, `Location`, or `File` styles.
* **`RandomDollSelected` (`RandomDollSelectedArgs`)**: Fired when a random doll is selected for cutscenes or dialogue triggers. Allows overriding `args.SelectedDoll`.

## 7. Save & Code Events

| Event | Argument Type | Description |
| :--- | :--- | :--- |
| `PreGameSave` | `Action` | Fired before saving game state to disk. Ideal for flushing custom mod state into `ModSaveData`. |
| `PostGameSave` | `Action` | Fired after save completes. |
| `PostCodeSubmitted` | `Action<CodeDefinition>` | Fired after a developer code is submitted in the terminal. |