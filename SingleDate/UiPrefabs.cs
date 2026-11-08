using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Holds the Unity objects and sprites for the single date UI: the pair slot backgrounds, sensitivity art, charms,
/// the single date action bubbles and the single date meeting cutscene. Loaded once and reused.
/// </summary>
public static class UiPrefabs
{
    // Folder of the Digital Art Collection mod's charm art (two folders above the plugins folder). Charms are only loaded if it exists.
    private static readonly string _charmsDir = Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection", "Charms");

    /// <summary>Background of a single pair's slot in the cellphone's pairs app, when moused over.</summary>
    public static Sprite SingleUiAppPairSlotBgOver => _singleUiAppPairSlotBgOver;
    private static Sprite _singleUiAppPairSlotBgOver;

    /// <summary>Background of a single pair's slot in the cellphone's pairs app.</summary>
    public static Sprite SingleUiAppPairSlotBg => _singleUiAppPairSlotBg;
    private static Sprite _singleUiAppPairSlotBg;

    /// <summary>Icon for sensitivity in the pair app.</summary>
    public static Sprite SensitivityIcon => _sensitivityIcon;
    private static Sprite _sensitivityIcon;

    /// <summary>Plate sprite for the sensitivity level tooltip.</summary>
    public static Sprite SensitivityPlate => _sensitivityPlate;
    private static Sprite _sensitivityPlate;

    /// <summary>Front sprite of the sensitivity EXP meter.</summary>
    public static Sprite SensitivityMeter => _sensitivityMeter;
    private static Sprite _sensitivityMeter;

    /// <summary>The talk/date action bubbles for single dates, a modified copy of the base game's. Built by <see cref="InitActionBubbles"/>.</summary>
    public static UiWindow SingleDateBubbles => _singleBubbles;
    private static UiWindowActionBubbles _singleBubbles;

    /// <summary>The arrival cutscene on single dates, built by <see cref="InitCutsceneMeeting"/>.</summary>
    public static CutsceneDefinition SingleCutsceneMeeting => _singleCutsceneMeeting;
    private static CutsceneDefinition _singleCutsceneMeeting;

    // Charm used for girls that don't have one set.
    private static Sprite _defaultCharmSprite;

    /// <summary>Loads the sprites from the asset bundle and the charms for the base game girls.</summary>
    public static void InitExternals(AssetBundle assetBundle)
    {
        _singleUiAppPairSlotBg = assetBundle.LoadAsset<Sprite>("ui_app_pairs_pair_bg");
        _singleUiAppPairSlotBgOver = assetBundle.LoadAsset<Sprite>("ui_app_pairs_pair_bg_over");
        _sensitivityIcon = assetBundle.LoadAsset<Sprite>("ui_app_pair_icon_sensitivity");
        _sensitivityPlate = assetBundle.LoadAsset<Sprite>("ui_app_pair_sensitivity");
        _sensitivityMeter = assetBundle.LoadAsset<Sprite>("ui_app_pair_meter_sensitivity");

        if (Directory.Exists(_charmsDir))
        {
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Abia, GetCharmSprite("Abia"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Ashley, GetCharmSprite("Ashley"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Brooke, GetCharmSprite("Brooke"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Candace, GetCharmSprite("Candace"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Jessie, GetCharmSprite("Jessie"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Jewn, GetCharmSprite("Jewn"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Kyu, GetCharmSprite("Kyu"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Lailani, GetCharmSprite("Lailani"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Lillian, GetCharmSprite("Lillian"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Lola, GetCharmSprite("Lola"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Moxie, GetCharmSprite("Moxie"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Nora, GetCharmSprite("Nora"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Polly, GetCharmSprite("Polly"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Sarah, GetCharmSprite("Sarah"));
            Plugin.SetGirlCharm(Hp2BaseMod.GirlId.Zoey, GetCharmSprite("Zoey"));
        }

        _defaultCharmSprite = assetBundle.LoadAsset<Sprite>("DefaultCharm");
    }

    // Charm art is scaled down to 14/45 (about 31%) of its source size when it is loaded and cached.
    private static TextureRsScale _charmScale = new TextureRsScale(new Vector2(14 / 45f, 14 / 45f));

    /// <summary>
    /// Loads a base game girl's charm from the art collection folder, scaled and cached as {name}Charm.png in the mod's
    /// images folder so scaling only happens once.
    /// </summary>
    private static Sprite GetCharmSprite(string name) => new SpriteInfoTexture(new TextureInfoCache(
        Path.Combine(SingleDate.Plugin.IMAGES_DIR, $"{name}Charm.png"),
        new TextureInfoExternal(Path.Combine(_charmsDir, $"{name}.png"), false, FilterMode.Bilinear, [
            _charmScale
        ])
    )).GetSprite();

    /// <summary>
    /// Builds the single date action bubbles by copying the base game's and rearranging them for one girl: everything
    /// is shifted right by the header's x range, the date bubble is placed where the bad talk bubble was, the good talk
    /// bubble is nudged next to it and the bad talk bubble is removed, leaving one talk and one date bubble.
    /// </summary>
    /// <param name="actionBubblesWindow">The base game's action bubbles window to copy.</param>
    public static void InitActionBubbles(UiWindow actionBubblesWindow)
    {
        // How far the header's x positions move between its two states, the bubbles are moved by that to follow the shifted UI.
        var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;
        _singleBubbles = (UiWindowActionBubbles)Object.Instantiate(actionBubblesWindow);

        // Find the bubbles by type. Talk bubbles are told apart by value, 0 is the bad talk bubble.
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

        // Move the date bubble to the bad talk bubble's spot (+48 x, +32 y offset, in screen units, tuned by eye).
        // It's wrapped in a scaled padding object (29/33, the date bubble's size relative to the talk bubbles) so the
        // date bubble is drawn smaller to fit in.
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

        // Nudge the good talk bubble the opposite way (-48 x, +32 y) so the two remaining bubbles sit side by side.
        if (goodTalkBubble != null)
        {
            goodTalkBubble.transform.position = new Vector3(goodTalkBubble.transform.position.x - 48,
                goodTalkBubble.transform.position.y + 32f);
        }

        if (badTalkBubble != null)
        {
            // Remove the bad talk bubble (a single girl has no one to talk badly to).
            badTalkBubble.transform.SetParent(null);
            UnityEngine.Object.Destroy(badTalkBubble);
            _singleBubbles.actionBubbles.Remove(badTalkBubble);
        }
    }

    /// <summary>Gets the girl's charm sprite, or the default charm if she has none.</summary>
    public static Sprite GetCharmSprite(RelativeId girlId)
    {
        if (Plugin.TryGetSingleDateGirl(girlId, out var singleDateGirl)
            && singleDateGirl.CharmSprite != null)
        {
            return singleDateGirl.CharmSprite;
        }

        return _defaultCharmSprite;
    }

    /// <summary>
    /// Builds the single date arrival cutscene from the base game's meeting cutscene: moves the doll to the inner
    /// position facing right, plays the unknown-relationship sub cutscene for the pair, then the base cutscene's
    /// compatibility banner step and shows the phone and header.
    /// </summary>
    /// <param name="cutsceneMeeting">The base game's meeting cutscene, its step 5 is reused.</param>
    internal static void InitCutsceneMeeting(CutsceneDefinition cutsceneMeeting)
    {
        _singleCutsceneMeeting = new CutsceneDefinition()
        {
            cleanUpType = CutsceneCleanUpType.NONE,
            steps = new(){
                CutsceneStepUtility.MakeDollMove(DollPositionType.INNER).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeSubCutsceneGirlPair(GirlPairRelationshipType.UNKNOWN, CutsceneStepProceedType.AUTOMATIC),
                // Index 5 of the base meeting cutscene is the compatibility banner, if the base game's cutscene changes this index must too.
                cutsceneMeeting.steps[5],//compat banner
                CutsceneStepUtility.MakeTogglePhoneAndHeader(true, CutsceneStepProceedType.AUTOMATIC)
            }
        };
    }
}
