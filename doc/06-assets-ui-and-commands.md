# Assets, UI, and Commands in Hp2BaseMod

This guide covers working with internal and external assets using `AssetProvider`, creating custom in-game terminal commands via `ICommand`, and building safe UI modifications using `UiPatchController<T>` and `Hp2ButtonWrapper`.

---

## 1. Asset Management (`AssetProvider`)

The `AssetProvider` (`ModInterface.Assets`) manages both internal Unity assets scraped from base-game resources and external assets supplied by mods.

### Internal Asset Scraping

If your mod requires existing base-game prefabs, materials, sprites, or audio clips, you must request them during or before the `ModEvents.PreDataMods` lifecycle stage. `AssetProvider` scrapes Unity resources prior to compiling data mods.

#### Requesting Internal Assets (`PreDataMods`)
```csharp
protected override void Awake()
{
    base.Awake();

    // Request internal base-game assets on or before PreDataMods
    ModInterface.Assets.RequestInternal<UiWindow>("PhotosWindow");
    ModInterface.Assets.RequestInternal<Material>("UIDefault");
    ModInterface.Assets.RequestInternal<Sprite>("item_unique_hot_stones");
}
```

#### Accessing Internal Assets (`PostDataMods`)
Once `ModEvents.PostDataMods` has fired, requested assets can be retrieved using `GetInternalAsset<T>`. Calling `GetInternalAsset<T>` before `PostDataMods` will throw an exception.

```csharp
private void OnPostDataMods()
{
    UiWindow photosWindow = ModInterface.Assets.GetInternalAsset<UiWindow>("PhotosWindow");
    Material uiMat = ModInterface.Assets.GetInternalAsset<Material>("UIDefault");
}
```

The names needed to look up internal assets are typically the name given for their unity asset. Some assets were given identical names so they have been extended (this is the case for most doll parts). You can find these names by looking at the extraction process and dumping the AssetManager.

---

### External Asset Loading & Texture Utilities

`Hp2BaseMod` provides wrappers and utility classes to load and process PNG images and WAV/OGG/MP3 audio files from mod directories:

* **`TextureUtility.SpriteFromPng(filePath, readOnly)`**: Creates a Unity `Sprite` directly from an external PNG file.
* **`TextureInfoExternal`**: Encapsulates external texture files with filter and wrap modes.
* **`TextureInfoCache`**: Caches runtime-generated or external textures to disk to avoid repeated decoding overhead.
* **`TextureRsCellphoneOutline`**: An `ITextureRenderStep` that applies a cellphone-style outline to custom icons.
* **`AudioClipInfo`**: Points to external audio file paths (e.g., custom location background music) or internal audio resource keys.

```csharp
// Example: Creating location backgrounds and icons with external texture wrappers
var externalBg = new SpriteInfoTexture(
    new TextureInfoCache(
        Path.Combine(IMAGES_DIR, "custom_bg_cache.png"),
        new TextureInfoExternal(
            Path.Combine(IMAGES_DIR, "custom_bg.png"),
            readOnly: false,
            filter: FilterMode.Bilinear,
            renderSteps: [new TextureRsScale(new Vector2(0.5f, 0.5f))]
        )
    )
);

// Example: Referencing external audio files
var customBgm = new AudioClipInfo()
{
    IsExternal = true,
    Path = Path.Combine(AUDIO_DIR, "CustomMusic.wav")
};
```

---

## 2. In-Game Terminal Commands (`ICommand`)

`Hp2BaseMod` intercepts inputs in the Cellphone Code App starting with `/` and routes them to registered `ICommand` instances.

### Implementing `ICommand`

To create a custom command, implement the `ICommand` interface:

```csharp
using Hp2BaseMod;
using Hp2BaseMod.Commands;

namespace MyCustomMod
{
    public class HelloWorldCommand : ICommand
    {
        // Command name used after the '/' (case-insensitive, no spaces)
        public string Name => "helloWorld";

        // Description shown when executing "/help helloWorld"
        public string Help => "Prints Hello world";

        public bool Invoke(string[] inputs, out string result)
        {
            int amount = 1;
            if (inputs.Length > 0 && int.TryParse(inputs[0], out int parsed))
            {
                amount = parsed;
            }
            else
            {
                result = $"Failed to parse {amount} as an integer.";
                return false;
            }

            for (int i = 0; i < amount; i++)
            {
                ModInterface.Log.Message("Hello World!");
            }

            result = $"Greeted the world {amount} times.";
            return true;
        }
    }
}
```

### Registering Commands

Register your command via `ModInterface.AddCommand(...)` during plugin initialization:

```csharp
protected override void Awake()
{
    base.Awake();

    ModInterface.AddCommand(new HelloWorldCommand());
}
```

### In-Game Usage
In the HuniePop 2 Cellphone Code App:
* Entering `/helloWorld 3` executes `HelloWorldCommand` with `inputs = ["3"]`.
* Entering `/help helloWorld` displays the `Help` string.

## 3. UI Modification Framework

### Asynchronous UI Management (`UiPatchController<T>`)

Directly modifying Unity UI elements in standard Harmony patches can lead to layout timing glitches or missing references. `UiPatchController<T>` provides a safe, structured pipeline for attaching custom UI logic to base MonoBehaviours.

#### Pipeline Phases:
1. **`Start()`**: Triggered via a Harmony prefix on `T.Start()`. Calls `OnAttach()`.
2. **`Pipeline()` Coroutine**: Waits across frames for Unity layout calculations and base-game refresh methods to finish before calling `Apply()`.
3. **`Apply()`**: Abstract method where custom UI alterations are executed.
4. **`OnCleanup()`**: Invoked on `T.OnDestroy()` to clean up listeners and coroutines safely.

```csharp
using Hp2BaseMod;
using UnityEngine;
using UnityEngine.UI;

public class CustomPhoneAppController : UiPatchController<UiCellphoneApp>
{
    public CustomPhoneAppController(UiCellphoneApp core) : base(core) { }

    protected override void OnAttach()
    {
        // Setup initial fields prior to frame rendering
    }

    protected override void Apply()
    {
        // Safe to modify layouts, buttons, and child text components here
        ModInterface.Log.Message($"UI Patch applied to {Core.name}");
    }

    protected override void OnCleanup()
    {
        // Remove event handlers and references
    }
}
```

---

### Button Wrappers (`Hp2ButtonWrapper`)

`Hp2ButtonWrapper` provides reflection-based access to `ButtonBehavior` transition definitions (`f_overTransitions`, `f_downTransitions`, `f_disableTransitions`), allowing modders to easily override hover/click visual effects or attach custom sprites.

```csharp
using Hp2BaseMod.Ui;
using UnityEngine.UI;

public void ModifyButtonVisuals(ButtonBehavior button, Sprite newHoverSprite)
{
    var wrapper = new Hp2ButtonWrapper(button);

    // Modify or inspect button transition definitions safely
    var overTransition = button.GetStateTransition(ButtonBehaviorState.OVER);
    if (overTransition != null)
    {
        overTransition.sprite = newHoverSprite;
    }
}
```
