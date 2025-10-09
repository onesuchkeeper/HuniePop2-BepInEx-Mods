using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.Extension.OrderedDictionaryExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    private int _locationCount = 0;

    private ITextureRenderStep[] _finderIconSteps = [
        new TextureRsCellphoneOutline(4f, 0f, 1f),
    ];

    private Dictionary<string, string> _locationNameToIconOutlined = new Dictionary<string, string>()
    {
        {"Bar","item_unique_whisky"},
        {"Beach","item_date_beach_ball"},
        {"Cafe","item_baggage_caffeine_junkie"},
        {"Campus","item_baggage_intellectually_challenged"},
        {"Gym","item_baggage_abandonment_issues"},
        {"Mall","item_baggage_brand_loyalist"},
        {"Nightclub","item_unique_gin"},
        {"Park","item_date_green_clover"},
    };

    private Dictionary<string, string> _locationNameToIconInternal = new Dictionary<string, string>()
    {
        {"Bedroom","ui_location_icon_room"},
    };

    private void ExtractLocation(SerializedFile file, OrderedDictionary locationDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        var locationMod = new LocationDataMod(new RelativeId(Plugin.ModId, _locationCount++), InsertStyle.replace);

        if (locationDef.TryGetValue("name", out string locationName))
        {
            locationMod.LocationName = locationName;
            ModInterface.Log.LogInfo(locationName);
        }

        if (_locationNameToIconOutlined.TryGetValue(locationName, out var iconOutlinedName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoTexture(
                new TextureInfoCache(
                    Path.Combine(Plugin.RootDir, "images", $"{locationName}_icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal(iconOutlinedName), false, true, _finderIconSteps)
                )
            );
        }
        else if (_locationNameToIconInternal.TryGetValue(locationName, out var iconInternalName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoInternal(iconInternalName);
        }

        using (ModInterface.Log.MakeIndent())
        {
            if (locationDef.TryGetValue("type", out int locationType))
            {
                locationMod.LocationType = locationType == 0
                    ? LocationType.SIM
                    : LocationType.DATE;
            }

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
                            Volume = musicVolume * 1.8f //hp2 is louder
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
                    locationMod.Backgrounds = [defaultBg, defaultBg];
                }
            }

            locationMod.DateTimes = [
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT,
            ];
            ModInterface.AddDataMod(locationMod);
        }
    }
}
