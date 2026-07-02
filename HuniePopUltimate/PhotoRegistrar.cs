using System.IO;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

internal static class PhotoRegistrar
{
    private const int THUMB_WIDTH = 301;
    private const int THUMB_HEIGHT = 171;

    public static (bool hasKyu, bool hasThankYou)  RegisterPhotos(PluginConfig config, AssetBundle assetBundle)
    {
        var hasKyu = RegisterKyuOldPhoto(config, assetBundle);
        RegisterAudrey10thPhoto(assetBundle);
        RegisterDacPhotos(config);
        var hasThankYou = RegisterThankYouPhoto(config);

        return (hasKyu, hasThankYou);
    }

    private static bool RegisterKyuOldPhoto(PluginConfig config, AssetBundle assetBundle)
    {
        var dacDir = Path.Combine(config.HuniePopDir.Value, Plugin.DAC_DIR);

        var kyuPhotoDir = Path.Combine(dacDir, "Photos", "Kyu");
        if (!Directory.Exists(kyuPhotoDir)) return false;

        var censoredTexture = new TextureInfoExternal(
            Path.Combine(kyuPhotoDir, "Old Bedroom1.jpg"),
            true);

        var wetTexture = new TextureInfoExternal(
            Path.Combine(kyuPhotoDir, "Old Bedroom2.jpg"),
            true);

        ModInterface.AddDataMod(new PhotoDataMod(Photos.KyuOld, InsertStyle.replace)
        {
            ThumbnailCensored = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("kyu_old_thumb_censored")),

            ThumbnailWet = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("kyu_old_thumb_wet")),

            BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
            BigPhotoWet = new SpriteInfoTexture(wetTexture),
        });

        return true;
    }

    private static void RegisterAudrey10thPhoto(AssetBundle assetBundle)
    {
        ModInterface.AddDataMod(new PhotoDataMod(Photos.Audrey10th, InsertStyle.replace)
        {
            BigPhotoUncensored = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("hp_10th_anniversary_audrey_dry")),

            ThumbnailUncensored = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("audrey_10th_thumb_uncensored")),

            BigPhotoWet = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("hp_10th_anniversary_audrey_wet")),

            ThumbnailWet = new SpriteInfoSprite(
                assetBundle.LoadAsset<Sprite>("audrey_10th_thumb_wet")),
        });
    }

    private static void RegisterDacPhotos(PluginConfig config)
    {
        var dacDir = Path.Combine(config.HuniePopDir.Value, Plugin.DAC_DIR);

        Photos.HpDacPromoPhotoCount = RegisterPhotoDirectory(
            Path.Combine(dacDir, "Promo"),
            Photos.HpDacPromoPhotoBase,
            2400,
            1800);

        Photos.HpDacProfilePhotoCount = RegisterPhotoDirectory(
            Path.Combine(dacDir, "Profiles"),
            Photos.HpDacProfilePhotoBase,
            1200,
            675);

        Photos.HpDacArtTestPhotoCount = RegisterPhotoDirectory(
            Path.Combine(dacDir, "Misc", "Art Test"),
            Photos.HpDacArtTestPhotoBase,
            2400,
            1800);

        Photos.HpDacGuestPiecePhotoCount = RegisterPhotoDirectory(
            Path.Combine(dacDir, "Misc", "Guest Piece"),
            Photos.HpDacGuestPiecePhotoBase,
            1915,
            2816);

        Photos.HpDacKsRewardPhotoCount = RegisterPhotoDirectory(
            Path.Combine(dacDir, "KS Rewards"),
            Photos.HpDacKsRewardPhotoBase,
            2400,
            1800);
    }

    private static bool RegisterThankYouPhoto(PluginConfig config)
    {
        var dacDir = Path.Combine(config.HuniePopDir.Value, Plugin.DAC_DIR);

        var thankYouDir = Path.Combine(dacDir, "Misc", "Thank You");
        if (!Directory.Exists(thankYouDir)) return false;

        var censoredTexture = new TextureInfoExternal(
            Path.Combine(thankYouDir, "Thank You1.jpg"),
            false);

        var uncensoredTexture = new TextureInfoExternal(
            Path.Combine(thankYouDir, "Thank You2.jpg"),
            false);

        var wetTexture = new TextureInfoExternal(
            Path.Combine(thankYouDir, "Thank You3.jpg"),
            false);

        var thumbSteps = CreateThumbnailSteps(1400, 1642);

        ModInterface.AddDataMod(new PhotoDataMod(Photos.ThankYou, InsertStyle.replace)
        {
            BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
            ThumbnailCensored = CreateThumbnail(
                "thankYou1_thumb.png",
                censoredTexture,
                thumbSteps),

            BigPhotoUncensored = new SpriteInfoTexture(uncensoredTexture),
            ThumbnailUncensored = CreateThumbnail(
                "thankYou2_thumb.png",
                uncensoredTexture,
                thumbSteps),

            BigPhotoWet = new SpriteInfoTexture(wetTexture),
            ThumbnailWet = CreateThumbnail(
                "thankYou3_thumb.png",
                wetTexture,
                thumbSteps),
        });

        return true;
    }

    private static int RegisterPhotoDirectory(
        string directory,
        int baseId,
        float nativeWidth,
        float nativeHeight)
    {
        if (!Directory.Exists(directory)) return 0;

        var thumbSteps = CreateThumbnailSteps(nativeWidth, nativeHeight);
        var count = 0;

        foreach (var file in Directory
            .EnumerateFiles(directory)
            .Where(x => x.EndsWith(".jpg")))
        {
            var id = new RelativeId(Plugin.ModId, baseId + count);
            var texture = new TextureInfoExternal(file, false);

            ModInterface.AddDataMod(new PhotoDataMod(id, InsertStyle.replace)
            {
                BigPhotoCensored = new SpriteInfoTexture(texture),
                ThumbnailCensored = CreateThumbnail(
                    $"{Path.GetFileName(file)}_thumb.png",
                    texture,
                    thumbSteps),
            });

            count++;
        }

        return count;
    }

    private static SpriteInfoTexture CreateThumbnail(
        string cacheFileName,
        ITextureInfo texture,
        ITextureRenderStep[] steps)
    {
        return new SpriteInfoTexture(
            new TextureInfoCache(
                Path.Combine(Plugin.IMAGES_DIR, cacheFileName),
                new TextureInfoRender(texture, false, steps)));
    }

    private static ITextureRenderStep[] CreateThumbnailSteps(
        float nativeWidth,
        float nativeHeight)
    {
        var ratio = Mathf.Min(
            THUMB_WIDTH / nativeWidth,
            THUMB_HEIGHT / nativeHeight);

        return [
            new TextureRsScale(new Vector2(ratio, ratio)),
            new TextureRsPad(3, Color.white),
            new TextureRsPad(7),
        ];
    }
}