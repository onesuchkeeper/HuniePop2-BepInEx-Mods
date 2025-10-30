using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

internal static class PhotoLailani
{
    public static RelativeId Id => _id;
    private static RelativeId _id;

    public static void AddDataMods()
    {
        _id = new RelativeId(State.ModId, 3);

        var lailaniPhotoInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "photo_lailani_1.png"), true));

        var lailaniPhotoThumbInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "photo_lailani_1_thumb.png"), true));

        ModInterface.AddDataMod(new PhotoDataMod(_id, InsertStyle.replace)
        {
            HasAlts = false,

            //BigPhotoCensored = lailaniPhotoInfo,
            BigPhotoUncensored = lailaniPhotoInfo,
            //BigPhotoWet = lailaniPhotoInfo,

            //ThumbnailCensored = lailaniPhotoThumbInfo,
            ThumbnailUncensored = lailaniPhotoThumbInfo,
            //ThumbnailWet = lailaniPhotoThumbInfo,
        });
    }
}