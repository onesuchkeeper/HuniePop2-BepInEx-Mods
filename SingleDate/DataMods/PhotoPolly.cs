using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

internal static class PhotoPolly
{
    public static RelativeId Id => _id;
    private static RelativeId _id;

    public static void AddDataMods()
    {
        _id = new RelativeId(State.ModId, 6);

        var photoInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_polly_1.png"), true));
        var photoAltInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_polly_1_alt.png"), true));

        var thumbInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_polly_1_thumb.png"), true));
        var thumbAltInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_polly_1_alt_thumb.png"), true));

        ModInterface.AddDataMod(new PhotoDataMod(_id, InsertStyle.replace)
        {
            HasAlts = true,
            AltFlagName = "pollys_junk",

            //BigPhotoCensored = photoInfo,
            BigPhotoUncensored = photoInfo,
            BigPhotoUncensoredAlt = photoAltInfo,
            //BigPhotoWet = photoInfo,

            //ThumbnailCensored = thumbInfo,
            ThumbnailUncensored = thumbInfo,
            ThumbnailUncensoredAlt = thumbAltInfo,
            //ThumbnailWet = thumbInfo,
        });
    }
}
