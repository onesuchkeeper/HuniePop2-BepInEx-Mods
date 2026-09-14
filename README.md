# HuniePop 2 BepinEx Mods

This repository contains the **`Hp2BaseMod`** along with a collection of content expansions, quality-of-life enhancements, gameplay overhauls, and developer tools built on top of it.

## Getting Started for Players

1. **Install BepInEx**: Download and install [BepInEx 5.x](https://github.com/BepInEx/BepInEx) (x64) into your *HuniePop 2* game folder.
2. **Install Core Framework**: Place `OSK.BepInEx.Hp2BaseMod.dll` into `BepInEx/plugins/`.
3. **Install Desired Mods**: Place any dependent mod DLLs and their accompanying `images/` or `audio/` folders into `BepInEx/plugins/`.

## Developer Guides

Documentation for developing mods using `Hp2BaseMod` is included in the project:
* [**`01-getting-started-and-plugin-base.md`**](doc/01-getting-started-and-plugin-base.md): Project setup, `Hp2BaseModPlugin`, `[ConfigProperty]`, and `[InteropMethod]`.
* [**`02-data-modding-and-relative-id.md`**](doc/02-data-modding-and-relative-id.md): `RelativeId` mapping, runtime ID assignment, and `DataModRegistry`.
* [**`03-events-and-lifecycle.md`**](doc/03-events-and-lifecycle.md): Event pipeline, location transitions, store restocking, and style changes.
* [**`04-expansions-and-scripted-systems.md`**](doc/04-expansions-and-scripted-systems.md): The `[Expansion]` pattern, `IScriptedAbility`, `IScriptedAilment`, and `Hp2BaseMod.Analyzer`.
* [**`05-save-data-and-persistence.md`**](doc/05-save-data-and-persistence.md): Save data isolation architecture and custom per-mod JSON persistence.
* [**`06-assets-ui-and-commands.md`**](doc/06-assets-ui-and-commands.md): `AssetProvider` resource scraping, `ICommand` terminal integration, and `Hp2ButtonWrapper`.
## Core Framework & Developer Tools

### **[Hp2BaseMod](./Hp2BaseMod)**
The core framework providing the central architecture for modding *HuniePop 2*.
* **`ModInterface`**: Central API facade for logging, state management, event handling, and data lookups.
* **`RelativeId`**: A `(SourceId, LocalId)` system preventing ID collisions between independent mods and assigning runtime IDs (`1,000,000+`).
* **Save Isolation (`ModSaveData.json`)**: Keeps custom mod data isolated from the base binary save file (`HP2SaveData1.dat`), preventing save file corruption if mods are removed.
* **`ModEvents`**: Event pipeline for intercepting game transitions, store restocking, puzzle events, and girl style changes.

### **[Hp2BaseModTweaks](./Hp2BaseModTweaks)**
An essential companion mod delivering quality-of-life UI improvements and inter-mod feature registries.
* **UI App Pagination**: Adds page navigation controls to the phone’s Finder (8/page), Girls (12/page), Pairs (24/page), Profiles, and Photo Album (29/page) apps.
* **Inter-Mod Registries**: Exposes `AddModCredit` for custom developer credits and `AddLogoSprite` for title screen logo rotations.
* **Built-in Console Commands & Codes**: Adds toggle codes (e.g., `JIZZ FOR ALL`, `STAY FOCUSED`, `POR QUE NO LOS TRES`, `PINK BITCH!`) and console commands like `/seticon`.

### **[Hp2BaseMod.Analyzer](./Hp2BaseMod.Analyzer)**
A Roslyn C# source generator and diagnostic analyzer.
* **`[Expansion]` Code Generator**: Automatically generates boilerplate `Get`/`Destroy` instances and reflection wrappers for extending base Unity classes.
* **`[InteropMethod]` Generator**: Auto-registers decorated plugin methods with `ModInterface` at compile time.
* **Diagnostics**: Enforces naming conventions and flags deprecated base-game member usage.

## Content Expansions & Dependent Mods

### Gameplay Overhauls & New Mechanics
* **[SingleDate](./SingleDate)**: Introduces solo dating mechanics by pairing girls with a "Nobody" placeholder. Features a custom "Sensitivity" stat progression, unique single date photos, custom action bubbles, and dedicated cutscenes.
* **[RepeatThreesome](./RepeatThreesome)**: Allows repeating threesome dates after lovers status is reached. Includes configuration options for location restrictions and bonus round nudity toggles (`OH THE PLACES YOU'LL GO`, `BEWBS`).
* **[Randomizer](./Randomizer)**: Randomizes date pairings, location assignments, favorite questions, character traits, items, and abilities. Includes seed control via the `/SetSeed` console command.

### Content & Visual Expansions
* **[HuniePopUltimate](./HuniePopUltimate)**: A massive expansion integrating girls, locations, cutscenes, dialog lines, and CG photos from the original *HuniePop 1* directly into *HuniePop 2*.
* **[ExtraLocations](./ExtraLocations)**: Unlocks and registers extra location backgrounds (Hidden Waterfall, Volcano, Hotel Room, Outer Space, Airplane Bathroom/Cabin, Poolside, Apartment) with OST audio support.
* **[ExpandedWardrobe](./ExpandedWardrobe)**: Adds new outfits, hairstyles, and part definitions across the character cast using `GirlDataMod` registration.
* **[AnniversaryTitle](./AnniversaryTitle)**: Replaces the title screen with a multi-layered 10th-anniversary cover art display, custom DOTween animation loops, and custom theme audio (`Sloppie Boppie.wav`).
* **[MidSingleDatePhotos](./MidSingleDatePhotos)**: Registers additional CG date photos for relationship progression milestones across girls.
* **[HiraganaLogo](./HiraganaLogo)**: Adds custom Hiragana title screen logos to the `Hp2BaseModTweaks` logo rotation pool.

### Utility & Cheats
* **[Cheat](./Cheat)**: Configurable startup options to unlock all styles, meet all girls, learn all baggage/favorites, and apply custom date resource boosts (Moves, Affection, Stamina, Passion, Sentiment) via a scripted cheat ailment.

## Contributors

* **OneSuchKeeper**: Developer of `Hp2BaseMod` and dependant mods.
* **SilverwoodWork**: Artist of hp1 character charms for HuniePopUltimate and alternate polly sprites for RepeatThreesome.
* **ScallyCapFan**: Lead artist for ExpandedWardrobe and MidSingleDatePhotos.
