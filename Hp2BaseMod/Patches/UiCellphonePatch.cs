using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphone))]
internal static class UiCellphoneStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphone, bool> IsOpen = AccessTools.FieldRefAccess<UiCellphone, bool>("_isOpen");
    private static readonly AccessTools.FieldRef<UiCellphone, bool> IsTransitioning = AccessTools.FieldRefAccess<UiCellphone, bool>("_isTransitioning");
    private static readonly AccessTools.FieldRef<UiCellphone, bool> IsLocked = AccessTools.FieldRefAccess<UiCellphone, bool>("_isLocked");
    private static readonly AccessTools.FieldRef<UiCellphone, int> OpenAppIndex = AccessTools.FieldRefAccess<UiCellphone, int>("_openAppIndex");
    private static readonly AccessTools.FieldRef<UiCellphone, int> CurrentAppIndex = AccessTools.FieldRefAccess<UiCellphone, int>("_currentAppIndex");
    private static readonly AccessTools.FieldRef<UiCellphone, UiCellphoneApp> CurrentApp = AccessTools.FieldRefAccess<UiCellphone, UiCellphoneApp>("_currentApp");
    private static readonly AccessTools.FieldRef<UiCellphone, Sequence> PhoneSequence = AccessTools.FieldRefAccess<UiCellphone, Sequence>("_phoneSequence");
    private static readonly MethodInfo m_onCloseComplete = AccessTools.Method(typeof(UiCellphone), "OnCloseComplete");

    /// <summary>Whether the phone is kept off screen while closed: the state's call. Without a session there is no state.</summary>
    private static bool IsHidden(UiCellphone phone)
        => Game.Session == null || ModInterface.GameState.CurrentState.HidesCellphone(phone);

    [HarmonyPatch(nameof(UiCellphone.Close))]
    [HarmonyPrefix]
    private static bool Close(UiCellphone __instance)
    {
        var phone = __instance;

        if (!IsOpen(phone) || IsTransitioning(phone) || IsLocked(phone)) return false;

        IsTransitioning(phone) = true;
        phone.canvasGroup.blocksRaycasts = false;
        phone.openAppLocked = false;
        phone.overlayDisabled = false;
        Game.Manager.DragDrop.shouldSnapBack = false;
        Game.Manager.Time.KillTween(PhoneSequence(phone), false, true);

        var sequence = DOTween.Sequence().OnComplete(() => m_onCloseComplete.Invoke(phone, null));
        PhoneSequence(phone) = sequence;

        sequence.Insert(phone.centeredPos ? 0.125f : 0.05f,
            phone.overlayCanvasGroup.DOFade(0f, phone.centeredPos ? 0.125f : 0.2f).SetEase(Ease.Linear));
        sequence.Insert(0f,
            phone.rectTransform.DOAnchorPosY(IsHidden(phone) ? phone.yValues.x : phone.yValues.y, 0.25f, false)
                .SetEase(phone.centeredPos ? Ease.InBack : Ease.InSine));
        sequence.Insert(0f, phone.flasherCanvasGroup.DOFade(1f, 0.125f).SetEase(Ease.OutSine));
        sequence.Insert(0.125f, phone.flasherCanvasGroup.DOFade(0f, 0.125f).SetEase(Ease.InSine));
        sequence.InsertCallback(0.125f, new TweenCallback(phone.LoadClosedApp));
        sequence.timeScale = phone.centeredPos ? 0.64f : 1f;

        Game.Manager.Time.Play(sequence, phone.pauseBehavior.pauseDefinition, 0f);
        Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxCellphoneClose, phone.pauseBehavior.pauseDefinition);

        EventUtil.Raise(phone, nameof(UiCellphone.CloseEvent));
        return false;
    }

    [HarmonyPatch(nameof(UiCellphone.LoadApp))]
    [HarmonyPrefix]
    private static bool LoadApp(UiCellphone __instance, int appIndex)
    {
        var phone = __instance;

        if (appIndex < 0) return false;

        if (CurrentApp(phone) != null)
        {
            if (phone.phoneErrorMsg != null)
            {
                phone.phoneErrorMsg.ClearMessage();
            }

            if (phone.cellphoneButtons.Count > 0 && CurrentApp(phone).phoneButtonIndex >= 0)
            {
                phone.cellphoneButtons[CurrentApp(phone).phoneButtonIndex].buttonBehavior.Enable();
            }

            UnityEngine.Object.Destroy(CurrentApp(phone).gameObject);
        }

        CurrentApp(phone) = null;
        CurrentAppIndex(phone) = appIndex;

        // a hidden phone has no closed app to show
        if (IsHidden(phone) && CurrentAppIndex(phone) == phone.closedAppIndex) return false;

        if (CurrentAppIndex(phone) != phone.closedAppIndex)
        {
            OpenAppIndex(phone) = CurrentAppIndex(phone);
        }

        var app = UnityEngine.Object.Instantiate(phone.appPrefabs[CurrentAppIndex(phone)]);
        CurrentApp(phone) = app;
        app.cellphone = phone;
        app.rectTransform.SetParent(CurrentAppIndex(phone) != phone.closedAppIndex ? phone.appContainerLow : phone.appContainerHigh, false);
        app.rectTransform.anchoredPosition = Vector2.zero;

        if (phone.cellphoneButtons.Count > 0 && app.phoneButtonIndex >= 0)
        {
            phone.cellphoneButtons[app.phoneButtonIndex].buttonBehavior.Disable();
        }

        return false;
    }
}