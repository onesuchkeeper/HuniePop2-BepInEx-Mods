using System.Collections.Generic;
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

    private static Dictionary<RelativeId, Sprite> _charmSprites = new Dictionary<RelativeId, Sprite>();
    private static Sprite _defaultCharmSprite;

    public static void InitExternals()
    {
        _singleUiAppPairSlotBg = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.ImagesDir, "ui_app_pairs_pair_bg.png"));
        _singleUiAppPairSlotBgOver = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.ImagesDir, "ui_app_pairs_pair_bg_over.png"));
        _sensitivityIcon = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.ImagesDir, "ui_app_pair_icon_sensitivity.png"));
        _sensitivityPlate = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.ImagesDir, "ui_app_pair_sensitivity.png"));
        _sensitivityMeter = TextureUtility.SpriteFromPng(Path.Combine(SingleDate.Plugin.ImagesDir, "ui_app_pair_meter_sensitivity.png"));

        if (Directory.Exists(_charmsDir))
        {
            _charmSprites[Girls.AbiaId] = GetCharmSprite("Abia");
            _charmSprites[Girls.AshleyId] = GetCharmSprite("Ashley");
            _charmSprites[Girls.BrookeId] = GetCharmSprite("Brooke");
            _charmSprites[Girls.CandaceId] = GetCharmSprite("Candace");
            _charmSprites[Girls.JessieId] = GetCharmSprite("Jessie");
            _charmSprites[Girls.JewnId] = GetCharmSprite("Jewn");
            _charmSprites[Girls.KyuId] = GetCharmSprite("Kyu");
            _charmSprites[Girls.LailaniId] = GetCharmSprite("Lailani");
            _charmSprites[Girls.LillianId] = GetCharmSprite("Lillian");
            _charmSprites[Girls.LolaId] = GetCharmSprite("Lola");
            _charmSprites[Girls.MoxieId] = GetCharmSprite("Moxie");
            _charmSprites[Girls.NoraId] = GetCharmSprite("Nora");
            _charmSprites[Girls.PollyId] = GetCharmSprite("Polly");
            _charmSprites[Girls.SarahId] = GetCharmSprite("Sarah");
            _charmSprites[Girls.ZoeyId] = GetCharmSprite("Zoey");
        }
    }

    public static void SwapCharms(RelativeId girlA, RelativeId girlB)
    {
        if (_charmSprites.TryGetValue(girlA, out var charmA))
        {
            if (_charmSprites.TryGetValue(girlB, out var charmB))
            {
                _charmSprites[girlA] = charmB;
            }
            else
            {
                _charmSprites.Remove(girlA);
            }

            _charmSprites[girlB] = charmA;
        }
        else if (_charmSprites.TryGetValue(girlB, out var charmB))
        {
            if (_charmSprites.TryGetValue(girlA, out var charmA2))
            {
                _charmSprites[girlB] = charmA2;
            }
            else
            {
                _charmSprites.Remove(girlB);
            }

            _charmSprites[girlA] = charmB;
        }
    }

    private static TextureRsScale _charmScale = new TextureRsScale(new Vector2(14 / 45f, 14 / 45f));

    private static Sprite GetCharmSprite(string name) => new SpriteInfoTexture(new TextureInfoCache(
        Path.Combine(SingleDate.Plugin.ImagesDir, $"{name}Charm.png"),
        new TextureInfoExternal(Path.Combine(_charmsDir, $"{name}.png"), FilterMode.Bilinear, [
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
        if (_charmSprites.TryGetValue(girlId, out var sprite))
        {
            return sprite;
        }

        return _defaultCharmSprite;
    }
}
