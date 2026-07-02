using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Plugin.BASEMOD_GUID, "1.0.2")]
[BepInDependency(Plugin.TWEAKS_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Plugin.SINGLE_DATE_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Plugin.REPEAT_THREESOME_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Plugin.EXTRA_LOCATIONS_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : Hp2BaseModPlugin
{
    #region Constants

    public const string DATA_DIR = "HuniePop_Data";
    public const string DAC_DIR = "Digital Art Collection";

    public const string REPEAT_THREESOME_GUID = "OSK.BepInEx.RepeatThreesome";
    public const string SINGLE_DATE_GUID = "OSK.BepInEx.SingleDate";
    public const string EXTRA_LOCATIONS_GUID = "OSK.BepInEx.ExtraLocations";
    private const string TWEAKS_GUID = "OSK.BepInEx.Hp2BaseModTweaks";
    private const string BASEMOD_GUID = "OSK.BepInEx.Hp2BaseMod";

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");
    public static readonly string ASSEMBLY_DIR = Path.Combine(DATA_DIR, "Managed");

    #endregion

    #region Static Accessors

    public static PluginConfig PConfig => _instance._config;
    private PluginConfig _config;

    public static IEnumerable<RelativeId> SexLocs => _instance._sexLocs;
    private RelativeId[] _sexLocs;

    public static RelativeId SingleDateNobodyId => _instance._singleDateNobodyId;
    private RelativeId _singleDateNobodyId;

    public static AssetBundle AssetBundle => _instance._assetBundle;
    private AssetBundle _assetBundle;

    public static bool HasKyuOldPhoto => _instance._hasKyuOldPhoto;
    private bool _hasKyuOldPhoto;

    public static bool HasThankYouPhoto => _instance._hasThankYouPhoto;
    private bool _hasThankYouPhoto;

    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    #endregion

    #region Runtime State

    public static bool HasSingleDate { get; internal set; }

    public static bool ThrewOutGoldfish { get; internal set; }

    public static bool GameStarted { get; internal set; }

    private static Plugin _instance;

    #endregion

    #region Initialization

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) {}

    protected override void Awake()
    {
        _instance = this;

        base.Awake();

        InitializeConfiguration();
        if (!ValidateInstallation()) return;

        InitializeCompatibility();
        if (!InitializeAssets()) return;

        InitializeLocations();

        RegisterData();
        RegisterEvents();
        ApplyPatches();
    }

    private void InitializeConfiguration()
    {
        _config = new PluginConfig(Config);
    }

    private void InitializeCompatibility()
    {
        _singleDateNobodyId = new RelativeId(ModInterface.GetSourceId(SINGLE_DATE_GUID), 0);

        AssetStudio.Logger.Default = new Logger();

        RegisterModCredits();
    }

    private bool InitializeAssets()
    {
        _assetBundle = AssetBundle.LoadFromFile(
            Path.Combine(ROOT_DIR, "hpultimate_assetbundle"));

        if (_assetBundle == null)
        {
            ModInterface.Log.Error("Failed to load AssetBundle");
            return false;
        }

        TextureInfoRasterized._shader =
            _assetBundle.LoadAsset<Shader>("TextureRasterize");

        if (TextureInfoRasterized._shader == null)
        {
            ModInterface.Log.Error("Failed to load TextureRasterize shader");
            return false;
        }

        TextureInfoCenterMirrored._material =
            new Material(_assetBundle.LoadAsset<Shader>("EdgePadHorizontal"));

        if (TextureInfoCenterMirrored._material == null)
        {
            ModInterface.Log.Error("Failed to load EdgePadHorizontal shader");
            return false;
        }

        return true;
    }

    private void InitializeLocations()
    {
        var sexLocs = new List<RelativeId>
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
    }

    #endregion

    #region Validation

    private bool ValidateInstallation()
    {
        if (string.IsNullOrWhiteSpace(_config.HuniePopDir.Value)
            || !Directory.Exists(_config.HuniePopDir.Value))
        {
            ModInterface.Log.Error(
                $"HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return false;
        }

        var dataDir = Path.Combine(_config.HuniePopDir.Value, DATA_DIR);

        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop data directory");
            return false;
        }

        var assemblyDir = Path.Combine(_config.HuniePopDir.Value, ASSEMBLY_DIR);

        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop assembly directory");
            return false;
        }

        return true;
    }

    #endregion

    #region Registration

    private void RegisterData()
    {
        (_hasKyuOldPhoto, _hasThankYouPhoto) = PhotoRegistrar.RegisterPhotos(_config, _assetBundle);
        RegisterFavorites();
        RegisterDialogTriggers();
    }

    private void RegisterDialogTriggers()
    {
        ModInterface.AddDataMod(
            new DialogTriggerDataMod(
                DialogTriggers.PreBedroom,
                InsertStyle.append)
            {
                ForceType = DialogTriggerForceType.NONE,
            });

        ModInterface.AddDataMod(
            new DialogTriggerDataMod(
                DialogTriggers.PreSex,
                InsertStyle.append)
            {
                ForceType = DialogTriggerForceType.NONE,
            });

        ModInterface.AddDataMod(
            new DialogTriggerDataMod(
                DialogTriggers.PostSex,
                InsertStyle.append)
            {
                ForceType = DialogTriggerForceType.NONE,
            });
    }

    private void RegisterFavorites()
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

    private void RegisterEvents()
    {
        ModInterface.Events.PreDataMods +=
            ModEventHandles.On_PreDataMods;

        ModInterface.Events.RequestUnlockedPhotos +=
            ModEventHandles.On_RequestUnlockedPhotos;

        ModInterface.Events.PreLoadPlayerFile +=
            ModEventHandles.On_PreLoadPlayerFile;

        ModInterface.Events.FinderSlotsPopulate +=
            ModEventHandles.On_FinderSlotsPopulate;

        ModInterface.Events.PopulateStoreProducts +=
            ModEventHandles.On_PopulateStoreProducts;
    }

    private void RegisterModCredits()
    {
        if (!ModInterface.TryGetInterModValue(
                TWEAKS_GUID,
                "AddModCredit",
                out Action<Sprite,
                    IEnumerable<(
                        Sprite creditButtonPath,
                        Sprite creditButtonOverPath,
                        string redirectLink)>> addModCredit))
        {
            return;
        }

        addModCredit(
            TextureUtility.SpriteFromPng(
                Path.Combine(IMAGES_DIR, "CreditsLogo.png"),
                true),
            [
                (
                    TextureUtility.SpriteFromPng(
                        Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev.png"),
                        true),
                    TextureUtility.SpriteFromPng(
                        Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev_over.png"),
                        true),
                    "https://linktr.ee/onesuchkeeper"
                ),
                (
                    TextureUtility.SpriteFromPng(
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art.png"),
                        true),
                    TextureUtility.SpriteFromPng(
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art_over.png"),
                        true),
                    "https://twitter.com/silverwoodwork"
                ),
            ]);
    }

    #endregion

    #region Harmony

    private void ApplyPatches()
    {
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    #endregion

    internal static new void StartCoroutine(IEnumerator enumerator)
    {
        ((MonoBehaviour)_instance).StartCoroutine(enumerator);
    }
}
