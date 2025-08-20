using System.Collections.Generic;
using System.IO;
using AssetStudio.PInvoke;
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
public class Plugin : BaseUnityPlugin
{
    private static readonly string _dataDir = "HuniePop_Data";
    private static readonly string _dacDir = "Digital Art Collection";
    private static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    public static string RootDir => _dir;
    private static string _dir;

    public static int ModId => _modId;
    private static int _modId;

    public static int PhotoModCount = 0;

    private void Awake()
    {
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

        using (var hpExtraction = new HpExtraction(hpDirConfig.Value))
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

        ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;
    }

    private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        args.UnlockedPhotos ??= new List<PhotoDefinition>();

        for (int i = 0; i < PhotoModCount; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(ModId, i)));
        }
    }

    private bool TryLocalizeGirlId(int nativeId, out int localizedId)
    {
        localizedId = nativeId;
        return true;
        // switch (nativeId)
        // {
        //     case 1:
        //         localizedId = _catalog.Models.Character.Tiffany;
        //         return true;
        //     case 2:
        //         localizedId = _catalog.Models.Character.Aiko;
        //         return true;
        //     case 3:
        //         localizedId = _catalog.Models.Character.Kyanna;
        //         return true;
        //     case 4:
        //         localizedId = _catalog.Models.Character.Audrey;
        //         return true;
        //     case 5:
        //         localizedId = _catalog.Models.Character.Lola;
        //         return true;
        //     case 6:
        //         localizedId = _catalog.Models.Character.Nikki;
        //         return true;
        //     case 7:
        //         localizedId = _catalog.Models.Character.Jessie;
        //         return true;
        //     case 8:
        //         localizedId = _catalog.Models.Character.Beli;
        //         return true;
        //     case 9:
        //         localizedId = _catalog.Models.Character.Kyu;
        //         return true;
        //     case 10:
        //         localizedId = _catalog.Models.Character.Momo;
        //         return true;
        //     case 11:
        //         localizedId = _catalog.Models.Character.Celeste;
        //         return true;
        //     case 12:
        //         localizedId = _catalog.Models.Character.Venus;
        //         return true;
        // }
        // localizedId = 0;
        // return false;
    }


}

[HarmonyPatch(typeof(DllLoader))]
public static class Foo
{
    [HarmonyPatch("GetDirectedDllDirectory")]
    [HarmonyPostfix]
    public static void GetDirectedDllDirectory(ref string __result)
    {
        //__result = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, Environment.Is64BitProcess ? "x64" : "x86");
        ModInterface.Log.LogInfo(__result);

        // LoadLibraryEx()

        // return false;
    }
}