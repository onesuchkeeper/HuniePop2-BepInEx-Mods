using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    private ITextureRenderStep[] _finderIconSteps = [
        new TextureRsCellphoneOutline(4f, 0f, 1f),
    ];

    private Dictionary<RelativeId, string> _locationIdToIconOutlined = new()
    {
        {LocationIds.Bar,"item_unique_whisky"},
        {LocationIds.Beach,"item_date_beach_ball"},
        {LocationIds.Cafe,"item_baggage_caffeine_junkie"},
        {LocationIds.Campus,"item_baggage_intellectually_challenged"},
        {LocationIds.Gym,"item_baggage_abandonment_issues"},
        {LocationIds.Mall,"item_baggage_brand_loyalist"},
        {LocationIds.NightClub,"item_unique_gin"},
        {LocationIds.Park,"item_date_green_clover"},
    };

    private Dictionary<RelativeId, string> _locationIdToIconInternal = new()
    {
        {LocationIds.BedRoom,"ui_location_icon_room"},
    };

    private Dictionary<RelativeId, ClockDaytimeType> _locationIdToDateTime = new()
    {
        {LocationIds.BotanicalGarden, ClockDaytimeType.MORNING},
        {LocationIds.HikingTrail, ClockDaytimeType.MORNING},
        {LocationIds.FarmersMarket, ClockDaytimeType.MORNING},
        {LocationIds.IceRink, ClockDaytimeType.AFTERNOON},
        {LocationIds.WaterPark, ClockDaytimeType.AFTERNOON},
        {LocationIds.TennisCourts, ClockDaytimeType.AFTERNOON},
        {LocationIds.ScenicOverlook, ClockDaytimeType.EVENING},
        {LocationIds.Casino, ClockDaytimeType.EVENING},
        {LocationIds.HotSprings, ClockDaytimeType.EVENING},
        {LocationIds.OutdoorLounge, ClockDaytimeType.NIGHT},
        {LocationIds.Carnival, ClockDaytimeType.NIGHT},
        {LocationIds.Restaurant, ClockDaytimeType.NIGHT}
    };

    private void ExtractLocation(SerializedFile file, OrderedDictionary locationDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        if (!locationDef.TryGetValue("id", out int hp1Id)) return;
        var id = LocationIds.FromHp1Id(hp1Id);

        // Ignore the hub bedroom loc, and add the date bedroom as a
        // special location only
        if (id == LocationIds.BedRoom)
        {
            return;
        }

        var locationMod = new LocationDataMod(id, InsertStyle.replace)
        {
            AllowNormal = true
        };

        if (id == LocationIds.BedRoomDate)
        {
            locationMod.AllowNormal = false;
        }

        if (id == LocationIds.Beach)
        {
            locationMod.DefaultStyle = Hp2BaseMod.Styles.Water;
        }

        if (locationDef.TryGetValue("name", out string locationName))
        {
            locationMod.LocationName = locationName;
        }

        if (_locationIdToIconOutlined.TryGetValue(id, out var iconOutlinedName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoTexture(
                new TextureInfoCache(Path.Combine(Plugin.IMAGES_DIR, $"{locationName}_icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal(iconOutlinedName), false, false, true, _finderIconSteps)
                )
            );
        }
        else if (_locationIdToIconInternal.TryGetValue(id, out var iconInternalName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoInternal(iconInternalName);
        }

        using (ModInterface.Log.MakeIndent())
        {
            if (locationDef.TryGetValue("type", out int locationType))
            {
                locationMod.LocationType = locationMod.Id == LocationIds.BedRoomDate
                    ? LocationType.SPECIAL
                    : (locationType == 0
                        ? LocationType.SIM
                        : LocationType.DATE);
            }

            ModInterface.Log.Message($"{hp1Id} {locationName}, - Loc type locationType{locationType} {locationMod.LocationType}");

            if (locationDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
                && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
                && TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo)
                && locationDef.TryGetValue("backgrounds", out List<object> backgrounds))
            {
                var backgroundSprites = new Dictionary<ClockDaytimeType, IGameDefinitionInfo<UnityEngine.Sprite>>();

                foreach (var background in backgrounds.OfType<OrderedDictionary>())
                {
                    if (background.TryGetValue("backgroundName", out string backgroundName)
                        && !string.IsNullOrEmpty(backgroundName)
                        && spriteLookup.TryGetValue(backgroundName, out var spriteDef)
                        && TryMakeSpriteInfo(spriteDef, spriteTextureInfo, out var spriteInfo)
                        && background.TryGetValue("daytime", out int dayTime))
                    {
                        backgroundSprites[(ClockDaytimeType)dayTime] = spriteInfo;
                    }

                    if (background.TryGetValue("musicDefinition", out OrderedDictionary musicDef)
                        && TryExtractAudioDef(musicDef, file, out var clipInfo)
                        && background.TryGetValue("musicVolume", out float musicVolume))
                    {
                        locationMod.BgMusic = new AudioKlipInfo()
                        {
                            AudioClipInfo = clipInfo,
                            Volume = musicVolume * 2.5f //hp2 is louder
                        };
                    }
                }

                var defaultBg = backgroundSprites.Values.First();
                if (locationMod.LocationType == LocationType.SIM)
                {
                    locationMod.Backgrounds = [
                        backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var morningBg) ? morningBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var afternoonBg) ? afternoonBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.EVENING, out var eveningBg) ? eveningBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.NIGHT, out var nightBg) ? nightBg : defaultBg
                    ];
                }
                else
                {
                    var blurBg = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(Plugin.IMAGES_DIR, $"{locationName}_blur.png"),
                            new TextureInfoSprite(defaultBg, false, false, true, [new TextureRsBlur(36)])));//super inefficient...
                    //pre render so we can later make readonly
                    blurBg.GetSprite();
                    locationMod.Backgrounds = [defaultBg, blurBg];
                }
            }

            if (locationMod.LocationType == LocationType.DATE && _locationIdToDateTime.TryGetValue(id, out var dateTime))
            {
                locationMod.DateTimes = [dateTime];
            }

            ModInterface.AddDataMod(locationMod);
        }
    }
}
