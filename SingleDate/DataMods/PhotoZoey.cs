using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

internal static class PhotoZoey
{
    public static RelativeId Id => _id;
    private static RelativeId _id;

    public static void AddDataMods()
    {
        _id = new RelativeId(State.ModId, 8);

        var photoInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "photo_zoey_1.png")));

        var thumbInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "photo_zoey_1_thumb.png")));

        ModInterface.AddDataMod(new PhotoDataMod(_id, InsertStyle.replace)
        {
            HasAlts = false,

            //BigPhotoCensored = photoInfo,
            BigPhotoUncensored = photoInfo,
            //BigPhotoWet = photoInfo,

            //ThumbnailCensored = thumbInfo,
            ThumbnailUncensored = thumbInfo,
            //ThumbnailWet = thumbInfo,
        });
    }
}