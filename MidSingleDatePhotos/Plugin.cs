using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace MidDatePhotos;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<bool> UnlockPhotos => _unlockPhotos;
    private static ConfigEntry<bool> _unlockPhotos;

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    private int _modId;
    private int _photoModCount;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
        _unlockPhotos = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, "Unlock All Photos", false, "If all photos should be automatically unlocked.");

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
            out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), [
                (
                        Path.Combine(IMAGES_DIR, "ScallyCapFan_credits.png"),
                        Path.Combine(IMAGES_DIR, "ScallyCapFan_credits_over.png"),
                        "https://www.reddit.com/user/scallycapfan/"
                    ),
                    (
                        Path.Combine(IMAGES_DIR, "onesuchkeeper_credits_dev.png"),
                        Path.Combine(IMAGES_DIR, "onesuchkeeper_credits_dev_over.png"),
                        "https://linktr.ee/onesuchkeeper"
                    )
            ]);
        }

        ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "AddGirlDatePhotos",
            out Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos);

        AddPhoto("zoey", Girls.Zoey, m_AddGirlDatePhotos);
        AddPhoto("ashley", Girls.Ashley, m_AddGirlDatePhotos);
        AddPhoto("lola", Girls.Lola, m_AddGirlDatePhotos);
        AddPhoto("lailani", Girls.Lailani, m_AddGirlDatePhotos);
        AddPhoto("suki", Girls.Sarah, m_AddGirlDatePhotos);
        AddPhoto("brooke", Girls.Brooke, m_AddGirlDatePhotos);
        AddPhoto("jessie", Girls.Jessie, m_AddGirlDatePhotos);
        AddPhoto("nora", Girls.Nora, m_AddGirlDatePhotos);
        AddPhoto("abia", Girls.Abia, m_AddGirlDatePhotos);
        AddPhoto("lillian", Girls.Lillian, m_AddGirlDatePhotos);
        AddPhoto("candy", Girls.Candace, m_AddGirlDatePhotos);
        AddPhoto("polly", Girls.Polly, m_AddGirlDatePhotos);

        ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;
    }

    private void AddPhoto(string name, RelativeId girlId, Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos)
    {
        var photoId = new RelativeId(_modId, _photoModCount++);
        ModInterface.AddDataMod(new PhotoDataMod(photoId, Hp2BaseMod.Utility.InsertStyle.append)
        {
            BigPhotoCensored = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(IMAGES_DIR, $"{name}.png"), true)),
            ThumbnailCensored = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(IMAGES_DIR, $"{name}_thumb.png"), true))
        });

        if (m_AddGirlDatePhotos != null)
        {
            m_AddGirlDatePhotos(girlId, [(photoId, 0.5f)]);
        }
    }

    // unlock all photos
    private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        if (!UnlockPhotos.Value) return;

        args.UnlockedPhotos ??= new List<PhotoDefinition>();

        for (int i = 0; i < _photoModCount; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(_modId, i)));
        }
    }
}
