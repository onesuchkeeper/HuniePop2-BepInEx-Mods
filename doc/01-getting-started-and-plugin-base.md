# 01 - Getting Started and Plugin Base

This guide covers setting up a modding project for **HuniePop 2: Double Date** using `Hp2BaseMod`, creating your main plugin class with `Hp2BaseModPlugin`, and interacting with core framework services through `ModInterface`.

---

## Prerequisites & Project Setup

### Target Framework & Assembly References
To build a mod compatible with `Hp2BaseMod` and HuniePop 2, set up your C# project with the following parameters:

* **Target Framework**: `.NET Standard 2.0` or `.NET Framework 4.7.1`
* **Core Dependencies**:
  * `BepInEx` (v5.x) — Assembly: `BepInEx.dll`
  * `Harmony` — Assembly: `0Harmony.dll`
  * `UnityEngine` modules (`UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.UI.dll`, etc.)
  * `Hp2BaseMod.dll` — The base mod framework library
  * `Assembly-CSharp.dll` — Game assembly

### BepInEx Plugin Attributes
Every mod must declare its BepInEx metadata and state its dependency on `Hp2BaseMod`:

```csharp
using BepInEx;
using Hp2BaseMod;

[BepInPlugin("Me.BepInEx.MyMod", "My Mod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public partial class Plugin : Hp2BaseModPlugin
{
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }
    // ...
}
```

> **Note**: Your plugin class **must** be declared as `partial` to support automatic source generation for configuration and interop method registration.

---

## The `Hp2BaseModPlugin` Base Class

Inheriting from `Hp2BaseModPlugin` (instead of raw `BaseUnityPlugin`) gives your mod automatic integration with `Hp2BaseMod`'s ID tracking, and intermod communication systems.

### 1. Automatic `ModId` Assignment
When your plugin constructor calls `base(pluginGuid)`, `Hp2BaseModPlugin` automatically resolves or registers a unique integer `ModId` via `ModInterface.GetSourceId(pluginGuid)`.
* This `ModId` is stored in the `ModId` property:
  ```csharp
  public int ModId { get; private set; }
  ```
* All custom game definitions created by your mod should use this integer as their `SourceId` when constructing `RelativeId` instances.

### 2. Overriding `Awake()`
If your plugin overrides Unity's `Awake()` method, you **must call `base.Awake()`**. Calling `base.Awake()` ensures that `Hp2BaseModPlugin` executes its internal setup, including interop method registration.

```csharp
protected override void Awake()
{
    base.Awake(); // REQUIRED: Registers interop methods & initializes plugin services
    
    ModInterface.Log.Message($"{MyPluginInfo.PLUGIN_NAME} awake!");
}
```

### 3. Intermod Communication with `[InteropMethod]`
Methods with the `[InteropMethod]` attribute are automatically registered as interop endpoints on `base.Awake()`. Other mods can query and invoke these methods without needing hard compile-time assembly references.

```csharp
[InteropMethod]
public void CustomModAction(string message)
{
    ModInterface.Log.Message($"Received interop call: {message}");
}
```

To invoke interop methods provided by another mod:
```csharp
if (ModInterface.TryGetInterModValue("other.mod.guid", "CustomModAction", out Action<string> action))
{
    action("Hello from my mod!");
}
```

---

## Core Services via `ModInterface`

The `ModInterface` static facade provides centralized access to framework features:

| Facade Property | Type | Description |
| :--- | :--- | :--- |
| `ModInterface.Log` | `ModLog` | Central logging utility for outputting info, warnings, errors, and indented log blocks. |
| `ModInterface.Events` | `ModEvents` | Event hub for subscribing to game lifecycle, travel, puzzle, and store events. |
| `ModInterface.DataMod` | `DataModRegistry` | Registry for registering custom game definitions (`Girls`, `Items`, `Locations`, etc.). |
| `ModInterface.GameData` | `GameDefinitionProvider` | Lookup provider for runtime definitions. *Only access after `PostDataMods` triggers!* |
| `ModInterface.State` | `ModState` | Global tweakable options (e.g., `CellphoneOnLeft`, `FavQuestionOptionCount`). |
| `ModInterface.Commands` | `IReadOnlyDictionary<string, ICommand>` | Registry for custom developer terminal commands. |

### Logging with `ModLog`
Use `ModInterface.Log` rather than standard `Console.WriteLine` or Unity's `Debug.Log`. `ModLog` supports structured indentation blocks, making initialization logs clean and readable:

```csharp
using (ModInterface.Log.MakeIndent("Initializing Subsystem"))
{
    ModInterface.Log.Message("Loading assets...");
    ModInterface.Log.Warning("Asset cache empty, using fallback.");
}
```

You may also choose to use your mods own logging system, just keep in mind it may not align with the indent of the rest of the logging.

---

## Quickstart Example

Here is a minimal starting plugin demonstrating project structure, event subscription, and logging:

```csharp
using BepInEx;
using Hp2BaseMod;
using System;

namespace MyCustomHp2Mod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
    public partial class Plugin : Hp2BaseModPlugin
    {
        public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

        protected override void Awake()
        {
            base.Awake(); // Essential for Hp2BaseModPlugin setup

            using (ModInterface.Log.MakeIndent($"[{MyPluginInfo.PLUGIN_NAME}] Initializing"))
            {
                ModInterface.Log.Message($"Assigned Source ID: {ModId}");

                // Subscribe to lifecycle events
                ModInterface.Events.PreLocationArrive += OnPreLocationArrive;
            }
        }

        private void OnPreLocationArrive(LocationArriveArgs args)
        {
            ModInterface.Log.Message($"Arriving at location: {args.locationDef?.locationName ?? "null"}");
        }

        [InteropMethod]
        public void PingMod(string sender)
        {
            ModInterface.Log.Message($"Ping received from {sender}");
        }
    }
}
```