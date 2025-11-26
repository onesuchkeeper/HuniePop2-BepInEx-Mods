using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Holds unity engine objects to use as prefabs for alternate single date ui
/// </summary>
public static class UiPrefabs
{
    private static readonly string _charmsDir = Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection", "Charms");

    public static Sprite SingleUiAppPairSlotBgOver => _singleUiAppPairSlotBgOver;
    private static Sprite _singleUiAppPairSlotBgOver;

    public static Sprite SingleUiAppPairSlotBg => _singleUiAppPairSlotBg;
    private static Sprite _singleUiAppPairSlotBg;

    public static Sprite SensitivityIcon => _sensitivityIcon;
    private static Sprite _sensitivityIcon;

    public static Sprite SensitivityPlate => _sensitivityPlate;
    private static Sprite _sensitivityPlate;

    public static Sprite SensitivityMeter => _sensitivityMeter;
    private static Sprite _sensitivityMeter;

    public static UiWindow SingleDateBubbles => _singleBubbles;
    private static UiWindowActionBubbles _singleBubbles;

    public static UiWindow DefaultDateBubbles => _defaultBubbles;
    private static UiWindow _defaultBubbles;

    private static Sprite _defaultCharmSprite;

    public static void InitExternals()
    {
        _singleUiAppPairSlotBg = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.IMAGES_DIR, "ui_app_pairs_pair_bg.png"), true);
        _singleUiAppPairSlotBgOver = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.IMAGES_DIR, "ui_app_pairs_pair_bg_over.png"), true);
        _sensitivityIcon = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.IMAGES_DIR, "ui_app_pair_icon_sensitivity.png"), true);
        _sensitivityPlate = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.IMAGES_DIR, "ui_app_pair_sensitivity.png"), true);
        _sensitivityMeter = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.IMAGES_DIR, "ui_app_pair_meter_sensitivity.png"), true);

        if (Directory.Exists(_charmsDir))
        {
            Plugin.SetGirlCharm(Girls.AbiaId, GetCharmSprite("Abia"));
            Plugin.SetGirlCharm(Girls.AshleyId, GetCharmSprite("Ashley"));
            Plugin.SetGirlCharm(Girls.BrookeId, GetCharmSprite("Brooke"));
            Plugin.SetGirlCharm(Girls.CandaceId, GetCharmSprite("Candace"));
            Plugin.SetGirlCharm(Girls.JessieId, GetCharmSprite("Jessie"));
            Plugin.SetGirlCharm(Girls.JewnId, GetCharmSprite("Jewn"));
            Plugin.SetGirlCharm(Girls.KyuId, GetCharmSprite("Kyu"));
            Plugin.SetGirlCharm(Girls.LailaniId, GetCharmSprite("Lailani"));
            Plugin.SetGirlCharm(Girls.LillianId, GetCharmSprite("Lillian"));
            Plugin.SetGirlCharm(Girls.LolaId, GetCharmSprite("Lola"));
            Plugin.SetGirlCharm(Girls.MoxieId, GetCharmSprite("Moxie"));
            Plugin.SetGirlCharm(Girls.NoraId, GetCharmSprite("Nora"));
            Plugin.SetGirlCharm(Girls.PollyId, GetCharmSprite("Polly"));
            Plugin.SetGirlCharm(Girls.SarahId, GetCharmSprite("Sarah"));
            Plugin.SetGirlCharm(Girls.ZoeyId, GetCharmSprite("Zoey"));
        }

        _defaultCharmSprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(SingleDate.Plugin.IMAGES_DIR, $"DefaultCharm.png"), true)).GetSprite();
    }

    private static TextureRsScale _charmScale = new TextureRsScale(new Vector2(14 / 45f, 14 / 45f));

    private static Sprite GetCharmSprite(string name) => new SpriteInfoTexture(new TextureInfoCache(
        Path.Combine(SingleDate.Plugin.IMAGES_DIR, $"{name}Charm.png"),
        new TextureInfoExternal(Path.Combine(_charmsDir, $"{name}.png"), false, FilterMode.Bilinear, [
            _charmScale
        ])
    )).GetSprite();

    public static void InitActionBubbles(UiWindow actionBubblesWindow)
    {
        _defaultBubbles = actionBubblesWindow;

        var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;
        _singleBubbles = (UiWindowActionBubbles)Object.Instantiate(actionBubblesWindow);

        UiActionBubble badTalkBubble = null;
        UiActionBubble dateBubble = null;
        UiActionBubble goodTalkBubble = null;
        foreach (var bubble in _singleBubbles.actionBubbles)
        {
            switch (bubble.actionBubbleType)
            {
                case ActionBubbleType.TALK:
                    if (bubble.actionBubbleValue == 0)
                    {
                        badTalkBubble = bubble;
                    }
                    else
                    {
                        goodTalkBubble = bubble;
                    }
                    break;
                case ActionBubbleType.DATE:
                    dateBubble = bubble;
                    break;
            }

            bubble.transform.position = new Vector3(bubble.transform.position.x + delta, bubble.transform.position.y, bubble.transform.position.z);
        }

        if (badTalkBubble != null && dateBubble != null)
        {
            dateBubble.transform.position = new Vector3(badTalkBubble.transform.position.x + 48,
                badTalkBubble.transform.position.y + 32f);

            var padding_GO = new GameObject($"{dateBubble.name}Padding");
            padding_GO.transform.SetParent(dateBubble.transform.parent, false);
            var padding_RectTransform = padding_GO.AddComponent<RectTransform>();
            var ratio = 29f / 33f;
            padding_RectTransform.localScale = new Vector3(ratio, ratio, 1f);

            dateBubble.transform.SetParent(padding_RectTransform, true);
        }

        if (goodTalkBubble != null)
        {
            goodTalkBubble.transform.position = new Vector3(goodTalkBubble.transform.position.x - 48,
                goodTalkBubble.transform.position.y + 32f);
        }

        if (badTalkBubble != null)
        {
            badTalkBubble.transform.SetParent(null);
            UnityEngine.Object.Destroy(badTalkBubble);
            _singleBubbles.actionBubbles.Remove(badTalkBubble);
        }
    }

    public static Sprite GetCharmSprite(RelativeId girlId)
    {
        if (Plugin.TryGetSingleDateGirl(girlId, out var singleDateGirl)
            && singleDateGirl.CharmSprite != null)
        {
            return singleDateGirl.CharmSprite;
        }

        return _defaultCharmSprite;
    }
}
