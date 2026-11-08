using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedStyleDefinition"/> fields.
/// </summary>
[Expansion(typeof(UiDoll))]
public partial class ExpandedUiDoll
{
    [HarmonyPatch(typeof(UiDoll))]
    private class Patch
    {
        [HarmonyPatch(nameof(UiDoll.LoadGirl))]
        [HarmonyPostfix]
        private static void LoadGirl(UiDoll __instance, GirlDefinition girlDef, int expressionIndex, int hairstyleIndex, int outfitIndex, GirlDefinition soulGirlDef)
            => ExpandedUiDoll.Get(__instance).LoadGirl(girlDef);

        [HarmonyPatch(nameof(UiDoll.UnloadGirl))]
        [HarmonyPostfix]
        private static void UnloadGirl(UiDoll __instance)
            => ExpandedUiDoll.Get(__instance).UnloadGirl();

        [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
        [HarmonyPrefix()]
        private static void ChangeOutfit(UiDoll __instance, ref int outfitIndex)
            => ExpandedUiDoll.Get(__instance).ChangeOutfit(ref outfitIndex);

        [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
        [HarmonyPostfix()]
        private static void PostChangeOutfit(UiDoll __instance, int outfitIndex)
            => ExpandedUiDoll.Get(__instance).PostChangeOutfit();

        [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
        [HarmonyPrefix()]
        private static bool ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
            => ExpandedUiDoll.Get(__instance).ChangeHairstyle(hairstyleIndex);

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix()]
        private static void OnDestroy(UiDoll __instance)
            => ExpandedUiDoll.Destroy(__instance);

        [HarmonyPatch(nameof(UiDoll.ShowEnergySurge))]
        [HarmonyPrefix]
        private static bool ShowEnergySurge(UiDoll __instance, EnergyDefinition energyDef, float duration, bool knockback, bool negative = false, bool silent = false)
        {
            // Safety - Skip showing surge is there is no definition
            return __instance.girlDefinition != null;
        }

        private static readonly AccessTools.FieldRef<UiDoll, DialogLine> CurrentDialogLine = AccessTools.FieldRefAccess<UiDoll, DialogLine>("_currentDialogLine");
        private static readonly AccessTools.FieldRef<UiDoll, int> CurrentDialogPriority = AccessTools.FieldRefAccess<UiDoll, int>("_currentDialogPriority");

        /// <summary>
        /// Replaces the base game's ReadDialogTrigger, which picks a date greeting from the location type. The line is picked by
        /// <see cref="ExpandedUiDoll.ReadDialogTrigger"/> and the rest is the base game's.
        /// </summary>
        [HarmonyPatch(nameof(UiDoll.ReadDialogTrigger))]
        [HarmonyPrefix()]
        private static bool ReadDialogTrigger(UiDoll __instance, DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, ref int lineIndex)
        {
            // an unloaded doll has nothing to say, and the puzzle state may hold the girls back (the tutorial's cutscenes do the talking)
            if (__instance.girlDefinition == null
                || (ModInterface.GameState.CurrentState as IPuzzleState)?.SuppressesDollDialog == true)
            {
                return false;
            }

            // a line already being said is only interrupted by one of at least the same priority
            if (CurrentDialogLine(__instance) != null
                && dialogTriggerDef.priority < CurrentDialogPriority(__instance)
                && format == DialogLineFormat.UNCHECKED)
            {
                return false;
            }

            ExpandedUiDoll.Get(__instance).ReadDialogTrigger(dialogTriggerDef, format, ref lineIndex);

            var lineSet = dialogTriggerDef.GetLineSetByGirl(__instance.soulGirlDefinition);
            lineIndex = Mathf.Clamp(lineIndex, 0, lineSet.dialogLines.Count - 1);

            Game.Session.Dialog.SetLastDialogTriggerIndex(__instance.girlDefinition, dialogTriggerDef, lineIndex);
            __instance.ReadDialogLine(lineSet.dialogLines[lineIndex], format, dialogTriggerDef.priority);

            return false;
        }

        [HarmonyPatch(nameof(UiDoll.SetExhaustion))]
        [HarmonyPrefix]
        private static void SetExhaustion_Prefix(UiDoll __instance, bool exhausted, bool upset, ref bool silent)
        {
            // Dialog lines are now handled by the IPuzzleStatusGirlState instances. They will play lines when needed
            silent = true;
        }

        [HarmonyPatch(nameof(UiDoll.ReadDialogLine))]
        [HarmonyPrefix]
        private static bool ReadDialogLine_Prefix(UiDoll __instance, DialogLine dialogLine, DialogLineFormat format, int priority)
            => ExpandedUiDoll.Get(__instance).ReadDialogLine_Prefix(dialogLine, format, priority);

        [HarmonyPatch(nameof(UiDoll.SetFocus))]
        [HarmonyPrefix]
        private static bool SetFocus(UiDoll __instance, bool focused, float duration)
            => ExpandedUiDoll.Get(__instance).SetFocus_Prefix(focused, duration);
    }

    private bool SetFocus_Prefix(bool focused, float duration)
    {
        if (!focused == _isUnfocused) return false;

        _isUnfocused = !focused;

        if (ModInterface.GameState.CurrentState?.DollFocusIsInteractive ?? false)
        {
            _core.focusButton.image.raycastTarget = _isUnfocused;
            _core.dropZoneCanvasGroup.blocksRaycasts = !_isUnfocused;
        }
        else
        {
            _core.focusButton.image.raycastTarget = false;
            _core.dropZoneCanvasGroup.blocksRaycasts = true;
        }

        Game.Manager.Time.KillTween(_focusSequence, false, true);

        var sequence = DOTween.Sequence().SetEase(Ease.InOutSine);
        _focusSequence = sequence;

        var mirror = _core.mirrored ? -1 : 1;

        if (_isUnfocused)
        {
            sequence.Insert(0f, DOTween.To(() => _core.uiEffectGroup.effectFactor, x => _core.uiEffectGroup.effectFactor = x, 0.66f, duration).SetEase(Ease.Linear));
            sequence.Insert(0f, _core.verticalLayer.DOAnchorPosX(100f, duration, false).SetEase(Ease.OutSine));
            sequence.Insert(0f, _core.verticalLayer.DOAnchorPosY(-150f, duration, false).SetEase(Ease.InSine));

            if (_core.unfocusedCover != null)
            {
                sequence.Insert(0f, _core.unfocusedCover.rectTransform.DOScaleX(4 * mirror, duration).SetEase(Ease.OutSine));
                sequence.Insert(0f, _core.unfocusedCover.DOFade(0.3f, duration).SetEase(Ease.Linear));
            }
        }
        else
        {
            sequence.Insert(0f, DOTween.To(() => _core.uiEffectGroup.effectFactor, x => _core.uiEffectGroup.effectFactor = x, 0f, duration).SetEase(Ease.Linear));
            sequence.Insert(0f, _core.verticalLayer.DOAnchorPosX(0f, duration, false).SetEase(Ease.InSine));
            sequence.Insert(0f, _core.verticalLayer.DOAnchorPosY(0f, duration, false).SetEase(Ease.OutSine));

            if (_core.unfocusedCover != null)
            {
                sequence.Insert(0f, _core.unfocusedCover.rectTransform.DOScaleX(mirror, duration).SetEase(Ease.InSine));
                sequence.Insert(0f, _core.unfocusedCover.DOFade(0f, duration).SetEase(Ease.Linear));
            }
        }

        Game.Manager.Time.Play(sequence, _core.pauseDefinition, 0f);

        RaiseFocusChangeEvent(_core);
        return false;
    }

    private bool ReadDialogLine_Prefix(DialogLine dialogLine, DialogLineFormat format, int priority)
    {
        if (_girlDefinition == null) return false;

        if (_currentDialogLine != null)
        {
            _core.StopDialogLine(true, true);
        }

        _currentDialogLine = dialogLine;

        var yuri = dialogLine.yuri && Game.Persistence.playerFile.settingGender == SettingGender.FEMALE;
        _currentDialogText = (yuri ? dialogLine.yuriDialogText : dialogLine.dialogText) + "▸▸▸▸▸";
        _currentDialogFormat = format;
        _currentDialogPriority = priority;
        _clickSkippedDialog = false;

        var audioClip = yuri ? dialogLine.yuriAudioClip : dialogLine.audioClip;
        var horny = _hasMood && _moodExpression == GirlExpressionType.HORNY;

        if (audioClip != null)
        {
            _dialogLineAudioLink = Game.Manager.Audio.Play(
                AudioCategory.VOICE, audioClip, _core.pauseDefinition,
                horny 
                    ? _core.soulGirlDefinition.sexVoiceVolume 
                    : _core.soulGirlDefinition.voiceVolume
            );
        }

        var text = _currentDialogText;

        _dialogTimestamp = Game.Manager.Time.Lifetime(_core.pauseDefinition);
        _dialogDuration = audioClip != null ? Mathf.Max(audioClip.length, 0.25f) : Mathf.Max(text.Length * 0.05f, 0.25f);
        _dialogLetterDuration = Mathf.Min(_dialogDuration / text.Length, 0.05f);
        _dialogExpressionIndex = 0;

        var state = ModInterface.GameState.CurrentState;
        if (!Game.Session.gameCanvas.cellphone.IsShowing() || (state?.DialogIgnoresCellphone ?? false))
        {
            if (_core.slideLayer.anchoredPosition.x >= 0f)
            {
                ResizeDialogBox(_core.dialogBoxPosValues.y, DIALOG_BOX_WIDTH_BIG, DIALOG_TEXT_WIDTH_BIG);
            }
            else
            {
                ResizeDialogBox(_core.dialogBoxPosValues.z, DIALOG_BOX_WIDTH_BIG, DIALOG_TEXT_WIDTH_BIG);
            }
        }
        else
        {
            ResizeDialogBox(_core.dialogBoxPosValues.x, DIALOG_BOX_WIDTH_SMALL, DIALOG_TEXT_WIDTH_SMALL);
        }

        _core.dialogText.text = PurifyDialogText(INLINE_STYLER_START + text + INLINE_STYLER_END);

        var boxHeight = Mathf.Max(192f + (_core.dialogText.preferredHeight - 138f), 192f);

        _core.ChangeExpression(dialogLine.startExpression.expressionType, dialogLine.startExpression.eyesClosed);
        Game.Manager.Time.KillTween(_dialogBoxSequence, true, true);

        if (!_isDialogBoxShowing && !_isDialogBoxLocked)
        {
            _core.dialogBox.rectTransform.sizeDelta = new Vector2(_core.dialogBox.rectTransform.sizeDelta.x, boxHeight);
            _core.dialogBox.rectTransform.anchoredPosition = new Vector2(0f, (boxHeight - 192f) * 0.5f);
        }
        else
        {
            _core.dialogBoxLayerCanvasGroup.alpha = horny ? 0f : 0.5f;
            _core.dialogBoxLayer.localScale = Vector3.one * 0.75f;
        }

        var sequence = DOTween.Sequence();
        _dialogBoxSequence = sequence;
        sequence.Insert(0f, _core.dialogBoxLayerCanvasGroup.DOFade(horny ? 0 : 1, 0.25f).SetEase(Ease.Linear));
        sequence.Insert(0f, _core.dialogBoxLayer.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        sequence.Insert(0f, _core.dialogBox.rectTransform.DOSizeDelta(new Vector2(_core.dialogBox.rectTransform.sizeDelta.x, boxHeight), 0.25f, false).SetEase(Ease.OutBack));
        sequence.Insert(0f, _core.dialogBox.rectTransform.DOAnchorPos(new Vector2(0f, (boxHeight - 192f) * 0.5f), 0.25f, false).SetEase(Ease.OutBack));
        Game.Manager.Time.Play(sequence, _core.pauseDefinition, 0f);

        _isDialogBoxShowing = true;
        _isDialogBoxLocked = false;

        RaiseDialogLineStartedEvent(_core);
        return false;
    }

    private UiDollSpecialEffect _specialEffect_hold;

    private void LoadGirl(GirlDefinition girlDef)
    {
        // Scale to match body
        var body = girlDef.GetExpansion().GetCurrentBody();

        if (body != null)
        {
            var vecScale = new Vector3(body.Scale, body.Scale, 1f);
            _core.partsLayer.localScale = vecScale;
            _core.outlineUiEffectGroup.transform.localScale = vecScale;
        }

        // Non-special girls won't load special effects by default
        if (_core?.soulGirlDefinition == null
            || _core.soulGirlDefinition.specialEffectPrefab == null)
        {
            return;
        }

        ModInterface.Log.Message($"Loading special effect {_core.soulGirlDefinition.specialEffectPrefab.name} for {_core.soulGirlDefinition.girlName}");

        var soul = _core.soulGirlDefinition;
        var prefab = soul.specialEffectPrefab;

        Vector2? offset = null;
        if (prefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
        {
            offset = body?.BackPos;
        }
        else if (prefab.GetType() == typeof(UiDollSpecialEffectGloWings))
        {
            offset = body?.HeadPos;
        }
        else if (prefab is IUiDollSpecialEffect uiDollSpecialEffect)
        {
            offset = uiDollSpecialEffect.GetSpecialEffectOffset(girlDef);
        }

        if (offset.HasValue)
        {
            soul.specialEffectOffset = offset.Value;
            // the wings read the offset from the doll's girlDefinition, not the soul
            _core.girlDefinition.specialEffectOffset = offset.Value;
        }

        // var specialEffectPrefab = _core.soulGirlDefinition.specialEffectPrefab;

        // if (specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
        // {
        //     _core.soulGirlDefinition.specialEffectOffset = body.BackPos;
        // }
        // else if (specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
        // {
        //     _core.soulGirlDefinition.specialEffectOffset = body.HeadPos;
        // }
        // else if (specialEffectPrefab is IUiDollSpecialEffect uiDollSpecialEffect)
        // {
        //     _core.soulGirlDefinition.specialEffectOffset = uiDollSpecialEffect.GetSpecialEffectOffset(girlDef);
        // }

        if (_core.soulGirlDefinition.specialCharacter) return;

        var specialEffectInstance = UnityEngine.Object.Instantiate(_core.soulGirlDefinition.specialEffectPrefab);

        _specialEffect = specialEffectInstance;
        specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
        specialEffectInstance.Init(_core);

        // outfit is changed before special effects are set, so correct specials here
        if (_core.soulGirlDefinition.outfits[f_currentOutfitIndex.GetValue<int>(_core)].GetExpansion().HideSpecial)
        {
            _specialEffect_hold = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect_hold?.rectTransform.SetParent(null, false);
        }
    }

    private void UnloadGirl()
    {
        if (_specialEffect_hold != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect_hold);
            _specialEffect_hold = null;
        }
    }

    private void ChangeOutfit(ref int outfitIndex)
    {
        if (_core.girlDefinition == null) return;

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_core.girlDefinition);
        var index = outfitIndex == -1
                ? playerFileGirl.outfitIndex
                : outfitIndex;

        if (index < 0 || index >= _core.girlDefinition.outfits.Count)
        {
            ModInterface.Log.Message($"out of range outfit index {index}/{_core.girlDefinition.outfits.Count} changed to default {_core.girlDefinition.defaultOutfitIndex}.");
            index = _core.girlDefinition.defaultOutfitIndex;
        }

        var outfit = _core.girlDefinition.outfits[index];

        if (outfit == null)
        {
            index = _core.girlDefinition.defaultOutfitIndex;
            outfit = _core.girlDefinition.outfits[index];
        }

        var expansion = outfit.GetExpansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            ModInterface.Log.Message("Hiding NSFW outfit for censored mode");
            index = _core.girlDefinition.defaultOutfitIndex;
            outfit = _core.girlDefinition.outfits[index];
            expansion = outfit.GetExpansion();
        }

        if (expansion.HideSpecial && _specialEffect_hold == null)
        {
            _specialEffect_hold = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect_hold?.rectTransform.SetParent(null, false);
        }
        else if (!expansion.HideSpecial && _specialEffect_hold != null)
        {
            _specialEffect_hold.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
            _specialEffect_hold = null;
        }

        outfitIndex = index;
    }

    /// <summary>
    /// After changing outfit, move it to the bottom of its layer
    /// </summary>
    private void PostChangeOutfit()
    {
        var girlExp = _core.girlDefinition.GetExpansion();
        RefreshSpecialParts(girlExp.HairstyleLookup[_core.currentHairstyleIndex],
            girlExp.OutfitLookup[_core.currentOutfitIndex]);
    }

    private bool ChangeHairstyle(int hairstyleIndex)
    {
        if (_core.girlDefinition == null) return true;

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_core.girlDefinition);

        // clean input
        hairstyleIndex = hairstyleIndex == -1
            ? playerFileGirl.hairstyleIndex
            : hairstyleIndex;

        if (hairstyleIndex < 0 || hairstyleIndex >= _core.girlDefinition.hairstyles.Count)
        {
            ModInterface.Log.Message($"out of range hairstyle index {hairstyleIndex}/{_core.girlDefinition.hairstyles.Count} changed to default {_core.girlDefinition.defaultHairstyleIndex}");
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
        }

        var hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];

        if (hairstyle == null)
        {
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
            hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];
        }

        var expansion = hairstyle.GetExpansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
            hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];
        }

        _currentHairstyleIndex = hairstyleIndex;

        // load hair
        m_LoadPart.Invoke(_core, [_core.partBackhair, hairstyle.partIndexBackhair, -1f]);
        m_LoadPart.Invoke(_core, [_core.partFronthair, hairstyle.partIndexFronthair, -1f]);

        // special parts
        // make new parts if needed
        if (_core.girlDefinition.specialParts.Count > _core.partSpecials.Length)
        {
            var newParts = new List<UiDollPartSpecial>(_core.partSpecials);
            var sample = _core.partSpecials[0];

            for (int i = _core.girlDefinition.specialParts.Count - _core.partSpecials.Length; i > 0; i--)
            {
                newParts.Add(GameObject.Instantiate(sample));
            }

            _core.partSpecials = newParts.ToArray();
        }

        var girlExp = _core.girlDefinition.GetExpansion();
        var hairId = girlExp.HairstyleLookup[_core.currentHairstyleIndex];
        var outfitId = girlExp.OutfitLookup[_core.currentOutfitIndex];

        RefreshSpecialParts(hairId, outfitId);

        return false;
    }

    private void RefreshSpecialParts(RelativeId hairId, RelativeId outfitId)
    {
        int i;
        for (i = 0; i < _core.partSpecials.Length; i++)
        {
            _core.partSpecials[i].StopAnimation();
            _core.partSpecials[i].dollPart.rectTransform.SetSiblingIndex(0);
            _core.partSpecials[i].dollPart.UnloadPart();
        }

        i = 0;
        foreach (var part in _core.girlDefinition.specialParts)
        {
            // make sure special part is allowed
            // empty or null allows all, otherwise whitelist
            var specialPartExpansion = part.GetExpansion();
            if ((specialPartExpansion.RequiredHairstyles?.Any() == true
                    && !specialPartExpansion.RequiredHairstyles.Contains(hairId))
                || (specialPartExpansion.RequiredOutfits?.Any() == true
                    && !specialPartExpansion.RequiredOutfits.Contains(outfitId)))
            {
                continue;
            }

            m_LoadPart.Invoke(_core, [_core.partSpecials[i].dollPart, part.partIndexSpecial, -1f]);

            if (part.sortingPartType != GirlPartType.SPECIAL1
                && part.sortingPartType != GirlPartType.SPECIAL2
                && part.sortingPartType != GirlPartType.SPECIAL3)
            {
                var sibling = (UiDollPart)m_GetDollPartByType.Invoke(_core, [part.sortingPartType]);
                _core.partSpecials[i].dollPart.rectTransform.SetSiblingIndex(sibling.rectTransform.GetSiblingIndex());
            }

            _core.partSpecials[i].StartAnimation(part.animType);

            i++;
        }
    }

    private void OnDestroy()
    {
        if (_specialEffect_hold != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect_hold);
            _specialEffect_hold = null;
        }
    }

    private void ReadDialogTrigger(DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, ref int lineIndex)
    {
        var girlId = _core.girlDefinition.ModId();

        if (!dialogTriggerDef.GetExpansion().TryGetLineSet(dialogTriggerDef, girlId, out var lineSet))
        {
            throw new Exception($"Failed to find dt line set {dialogTriggerDef.ModId()}");
        }

        if (!lineSet.dialogLines.Any())
        {
            throw new Exception($"dt line set {dialogTriggerDef.ModId()} has no lines");
        }

        // the normal index used is the index of the current location in Game.Session.Location.dateLocationDefs
        // instead we go by identifier
        if (lineIndex <= -1
            && dialogTriggerDef.forceType == DialogTriggerForceType.DATE_LOCATION)
        {
            var girlExp = _core.girlDefinition.GetExpansion();
            var dtExp = dialogTriggerDef.GetExpansion();
            var locId = Game.Session.Location.currentLocation.ModId();
            lineIndex = girlExp.DateGreetingLocIdToDtIndex[locId];
            ModInterface.Log.Message($"Using index {lineIndex} for date greeting at location {locId} - {Game.Session.Location.currentLocation.locationName}");
        }

        // when no valid index is specified, pick a random one
        if (lineIndex <= -1
            || lineSet.dialogLines.Count <= lineIndex
            || lineSet.dialogLines[lineIndex] == null)
        {
            var i = 0;
            lineIndex = lineSet.dialogLines.Select(line => (line, i++)).Where(x => x.line != null).ToArray().GetRandom().Item2;
            ModInterface.Log.Message($"Using random index {lineIndex} to replace invalid dt index");
        }
    }
}