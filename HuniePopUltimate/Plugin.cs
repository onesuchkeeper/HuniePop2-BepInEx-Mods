using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.RepeatThreesome", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.ExtraLocations", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : Hp2BaseModPlugin
{
    public const string DATA_DIR = "HuniePop_Data";
    public const string DAC_DIR = "Digital Art Collection";
    public const string REPEAT_THREESOME_GUID = "OSK.BepInEx.RepeatThreesome";
    public const string SINGLE_DATE_GUID = "OSK.BepInEx.SingleDate";
    public const string EXTRA_LOCATIONS_GUID = "OSK.BepInEx.ExtraLocations";
    private static readonly string TWEAKS_GUID = "OSK.BepInEx.Hp2BaseModTweaks";

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");
    public static readonly string ASSEMBLY_DIR = Path.Combine(DATA_DIR, "Managed");

    public static ConfigEntry<bool> UnlockStyles => _unlockStyles;
    private static ConfigEntry<bool> _unlockStyles;

    public static ConfigEntry<bool> UnlockPhotos => _unlockPhotos;
    private static ConfigEntry<bool> _unlockPhotos;

    public static ConfigEntry<string> HuniePopDir => _huniePopDir;
    private static ConfigEntry<string> _huniePopDir;

    public static ConfigEntry<bool> AddDateLocations => _addDateLocations;
    private static ConfigEntry<bool> _addDateLocations;

    public static ConfigEntry<bool> AddSimLocations => _addSimLocations;
    private static ConfigEntry<bool> _addSimLocations;

    public static ConfigEntry<bool> AddCharacters => _addCharacters;
    private static ConfigEntry<bool> _addCharacters;

    public static ConfigEntry<bool> UseHp1LolaStats => _useHp1LolaStats;
    private static ConfigEntry<bool> _useHp1LolaStats;

    public static ConfigEntry<bool> UseHp1JessieStats => _useHp1JessieStats;
    private static ConfigEntry<bool> _useHp1JessieStats;

    public static ConfigEntry<bool> UseHp1LolaLines => _useHp1LolaLines;
    private static ConfigEntry<bool> _useHp1LolaLines;

    public static ConfigEntry<bool> UseHp1JessieLines => _useHp1JessieLines;
    private static ConfigEntry<bool> _useHp1JessieLines;

    /// <summary>
    /// Ids of locations where single data bonus rounds can take place.
    /// </summary>
    public static IEnumerable<RelativeId> SexLocs => _instance._sexLocs;
    private RelativeId[] _sexLocs;

    /// <summary>
    /// Id of SingleDate's nobody character.
    /// </summary>
    public static RelativeId SingleDateNobodyId => _instance._singleDateNobodyId;
    private RelativeId _singleDateNobodyId;

    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    public static bool HasSingleDate { get; internal set; }
    public static bool ThrewOutGoldfish { get; internal set; }
    public static bool GameStarted { get; internal set; }

    private static Plugin _instance;

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        _instance = this;
        base.Awake();

        _singleDateNobodyId = new(ModInterface.GetSourceId(SINGLE_DATE_GUID), 0);

        AssetStudio.Logger.Default = new Logger();

        if (ModInterface.TryGetInterModValue(TWEAKS_GUID, "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), [
                (
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev.png"),
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev_over.png"),
                    "https://linktr.ee/onesuchkeeper"
                ),
                (
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art.png"),
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art_over.png"),
                        "https://twitter.com/silverwoodwork"
                )
            ]);
        }

        _huniePopDir = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(HuniePopDir), Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the HuniePop install directory.");

        _addDateLocations = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddDateLocations), true, "If HuniePop date locations should be added.");
        _addSimLocations = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddSimLocations), true, "If HuniePop sim locations should be added.");
        _addCharacters = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddCharacters), true, "If HuniePop characters should be added.");

        _useHp1LolaStats = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1LolaStats), true, "If HuniePop stats should be used for Lola.");
        _useHp1LolaLines = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1LolaLines), true, "If HuniePop lines should be used for Lola.");

        _useHp1JessieStats = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1JessieStats), true, "If HuniePop stats should be used for Jessie.");
        _useHp1JessieLines = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1JessieLines), true, "If HuniePop lines should be used for Jessie.");

        _unlockStyles = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockStyles), false, "If all HuniePop outfit and hairstyles should auto-unlock.");
        _unlockPhotos = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockPhotos), false, "If all HuniePop photos should auto-unlock.");

        if (!(!string.IsNullOrWhiteSpace(HuniePopDir.Value)
                && Directory.Exists(HuniePopDir.Value)))
        {
            ModInterface.Log.Error("HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return;
        }

        var dataDir = Path.Combine(HuniePopDir.Value, DATA_DIR);
        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop data directory");
            return;
        }

        var assemblyDir = Path.Combine(HuniePopDir.Value, ASSEMBLY_DIR);
        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop assembly directory");
            return;
        }

        var sexLocs = new List<RelativeId>()
        {
            LocationIds.BedRoomDate,
            Locations.RoyalSuite,
        };

        if (Chainloader.PluginInfos.ContainsKey(EXTRA_LOCATIONS_GUID))
        {
            var extraLocationsId = ModInterface.GetSourceId(EXTRA_LOCATIONS_GUID);
            sexLocs.Add(new RelativeId(extraLocationsId, 2));
            sexLocs.Add(new RelativeId(extraLocationsId, 7));
        }

        _sexLocs = sexLocs.ToArray();

        AddPhotos();
        AddFavorites();
        AddDialogTriggers();

        ModInterface.Events.PreDataMods += ModEventHandles.On_PreDataMods;
        ModInterface.Events.RequestUnlockedPhotos += ModEventHandles.On_RequestUnlockedPhotos;
        ModInterface.Events.PreLoadPlayerFile += ModEventHandles.On_PreLoadPlayerFile;
        ModInterface.Events.FinderSlotsPopulate += ModEventHandles.On_FinderSlotsPopulate;
        ModInterface.Events.PopulateStoreProducts += ModEventHandles.On_PopulateStoreProducts;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    internal static new void StartCoroutine(IEnumerator enumerator) => ((MonoBehaviour)_instance).StartCoroutine(enumerator);

    private void AddDialogTriggers()
    {
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PreBedroom, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PreSex, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PostSex, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });
    }

    private void AddFavorites()
    {
        FavDrink.AddDataMods();
        FavExercise.AddDataMods();
        FavFridayNight.AddDataMods();
        FavHoliday.AddDataMods();
        FavIceCream.AddDataMods();
        FavMovieGenre.AddDataMods();
        FavMusicGenre.AddDataMods();
        FavOnlineActivity.AddDataMods();
        FavOutdoorActivity.AddDataMods();
        FavOwnBodyPart.AddDataMods();
        FavPet.AddDataMods();
        FavPhoneApp.AddDataMods();
        FavPornCategory.AddDataMods();
        FavSchoolSubject.AddDataMods();
        FavSexPos.AddDataMods();
        FavShop.AddDataMods();
        FavSundayMorning.AddDataMods();
        FavThemeParkRide.AddDataMods();
        FavTrait.AddDataMods();
        FavWeather.AddDataMods();

        Birthday.AddDataMods();
        CupSize.AddDataMods();
        Education.AddDataMods();
        FavColour.AddDataMods();
        FavHangout.AddDataMods();
        FavHobby.AddDataMods();
        FavSeason.AddDataMods();
        Height.AddDataMods();
        Homeworld.AddDataMods();
        LastName.AddDataMods();
        Occupation.AddDataMods();
        Weight.AddDataMods();
        Age.AddDataMods();
    }

    private void AddPhotos()
    {
        var whiteVal = 248f / 255f;
        var whiteCol = new Color(whiteVal, whiteVal, whiteVal);

        ITextureRenderStep[] hp1ThumbSteps = [
            new TextureRsScale(new Vector2(156f/2400, 112f/1800f)),
            new TextureRsPad(2, whiteCol),
            new TextureRsPad(1),
        ];

        // kyu old photo
        var kyuPhotoDir = Path.Combine(HuniePopDir.Value, DAC_DIR, "Photos", "Kyu");
        if (Directory.Exists(kyuPhotoDir))
        {
            var censoredTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom1.jpg"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom2.jpg"), true);

            ModInterface.AddDataMod(new PhotoDataMod(Photos.KyuOld, InsertStyle.replace)
            {
                BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
                ThumbnailCensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "kyu_old_thumb_censored.png"), new TextureInfoRender(censoredTexture, false, hp1ThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "kyu_old_thumb_wet.png"), new TextureInfoRender(wetTexture, false, hp1ThumbSteps))),
            });
        }

        // audrey 10th photo
        {
            ITextureRenderStep[] audreyThumbSteps = [
                new TextureRsScale(new Vector2(156f/1440f, 112f/1080f)),
                new TextureRsPad(2, whiteCol),
                new TextureRsPad(1),
            ];

            var UncensoredTexture = new TextureInfoExternal(Path.Combine(IMAGES_DIR, "hp_10th_anniversary_audrey_dry.png"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(IMAGES_DIR, "hp_10th_anniversary_audrey_wet.png"), true);

            ModInterface.AddDataMod(new PhotoDataMod(Photos.Audrey10th, InsertStyle.replace)
            {
                BigPhotoUncensored = new SpriteInfoTexture(UncensoredTexture),
                ThumbnailUncensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "audrey_10th_thumb_uncensored.png"), new TextureInfoRender(UncensoredTexture, false, audreyThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "audrey_10th_thumb_wet.png"), new TextureInfoRender(wetTexture, false, audreyThumbSteps))),
            });
        }
    }
}
