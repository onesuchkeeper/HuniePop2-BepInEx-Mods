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
    internal static readonly string DacDir = Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection");
    internal static readonly string OstDir = Path.Combine(Paths.PluginPath, "..", "..", "..", "..", "music", "Huniepop 2 - Double Date OST", "WAV");

    private static Vector2 BgSize = new Vector2(2008, 1130);
    private static Vector2 WaterfallSize = new Vector2(3568, 2025);

    private void Awake()
    {
        var modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        var ostHiddenPartyMix = Path.Combine(OstDir, "31 Double Date Party Mix (Bonus).wav");
        var volcanoBgMusic = File.Exists(ostHiddenPartyMix)
            ? new AudioClipInfo()
            {
                IsExternal = true,
                Path = ostHiddenPartyMix
            }
            : new AudioClipInfo()
            {
                IsExternal = false,
                Path = "bgm_volcano_top"
            };

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 0), InsertStyle.replace)
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
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        var ostSurf = Path.Combine(OstDir, "32 Surfin' IDP (Bonus).wav");
        var hotelRoomBgMusic = File.Exists(ostSurf)
            ? new AudioClipInfo()
            {
                IsExternal = true,
                Path = ostSurf
            }
            : new AudioClipInfo()
            {
                IsExternal = false,
                Path = "bgm_hotel_room"
            };

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 0), InsertStyle.replace)
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
                new SpriteInfoInternal("loc_bg_hub_room_1"),
                new SpriteInfoInternal("loc_bg_hub_room_1")
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

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 1), InsertStyle.replace)
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

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 2), InsertStyle.replace)
        {
            LocationName = "Airplane Bathroom",
            LocationType = LocationType.DATE,
            NonStopOptionText = "I think the [[highlight]AIRPLANE BATHROOM] may have awoken something im me... Not sure how I feel about that...",
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

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 3), InsertStyle.replace)
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
                new SpriteInfoInternal("loc_bg_special_airplanecabin_0"),
                new SpriteInfoInternal("loc_bg_special_airplanecabin_0")
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

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 4), InsertStyle.replace)
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
                new SpriteInfoInternal("loc_bg_special_poolside_0"),
                new SpriteInfoInternal("loc_bg_special_poolside_0")
            },
            PostBoss = false,
            AllowNormal = true,
            AllowNonStop = true,
            DateTimes = new List<ClockDaytimeType>(){
                ClockDaytimeType.NIGHT
            }
        });

        ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 5), InsertStyle.replace)
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
                new SpriteInfoInternal("loc_bg_special_apartment_0"),
                new SpriteInfoInternal("loc_bg_special_apartment_0")
            },
            PostBoss = false,
            AllowNormal = false,
            AllowNonStop = true,
            DateTimes = new List<ClockDaytimeType>() {
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT
            }
        });

        //3568 x 2025

        if (Directory.Exists(DacDir))
        {
            var ostHiddenWaterfall = Path.Combine(OstDir, "33 Hidden Waterfall (Bonus).wav");

            var audioClip = File.Exists(ostHiddenWaterfall)
                ? new AudioClipInfo()
                {
                    IsExternal = true,
                    Path = ostHiddenWaterfall
                }
                : new AudioClipInfo()
                {
                    IsExternal = false,
                    Path = "bgm_secret_grotto"
                };

            var waterfallImgDir = Path.Combine(DacDir, "Misc", "Cut Locations", "Waterfall");
            var waterfallScaleRs = new TextureRsScale(BgSize / WaterfallSize);

            ModInterface.AddDataMod(new LocationDataMod(new RelativeId(modId, 5), InsertStyle.replace)
            {
                LocationName = "Hidden Waterfall",
                LocationType = LocationType.SIM,
                BgMusic = new AudioKlipInfo()
                {
                    AudioClipInfo = audioClip,
                    Volume = 0.8f
                },
                Backgrounds = new List<IGameDefinitionInfo<Sprite>>(){
                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Morning.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Morning.jpg"),
                            FilterMode.Bilinear,
                            [waterfallScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Afternoon.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Afternoon.jpg"),
                            FilterMode.Bilinear,
                            [waterfallScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Evening.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Evening.jpg"),
                            FilterMode.Bilinear,
                            [waterfallScaleRs]))),

                    new SpriteInfoTexture(new TextureInfoCache(
                        Path.Combine(ImagesDir, "hiddenWaterfall_Night.png"),
                        new TextureInfoExternal(Path.Combine(waterfallImgDir, "Night.jpg"),
                            FilterMode.Bilinear,
                            [waterfallScaleRs])))
                },
                FinderLocationIcon = new SpriteInfoTexture(new TextureInfoCache(
                    Path.Combine(ImagesDir, "hiddenWaterfall_Icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal("item_unique_hot_stones"), true, [new TextureRsCellphoneOutline(4f, 0f, 1f)])))
            });
        }
    }
}
