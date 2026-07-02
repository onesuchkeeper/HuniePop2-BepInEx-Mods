using System.Collections;
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

internal class ExpandedUiCellphoneAppStatus : UiPatchController<UiCellphoneAppStatus>
{
    private static readonly float _heartDist = 96;
    private static readonly Vector3 _heartSize = new Vector3(100, 100);
    private static readonly Vector3 _heartOffset = new Vector3(37.5f, -20 + 16, 0);
    private static readonly Vector3 _charmOffset = new Vector3(37.5f, -30 - 110, 0);
    private static readonly Vector2 _charmSize = new Vector2(220, 220);

    private static Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus> _expansions
        = new Dictionary<UiCellphoneAppStatus, ExpandedUiCellphoneAppStatus>();

    public static ExpandedUiCellphoneAppStatus Get(UiCellphoneAppStatus uiCellphoneAppPair)
    {
        if (!_expansions.TryGetValue(uiCellphoneAppPair, out var expansion))
        {
            expansion = new ExpandedUiCellphoneAppStatus(uiCellphoneAppPair);
            _expansions[uiCellphoneAppPair] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_image = AccessTools.Field(typeof(ButtonBehavior), "_image");

    private RelativeId _girlId;
    private RectTransform _charmTransform;
    private List<Sequence> _heartSequences = new List<Sequence>();
    private Sequence _charmSequence;
    private CharmAnimationContext _charmContext;

    public ExpandedUiCellphoneAppStatus(UiCellphoneAppStatus uiCellphoneAppStatus) : base(uiCellphoneAppStatus) {}

    protected override void Apply()
    {
        if (!State.IsSingleDate) return;
        ModInterface.Log.Message("Using single date UiCellphoneAppStatus");

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

        // Align sentiment roller to passion roller
        var sentimentRect = _core.sentimentRollerRight.GetComponent<RectTransform>();
        var passionRect = _core.passionRollerLeft.GetComponent<RectTransform>();
        sentimentRect.anchoredPosition = passionRect.anchoredPosition;

        // Remove left and relationship slot
        _girlId = ModInterface.Data.GetDataId(GameDataType.Girl, Game.Session.Puzzle.puzzleStatus.girlStatusRight.girlDefinition.id);

        // Hide stamina on dates
        if (Game.Session.Location.AtLocationType(LocationType.DATE))
        {
            var staminaRect = _core.staminaMeterRight.GetComponent<RectTransform>();
            _core.staminaMeterRight.transform.SetParent(null);
            rightPortraitRect.anchoredPosition -= new Vector2(staminaRect.sizeDelta.x / 2, 0);
        }

        // Charm background
        var charmBG_go = new GameObject("CharmBG");
        var charmBG_transform = charmBG_go.AddComponent<RectTransform>();
        var charmBG_image = charmBG_go.AddComponent<Image>();

        var portraitImage = f_image.GetValue<Image>(_core.statusPortraitRight.buttonBehavior);
        charmBG_transform.sizeDelta = portraitImage.rectTransform.sizeDelta;
        charmBG_image.sprite = portraitImage.sprite;

        charmBG_transform.SetParent(canvasRect, false);
        charmBG_transform.anchoredPosition = leftPortraitRect.anchoredPosition + new Vector2(37.5f, 0);

        // Build hearts
        var saveGirl = State.SaveFile.GetGirl(_girlId);

        // if (saveGirl == null)
        // {
        //     ModInterface.Log.Message("save girl null D:");
        // }

        // There is some kind of DoTween issue here I think
        // this fixed it but it isn't a great solution...

        //this literally kills all the game's tweens
        //_uiCellphoneAppStatus.relationshipSlot.DestroyAndKillTweens();

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
                
            for (int i = 0; i < maxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_core.relationshipSlot.relationshipIcons[2], i, radAllotment, _core.relationshipSlot.pauseDefinition, leftPortraitRect);
            }  
        }

        // Charm
        var charm_go = new GameObject("Charm");
        _charmTransform = charm_go.AddComponent<RectTransform>();
        _charmTransform.sizeDelta = _charmSize;
        var charm_image = charm_go.AddComponent<Image>();
        charm_image.sprite = UiPrefabs.GetCharmSprite(_girlId);

        _charmTransform.SetParent(canvasRect, false);
        _charmTransform.anchoredPosition = leftPortraitRect.anchoredPosition + new Vector2(_charmOffset.x, _charmOffset.y);
        _charmTransform.pivot = new Vector2(0.5f, 0f);

        MakeCharmSequence();

        _core.canvasGroupLeft.transform.SetParent(null);
        _core.statusPortraitLeft.transform.SetParent(null);
        _core.relationshipSlot.transform.SetParent(null);
    }

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

        _charmSequence = Plugin.CharmAnimationRegistry.CreateSequence(_charmContext, 10f);
        _charmSequence.OnComplete(MakeCharmSequence);

        _charmSequence.Append(_charmTransform.DOLocalJump(_charmTransform.localPosition, 20f, 2, 3f).SetEase(Ease.OutBounce));
        _charmSequence.Join(_charmTransform.DOScaleX(_charmContext.Dir, 1.5f).SetEase(Ease.InOutElastic));
        _charmSequence.Join(_charmTransform.DOScaleY(1f, 1.5f).SetEase(Ease.InOutElastic));
        _charmSequence.AppendInterval(1f);

        Game.Manager.Time.Play(_charmSequence, _core.relationshipSlot.pauseDefinition, 0f);
    }

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

    private void MakeHeart(Sprite sprite, int index, float radAllotment, PauseDefinition pauseDefinition, RectTransform parentRect)
    {
        var heart_go = new GameObject($"heart_{index}");
        var heart_rectTransform = heart_go.AddComponent<RectTransform>();
        heart_rectTransform.sizeDelta = _heartSize;
        var heart_image = heart_go.AddComponent<Image>();
        heart_image.sprite = sprite;

        heart_rectTransform.SetParent(parentRect.parent, false); // same canvas as portraits

        var rads = radAllotment * (index + 0.5f);
        heart_rectTransform.anchoredPosition = parentRect.anchoredPosition
            + (Vector2)_heartOffset
            + new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * _heartDist;

        var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
        _heartSequences.Add(sequence);

        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y + 8f, 0.7f).SetEase(Ease.InOutSine));
        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y, 2.5f).SetEase(Ease.OutElastic));
        sequence.AppendInterval(3f);

        Game.Manager.Time.Play(sequence, pauseDefinition, 0.8f * index);
    }
}