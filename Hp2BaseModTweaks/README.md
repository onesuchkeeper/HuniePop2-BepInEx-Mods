# Hp2BaseModTweaks

**Hp2BaseModTweaks** (`OSK.BepInEx.Hp2BaseModTweaks`) is a companion Quality-of-Life (QoL), UI extension, and feature enhancement mod for **HuniePop 2: Double Date**. Built on top of `Hp2BaseMod`, it provides cellphone app expansions, UI pagination for modded content, style randomization, custom secret codes, and inter-mod APIs for credits and title screen logos.

---

## Key Features

### 📱 Expanded Cellphone Apps & Pagination
Base game cellphone apps suffer from layout overflow when custom content is added. `Hp2BaseModTweaks` injects dynamic pagination and UI wrappers across the phone UI:
* **Finder App (`ExpandedUiCellphoneFinderApp`)**: Pages simulation locations (8 per page).
* **Girls App (`ExpandedUiCellphoneGirlsApp`)**: Pages met girls (12 per page).
* **Pairs App (`ExpandedUiCellphonePairsApp`)**: Pages met girl pairs (24 per page).
* **Profile App (`ExpandedUiCellphoneProfileApp`)**: Features a scrollable favorites/questions list and pair pagination (4 per page).
* **Photos App (`ExpandedUiWindowPhotos`)**: Adds page navigation (29 photos per page) and depth-blur background rendering.
* **App Level Meters (`ExpandedUiCellphoneAppPair`)**: Dynamically adds scrollable level meters for custom stat/affection extensions (`IExpInfo`).

### 🎨 Mod Developer Interop APIs
Other mods can interact with `Hp2BaseModTweaks` via `ModInterface.RegisterInterModValue` / `TryGetInterModValue` without taking a hard assembly dependency:
* **`AddModCredit`**: Registers custom developer/artist logos and clickable credit buttons (with web redirects) inside the phone Credits app.
* **`AddLogoSprite`**: Adds custom logo sprites to the main title screen logo rotation pool.

### 👗 Wardrobe & Style Randomizer
* **Style Randomization**: Automatically randomizes hairstyles and outfits on dates or location travel, with per-girl persistence for NSFW filtering and unpaired hair/outfit combos.
* **Extra Wardrobe Characters**: Adds Kyu and Nymphojinn into the wardrobe with unlockable styles upon defeating them.

### 🔑 Secret Codes & Cheats
Registers custom toggle codes in the phone's Code app:
* **`JIZZ FOR ALL`**: Enables female "wet" CG photos in the photo album view mode.
* **`STAY FOCUSED`**: Keeps the game process running when unfocused (`Application.runInBackground`).
* **`POR QUE NO LOS TRES`**: Unlocks all 3 Kyu CG photo variations simultaneously when earned.
* **`PINK BITCH!`**: Equips Kyu's fairy wings on all girls.

### 💻 Console Commands
* **`/seticon <fileIndex> <sourceGuid> <girlLocalId>`**: Sets the save file slot's display icon to any girl definition from a specified mod source.

### 🖼️ Asset & Visual Enhancements
* **Digital Art Collection (DAC) Integration**: Automatically loads high-res head and portrait sprites from the DAC DLC folder if present.
* **UI Polish**: Enables `useSpriteMesh` across doll parts, inventory slots, and items for smooth rendering.

---

## Requirements & Installation

### Prerequisites
1. **BepInEx** (v5.x)
2. **Harmony**
3. **Hp2BaseMod** (`OSK.BepInEx.Hp2BaseMod` v1.0.0+)

### Installation
Place `Hp2BaseModTweaks.dll` into your `BepInEx/plugins/Hp2BaseModTweaks/` directory along with its associated asset bundle (`hp2basemodtweaks_assetbundle`).

---

## Configuration

Settings can be configured via `BepInEx/config/OSK.BepInEx.Hp2BaseModTweaks.cfg`:

| Section | Setting | Default | Description |
| :--- | :--- | :--- | :--- |
| `general` | `UseModLogo` | `true` | Include the default "HuniePop 2: Modded" title logo in rotation. |
| `general` | `DigitalArtCollectionDir` | `../../Digital Art Collection` | Path to the HuniePop 2 Digital Art Collection directory. |
| `Codes` | *(Code Hash Keys)* | `false` | Saved toggle state for unlocked secret codes across game sessions. |

---

## Developer Integration Examples

### Adding Custom Mod Credits
```csharp
using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Utility;
using UnityEngine;

if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
    out Action<Sprite, IEnumerable<(Sprite creditButtonSprite, Sprite creditButtonOverSprite, string redirectLink)>> addModCredit))
{
    addModCredit(
        TextureUtility.SpriteFromPng(Path.Combine(imagesDir, "CreditsLogo.png"), true),
        new[]
        {
            (
                TextureUtility.SpriteFromPng(Path.Combine(imagesDir, "dev_button.png"), true),
                TextureUtility.SpriteFromPng(Path.Combine(imagesDir, "dev_button_over.png"), true),
                "https://linktr.ee/mydevlink"
            )
        }
    );
}
```

### Adding a Custom Main Menu Logo
```csharp
if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddLogoSprite",
    out Action<Sprite> addLogoSprite))
{
    addLogoSprite(TextureUtility.SpriteFromPng(Path.Combine(imagesDir, "custom_logo.png"), true));
}
```

---

## Credits & License

* **Developer**: OneSuchKeeper
* **License**: Released under the [MIT License](https://opensource.org/licenses/MIT).
