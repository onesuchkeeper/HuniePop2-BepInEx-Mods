# 02. Data Modding & Relative ID System

In **Hp2BaseMod**, custom game content (Girls, Locations, Items, Cutscenes, Abilities, etc.) is registered and managed through a data abstraction layer. This guide explains how `Hp2BaseMod` avoids ID collisions using `RelativeId`, how runtime IDs are allocated, how to register content using `DataModRegistry`, and how to query game data using `GameDefinitionProvider`.

## 1. The `RelativeId` Concept & Runtime Mapping

Vanilla *HuniePop 2* references game definitions using simple integer IDs (`0`, `1`, `2`, ...). If multiple mods created new items or girls using static integer IDs, they would collide and overwrite each other's data.

To solve this, `Hp2BaseMod` introduces **`RelativeId`**, a composite identifier pair.

### Components of a `RelativeId`
* **`SourceId`** (`int`): An integer assigned to a mod's unique BepInEx `PLUGIN_GUID`.
  * Vanilla game definitions always have a `SourceId` of `-1` (represented as `"HP2"`).
  * Dependant mods obtain their unique `SourceId` by calling `ModInterface.GetSourceId(PLUGIN_GUID)`.
* **`LocalId`** (`int`): An integer chosen locally by the mod developer (e.g., `0`, `1`, `2`, ...) to differentiate definitions created within that specific mod.

### Static Defaults
* `RelativeId.Default`: Represents an unassigned or invalid ID (`SourceId = -1`, `LocalId = -1`).
* `RelativeId.Zero`: Represents vanilla zero (`SourceId = -1`, `LocalId = 0`).

### Runtime ID Allocation (`1,000,000+`)
During game initialization, `ModData` translates each modded `RelativeId` into a unique runtime integer ID:
* Vanilla definitions retain their original integer IDs (`0` to `999,999`).
* Modded definitions are dynamically assigned runtime IDs starting at **`1,000,000`** sequentially per `GameDataType`.

```csharp
// Example RelativeId definition
public static class MyModLocations 
{
    // If you extend Hp2BaseModPlugin, you can just use Plugin.ModId which is the same thing
    public static readonly int ModId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
    
    // RelativeId with SourceId = ModId, LocalId = 0
    public static readonly RelativeId SecretBeach = new RelativeId(ModId, 0);
}
```

## 2. Registering Content via `DataModRegistry`

Mods do not instantiate raw Unity `ScriptableObject` definitions directly. Instead, they create **Data Mods** (`DataMod` subclasses) and register them with `DataModRegistry` (`ModInterface.DataMod`).

### Adding Definition Data Mods (`AddDataMod`)
The `AddDataMod` method accepts data mod instances (such as `LocationDataMod`, `ItemDataMod`, `GirlDataMod`, etc.) and automatically registers their `RelativeId` in `ModData`.

```csharp
// Registering a custom location
ModInterface.DataMod.AddDataMod(new LocationDataMod(MyModLocations.SecretBeach, InsertStyle.replace)
{
    LocationName = "Secret Beach",
    LocationType = LocationType.DATE,
    DefaultStyle = Styles.Water,
    BgMusic = new AudioKlipInfo()
    {
        AudioClipInfo = new AudioClipInfo()
        {
            IsExternal = false,
            Path = "bgm_secret_grotto"
        },
        Volume = 0.8f
    },
    Backgrounds = new List<IGameDefinitionInfo<Sprite>>()
    {
        new SpriteInfoInternal("loc_bg_special_volcano_0")
    },
    AllowNormal = true,
    AllowNonStop = true,
    DateTimes = new List<ClockDaytimeType>() { ClockDaytimeType.NIGHT }
});
```

### Supported Data Mod Types
`DataModRegistry` provides overloads for all primary `GameDataType` definitions:
* `LocationDataMod` (`GameDataType.Location`)
* `ItemDataMod` (`GameDataType.Item`)
* `GirlDataMod` (`GameDataType.Girl`) & `GirlPairDataMod` (`GameDataType.GirlPair`)
* `CutsceneDataMod` (`GameDataType.Cutscene`)
* `AbilityDataMod` (`GameDataType.Ability`) & `AilmentDataMod` (`GameDataType.Ailment`)
* `CodeDataMod` (`GameDataType.Code`)
* `TokenDataMod` (`GameDataType.Token`)
* `FavQuestionDataMod` (`GameDataType.Question`)
* `PhotoDataMod`, `EnergyDataMod`, `DialogTriggerDataMod`, `DlcDataMod`

### Registering Sub-Systems (`AddData`)
For non-definition objects associated with a `RelativeId`, use `AddData`:
* `AddData(RelativeId, IItemGiftHandler)`
* `AddData(RelativeId, IItemStoreHandler)`
* `AddData(RelativeId, IPuzzleResource)`
* `AddData(RelativeId, ITokenHandler)`
* `AddData(RelativeId, IAffection)`
* `AddData(RelativeId, IPuzzleStatusGirlState)`
* `AddData(RelativeId, UiDollSpecialEffect)`

### Understanding `InsertStyle`
When creating a `DataMod`, you specify an `InsertStyle` parameter which determines how properties are applied to target definitions:
* **`InsertStyle.replace`**: Overwrites existing definition fields or replaces elements so long as the DataMod's corresponding field is not null.
* **`InsertStyle.assignNull`**: Overwrites existing definition fields even if the DataMod's corresponding field is null.
* **`InsertStyle.append`**: Appends list items to existing definition lists.
* **`InsertStyle.prepend`**: Prepends list items to existing definition lists.

If you need to use multiple styles, for instance if you want some items to be prepended and others to be appended, you can just register 2 DataMods to the same Id and they both will be applied.

## 3. The Registration Lifecycle

Content registration must strictly adhere to `Hp2BaseMod`'s initialization lifecycle:

1. **`Awake()` / `PreDataMods`**: Mods construct `DataMod` instances and call `ModInterface.DataMod.AddDataMod(...)` or `AddData(...)`.
2. **`ApplyDataMods()`**: `Hp2BaseMod` allocates runtime IDs (`1,000,000+`), instantiates the required `ScriptableObject` instances, scrapes internal assets via `AssetProvider`, and applies all data mods in priority order.
3. **`PostDataMods`**: `ModInterface.GameData` becomes fully initialized and safe to query.

> **Warning**: Do NOT access `ModInterface.GameData` or vanilla `Game.Data` during `Awake()`. Always wait until `ModEvents.PostDataMods` fires or during runtime game logic.

## 4. Querying Definitions with `GameDefinitionProvider`

Once data mods are applied, `ModInterface.GameData` provides a unified `GameDefinitionProvider` interface for looking up definitions by `RelativeId` or runtime ID.

### Typed Lookup Methods

```csharp
// Looking up definitions by RelativeId
GirlDefinition lola = ModInterface.GameData.GetGirl(Girls.Lola);
LocationDefinition volcano = ModInterface.GameData.GetLocation(Locations.VolcanoTop);
ItemDefinition smoothie = ModInterface.GameData.GetItem(mySmoothieRelativeId);

// Generic lookup by GameDataType and RelativeId
Definition def = ModInterface.GameData.GetDefinition(GameDataType.Location, MyModLocations.SecretBeach);

// Lookup by runtime integer ID
GirlDefinition girlByRuntime = ModInterface.GameData.GetGirl(new RelativeId(-1, 1)); // Vanilla Lola
```

### Specialized Sub-System Lookups
```csharp
IAffection talentAffection = ModInterface.GameData.GetAffection(PuzzleAffectionId.Talent);
IPuzzleResource movesResource = ModInterface.GameData.GetPuzzleResource(PuzzleResourceId.Moves);
ITokenHandler tokenHandler = ModInterface.GameData.GetTokenHandler(TokenHandlerId.Default);
IItemStoreHandler storeHandler = ModInterface.GameData.GetItemStoreHandler(ItemTypes.DateGift);
```

### ID Translation Helpers
To translate between runtime IDs and `RelativeId` structs manually, use `ModInterface.Data`:

```csharp
// RelativeId -> Runtime ID
int runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Location, MyModLocations.SecretBeach);

// Runtime ID -> RelativeId
RelativeId relId = ModInterface.Data.GetDataId(GameDataType.Location, runtimeId);

// Definition -> RelativeId
RelativeId girlRelId = ModInterface.Data.GetDataId(someGirlDefinition);
```

GameData classes also have extensions in the Hp2BaseMod that make this easier when working from a base game instance
```csharp

RelativeId girlRelId = someGirlDefinition.ModId();

```
