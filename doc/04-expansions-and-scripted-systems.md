# Expansions and Scripted Systems in Hp2BaseMod

The `Hp2BaseMod` architecture extends base game functionality through two primary mechanisms: the **`[Expansion]` Wrapper Pattern** and **Scripted Systems** (Code-Driven Abilities, Ailments, and Handlers). This document explains how to use these systems and details the compile-time assistance provided by the **`Hp2BaseMod.Analyzer`** Roslyn Source Generator and Analyzer.

---

## The `[Expansion]` Wrapper Pattern

Native Unity and base game types (`GirlDefinition`, `LocationManager`, `PuzzleSet`, `Ailment`, etc.) cannot be directly modified at runtime without breaking save compatibility or causing conflicts between mods. `Hp2BaseMod` solves this with the `[Expansion]` pattern.

An **Expansion** is a `partial class` that wraps a base game class, maintaining a 1:1 relationship with instances of that base type.

### Key Features of Expansions
* **Safe Extension**: Attach custom runtime properties, state, and methods to vanilla objects.
* **Automatic Registration**: The Roslyn Source Generator automatically emits static accessors (`Get`, `Destroy`) and extension methods (`GetExpansion()`, `DestroyExpansion()`).
* **Private Member Access**: Reflection wrappers (`AccessTools`) are generated automatically for private or protected fields, properties, and methods on the target base type.

### Creating an Expansion
To create an expansion, declare a `partial class` named `Expanded<BaseType>` and decorate it with `[Expansion(typeof(BaseType))]`:

```csharp
using BepInEx;
using Hp2BaseMod;

namespace MyMod
{
    [Expansion(typeof(LocationManager))]
    public partial class ExpandedLocationManager
    {
        // Custom state attached to LocationManager instances
        public string CustomLocationTag { get; set; }

        // Optional lifecycle hook called during instance creation
        private void OnInit()
        {
            CustomLocationTag = "DefaultTag";
        }

        // Optional lifecycle hook called when instance is destroyed
        private void OnDestroy()
        {
            // Cleanup logic
        }
    }
}
```

### Accessing Expansion Instances
The Source Generator automatically emits the `GetExpansion()` extension method on the target base type:

```csharp
LocationManager locationManager = Game.Session.Location;

// Fetch or create the expanded wrapper
ExpandedLocationManager expandedLoc = locationManager.GetExpansion();
expandedLoc.CustomLocationTag = "BeachParty";
```

If `HasModId = true` is set on `[Expansion(typeof(BaseType), HasModId = true)]`, the expansion instance is keyed by `RelativeId` rather than object reference.

---

## Compile-Time Roslyn Analyzer (`Hp2BaseMod.Analyzer`)

`Hp2BaseMod` includes a dedicated Roslyn C# Source Generator and Analyzer (`Hp2BaseMod.Analyzer`) that analyzes your mod code at compile time in Visual Studio / MSBuild.

### 1. Automatic Code Generation (`ExpansionGenerator`)
When you declare a partial class with `[Expansion(typeof(T))]`, `ExpansionGenerator` automatically emits:
* **Extension Methods**: `GetExpansion(this T core)` and `DestroyExpansion(this T core)'.
* **Static Accessors**: `Get(T core)` and `Destroy(T core)`.
* **Reflection Wrappers**: Private/protected fields (`f_field`), properties (`p_property`), and methods (`m_method`) on the base class are exposed as public typed wrappers backed by Harmony's `AccessTools`.
* **`IExpansionCore<T>` Integration**: If your class inherits from a base controller (such as `UiPatchController<T>`), the generator reuses the existing `Core` reference instead of creating duplicate backing fields.

### 2. Structural & Naming Diagnostics
The analyzer performs strict checks to enforce project conventions and prevent runtime crashes:

| Diagnostic ID | Severity | Description | Trigger Condition |
| :--- | :--- | :--- | :--- |
| **`HP005`** | Error | Class must be partial | Class contains `[InteropMethod]` but is not declared `partial`. |
| **`HP006`** | Warning | Expansion naming mismatch | Class expanding `BaseType` is not named `ExpandedBaseType`. |
| **`HP009`** | Error | Member rule outside Expansion | `[Deprecates]`, `[Overwrites]`, or `[Repurposes]` used outside an `[Expansion]` class. |
| **`HP010`** | Error | Expansion class not partial | An `[Expansion]` class is missing the `partial` keyword. |
| **`HP012`** | Error | Generated code conflict | A manual member in your partial class collides with a generated member name. |
| **`HP013`** | Diagnostic | Base member not found | Specified field/method in attributes does not exist on the base type. |
| **`HP014`** | Warning | Type inaccessible | Parameter or return type of a base member is private/inaccessible in generated code. |

### 3. Deprecation & API Transition Tracking
When base-game methods or fields are overridden, deprecated, or repurposed by `Hp2BaseMod`, the analyzer intercepts usage of vanilla members and emits compiler warnings/infos with instructions on what replacement API to use:

```csharp
// Example attributes placed on Expansion classes/members:
[Deprecates(nameof(GirlDefinition.GetMostFavAffectionType), "Use ExpandedGirlDefinition.GetMostFavAffectionType instead.")]
[Overwrites(nameof(TalkManager.HasFavAnswer), "It now uses question ids.")]
[Repurposes(nameof(AilmentDefinition.itemDefinition))]
```

* **`HP001` (Warning)**: Emitted when calling a deprecated base member (e.g. `TalkManager.favQuestionDefinitions` $\rightarrow$ use `Game.Data.Questions`).
* **`HP002` (Info)**: Emitted when using a base member whose internal algorithm has been overwritten by the base mod.
* **`HP003` (Info)**: Emitted when a field's underlying purpose has been repurposed.

---

## Scripted Abilities (`IScriptedAbility`)

While standard abilities in HuniePop 2 are data-driven step lists (`AbilityStepSubDefinition`), `Hp2BaseMod` allows developers to attach custom C# logic to abilities via `IScriptedAbility`.

### Implementation
Implement the `IScriptedAbility` interface:

```csharp
using Hp2BaseMod;

public class MyScriptedAbility : IScriptedAbility
{
    // Return false to abort ability execution before steps run
    public bool PrePerform(Ability ability, bool altGirl)
    {
        ModInterface.Log.Message("PrePerform check for ability!");
        return true;
    }

    // Return non-null to completely replace standard step execution
    public bool? ReplacePerform(Ability ability, bool altGirl)
    {
        // Custom C# ability logic here
        return true; // Replaced execution succeeded
    }

    // Called after ability steps finish; return modified success state
    public bool PostPerform(Ability ability, bool altGirl, bool originalResult)
    {
        return originalResult;
    }
}
```

### Attaching Factory to `AbilityDataMod`
Attach your factory delegate to `ExpandedAbilityDefinition.ScriptedAbilityFactory` during data registration:

```csharp
var abilityMod = new AbilityDataMod(myAbilityId, InsertStyle.replace);

// Attach factory delegate
ModInterface.DataMod.AddDataMod(abilityMod);

// Register factory on the expanded definition
ExpandedAbilityDefinition expDef = ModInterface.GameData.GetAbility(myAbilityId).GetExpansion();
expDef.ScriptedAbilityFactory = (ability) => new MyScriptedAbility();
```

---

## Scripted Ailments (`IScriptedAilment`)

Scripted Ailments provide full C# event-driven logic for puzzle ailments, baggage effects, and puzzle modifiers.

### Implementation
Implement `IScriptedAilment` and subscribe to `ExpandedAilmentManager` events:

```csharp
using Hp2BaseMod;

public class MyScriptedAilment : IScriptedAilment
{
    private ExpandedAilmentManager _manager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _manager = ailmentManager;
        
        // Subscribe to puzzle lifecycle events
        _manager.PreMove += OnPreMove;
        _manager.Settled += OnSettled;
    }

    public void Disable()
    {
        if (_manager != null)
        {
            _manager.PreMove -= OnPreMove;
            _manager.Settled -= OnSettled;
            _manager = null;
        }
    }

    private void OnPreMove(AilmentTriggerArgs.PreMove args)
    {
        // Intercept move and modify costs or grid state
        ModInterface.Log.Message("Player is making a move!");
    }

    private void OnSettled(AilmentTriggerArgs.PostSettled args)
    {
        // Triggered after tokens settle
    }
}
```

### Registration
Attach the factory via `ExpandedAilmentDefinition.ScriptedAilmentFactory`:

```csharp
ExpandedAilmentDefinition expAilment = ModInterface.GameData.GetAilment(myAilmentId).GetExpansion();
expAilment.ScriptedAilmentFactory = (ailment) => new MyScriptedAilment();
```

This is typically done by a DataMod.

---

## Custom System Handlers

`Hp2BaseMod` provides extensible interfaces for modularizing game mechanics:

* **Gift Handlers (`IItemGiftHandler`)**: Defines gift validation, failure reactions, and success rewards for custom items.
* **Store Handlers (`IItemStoreHandler`)**: Controls item store eligibility, section priority, weighted selection pools, and restocking behavior.
* **Puzzle Resources (`IPuzzleResource`)**: Defines custom matchable token resources, affection calculations, and label formatting.
* **Token Handlers (`ITokenHandler`)**: Customizes stamina/move costs, bypass checks, and spawn rules for tokens.
* **Affection Interfaces (`IAffection`)**: Custom affection types and fruit reward pools.
* **Girl State Handlers (`IPuzzleStatusGirlState`)**: Defines state transitions for girls during dates (e.g., Normal, Upset, Exhausted).

Handlers are registered using `ModInterface.DataMod.AddData(...)`:

```csharp
ModInterface.DataMod.AddData(myHandlerId, new CustomItemGiftHandler());
ModInterface.DataMod.AddData(myResourceId, new CustomPuzzleResource());
```

---

*For details on save persistence, refer to [05-save-data-and-persistence.md](./05-save-data-and-persistence.md). For UI patching and console commands, see [06-assets-ui-and-commands.md](./06-assets-ui-and-commands.md).*
