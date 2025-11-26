using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

internal static class PhotoDefault
{
    public static RelativeId Id => _id;
    private static RelativeId _id;

    public static void AddDataMods()
    {
        _id = new RelativeId(State.ModId, -1);

        var photoInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_default_1.png"), true));

        var thumbInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "photo_default_1_thumb.png"), true));

        ModInterface.AddDataMod(new PhotoDataMod(_id, InsertStyle.replace)
        {
            HasAlts = false,

            BigPhotoCensored = photoInfo,

            ThumbnailCensored = thumbInfo,
        });
    }
}