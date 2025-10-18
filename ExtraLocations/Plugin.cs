using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace ExtraLocations;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static readonly string RootDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    internal static readonly string ImagesDir = Path.Combine(RootDir, "images");

    private static Vector2 BgSize = new Vector2(2008, 1130);
    private static Vector2 dacSize = new Vector2(3568, 2025);

    private static readonly string ConfigGeneralName = "General";
    private static readonly string ConfigDacName = "DigitalArtCollectionDir";
    private static readonly string ConfigOstName = "DoubleDateOstDir";

    private void Awake()
    {
        var modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        this.Config.Bind(ConfigGeneralName, ConfigDacName, Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection"), "Directory containing the HuniePop 2 Digital Art Collection Dlc");
        this.Config.Bind(ConfigGeneralName, ConfigOstName, Path.Combine(Paths.PluginPath, "..", "..", "..", "..", "music", "HuniePop 2 - Double Date OST", "WAV"), "Directory containing the HuniePop 2 OST");

        var dacBgScaleRs = new TextureRsScale(BgSize / dacSize);

        //ost audio
        AudioClipInfo waterfallBgMusic = null;
        AudioClipInfo volcanoBgMusic = null;
        AudioClipInfo hotelRoomBgMusic = null;

        if (this.Config.TryGetEntry<string>(ConfigGeneralName, ConfigOstName, out var ConfigOst)
            && !string.IsNullOrWhiteSpace(ConfigOst.Value)
            && Directory.Exists(ConfigOst.Value))
        {
            var ostHiddenPartyMix = Path.Combine(ConfigOst.Value, "31 Double Date Party Mix (Bonus).wav");

            if (File.Exists(ostHiddenPartyMix))
            {
                volcanoBgMusic = new AudioClipInfo()
                {
                    IsExternal = true,
                    Path = ostHiddenPartyMix
                };
            }

            var ostHiddenWaterfall = Path.Combine(ConfigOst.Value, "33 Hidden Waterfall (Bonus).wav");

            if (File.Exists(ostHiddenWaterfall))
            {
                waterfallBgMusic = new AudioClipInfo()
                {
                    IsExternal = true,
                    Path = ostHiddenWaterfall
                };
            }

            var ostSurf = Path.Combine(ConfigOst.Value, "32 Surfin' IDP (Bonus).wav");

            if (File.Exists(ostSurf))
            {
                hotelRoomBgMusic = new AudioClipInfo()
                {
                    IsExternal = true,
                    Path = ostSurf
                };
            }
        }

        waterfallBgMusic ??= new AudioClipInfo()
        {
            IsExternal = false,
            Path = "bgm_secret_grotto"
        };

        volcanoBgMusic ??= new AudioClipInfo()
        {
            IsExternal = false,
            Path = "bgm_volcano_top"
        };

        hotelRoomBgMusic ??= new AudioClipInfo()
        {
            IsExternal = false,
            Path = "bgm_hotel_room"
        };

        //dac locs
        if (this.Config.TryGetEntry<string>(ConfigGeneralName, ConfigDacName, out var DacDirConfig)
            && !string.IsNullOrWhiteSpace(DacDirConfig.Value)
            && Directory.Exists(DacDirConfig.Value))
        {
            var waterfallImgDir = Path.Combine(DacDirConfig.Value, "Misc", "Cut Locations", "Waterfall");

            ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 0), InsertStyle.replace)
            {
                LocationName = "Hidden Waterfall",
                LocationType = LocationType.SIM,
                BgMusic = new AudioKlipInfo()
                {
                    AudioClipInfo = waterfallBgMusic,
                    Volume = 0.8f
                },
                Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Morning.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Morning.jpg"),
                            FilterMode.Bilinear,
                            [dacBgScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Afternoon.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Afternoon.jpg"),
                            FilterMode.Bilinear,
                            [dacBgScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Evening.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Evening.jpg"),
                            FilterMode.Bilinear,
                            [dacBgScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Night.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Night.jpg"),
                            FilterMode.Bilinear,
                            [dacBgScaleRs])))
                },
                FinderLocationIcon = new SpriteInfoTexture(new TextureInfoCache(
                    Path.Combine(ImagesDir, "hiddenWaterfall_Icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal("item_unique_hot_stones"), false, true, [new TextureRsCellphoneOutline(4f, 0f, 1f)])))
            });
        }

        //others
        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 1), InsertStyle.replace)
        {
            LocationName = "Volcano",
            LocationType = LocationType.DATE,
            NonStopOptionText = "Kyu. The [[highlight]VOLCANO]. IT'S A VOLCANO KYU. FUCKING FUCKIN' IN A FUCKING VOLCANO.",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = volcanoBgMusic,
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                new SpriteInfoInternal("loc_bg_special_volcano_0"),
                new SpriteInfoInternal("loc_bg_special_volcano_0")
            },
            AllowNonStop = true,
            AllowNormal = true,
            PostBoss = true,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.NIGHT
            }
        });

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 2), InsertStyle.replace)
        {
            LocationName = "Hotel Room",
            LocationType = LocationType.DATE,
            NonStopOptionText = "I dunno, I guess [[highlight]HERE] is good.",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = hotelRoomBgMusic,
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                new SpriteInfoInternal("loc_bg_hub_room_0"),
                new SpriteInfoInternal("loc_bg_hub_room_1"),
                new SpriteInfoInternal("loc_bg_hub_room_2"),
                new SpriteInfoInternal("loc_bg_hub_room_3"),
            },
            AllowNonStop = true,
            AllowNormal = true,
            PostBoss = false,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 3), InsertStyle.replace)
        {
            LocationName = "Space",
            LocationType = LocationType.DATE,
            NonStopOptionText = "Hey, is [[highlight]SPACE] on the table? How the hell did we get there anyways?",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_outer_space_bonus"
                },
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                new SpriteInfoInternal("loc_bg_date_space_0"),
                new SpriteInfoInternal("loc_bg_date_space_1")
            },
            AllowNormal = false,
            AllowNonStop = true,
            PostBoss = true,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 4), InsertStyle.replace)
        {
            LocationName = "Airplane Bathroom",
            LocationType = LocationType.DATE,
            NonStopOptionText = "I think the [[highlight]AIRPLANE BATHROOM] may have awoken something in me... Not sure how I feel about that...",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_airplane_bathroom"
                },
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                new SpriteInfoInternal("loc_bg_date_airplanebath_0"),
                new SpriteInfoInternal("loc_bg_date_airplanebath_1")
            },
            AllowNormal = false,
            AllowNonStop = true,
            PostBoss = false,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        var airplaneCabinBg = new SpriteInfoInternal("loc_bg_special_airplanecabin_0");
        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 5), InsertStyle.replace)
        {
            LocationName = "Airplane Cabin",
            LocationType = LocationType.DATE,
            NonStopOptionText = "Something about the cramped quarters and needless expense of a date in an [[highlight]AIRPLANE CABIN] just gets me going.",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_airplane_cabin"
                },
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                airplaneCabinBg,
                airplaneCabinBg
            },
            AllowNormal = false,
            AllowNonStop = true,
            PostBoss = false,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        var poolsideBg = new SpriteInfoInternal("loc_bg_special_poolside_0");
        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 6), InsertStyle.replace)
        {
            LocationName = "Poolside",
            LocationType = LocationType.DATE,
            NonStopOptionText = "Maybe a trip to the [[highlight]POOLSIDE]?",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_poolside"
                },
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                poolsideBg,
                poolsideBg
            },
            PostBoss = false,
            AllowNormal = true,
            AllowNonStop = true,
            DateTimes = new List<ClockDaytimeType>(){
                ClockDaytimeType.NIGHT
            }
        });

        var apartmentBg = new SpriteInfoInternal("loc_bg_special_apartment_0");
        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 7), InsertStyle.replace)
        {
            LocationName = "Your Apartment",
            LocationType = LocationType.DATE,
            NonStopOptionText = "I'm starting to get a little homesick, with some fairy magic we could pop on over to [[highlight]MY APARTMENT] right?",
            BgMusic = new AudioKlipInfo()
            {
                AudioClipInfo = new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_your_apartment"
                },
                Volume = 0.8f
            },
            Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                apartmentBg,
                apartmentBg
            },
            PostBoss = false,
            AllowNormal = false,
            AllowNonStop = true,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });
    }
}
