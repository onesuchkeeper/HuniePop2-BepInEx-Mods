using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    // [HarmonyPatch(nameof(UiDoll.ReadDialogLine))]
    // [HarmonyPrefix]
    // public static bool ReadDialogLine(UiDoll __instance, DialogLine dialogLine, DialogLineFormat format, int priority)
    //     => ExpandedUiDoll.Get(__instance).ReadDialogLine(dialogLine, format, priority);

    [HarmonyPatch("PurifyDialogText")]
    [HarmonyPostfix]
    public static void PurifyDialogText(UiDoll __instance, string text, ref string __result)
        => ExpandedUiDoll.Get(__instance).PurifyDialogText(ref __result);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).OnDestroy();
}

public class ExpandedUiDoll
{
    private static float DIALOG_BOX_WIDTH_SMALL = 406f;

    private static float DIALOG_BOX_WIDTH_BIG = 566f;

    private static float DIALOG_TEXT_WIDTH_SMALL = 380f;

    private static float DIALOG_TEXT_WIDTH_BIG = 540f;

    private static readonly string INLINE_STYLER_START = "<color=#FFFFFF00>";

    private static readonly string INLINE_STYLER_END = "</color>";

    private static Dictionary<UiDoll, ExpandedUiDoll> _expansions
            = new Dictionary<UiDoll, ExpandedUiDoll>();

    public static ExpandedUiDoll Get(UiDoll core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiDoll(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static FieldInfo f_currentDialogLine = AccessTools.Field(typeof(UiDoll), "_currentDialogLine");
    private static FieldInfo f_currentDialogText = AccessTools.Field(typeof(UiDoll), "_currentDialogText");
    private static FieldInfo f_currentDialogFormat = AccessTools.Field(typeof(UiDoll), "_currentDialogFormat");
    private static FieldInfo f_currentDialogPriority = AccessTools.Field(typeof(UiDoll), "_currentDialogPriority");
    private static FieldInfo f_clickSkippedDialog = AccessTools.Field(typeof(UiDoll), "_clickSkippedDialog");
    private static FieldInfo f_hasMood = AccessTools.Field(typeof(UiDoll), "_hasMood");
    private static FieldInfo f_moodExpression = AccessTools.Field(typeof(UiDoll), "_moodExpression");
    private static FieldInfo f_dialogLineAudioLink = AccessTools.Field(typeof(UiDoll), "_dialogLineAudioLink");
    private static FieldInfo f_dialogTimestamp = AccessTools.Field(typeof(UiDoll), "_dialogTimestamp");
    private static FieldInfo f_dialogDuration = AccessTools.Field(typeof(UiDoll), "_dialogDuration");
    private static FieldInfo f_dialogLetterDuration = AccessTools.Field(typeof(UiDoll), "_dialogLetterDuration");
    private static FieldInfo f_dialogExpressionIndex = AccessTools.Field(typeof(UiDoll), "_dialogExpressionIndex");
    private static FieldInfo f_dialogBoxSequence = AccessTools.Field(typeof(UiDoll), "_dialogBoxSequence");
    private static FieldInfo f_isDialogBoxShowing = AccessTools.Field(typeof(UiDoll), "_isDialogBoxShowing");
    private static FieldInfo f_isDialogBoxLocked = AccessTools.Field(typeof(UiDoll), "_isDialogBoxLocked");

    private static MethodInfo m_PurifyDialogText = AccessTools.Method(typeof(UiDoll), "PurifyDialogText");
    private static MethodInfo m_ResizeDialogBox = AccessTools.Method(typeof(UiDoll), "ResizeDialogBox");

    public event Action<UiDoll> DialogLineStartedEvent;

    protected UiDoll _core;
    private ExpandedUiDoll(UiDoll core)
    {
        _core = core;
    }

    // add these fields to the class:
    private DialogLine _pendingDialogLine;
    private string _pendingDialogText;
    private AudioClip _pendingAudioClip;
    private DialogLineFormat _pendingFormat;
    private int _pendingPriority;

    // Replace ReadDialogLine with:
    public bool ReadDialogLine(DialogLine dialogLine, DialogLineFormat format, int priority)
    {
        if (_core.girlDefinition == null)
        {
            return true;
        }

        // If currently showing a line, stop it now (same as before).
        var currentDialogLine = f_currentDialogLine.GetValue<DialogLine>(_core);
        if (currentDialogLine != null)
        {
            _core.StopDialogLine(force: true, soft: true);
        }

        // Build the text and audio reference locally
        var currentDialogText = ((dialogLine.yuri && Game.Persistence.playerFile.settingGender == SettingGender.FEMALE) ? dialogLine.yuriDialogText : dialogLine.dialogText);
        currentDialogText += "▸▸▸▸▸";
        var audioClip = ((dialogLine.yuri && Game.Persistence.playerFile.settingGender == SettingGender.FEMALE) ? dialogLine.yuriAudioClip : dialogLine.audioClip);

        // Store locally until we actually register the dialog with UiDoll
        _pendingDialogLine = dialogLine;
        _pendingDialogText = currentDialogText;
        _pendingAudioClip = audioClip;
        _pendingFormat = format;
        _pendingPriority = priority;

        // if (AudioClipInfoVorbisLazy.TryGetVorbis(audioClip, out var vorbis))
        // {
        //     // Preload then register and start
        //     vorbis.PreloadClip(() =>
        //     {
        //         // Ensure object still exists and hasn't been destroyed
        //         if (_core == null) return;

        //         // Now commit the pending dialog into UiDoll internals and start
        //         CommitPendingAndStart();
        //     }, _core);
        // }
        // else
        // {
        // No preload required — commit and start immediately
        CommitPendingAndStart();
        //}

        return false;
    }

    private void CommitPendingAndStart()
    {
        // Sanity: ensure there is still a pending dialog (may have been cleared)
        if (_pendingDialogLine == null) return;

        // Now set UiDoll internals (mimics your original initializations)
        f_currentDialogLine.SetValue(_core, _pendingDialogLine);
        f_currentDialogText.SetValue(_core, _pendingDialogText);
        f_currentDialogFormat.SetValue(_core, _pendingFormat);
        f_currentDialogPriority.SetValue(_core, _pendingPriority);
        f_clickSkippedDialog.SetValue(_core, false);

        // Clear pending so we don't double-run
        var audioClip = _pendingAudioClip;
        var currentDialogText = _pendingDialogText;
        var dialogLine = _pendingDialogLine;

        _pendingDialogLine = null;
        _pendingDialogText = null;
        _pendingAudioClip = null;

        // Now call StartRead exactly once, synchronously, so UiDoll's animations happen in the same frame
        StartRead(audioClip, currentDialogText, dialogLine);
    }

    private void StartRead(AudioClip audioClip, string currentDialogText, DialogLine dialogLine)
    {
        var isHorny = f_hasMood.GetValue<bool>(_core)
            && f_moodExpression.GetValue<GirlExpressionType>(_core) == GirlExpressionType.HORNY;

        if (audioClip != null)
        {
            f_dialogLineAudioLink.SetValue(_core, Game.Manager.Audio.Play(AudioCategory.VOICE, audioClip, _core.pauseDefinition, isHorny ? _core.soulGirlDefinition.sexVoiceVolume : _core.soulGirlDefinition.voiceVolume));
        }

        f_dialogTimestamp.SetValue(_core, Game.Manager.Time.Lifetime(_core.pauseDefinition));
        var dialogDuration = (audioClip != null) ? Mathf.Max(audioClip.length, 0.25f) : Mathf.Max(currentDialogText.Length * 0.05f, 0.25f);
        f_dialogDuration.SetValue(_core, dialogDuration);
        f_dialogLetterDuration.SetValue(_core, Mathf.Min(dialogDuration / currentDialogText.Length, 0.05f));
        f_dialogExpressionIndex.SetValue(_core, 0);

        if (!Game.Session.gameCanvas.cellphone.IsShowing() || Game.Session.Location.AtLocationType(LocationType.HUB))
        {
            if (_core.slideLayer.anchoredPosition.x >= 0f)
            {
                m_ResizeDialogBox.Invoke(_core, [_core.dialogBoxPosValues.y, DIALOG_BOX_WIDTH_BIG, DIALOG_TEXT_WIDTH_BIG]);
            }
            else
            {
                m_ResizeDialogBox.Invoke(_core, [_core.dialogBoxPosValues.z, DIALOG_BOX_WIDTH_BIG, DIALOG_TEXT_WIDTH_BIG]);
            }
        }
        else
        {
            m_ResizeDialogBox.Invoke(_core, [_core.dialogBoxPosValues.x, DIALOG_BOX_WIDTH_SMALL, DIALOG_TEXT_WIDTH_SMALL]);
        }

        _core.dialogText.text = (string)m_PurifyDialogText.Invoke(_core, [INLINE_STYLER_START + currentDialogText + INLINE_STYLER_END]);
        float num = Mathf.Max(192f + (_core.dialogText.preferredHeight - 138f), 192f);
        _core.ChangeExpression(dialogLine.startExpression.expressionType, dialogLine.startExpression.eyesClosed);

        var dialogBoxSequence = f_dialogBoxSequence.GetValue<Sequence>(_core);
        Game.Manager.Time.KillTween(dialogBoxSequence, complete: true);

        if (!f_isDialogBoxShowing.GetValue<bool>(_core) && !f_isDialogBoxLocked.GetValue<bool>(_core))
        {
            _core.dialogBox.rectTransform.sizeDelta = new Vector2(_core.dialogBox.rectTransform.sizeDelta.x, num);
            _core.dialogBox.rectTransform.anchoredPosition = new Vector2(0f, (num - 192f) * 0.5f);
        }
        else
        {
            _core.dialogBoxLayerCanvasGroup.alpha = isHorny ? 0f : 0.5f;
            _core.dialogBoxLayer.localScale = Vector3.one * 0.75f;
        }

        dialogBoxSequence = DOTween.Sequence();
        f_dialogBoxSequence.SetValue(_core, dialogBoxSequence);

        dialogBoxSequence.Insert(0f, _core.dialogBoxLayerCanvasGroup.DOFade(isHorny ? 0 : 1, 0.25f).SetEase(Ease.Linear));
        dialogBoxSequence.Insert(0f, _core.dialogBoxLayer.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        dialogBoxSequence.Insert(0f, _core.dialogBox.rectTransform.DOSizeDelta(new Vector2(_core.dialogBox.rectTransform.sizeDelta.x, num), 0.25f).SetEase(Ease.OutBack));
        dialogBoxSequence.Insert(0f, _core.dialogBox.rectTransform.DOAnchorPos(new Vector2(0f, (num - 192f) * 0.5f), 0.25f).SetEase(Ease.OutBack));
        Game.Manager.Time.Play(dialogBoxSequence, _core.pauseDefinition);
        f_isDialogBoxShowing.SetValue(_core, true);
        f_isDialogBoxLocked.SetValue(_core, false);

        DialogLineStartedEvent?.Invoke(_core);
    }

    public void PurifyDialogText(ref string __result)
    {
        if (string.IsNullOrWhiteSpace(__result)) { return; }

        var cleaned = new StringBuilder();
        var write = true;
        var bracketDepth = 0;
        foreach (var character in __result)
        {
            if (bracketDepth > 0)
            {
                cleaned.Append(character);

                if (character == '>')
                {
                    bracketDepth--;
                }
            }
            else if (character == '<')
            {
                cleaned.Append(character);
                bracketDepth++;
            }
            else if (character == '*')
            {
                write = !write;
            }
            else if (write)
            {
                cleaned.Append(character);
            }
        }

        if (!write)
        {
            ModInterface.Log.Warning($"Encountered text with unpaired asterisk: {__result}");
            return;
        }
        else if (bracketDepth != 0)
        {
            ModInterface.Log.Warning($"Encountered unpaired angle brackets: {__result}");
            return;
        }
        else
        {
            __result = cleaned.ToString();
        }
    }

    public void OnDestroy() => _expansions.Remove(_core);
}