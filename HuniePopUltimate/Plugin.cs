using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    private static readonly string _configGeneralName = "general";
    private static readonly string _configKyuName = "useHp1Kyu";
    private static readonly string _configLolaName = "useHp1Lola";
    private static readonly string _configJessieName = "useHp1Jessie";

    private static readonly string _dataDir = "HuniePop_Data";
    private static readonly string _dacDir = "Digital Art Collection";
    private static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    public static string RootDir => _dir;
    private static string _dir;

    public static string ImgDir => Path.Combine(_dir, "images");

    public static int ModId => _modId;
    private static int _modId;

    public static int PhotoModCount = 0;

    private void Awake()
    {
        if (ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "AddGirlDatePhotos", out Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos)
            && ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "AddGirlSexPhotos", out Action<RelativeId, IEnumerable<RelativeId>> m_AddGirlSexPhotos)
            && ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "SetCharmSprite", out Action<RelativeId, Sprite> m_SetCharmSprite)
            )
        {
            ModInterface.Log.LogInfo("Successfully found single date interop");
        }
        else
        {
            m_AddGirlDatePhotos = null;
            m_AddGirlSexPhotos = null;
            m_SetCharmSprite = null;
        }

        this.Config.Bind(_configGeneralName, _configKyuName, true, "If Kyu should use Hp1 visuals.");
        this.Config.Bind(_configGeneralName, _configLolaName, true, "If Lola should use Hp1 visuals.");
        this.Config.Bind(_configGeneralName, _configJessieName, true, "If Jessie should use Hp1 visuals.");

        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
        _dir = Path.GetDirectoryName(this.Info.Location);

        var imagesDir = Path.Combine(_dir, "images");

        this.Config.Bind("General", "HuniePopDir", Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the HuniePop install directory.");

        if (!(this.Config.TryGetEntry<string>("General", "HuniePopDir", out var hpDirConfig)
                && !string.IsNullOrWhiteSpace(hpDirConfig.Value)
                && Directory.Exists(hpDirConfig.Value)))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return;
        }

        var dataDir = Path.Combine(hpDirConfig.Value, _dataDir);
        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop data directory");
            return;
        }

        var assemblyDir = Path.Combine(hpDirConfig.Value, _assemblyDir);
        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop assembly directory");
            return;
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        ModInterface.Log.LogInfo("Loading HuniePop assembly (this may take a bit)");

        using (var hpExtraction = new HpExtraction(hpDirConfig.Value, m_AddGirlDatePhotos, m_AddGirlSexPhotos, m_SetCharmSprite))
        {
            ModInterface.Log.LogInfo("HuniePop assembly loaded successfully, beginning import:");
            ModInterface.Log.IncreaseIndent();
            hpExtraction.Extract();
            ModInterface.Log.DecreaseIndent();
        }

        var whiteVal = 248f / 255f;
        var whiteCol = new Color(whiteVal, whiteVal, whiteVal);

        var greyVal = 153f / 255f;
        var greyCol = new Color(greyVal, greyVal, greyVal);

        ITextureRenderStep[] hp1ThumbSteps = [
            new TextureRsScale(new Vector2(156f/2400, 112f/1800f)),
            new TextureRsPad(2, whiteCol),
            new TextureRsPad(1),
        ];

        //kyu old photo
        var kyuPhotoDir = Path.Combine(hpDirConfig.Value, _dacDir, "Photos", "Kyu");
        if (Directory.Exists(kyuPhotoDir))
        {
            var censoredTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom1.jpg"));
            var wetTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom2.jpg"));

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(_modId, PhotoModCount++), InsertStyle.replace)
            {
                BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
                ThumbnailCensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "kyu_old_thumb_censored.png"), new TextureInfoRender(censoredTexture, hp1ThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "kyu_old_thumb_wet.png"), new TextureInfoRender(wetTexture, hp1ThumbSteps))),
            });
        }

        //audrey 10th photo
        {
            ITextureRenderStep[] audreyThumbSteps = [
                new TextureRsScale(new Vector2(156f/1440f, 112f/1080f)),
                new TextureRsPad(2, whiteCol),
                new TextureRsPad(1),
            ];

            var UncensoredTexture = new TextureInfoExternal(Path.Combine(imagesDir, "hp_10th_anniversary_audrey_dry.png"));
            var wetTexture = new TextureInfoExternal(Path.Combine(imagesDir, "hp_10th_anniversary_audrey_wet.png"));

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(_modId, PhotoModCount++), InsertStyle.replace)
            {
                BigPhotoUncensored = new SpriteInfoTexture(UncensoredTexture),
                ThumbnailUncensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "audrey_10th_thumb_uncensored.png"), new TextureInfoRender(UncensoredTexture, audreyThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "audrey_10th_thumb_wet.png"), new TextureInfoRender(wetTexture, audreyThumbSteps))),
            });
        }

        //favorites
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

        //ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;

        ModInterface.Events.FinderSlotsPopulate += On_FinderSlotsPopulate;
    }

    private void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

        switch (time)
        {
            case ClockDaytimeType.MORNING:
                args.RemoveGirlFromAllPools(Girls.Nikki);
                args.RemoveGirlFromAllPools(Girls.Celeste);
                break;
            case ClockDaytimeType.AFTERNOON:
                args.RemoveGirlFromAllPools(Girls.Celeste);
                break;
            case ClockDaytimeType.EVENING:
                args.RemoveGirlFromAllPools(Girls.Momo);
                break;
            case ClockDaytimeType.NIGHT:
                break;
        }
    }

    // private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    // {
    //     args.UnlockedPhotos ??= new List<PhotoDefinition>();

    //     for (int i = 0; i < PhotoModCount; i++)
    //     {
    //         args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(ModId, i)));
    //     }
    // }

    //unlock all
    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        using (ModInterface.Log.MakeIndent())
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                var expansion = ExpandedGirlDefinition.Get(girlId);

                foreach (var outfitId_Index in expansion.OutfitIdToIndex.Where(x => x.Key.SourceId == ModId))
                {
                    fileGirl.UnlockOutfit(outfitId_Index.Value);
                }

                foreach (var hairstyleId_index in expansion.HairstyleIdToIndex.Where(x => x.Key.SourceId == ModId))
                {
                    fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                }
            }
        }
    }
}