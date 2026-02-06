# Hp2BaseMod

The Hp2BaseMod overwrites many behaviors from the base game to make mod development practical. Its goal is to do this while leaving base gameplay unaffected. 

[Hp2BaseModTweaks](./Hp2BaseModTweaks.md) is a companion mod and provides ui changes and additional features that naturally arise from a modded environment.

If you are developing a Hp2BaseMod dependant mod, you will likely want to include [Hp2BaseMod.Analyzer](./Hp2BaseMod.Analyzer.md) as a dependency which will warn about use of depreciated features from the base game.

## Philosophy

HuniePop 2 is designed in a very coupled way, in many places designed in more of a functional programming style rather than an object oriented style. Because of this, the base mod must overwrite large chunks of the base game's behaviour. When developing mods, it is very important to check to see if the base mod has overwritten the method you are attempting to work with and if so, use the hooks provided by ModInterface.Events instead.

This is in active development and will hopefully become more organized over time. The nature of Hp2's design makes it very difficult.

## ModInterface

ModInterface is a static class that provides various utility methods for mods. These include:

- SourceIds - Each dependant mod is assigned an id which can be used to namespace identifiers.
- Data - Access to the modded id system.
- GameData - Access to GameData instances using modded ids.
- Log - Debug logging utility.
- State - Access to the replacements of hard coded values from the base game.
- DataMod
- Commands
- InteropValues - Registration of values to the base mod for
- Events - Notifies several behaviors of the base game and provides arguments subscribers can modify to change them.

## Runtime id vs mod ids

HuniePop 2 has a nasty habit of using collection indexes and enum values as identifiers. This is antithetic to mods looking to add new values of those types. To solve this, a secondary id system was created where identifiers are given a unique namespace.

The namespace for your mod or any other mod can be obtained by calling ModInterface.GetSourceId.

## GameData mods

HuniePop 2 has several asset types it calls GameData. The base mod provides a pipeline for these assets to be modified by providing the ModInterface with [IGameDataMod](../Hp2BaseMod/GameDataInfo.Interface/IGameDataMod.cs) instances. 

For example, if you would like to change a character's name this can be achieved by calling ModInterface.AddDataMod and passing it a [GirlDataMod](../Hp2BaseMod/GameDataInfo/DataMods/GirlDataMod.cs) instance with its name property set.

## Save file management

To maintain the save file's validity, the base mod strips all modded data from it when saving and creates a separate ModSaveData.json file.

Dependant mods can create and manage their own save system, or they can add information directly to ModSaveData.json by 

## Interop values

## Depreciated and repurposed features

The following is a list of depreciated or repurposed features of the base game and their replacement workflows when using the base mod. When working with the base mod you will likely want to include [Hp2BaseMod.SourceGen](./Hp2BaseMod.SourceGen.md) as a dependency which will warn about use of depreciated features from the base game.

### PlayerFileGirl.LearnedFavs

In the base game this was a list of the indexes of the girl's learned favorite questions in Game.Session.Talk.favQuestionDefinitions. It is now a list of those question's runtime ids.

this modifies:

- PlayerFileGirl.HasFavAnswer
- PlayerFileGirl.LearnFavAnswer
- PlayerFileGirl.learnedFavs

this depreciates:

- Game.Session.Talk.favQuestionDefinitions - Use Game.Data.Questions instead

### tset