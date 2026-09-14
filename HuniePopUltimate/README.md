# HuniePopUltimate

Adds characters and locations from [HuniePop](https://huniepop.com/) directly into [HuniePop 2 - Double Date](https://huniepop2doubledate.com/)

## Important Notes

This mod does not distribute any assets from the original HuniePop games, instead, it extracts assests directly from an installation of HuniePop.

**To run HuniePop Ultimate, you must have HuniePop installed**

By default the game looks for an installation of HuniePop in the same directory where HuniePop 2 is installed.

If you have it installed at another location you must edit the config file in
`BepInEx/config/OSK.BepInEx.HuniePopUltimate.cfg`
and set `HuniePopDir` to the correct value.

There are other options in there too, make sure to check them out!

The first time the mod runs it will take an extended time to start. It is converting and caching audio data from HuniePop. This will create an ~1.6 GB audio.pcmx file in the `BepInEx/plugins/HuniePopUltimate` directory.

At startup this mod needs to process a lot of data, expect a long startup time.
For me it takes about 20 seconds, but your time will vary.

## Recomended Mods

HuniePop Ultimate works best when paired with [Single Date](https://onesuchkeeper.itch.io/huniepop-2-single-date), which will add a single date pairing for all the HuniePop characters, or with the [Randomizer](https://onesuchkeeper.itch.io/huniepop-2-randomizer), which will swap the HuniePop characters into random existing pairs. Or with both!

## Dependencies & Installation

### Dependencies
* [**HuniePop 2 - Double Date**](https://huniepop2doubledate.com/)
* [**BepInEx**](https://github.com/BepInEx/BepInEx/releases)
* [**Hp2BaseMod**](../Hp2BaseMod/README.md)

### Installation
* Install [**Hp2BaseMod**](../Hp2BaseMod/README.md)
* Download, unzip and place the AnniversaryTitle folder in BepInEx/plugins

## Contributions

* **Developer**: OneSuchKeeper ([Linktree](https://linktr.ee/onesuchkeeper))
* Special thanks to HunieDev for allowing the use of Sloppie Boppie and the 10th anniversary art for the mod!