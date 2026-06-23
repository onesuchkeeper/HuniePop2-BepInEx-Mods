using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public class HpLocationExtractor
{
    private static readonly Dictionary<RelativeId, RelativeId> DEFAULT_STYLES = new()
    {
        {LocationIds.Beach, Hp2BaseMod.Styles.Water},
        {LocationIds.BotanicalGarden, Hp2BaseMod.Styles.Activity},
        {LocationIds.HikingTrail, Hp2BaseMod.Styles.Activity},
        {LocationIds.FarmersMarket, Hp2BaseMod.Styles.Activity},
        {LocationIds.IceRink, Hp2BaseMod.Styles.Activity},
        {LocationIds.WaterPark, Hp2BaseMod.Styles.Water},
        {LocationIds.TennisCourts, Hp2BaseMod.Styles.Activity},
        {LocationIds.HotSprings, Hp2BaseMod.Styles.Water},
        {LocationIds.ScenicOverlook, Hp2BaseMod.Styles.Romantic},
        {LocationIds.Casino, Hp2BaseMod.Styles.Party},
        {LocationIds.OutdoorLounge, Hp2BaseMod.Styles.Romantic},
        {LocationIds.Carnival, Hp2BaseMod.Styles.Activity},
        {LocationIds.Restaurant, Hp2BaseMod.Styles.Romantic},
    };

    private static readonly ITextureRenderStep[] FINDER_ICON_STEPS =
    [
        new TextureRsCellphoneOutline(4f, 0f, 1f),
    ];

    private static readonly Dictionary<RelativeId, string> LOCATION_ICON_OUTLINED = new()
    {
        {LocationIds.Bar, "item_unique_whisky"},
        {LocationIds.Beach, "item_date_beach_ball"},
        {LocationIds.Cafe, "item_baggage_caffeine_junkie"},
        {LocationIds.Campus, "item_baggage_intellectually_challenged"},
        {LocationIds.Gym, "item_baggage_abandonment_issues"},
        {LocationIds.Mall, "item_baggage_brand_loyalist"},
        {LocationIds.NightClub, "item_unique_gin"},
        {LocationIds.Park, "item_date_green_clover"},
    };

    private static readonly Dictionary<RelativeId, string> LOCATION_ICON_INTERNAL = new()
    {
        {LocationIds.BedRoom, "ui_location_icon_room"},
    };

    private static readonly Dictionary<RelativeId, ClockDaytimeType> LOCATION_DATETIME = new()
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
        {LocationIds.Restaurant, ClockDaytimeType.NIGHT},
    };

    private readonly HpSpriteCache _sprites;
    private readonly HpAudioCache _audio;
    private readonly UnityEngine.AssetBundle _assetBundle;

    public HpLocationExtractor(HpSpriteCache sprites, HpAudioCache audio, UnityEngine.AssetBundle assetBundle)
    {
        _sprites = sprites;
        _audio = audio;
        _assetBundle = assetBundle;
    }

    public void ExtractLocation(SerializedFile file, OrderedDictionary locationDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        if (!locationDef.TryGetValue("id", out int hp1Id))
        {
            return;
        }

        var id = LocationIds.FromHp1Id(hp1Id);

        // The hub bedroom is ignored; the date bedroom is added as a special location only
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

        if (locationDef.TryGetValue("name", out string locationName))
        {
            locationMod.LocationName = locationName;
        }

        if (LOCATION_ICON_OUTLINED.TryGetValue(id, out var iconOutlinedName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoTexture(
                new TextureInfoCache(
                    Path.Combine(Plugin.IMAGES_DIR, $"{locationName}_icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal(iconOutlinedName), false, false, true, FINDER_ICON_STEPS)));
        }
        else if (LOCATION_ICON_INTERNAL.TryGetValue(id, out var iconInternalName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoInternal(iconInternalName);
        }

        using (ModInterface.Log.MakeIndent())
        {
            if (locationDef.TryGetValue("type", out int locationType))
            {
                locationMod.LocationType = locationType == 0 ? LocationType.SIM : LocationType.DATE;
            }

            if (!Plugin.PConfig.AddSimLocations.Value && locationMod.LocationType == LocationType.SIM)
            {
                return;
            }

            if (!Plugin.PConfig.AddDateLocations.Value && locationMod.LocationType == LocationType.DATE)
            {
                return;
            }

            HpDebugLog.LocationMessage($"{hp1Id} {locationName} - type {locationType} ({locationMod.LocationType})");

            if (locationDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
                && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
                && _sprites.TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo)
                && locationDef.TryGetValue("backgrounds", out List<object> backgrounds))
            {
                ExtractBackgrounds(file, locationDef, locationMod, locationName, backgrounds, spriteLookup, spriteTextureInfo);
            }

            if (locationMod.LocationType == LocationType.DATE && LOCATION_DATETIME.TryGetValue(id, out var dateTime))
            {
                locationMod.DateTimes = [dateTime];
            }

            if (DEFAULT_STYLES.TryGetValue(id, out var styleId))
            {
                locationMod.DefaultStyle = styleId;
            }

            ModInterface.AddDataMod(locationMod);
        }
    }

    private void ExtractBackgrounds(
        SerializedFile file,
        OrderedDictionary locationDef,
        LocationDataMod locationMod,
        string locationName,
        List<object> backgrounds,
        Dictionary<string, OrderedDictionary> spriteLookup,
        TextureInfoRaw spriteTextureInfo)
    {
        var backgroundSprites = new Dictionary<ClockDaytimeType, IGameDefinitionInfo<UnityEngine.Sprite>>();

        foreach (var background in backgrounds.OfType<OrderedDictionary>())
        {
            if (background.TryGetValue("backgroundName", out string backgroundName)
                && !string.IsNullOrEmpty(backgroundName)
                && spriteLookup.TryGetValue(backgroundName, out var spriteDef)
                && _sprites.TryMakeSpriteInfoTiledMirror(
                    Path.Combine(Plugin.IMAGES_DIR, $"{backgroundName}.png"),
                    spriteDef, spriteTextureInfo, 2008, 1140, out var spriteInfo)
                && background.TryGetValue("daytime", out int dayTime))
            {
                backgroundSprites[(ClockDaytimeType)dayTime] = spriteInfo;
            }

            if (background.TryGetValue("musicDefinition", out OrderedDictionary musicDef)
                && musicDef.TryGetValue("clip", out OrderedDictionary clip)
                && UnityAssetPath.TryExtract(clip, out var unityPath)
                && _audio.TryGetClipInfo(file, unityPath, out var clipInfo)
                && background.TryGetValue("musicVolume", out float musicVolume))
            {
                locationMod.BgMusic = new AudioKlipInfo()
                {
                    AudioClipInfo = clipInfo,
                    Volume = musicVolume * 3f // HP2 is louder
                };
            }
        }

        if (backgroundSprites.Count == 0)
        {
            return;
        }

        var defaultBg = backgroundSprites.Values.First();

        if (locationMod.LocationType == LocationType.SIM)
        {
            locationMod.Backgrounds = [
                backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var morningBg) ? morningBg : defaultBg,
                backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var afternoonBg) ? afternoonBg : defaultBg,
                backgroundSprites.TryGetValue(ClockDaytimeType.EVENING, out var eveningBg) ? eveningBg : defaultBg,
                backgroundSprites.TryGetValue(ClockDaytimeType.NIGHT, out var nightBg) ? nightBg : defaultBg,
            ];
        }
        else
        {
            var blurBg = new SpriteInfoSprite(_assetBundle.LoadAsset<UnityEngine.Sprite>($"{locationName}_blur"));
            locationMod.Backgrounds = [defaultBg, blurBg];
        }
    }
}