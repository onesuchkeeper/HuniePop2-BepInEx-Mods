using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace SingleDate;

/// <summary>Hooks the cellphone's status app so it can be rebuilt for single dates when it starts.</summary>
[HarmonyPatch(typeof(UiCellphoneAppStatus))]
internal static class UiCellphoneAppStatusPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(UiCellphoneAppStatus __instance)
    {
        ExpandedUiCellphoneAppStatus.Get(__instance).Start();
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(UiCellphoneAppStatus __instance)
    {
        ExpandedUiCellphoneAppStatus.Get(__instance).OnDestroy();
    }
}

/// <summary>
/// Rebuilds the cellphone's status app (the two portraits with their stamina, baggage and relationship) for a single
/// date. The left portrait, relationship slot and left side are removed. In their place go a ring of hearts showing
/// the girl's single date level (a row of hearts that fill as she levels up) around where the left portrait was,
/// and an animated charm picture of the girl below them. Nothing changes on double dates.
/// </summary>
internal class ExpandedUiCellphoneAppStatus : UiPatchController<UiCellphoneAppStatus>
{
    // How far from the center of the left portrait the hearts are placed, in UI pixels. The hearts form a half circle above it.
    private const float HEART_DIST = 96;
    // Size of each heart image.
    private static readonly Vector3 HEART_SIZE = new Vector3(100, 100);
    // Where the center of the heart half circle is, relative to the left portrait. 37.5 pixels right, 4 down.
    private static readonly Vector3 HEART_OFFSET = new Vector3(37.5f, -4, 0);
    // Where the charm's pivot is, relative to the left portrait: 37.5 pixels right (centered on the heart ring) and 140 down.
    private static readonly Vector3 CHARM_OFFSET = new Vector3(37.5f, -140, 0);
    // Size of the charm image.
    private static readonly Vector2 CHARM_SIZE = new Vector2(220, 220);

    // One expansion per status app instance.
    private static Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus> _expansions
        = new Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus>();

    /// <summary>Gets the expansion for the status app, creating it the first time.</summary>
    public static ExpandedUiCellphoneAppStatus Get(UiCellphoneAppStatus uiCellphoneAppPair)
    {
        if (!_expansions.TryGetValue(uiCellphoneAppPair, out var expansion))
        {
            expansion = new ExpandedUiCellphoneAppStatus(uiCellphoneAppPair);
            _expansions[uiCellphoneAppPair] = expansion;
        }

        return expansion;
    }

    // ButtonBehavior._image (private), the portrait's image.
    private static readonly FieldInfo f_image = AccessTools.Field(typeof(ButtonBehavior), "_image");

    // The girl being displayed (the right girl).
    private RelativeId _girlId;
    // The charm image, the sequence that animates it, and the context passed to its animations. The hearts have a looping sequence each.
    private RectTransform _charmTransform;
    private List<Sequence> _heartSequences = new List<Sequence>();
    private Sequence _charmSequence;
    private CharmAnimationContext _charmContext;

    public ExpandedUiCellphoneAppStatus(UiCellphoneAppStatus uiCellphoneAppStatus) : base(uiCellphoneAppStatus) {}

    /// <summary>
    /// Rebuilds the status app for a single date, if this is one. Does nothing otherwise.
    /// </summary>
    protected override void Apply()
    {
        if (!State.IsSingleDate) return;
        ModInterface.Log.Message("Using single date UiCellphoneAppStatus");

        // Baggage: bring each right baggage slot halfway to the matching left slot, so the right slots are in the middle with the left side gone.
        // move right things more to the center
        foreach (var left_right in _core.baggageSlotsLeft
            .Reverse()
            .Zip(_core.baggageSlotsRight, (x, y) => (x.GetComponent<RectTransform>(), y.GetComponent<RectTransform>())))
        {
            left_right.Item2.position = (left_right.Item1.position + left_right.Item2.position) / 2;
        }

        // Get RectTransforms for easier positioning
        var canvasRect = _core.GetComponent<RectTransform>();
        var leftPortraitRect = _core.statusPortraitLeft.GetComponent<RectTransform>();
        var rightPortraitRect = _core.statusPortraitRight.GetComponent<RectTransform>();
        var relationshipSlotRect = _core.relationshipSlot.GetComponent<RectTransform>();

        // Put the right sentiment roller at the left passion roller's position.
        // Align sentiment roller to passion roller
        var sentimentRect = _core.sentimentRollerRight.GetComponent<RectTransform>();
        var passionRect = _core.passionRollerLeft.GetComponent<RectTransform>();
        sentimentRect.anchoredPosition = passionRect.anchoredPosition;

        // The girl is the right girl. (The left and relationship slots are removed at the end of this method.)
        // Remove left and relationship slot
        _girlId = ModInterface.Data.GetDataId(GameDataType.Girl, Game.Session.Puzzle.puzzleStatus.girlStatusRight.girlDefinition.id);

        // Stamina isn't used in single date puzzles (see SingleDateAilment), so detach the meter and shift the portrait left by half the meter's width to compensate.
        // Hide stamina on dates
        if (ModInterface.GameState.CurrentState.Id == GameStateId.Puzzle)
        {
            var staminaRect = _core.staminaMeterRight.GetComponent<RectTransform>();
            _core.staminaMeterRight.transform.SetParent(null);
            rightPortraitRect.anchoredPosition -= new Vector2(staminaRect.sizeDelta.x / 2, 0);
        }

        // A copy of the portrait art is put where the left portrait was, as a backdrop for the charm.
        // The charm, with its pivot at the bottom so animations squash and jump from its base.
        // Charm background
        var charmBG_go = new GameObject("CharmBG");
        var charmBG_transform = charmBG_go.AddComponent<RectTransform>();
        var charmBG_image = charmBG_go.AddComponent<Image>();

        var portraitImage = f_image.GetValue<Image>(_core.statusPortraitRight.buttonBehavior);
        charmBG_transform.sizeDelta = portraitImage.rectTransform.sizeDelta;
        charmBG_image.sprite = portraitImage.sprite;

        charmBG_transform.SetParent(canvasRect, false);
        charmBG_transform.anchoredPosition = leftPortraitRect.anchoredPosition + new Vector2(37.5f, 0);

        // Hearts: one per single date level, laid out in a half circle (see MakeHeart).
        // - Bonus round: all hearts are the horny icon.
        // - At max level (lovers): all hearts are the lovers icon (relationshipIcons[3]).
        // - Otherwise: the levels not yet earned are the first, empty hearts (relationshipIcons[0]), then the earned ones
        //   (relationshipIcons[2], the attracted icon) fill the rest.
        // Build hearts
        var saveGirl = State.SaveFile.GetGirl(_girlId);

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        var radAllotment = Mathf.PI / maxSingleGirlRelationshipLevel;

        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            for (int i = 0; i < maxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_core.relationshipSlot.hornyIcon, i, radAllotment, _core.relationshipSlot.pauseDefinition, leftPortraitRect);
            }
        }
        else if (maxSingleGirlRelationshipLevel == saveGirl.RelationshipLevel)
        {
            for (int i = 0; i < maxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_core.relationshipSlot.relationshipIcons[3], i, radAllotment, _core.relationshipSlot.pauseDefinition, leftPortraitRect);
            }  
        }
        else
        {
            for (int i = 0; i < maxSingleGirlRelationshipLevel - saveGirl.RelationshipLevel; i++)
            {
                MakeHeart(_core.relationshipSlot.relationshipIcons[0], i, radAllotment, _core.relationshipSlot.pauseDefinition, leftPortraitRect);
            }

            for (int i = maxSingleGirlRelationshipLevel - saveGirl.RelationshipLevel; i < maxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_core.relationshipSlot.relationshipIcons[2], i, radAllotment, _core.relationshipSlot.pauseDefinition, leftPortraitRect);
            }
        }

        // Charm
        var charm_go = new GameObject("Charm");
        _charmTransform = charm_go.AddComponent<RectTransform>();
        _charmTransform.sizeDelta = CHARM_SIZE;
        var charm_image = charm_go.AddComponent<Image>();
        charm_image.sprite = UiPrefabs.GetCharmSprite(_girlId);

        _charmTransform.SetParent(canvasRect, false);
        _charmTransform.anchoredPosition = leftPortraitRect.anchoredPosition + new Vector2(CHARM_OFFSET.x, CHARM_OFFSET.y);
        _charmTransform.pivot = new Vector2(0.5f, 0f);

        MakeCharmSequence();

        // Finally detach the left side (its canvas group, portrait and the relationship slot) so it's no longer displayed.
        _core.canvasGroupLeft.transform.SetParent(null);
        _core.statusPortraitLeft.transform.SetParent(null);
        _core.relationshipSlot.transform.SetParent(null);
    }

    /// <summary>
    /// Starts a new random charm animation sequence. Called once to begin, and again every time a sequence completes so
    /// the charm keeps animating. Sequences are built to fill a 10 point budget (see <see cref="CharmAnimationRegistry"/>)
    /// and end with a bounce back to rest, facing the current direction.
    /// </summary>
    public void MakeCharmSequence()
    {
        if (_charmSequence != null)
        {
            Game.Manager.Time.KillTween(_charmSequence);
        }
        else
        {
            _charmContext = new CharmAnimationContext()
            {
                Dir = 1,
                GirlId = _girlId,
                Transform = _charmTransform
            };
        }

        _charmSequence = Plugin.CharmAnimationRegistry.CreateSequence(_charmContext, 15f);
        _charmSequence.OnComplete(MakeCharmSequence);

        _charmSequence.Append(_charmTransform.DOLocalJump(_charmTransform.localPosition, 20f, 2, 3f).SetEase(Ease.OutBounce));
        _charmSequence.Join(_charmTransform.DOScaleX(_charmContext.Dir, 1.5f).SetEase(Ease.InOutElastic));
        _charmSequence.Join(_charmTransform.DOScaleY(1f, 1.5f).SetEase(Ease.InOutElastic));
        _charmSequence.AppendInterval(1f);

        Game.Manager.Time.Play(_charmSequence, _core.relationshipSlot.pauseDefinition, 0f);
    }

    /// <summary>Stops every animation and forgets the expansion when the status app is destroyed.</summary>
    protected override void OnCleanup()
    {
        foreach (var sequence in _heartSequences)
        {
            Game.Manager.Time.KillTween(sequence, false, false);
        }

        if (_charmSequence != null)
        {
            Game.Manager.Time.KillTween(_charmSequence, false, false);
        }
        
        _expansions.Remove(_core);
    }

    /// <summary>
    /// Creates one heart on the half circle above the left portrait and starts its bobbing animation (up 8 pixels, then
    /// elastic back, then wait). Each heart starts a little later (0.8 seconds each) than the one before so they bob in a wave.
    /// </summary>
    /// <param name="sprite">The heart's icon.</param>
    /// <param name="index">Which heart this is, left to right.</param>
    /// <param name="radAllotment">The angle each heart gets of the half circle (pi divided by the number of hearts).</param>
    /// <param name="pauseDefinition">What the animation pauses with.</param>
    /// <param name="parentRect">The portrait the half circle is centered on.</param>
    private void MakeHeart(Sprite sprite, int index, float radAllotment, PauseDefinition pauseDefinition, RectTransform parentRect)
    {
        var heart_go = new GameObject($"heart_{index}");
        var heart_rectTransform = heart_go.AddComponent<RectTransform>();
        heart_rectTransform.sizeDelta = HEART_SIZE;
        var heart_image = heart_go.AddComponent<Image>();
        heart_image.sprite = sprite;

        heart_rectTransform.SetParent(parentRect.parent, false); // same canvas as portraits

        // Place the heart in the middle of its slice of the half circle.
        var rads = radAllotment * (index + 0.5f);
        heart_rectTransform.anchoredPosition = parentRect.anchoredPosition
            + (Vector2)HEART_OFFSET
            + new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * HEART_DIST;

        var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
        _heartSequences.Add(sequence);

        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y + 8f, 0.7f).SetEase(Ease.InOutSine));
        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y, 2.5f).SetEase(Ease.OutElastic));
        sequence.AppendInterval(3f);

        Game.Manager.Time.Play(sequence, pauseDefinition, 0.8f * index);
    }
}
