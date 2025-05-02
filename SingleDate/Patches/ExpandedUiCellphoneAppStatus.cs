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

internal class ExpandedUiCellphoneAppStatus
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

    private static readonly FieldInfo _image = AccessTools.Field(typeof(ButtonBehavior), "_image");

    private RelativeId _girlId;
    private RectTransform _charmTransform;
    private List<Sequence> _sequences = new List<Sequence>();
    private UiCellphoneAppStatus _uiCellphoneAppPair;

    public ExpandedUiCellphoneAppStatus(UiCellphoneAppStatus uiCellphoneAppPair)
    {
        _uiCellphoneAppPair = uiCellphoneAppPair;
    }

    public void Start()
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        //move right things more to the center
        foreach (var left_right in _uiCellphoneAppPair.baggageSlotsLeft.Reverse().Zip(_uiCellphoneAppPair.baggageSlotsRight, (x, y) => (x, y)))
        {
            left_right.y.transform.position = (left_right.x.transform.position + left_right.y.transform.position) / 2;
        }

        _uiCellphoneAppPair.sentimentRollerRight.transform.position = _uiCellphoneAppPair.passionRollerLeft.transform.position;

        //remove left and relationship slot
        _girlId = ModInterface.Data.GetDataId(GameDataType.Girl, Game.Session.Puzzle.puzzleStatus.girlStatusRight.girlDefinition.id);

        _uiCellphoneAppPair.canvasGroupLeft.transform.SetParent(null);
        _uiCellphoneAppPair.statusPortraitLeft.transform.SetParent(null);

        _uiCellphoneAppPair.relationshipSlot.transform.SetParent(null);

        //hide stamina on dates
        if (Game.Session.Location.AtLocationType(LocationType.DATE))
        {
            _uiCellphoneAppPair.staminaMeterRight.transform.SetParent(null);

            var staminaRectTransform = _uiCellphoneAppPair.staminaMeterRight.GetComponent<RectTransform>();
            _uiCellphoneAppPair.statusPortraitRight.transform.position = new Vector3(
                _uiCellphoneAppPair.statusPortraitRight.transform.position.x - (staminaRectTransform.sizeDelta.x / 2),
                _uiCellphoneAppPair.statusPortraitRight.transform.position.y,
                _uiCellphoneAppPair.statusPortraitRight.transform.position.z);
        }

        // charm bg
        var charmBG_go = new GameObject();
        var charmBG_transform = charmBG_go.AddComponent<RectTransform>();
        charmBG_transform.sizeDelta = _charmSize;
        var charmBG_image = charmBG_go.AddComponent<Image>();

        var portraitImage = _image.GetValue<Image>(_uiCellphoneAppPair.statusPortraitRight.buttonBehavior);
        charmBG_transform.sizeDelta = portraitImage.rectTransform.sizeDelta;

        charmBG_image.sprite = portraitImage.sprite;
        charmBG_transform.SetParent(_uiCellphoneAppPair.transform);
        charmBG_transform.position = _uiCellphoneAppPair.statusPortraitLeft.transform.position + new Vector3(37.5f, 0, 0);

        //single relationship progress
        var saveGirl = State.SaveFile.GetGirl(_girlId);

        var radAllotment = Mathf.PI / State.MaxSingleGirlRelationshipLevel;

        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            for (int i = 0; i < State.MaxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_uiCellphoneAppPair.relationshipSlot.hornyIcon, i, radAllotment, _uiCellphoneAppPair.relationshipSlot.pauseDefinition);
            }
        }
        else if (State.MaxSingleGirlRelationshipLevel == saveGirl.RelationshipLevel)
        {
            for (int i = 0; i < State.MaxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_uiCellphoneAppPair.relationshipSlot.relationshipIcons[3], i, radAllotment, _uiCellphoneAppPair.relationshipSlot.pauseDefinition);
            }
        }
        else
        {
            int i = 0;
            for (; i < State.MaxSingleGirlRelationshipLevel - saveGirl.RelationshipLevel; i++)
            {
                MakeHeart(_uiCellphoneAppPair.relationshipSlot.relationshipIcons[0], i, radAllotment, _uiCellphoneAppPair.relationshipSlot.pauseDefinition);
            }

            for (; i < State.MaxSingleGirlRelationshipLevel; i++)
            {
                MakeHeart(_uiCellphoneAppPair.relationshipSlot.relationshipIcons[2], i, radAllotment, _uiCellphoneAppPair.relationshipSlot.pauseDefinition);
            }
        }

        //charm
        var charm_go = new GameObject();
        _charmTransform = charm_go.AddComponent<RectTransform>();
        _charmTransform.sizeDelta = _charmSize;
        var charm_image = charm_go.AddComponent<Image>();
        charm_image.sprite = UiPrefabs.GetCharmSprite(_girlId);
        _charmTransform.SetParent(_uiCellphoneAppPair.transform);
        _charmTransform.position = _uiCellphoneAppPair.statusPortraitLeft.transform.position + _charmOffset;
        _charmTransform.pivot = new Vector2(0.5f, 0f);

        MakeCharmSequence(null);
    }

    private int _charmDir = 1;

    public void MakeCharmSequence(Sequence previous)
    {
        if (previous != null)
        {
            Game.Manager.Time.KillTween(previous);
            _sequences.Remove(previous);
        }

        var sequence = DOTween.Sequence().SetLoops(1, LoopType.Restart);
        _sequences.Add(sequence);
        sequence.OnComplete(() => MakeCharmSequence(sequence));

        for (int i = 0; i < 15; i++)
        {
            switch (Random.Range(0, 15))
            {
                case 0:
                case 1:
                    sequence.Append(_charmTransform.DOLocalJump(_charmTransform.localPosition + new Vector3(Random.Range(-60f, 60f), 0), Random.Range(10f, 20f), Random.Range(1, 2), 1.2f).SetEase(Ease.Linear));
                    break;
                case 2:
                case 3:
                    _charmDir = _charmDir * -1;
                    break;
                case 4:
                    sequence.Join(_charmTransform.DOScaleY(0.95f, 1f).SetEase(Ease.InOutElastic));
                    sequence.Join(_charmTransform.DOScaleX(1.05f * _charmDir, 1f).SetEase(Ease.InOutElastic));
                    break;
                case 5:
                    sequence.Join(_charmTransform.DOScaleY(1.05f, 1f).SetEase(Ease.InOutElastic));
                    sequence.Join(_charmTransform.DOScaleX(0.95f * _charmDir, 1.2f).SetEase(Ease.InOutElastic));
                    break;
                case 6:
                case 7:
                    sequence.Append(_charmTransform.DOScaleX(-1f * _charmDir, 1).SetEase(Ease.InSine));
                    sequence.Append(_charmTransform.DOScaleX(1f * _charmDir, 1).SetEase(Ease.OutSine));
                    break;
                case 8:
                    //build up
                    sequence.Append(_charmTransform.DOScaleX(1.4f * _charmDir, 1.2f).SetEase(Ease.InOutSine));
                    sequence.Join(_charmTransform.DOScaleY(0.75f, 1.2f).SetEase(Ease.InOutSine));
                    sequence.AppendInterval(0.45f);

                    //jump
                    sequence.Append(_charmTransform.DOLocalJump(_charmTransform.localPosition, 200f, 1, 3.5f).SetEase(Ease.OutBack));

                    //spin
                    var squashNSpin = DOTween.Sequence().SetLoops(1, LoopType.Incremental);

                    squashNSpin.Append(_charmTransform.DOScaleX(0.85f * _charmDir, 0.25f).SetEase(Ease.OutSine)).OnComplete(() =>
                    {
                        _charmTransform.pivot = new Vector2(0.5f, 0.5f);
                    });
                    squashNSpin.Join(_charmTransform.DOScaleY(1.1f, 0.25f).SetEase(Ease.OutSine));

                    squashNSpin.Append(_charmTransform.DORotate(new Vector3(0f, 0f, _charmDir * 360f), 2.3f, RotateMode.FastBeyond360).SetEase(Ease.OutBack)).OnComplete(() =>
                    {
                        _charmTransform.pivot = new Vector2(0.5f, 0f);
                    });

                    squashNSpin.Append(_charmTransform.DOScaleX(_charmDir, 1f).SetEase(Ease.OutElastic));
                    squashNSpin.Join(_charmTransform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic));
                    sequence.Join(squashNSpin);
                    break;
                case 9:
                    if (_girlId == Girls.SarahId)
                    {
                        sequence.Append(_charmTransform.DOScaleX(_charmDir * 1.3f, 0.4f).SetEase(Ease.OutElastic));
                        sequence.Join(_charmTransform.DOScaleY(0.5f, 0.4f).SetEase(Ease.OutElastic));

                        sequence.Append(_charmTransform.DOScaleX(_charmDir * 0.5f, 0.4f).SetEase(Ease.OutElastic));
                        sequence.Join(_charmTransform.DOScaleY(1.3f, 0.4f).SetEase(Ease.OutElastic));

                        sequence.Append(_charmTransform.DOScaleX(_charmDir * 1.3f, 0.4f).SetEase(Ease.OutElastic));
                        sequence.Join(_charmTransform.DOScaleY(0.5f, 0.4f).SetEase(Ease.OutElastic));

                        sequence.Append(_charmTransform.DOScaleX(_charmDir * 0.5f, 0.4f).SetEase(Ease.OutElastic));
                        sequence.Join(_charmTransform.DOScaleY(1.3f, 0.4f).SetEase(Ease.OutElastic));
                    }
                    else if (_girlId == Girls.LillianId)
                    {
                        sequence.Append(_charmTransform.DOLocalMoveX(_charmTransform.localPosition.x + (90 * _charmDir), 1.2f));
                        sequence.Join(_charmTransform.DOLocalMoveY(_charmTransform.localPosition.y + 48, 1.2f));
                        sequence.Join(_charmTransform.DOScaleY(0.9f, 1.2f));
                        sequence.Join(_charmTransform.DOScaleX(1.1f * _charmDir, 1.2f));
                        sequence.Join(_charmTransform.DORotate(new Vector3(0f, 0f, _charmDir * 90), 1.2f));

                        sequence.Append(_charmTransform.DOScaleY(1.1f, 1.2f).SetEase(Ease.InOutElastic));
                        sequence.Join(_charmTransform.DOScaleX(0.9f * _charmDir, 1.2f).SetEase(Ease.InOutElastic));

                        sequence.AppendInterval(Random.Range(8f, 30f));

                        sequence.Append(_charmTransform.DOScaleY(0.9f, 1.2f).SetEase(Ease.InOutSine));
                        sequence.Join(_charmTransform.DOScaleX(1.1f * _charmDir, 1.2f).SetEase(Ease.InOutSine));
                        sequence.Join(_charmTransform.DORotate(new Vector3(0f, 0f, 0), 1.2f));
                        sequence.Join(_charmTransform.DOLocalMoveX(_charmTransform.localPosition.x, 1.2f));
                        sequence.Join(_charmTransform.DOLocalMoveY(_charmTransform.localPosition.y, 1.2f));

                        sequence.Append(_charmTransform.DOScaleY(1f, 1.2f).SetEase(Ease.InOutElastic));
                        sequence.Join(_charmTransform.DOScaleX(1f * _charmDir, 1.2f).SetEase(Ease.InOutElastic));
                    }
                    else if (_girlId == Girls.AbiaId)
                    {
                        sequence.Append(_charmTransform.DOShakeAnchorPos(4f, 30));
                    }
                    break;
                default:
                    sequence.AppendInterval(2f);
                    break;
            }
        }

        sequence.Append(_charmTransform.DOLocalJump(_charmTransform.localPosition, 20f, 2, 3f).SetEase(Ease.OutBounce));
        sequence.Join(_charmTransform.DOScaleX(_charmDir, 1.5f).SetEase(Ease.InOutElastic));
        sequence.Join(_charmTransform.DOScaleY(1f, 1.5f).SetEase(Ease.InOutElastic));
        sequence.AppendInterval(1f);

        Game.Manager.Time.Play(sequence, _uiCellphoneAppPair.relationshipSlot.pauseDefinition, 0f);
    }

    public void OnDestroy()
    {
        foreach (var sequence in _sequences)
        {
            Game.Manager.Time.KillTween(sequence, false, false);
        }

        _expansions.Remove(_uiCellphoneAppPair);
    }

    private void MakeHeart(Sprite sprite, int index, float radAllotment, PauseDefinition pauseDefinition)
    {
        var heart_go = new GameObject();
        heart_go.name = $"heart_{index}";
        var heart_rectTransform = heart_go.AddComponent<RectTransform>();
        heart_rectTransform.sizeDelta = _heartSize;
        var heart_image = heart_go.AddComponent<Image>();
        heart_image.sprite = sprite;
        heart_rectTransform.SetParent(_uiCellphoneAppPair.transform);

        var rads = radAllotment * (index + 0.5f);

        heart_rectTransform.position = _uiCellphoneAppPair.statusPortraitLeft.transform.position
            + _heartOffset
            + (new Vector3(Mathf.Cos(rads), Mathf.Sin(rads)) * _heartDist);

        var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
        _sequences.Add(sequence);

        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y + 8f, 0.7f).SetEase(Ease.InOutSine));
        sequence.Append(heart_rectTransform.DOLocalMoveY(heart_rectTransform.localPosition.y, 2.5f).SetEase(Ease.OutElastic));
        sequence.AppendInterval(3f);

        Game.Manager.Time.Play(sequence, pauseDefinition, 0.8f * index);
    }
}