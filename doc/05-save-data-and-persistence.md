# 05: Save Data & Persistence

`Hp2BaseMod` provides an isolated persistence system that prevents save file corruption and allows modded content to coexist safely alongside vanilla game progress.

## 1. Persistence Architecture & Save Isolation

Vanilla **HuniePop 2: Double Date** stores player save data in a binary serialized format (`HP2SaveData1.dat`). Directly inserting modded definitions (such as custom girls, locations, or items with runtime IDs $\ge 1,000,000$) into the vanilla save file creates a hard dependency on those mods—if the mod is ever removed, loading the vanilla save file will crash or corrupt.

To solve this, `Hp2BaseMod` introduces an isolated save architecture:

```
%APPDATA%/../LocalLow/HuniePot/HuniePop 2 - Double Date/
├── HP2SaveData1.dat     <-- Clean, vanilla-compatible binary save
└── ModSaveData.json     <-- Isolated JSON containing modded IDs & custom mod state
```

### Key Goals of the Isolation Layer
1. **Vanilla Compatibility**: The base game binary save (`HP2SaveData1.dat`) remains valid even if all mods are uninstalled.
2. **Mod Data Preservation**: Progress for modded content (unlocked custom outfits, custom items in inventory, relationship levels with custom girls) is preserved in `ModSaveData.json`.
3. **Graceful Fallback**: If a mod is uninstalled, `Hp2BaseMod` cleans the save during load, substituting missing modded content with safe defaults rather than throwing exceptions or crashing.

---

## 2. Save/Load Lifecycle (Stripping & Injection)

`Hp2BaseMod` uses Harmony patches on `GamePersistence` (`Save`, `Load`, `Reset`, `Init`) to automatically manage data stripping and injection.

```
┌────────────────────────────────────────────────────────────────────────┐
│                              SAVE CYCLE                                │
└────────────────────────────────────────────────────────────────────────┘
  GamePersistence.Save()
         │
         ▼
  PreSave Event Raised (`ModEvents.PreGameSave`)
         │
         ▼
  ModInterface.StripSave(saveData)
   ├── Extract modded unlocked codes, extra save slots (>4), custom girls/pairs
   ├── Strip custom inventory items, store products, & finder slots
   └── Write isolated mod state to `ModSaveData.json`
         │
         ▼
  Vanilla `HP2SaveData1.dat` written to disk (Clean State)
         │
         ▼
  PostSave Event Raised (`ModEvents.PostGameSave`)
```

```
┌────────────────────────────────────────────────────────────────────────┐
│                              LOAD CYCLE                                │
└────────────────────────────────────────────────────────────────────────┘
  GamePersistence.Load()
         │
         ▼
  Read vanilla `HP2SaveData1.dat`
         │
         ▼
  ModInterface.InjectSave(saveData)
   └── Re-inject modded codes, extra files, inventory items, and girls from `ModSaveData.json`
         │
         ▼
  CleanSave(saveData)
   └── Verify all runtime IDs against current `ModData`. Replace orphan/missing IDs with safe defaults.
```

---

## 3. Custom Per-Mod Save Persistence

In addition to automatically tracking custom game definitions (items, girls, locations), `Hp2BaseMod` allows dependant mods to write their own persistant state inside `ModSaveData.json`.

### `SetSourceSave` & `GetSourceSave`

You can store and retrieve raw string/JSON data keyed by your mod's `SourceId` (`ModId`):

```csharp
using Hp2BaseMod;
using Newtonsoft.Json;

public class MyModSaveState
{
    public int CustomScore { get; set; } = 0;
    public bool HasSeenSpecialCutscene { get; set; } = false;
}

public class MyModPlugin : Hp2BaseModPlugin
{
    private MyModSaveState _saveState = new MyModSaveState();

    public MyModPlugin() : base(MyPluginInfo.PLUGIN_NAME) { }

    protected override void Awake()
    {
        base.Awake();

        // Subscribe to save/load hooks
        ModInterface.Events.PreGameSave += OnPreSave;
        ModInterface.Events.PreLoadPlayerFile += OnPreLoad;
    }

    private void OnPreLoad(PlayerFile file)
    {
        // Retrieve persisted string data for this mod
        string rawJson = ModInterface.GetSourceSave(ModId);
        if (!string.IsNullOrEmpty(rawJson))
        {
            _saveState = JsonConvert.DeserializeObject<MyModSaveState>(rawJson) ?? new MyModSaveState();
            ModInterface.Log.Message($"Loaded custom mod save state. Score: {_saveState.CustomScore}");
        }
    }

    private void OnPreSave()
    {
        // Serialize custom state and pass to ModInterface
        string rawJson = JsonConvert.SerializeObject(_saveState);
        ModInterface.SetSourceSave(ModId, rawJson);
        ModInterface.Log.Message("Saved custom mod save state.");
    }
}
```

---

## 4. Graceful Degradation & Fallback Defaults

When a save file that previously used modded content is loaded without the corresponding mods active, `GamePersistencePatches.CleanSave` automatically sanitizes the `SaveData`:

| Modded Data Type | Clean/Fallback Strategy |
| :--- | :--- |
| **Wardrobe Girl ID** | Defaults to Ashley (`Girls.Ashley`) if modded girl ID cannot be resolved. |
| **File Icon Girl ID** | Defaults to Kyu (`Girls.Kyu`) if modded girl ID is missing. |
| **Current Location / Girl Pair** | If location or pair is missing/unresolved, resets location to Hotel Room (`Locations.HotelRoom`) and clears active pair. |
| **Hairstyle / Outfit Index** | Defaults to the girl's `DefaultHairstyleIndex` or `DefaultOutfitIndex` if the index exceeds active body styles. |
| **Inventory / Store Product** | Replaces unresolved modded item IDs with empty/invalid slots (`itemId = -1`). |
| **Unlocked Codes / Questions** | Strips unresolvable code and question runtime IDs from the unlocked list. |

This cleaning ensures that removing a mod never results in a corrupted or unloadable save file.
