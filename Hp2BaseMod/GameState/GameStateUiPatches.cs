// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Collections;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

// The base game's UI asked LocationManager.AtLocationType(SIM / DATE / HUB / SPECIAL) what to show. That is a question about the
// kind of place, and there is no kind of place: there are only game states. LocationManager.AtLocationType is not answered
// (ExpandedLocationManager makes it throw) and nothing may call it. What the UI decides is asked of the current IGameState
// instead, one question for each thing it decides (does the store work here, is the finder ready, does the phone hide...), or,
// where the question is "is a puzzle being played", of the state being an IPuzzleState (as UiCellphoneAppHeaderPatch and
// UiCellphoneAppSettingsPatch do).
//
// Every method here replaces the base game's method outright (the prefix returns false). They are copies of the base game's
// code with the AtLocationType calls swapped for the state's answers, and nothing else is changed. Private members are reached
// with cached accessors. Base game methods that asked AtLocationType and are replaced somewhere else:
// LocationManager.Arrive / ResetDolls (ExpandedLocationManager), CutsceneManager.NextStep, GiftManager.GiveGift,
// ItemSlotBehavior.ShowTooltip, UiCellphoneAppHeader.Refresh, UiCellphoneAppSettings.Refresh, UiDoll.ReadDialogTrigger,
// UiPuzzleGrid.Update and UiWindowPhotos.Hide (their expansions), and LocationTransitionNormal (ModLocationTransitionNormal is used instead).

/// <summary>Helpers for the replaced methods.</summary>
internal static class UiPatchUtil
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>Before the tutorial is over the apps are off whatever the state says (<c>UiCellphoneApp.IsPastTutorial(false)</c>).</summary>
    public static bool IsPastTutorial() => Game.Persistence.playerFile.storyProgress >= 7;

    /// <summary><c>UiCellphoneApp.ErrorMessage</c>: shows one of the app's messages on the phone.</summary>
    public static void ErrorMessage(UiCellphoneApp app, int messageIndex)
    {
        if (app.cellphone.phoneErrorMsg != null && messageIndex >= 0 && messageIndex < app.errorMessages.Length)
        {
            app.cellphone.phoneErrorMsg.ShowMessage(app.errorMessages[messageIndex]);
        }
    }

    /// <summary><c>publisher.eventName += subscriber.handlerName</c> for events and handlers that are not accessible.</summary>
    public static void Subscribe(object publisher, string eventName, object subscriber, string handlerName)
    {
        var ev = publisher.GetType().GetEvent(eventName, All);
        ev.AddEventHandler(publisher, Delegate.CreateDelegate(ev.EventHandlerType, subscriber, AccessTools.Method(subscriber.GetType(), handlerName)));
    }

    /// <summary><c>publisher.eventName -= subscriber.handlerName</c></summary>
    public static void Unsubscribe(object publisher, string eventName, object subscriber, string handlerName)
    {
        var ev = publisher.GetType().GetEvent(eventName, All);
        ev.RemoveEventHandler(publisher, Delegate.CreateDelegate(ev.EventHandlerType, subscriber, AccessTools.Method(subscriber.GetType(), handlerName)));
    }

    /// <summary>Raises an event of an object from outside of it.</summary>
    public static void Raise(object publisher, string eventName, params object[] args)
        => (AccessTools.Field(publisher.GetType(), eventName)?.GetValue(publisher) as Delegate)?.DynamicInvoke(args);
}

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

        UiPatchUtil.Raise(phone, nameof(UiCellphone.CloseEvent));
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

/// <summary>The finder: whether it's there, whether its slots are ready, and what picking a slot does are the state's decisions.</summary>
[HarmonyPatch(typeof(UiCellphoneAppFinder))]
internal static class UiCellphoneAppFinderStatePatch
{
    private static readonly FieldInfo f_available = AccessTools.Field(typeof(UiCellphoneAppFinder), "_available");
    private static readonly MethodInfo m_onFinderSlotSelected = AccessTools.Method(typeof(UiCellphoneAppFinder), "OnFinderSlotSelected");
    private static readonly MethodInfo m_onHeadSlotPressed = AccessTools.Method(typeof(UiCellphoneAppFinder), "OnHeadSlotPressed");

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppFinder __instance)
    {
        // before the tutorial is done the finder is always off
        var status = UiPatchUtil.IsPastTutorial()
            ? ModInterface.GameState.CurrentState.GetFinderStatus()
            : FinderStatus.Unavailable(3);

        f_available.SetValue(__instance, status.Available);

        if (status.Available)
        {
            var onSelected = AccessTools.MethodDelegate<UiAppFinderSlot.UiAppFinderSlotDelegate>(m_onFinderSlotSelected, __instance);
            var onHeadPressed = AccessTools.MethodDelegate<UiAppHeadSlot.UiAppHeadSlotDelegate>(m_onHeadSlotPressed, __instance);

            foreach (var slot in __instance.finderSlots)
            {
                Wire(slot, status.SlotsReady, onSelected, onHeadPressed);
            }

            Wire(__instance.hubFinderSlot, status.SlotsReady, onSelected, onHeadPressed);
        }
        else
        {
            __instance.activeCanvasGroup.alpha = 0f;
            __instance.activeCanvasGroup.blocksRaycasts = false;
            __instance.inactiveCanvasGroup.alpha = 1f;
            __instance.inactiveCanvasGroup.blocksRaycasts = true;
        }

        if (!status.SlotsReady)
        {
            UiPatchUtil.ErrorMessage(__instance, status.MessageIndex);
        }

        return false;
    }

    private static void Wire(UiAppFinderSlot slot, bool slotsReady, UiAppFinderSlot.UiAppFinderSlotDelegate onSelected, UiAppHeadSlot.UiAppHeadSlotDelegate onHeadPressed)
    {
        slot.Populate(slotsReady);
        slot.FinderSlotSelectedEvent += onSelected;
        slot.headSlotLeft.HeadSlotPressedEvent += onHeadPressed;
        slot.headSlotRight.HeadSlotPressedEvent += onHeadPressed;
    }

    [HarmonyPatch("OnFinderSlotSelected")]
    [HarmonyPrefix]
    private static bool OnFinderSlotSelected(UiCellphoneAppFinder __instance, UiAppFinderSlot finderSlot)
    {
        ModInterface.Events.NotifyFinderSlotSelected(finderSlot);

        // the state may handle the pick itself (the hub's first location choice)
        if (ModInterface.GameState.CurrentState.OnFinderSlotChosen(__instance, finderSlot)) return false;

        Game.Persistence.playerFile.daytimeElapsed++;
        Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxLocationSelect, __instance.cellphone.pauseBehavior.pauseDefinition);

        if (finderSlot.playerFileFinderSlot != null)
        {
            Game.Session.Location.Depart(finderSlot.locationDefinition,
                finderSlot.playerFileFinderSlot.girlPairDefinition,
                finderSlot.playerFileFinderSlot.sidesFlipped);
        }
        else
        {
            Game.Session.Location.Depart(finderSlot.locationDefinition, null, false);
        }

        return false;
    }
}

[HarmonyPatch(typeof(UiCellphoneAppStore))]
internal static class UiCellphoneAppStoreStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphoneAppStore, UiAppFilterButton> SelectedFilterButton
        = AccessTools.FieldRefAccess<UiCellphoneAppStore, UiAppFilterButton>("_selectedFilterButton");

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppStore __instance)
    {
        var store = __instance;
        var state = ModInterface.GameState.CurrentState;

        if (state.AllowsStore && UiPatchUtil.IsPastTutorial())
        {
            var selected = Game.Persistence.playerFile.GetFlagValue("store_filter_button");
            if (selected < 0)
            {
                selected = 0;
                Game.Persistence.playerFile.SetFlagValue("store_filter_button", selected);
            }

            for (var i = 0; i < store.filterButtons.Count; i++)
            {
                if (i == selected)
                {
                    SelectedFilterButton(store) = store.filterButtons[i];
                    SelectedFilterButton(store).button.Disable();
                }

                UiPatchUtil.Subscribe(store.filterButtons[i], "FilterSelectedEvent", store, "OnFilterSelected");
            }

            for (var j = 0; j < store.storeSections.Length; j++)
            {
                store.storeSections[j].Populate();
            }

            for (var k = 0; k < store.storeSlots.Length; k++)
            {
                store.storeSlots[k].Populate(SelectedFilterButton(store).filterItemType);
                UiPatchUtil.Subscribe(store.storeSlots[k], "ProductPurchasedEvent", store, "OnProductPurchased");
            }

            UiPatchUtil.Subscribe(Game.Persistence.playerFile, "FruitChangedEvent", store, "OnFruitChanged");
            UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragStartedEvent", store, "OnDragEvent");
            UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragReleasedEvent", store, "OnDragEvent");
            UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragEndedEvent", store, "OnDragEvent");

            // the store works but says it can only be browsed
            if (state.StoreShowsNotice)
            {
                UiPatchUtil.ErrorMessage(store, 0);
            }
        }
        else
        {
            store.activeCanvasGroup.alpha = 0f;
            store.activeCanvasGroup.blocksRaycasts = false;
            store.inactiveCanvasGroup.alpha = 1f;
            store.inactiveCanvasGroup.blocksRaycasts = true;
            UiPatchUtil.ErrorMessage(store, 1);
        }

        return false;
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    private static bool OnDestroy(UiCellphoneAppStore __instance)
    {
        var store = __instance;

        if (ModInterface.GameState.CurrentState.AllowsStore && UiPatchUtil.IsPastTutorial())
        {
            for (var i = 0; i < store.filterButtons.Count; i++)
            {
                UiPatchUtil.Unsubscribe(store.filterButtons[i], "FilterSelectedEvent", store, "OnFilterSelected");
            }

            for (var j = 0; j < store.storeSlots.Length; j++)
            {
                UiPatchUtil.Unsubscribe(store.storeSlots[j], "ProductPurchasedEvent", store, "OnProductPurchased");
            }

            UiPatchUtil.Unsubscribe(Game.Persistence.playerFile, "FruitChangedEvent", store, "OnFruitChanged");
            UiPatchUtil.Unsubscribe(Game.Manager.DragDrop, "DragStartedEvent", store, "OnDragEvent");
            UiPatchUtil.Unsubscribe(Game.Manager.DragDrop, "DragReleasedEvent", store, "OnDragEvent");
            UiPatchUtil.Unsubscribe(Game.Manager.DragDrop, "DragEndedEvent", store, "OnDragEvent");
        }

        return false;
    }
}

[HarmonyPatch(typeof(UiCellphoneAppPair))]
internal static class UiCellphoneAppPairStatePatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppPair __instance)
    {
        var app = __instance;
        var state = ModInterface.GameState.CurrentState;

        if (state.AllowsPairApp && UiPatchUtil.IsPastTutorial())
        {
            if (state.HasGirlsPresent)
            {
                var pair = Game.Session.Location.currentGirlPair;
                var status = Game.Session.Puzzle.puzzleStatus;

                app.relationshipSlot.Populate(pair != null && !pair.specialPair ? pair : null, 0, null, false);

                app.pairSideLeft.Populate(status.girlStatusLeft != null && !status.girlStatusLeft.girlDefinition.specialCharacter
                    ? status.girlStatusLeft.playerFileGirl
                    : null);

                app.pairSideRight.Populate(status.girlStatusRight != null && !status.girlStatusRight.girlDefinition.specialCharacter
                    ? status.girlStatusRight.playerFileGirl
                    : null);
            }
            else
            {
                app.sectionTop.alpha = 0f;
                app.sectionTop.blocksRaycasts = false;
            }

            app.levelsTitleBar.Populate(null, app.levelsTitleBarText);

            for (var i = 0; i < app.levelMeters.Length; i++)
            {
                app.levelMeters[i].Populate();
            }
        }
        else
        {
            app.activeCanvasGroup.alpha = 0f;
            app.activeCanvasGroup.blocksRaycasts = false;
            app.inactiveCanvasGroup.alpha = 1f;
            app.inactiveCanvasGroup.blocksRaycasts = true;
            UiPatchUtil.ErrorMessage(app, 0);
        }

        UiPatchUtil.Subscribe(app.pairSideLeft, "GirlHeadEnterEvent", app, "OnGirlHeadEnter");
        UiPatchUtil.Subscribe(app.pairSideLeft, "GirlHeadExitEvent", app, "OnGirlHeadExit");
        UiPatchUtil.Subscribe(app.pairSideRight, "GirlHeadEnterEvent", app, "OnGirlHeadEnter");
        UiPatchUtil.Subscribe(app.pairSideRight, "GirlHeadExitEvent", app, "OnGirlHeadExit");

        return false;
    }
}

[HarmonyPatch(typeof(UiCellphoneAppGifts))]
internal static class UiCellphoneAppGiftsStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphoneAppGifts, UiTooltipSimple> Tooltip
        = AccessTools.FieldRefAccess<UiCellphoneAppGifts, UiTooltipSimple>("_tooltip");

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppGifts __instance)
    {
        var app = __instance;
        var state = ModInterface.GameState.CurrentState;
        var status = Game.Session.Puzzle.puzzleStatus;

        Tooltip(app) = Game.Manager.Ui.GetTooltip<UiTooltipSimple>(TooltipType.SIMPLE);

        var pastTutorial = UiPatchUtil.IsPastTutorial();

        // A girl's head dims when gifts can't be given to her. While a puzzle is being played none can, whatever the state allows.
        var dimmed = !state.AllowGifts || (state is IPuzzleState && Game.Session.Puzzle.isPuzzleActive);

        if (state.HasGirlsPresent && pastTutorial && status.girlStatusLeft != null && !status.girlStatusLeft.girlDefinition.specialCharacter)
        {
            app.girlHeadLeftIcon.sprite = status.girlStatusLeft.girlDefinition.GetHead(false);

            if (dimmed)
            {
                app.canvasGroupLeft.alpha = 0.5f;
                app.canvasGroupLeft.blocksRaycasts = false;
            }
        }
        else
        {
            app.canvasGroupLeft.alpha = 0f;
            app.canvasGroupLeft.blocksRaycasts = false;
            app.girlHeadLeft.Disable();
        }

        if (state.HasGirlsPresent && pastTutorial && status.girlStatusRight != null && !status.girlStatusRight.girlDefinition.specialCharacter)
        {
            app.girlHeadRightIcon.sprite = status.girlStatusRight.girlDefinition.GetHead(false);

            if (dimmed)
            {
                app.canvasGroupRight.alpha = 0.5f;
                app.canvasGroupRight.blocksRaycasts = false;
            }
        }
        else
        {
            app.canvasGroupRight.alpha = 0f;
            app.canvasGroupRight.blocksRaycasts = false;
            app.girlHeadRight.Disable();
        }

        UiPatchUtil.Subscribe(app.girlHeadLeft, "ButtonEnterEvent", app, "OnGirlHeadEnter");
        UiPatchUtil.Subscribe(app.girlHeadLeft, "ButtonExitEvent", app, "OnGirlHeadExit");
        UiPatchUtil.Subscribe(app.girlHeadRight, "ButtonEnterEvent", app, "OnGirlHeadEnter");
        UiPatchUtil.Subscribe(app.girlHeadRight, "ButtonExitEvent", app, "OnGirlHeadExit");
        UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragStartedEvent", app, "OnDragEvent");
        UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragReleasedEvent", app, "OnDragEvent");
        UiPatchUtil.Subscribe(Game.Manager.DragDrop, "DragEndedEvent", app, "OnDragEvent");

        return false;
    }
}

[HarmonyPatch(typeof(UiDoll))]
internal static class UiDollStatePatch
{
    private static readonly AccessTools.FieldRef<UiDoll, GirlDefinition> DollGirlDefinition = AccessTools.FieldRefAccess<UiDoll, GirlDefinition>("_girlDefinition");
    private static readonly AccessTools.FieldRef<UiDoll, DialogLine> CurrentDialogLine = AccessTools.FieldRefAccess<UiDoll, DialogLine>("_currentDialogLine");
    private static readonly AccessTools.FieldRef<UiDoll, string> CurrentDialogText = AccessTools.FieldRefAccess<UiDoll, string>("_currentDialogText");
    private static readonly AccessTools.FieldRef<UiDoll, DialogLineFormat> CurrentDialogFormat = AccessTools.FieldRefAccess<UiDoll, DialogLineFormat>("_currentDialogFormat");
    private static readonly AccessTools.FieldRef<UiDoll, int> CurrentDialogPriority = AccessTools.FieldRefAccess<UiDoll, int>("_currentDialogPriority");
    private static readonly AccessTools.FieldRef<UiDoll, bool> ClickSkippedDialog = AccessTools.FieldRefAccess<UiDoll, bool>("_clickSkippedDialog");
    private static readonly AccessTools.FieldRef<UiDoll, AudioLink> DialogLineAudioLink = AccessTools.FieldRefAccess<UiDoll, AudioLink>("_dialogLineAudioLink");
    private static readonly AccessTools.FieldRef<UiDoll, float> DialogTimestamp = AccessTools.FieldRefAccess<UiDoll, float>("_dialogTimestamp");
    private static readonly AccessTools.FieldRef<UiDoll, float> DialogDuration = AccessTools.FieldRefAccess<UiDoll, float>("_dialogDuration");
    private static readonly AccessTools.FieldRef<UiDoll, float> DialogLetterDuration = AccessTools.FieldRefAccess<UiDoll, float>("_dialogLetterDuration");
    private static readonly AccessTools.FieldRef<UiDoll, int> DialogExpressionIndex = AccessTools.FieldRefAccess<UiDoll, int>("_dialogExpressionIndex");
    private static readonly AccessTools.FieldRef<UiDoll, bool> IsDialogBoxShowing = AccessTools.FieldRefAccess<UiDoll, bool>("_isDialogBoxShowing");
    private static readonly AccessTools.FieldRef<UiDoll, bool> IsDialogBoxLocked = AccessTools.FieldRefAccess<UiDoll, bool>("_isDialogBoxLocked");
    private static readonly AccessTools.FieldRef<UiDoll, bool> HasMood = AccessTools.FieldRefAccess<UiDoll, bool>("_hasMood");
    private static readonly AccessTools.FieldRef<UiDoll, GirlExpressionType> MoodExpression = AccessTools.FieldRefAccess<UiDoll, GirlExpressionType>("_moodExpression");
    private static readonly AccessTools.FieldRef<UiDoll, Sequence> DialogBoxSequence = AccessTools.FieldRefAccess<UiDoll, Sequence>("_dialogBoxSequence");
    private static readonly AccessTools.FieldRef<UiDoll, bool> IsUnfocused = AccessTools.FieldRefAccess<UiDoll, bool>("_isUnfocused");
    private static readonly AccessTools.FieldRef<UiDoll, Sequence> FocusSequence = AccessTools.FieldRefAccess<UiDoll, Sequence>("_focusSequence");

    private static readonly MethodInfo m_resizeDialogBox = AccessTools.Method(typeof(UiDoll), "ResizeDialogBox");
    private static readonly MethodInfo m_purifyDialogText = AccessTools.Method(typeof(UiDoll), "PurifyDialogText");

    private static readonly float DialogBoxWidthSmall = (float)AccessTools.Field(typeof(UiDoll), "DIALOG_BOX_WIDTH_SMALL").GetValue(null);
    private static readonly float DialogBoxWidthBig = (float)AccessTools.Field(typeof(UiDoll), "DIALOG_BOX_WIDTH_BIG").GetValue(null);
    private static readonly float DialogTextWidthSmall = (float)AccessTools.Field(typeof(UiDoll), "DIALOG_TEXT_WIDTH_SMALL").GetValue(null);
    private static readonly float DialogTextWidthBig = (float)AccessTools.Field(typeof(UiDoll), "DIALOG_TEXT_WIDTH_BIG").GetValue(null);
    private static readonly string InlineStylerStart = (string)AccessTools.Field(typeof(UiDoll), "INLINE_STYLER_START").GetValue(null);
    private static readonly string InlineStylerEnd = (string)AccessTools.Field(typeof(UiDoll), "INLINE_STYLER_END").GetValue(null);

    private static void ResizeDialogBox(UiDoll doll, float layerXPos, float boxWidth, float textWidth)
        => m_resizeDialogBox.Invoke(doll, new object[] { layerXPos, boxWidth, textWidth });

    private static string PurifyDialogText(UiDoll doll, string text)
        => (string)m_purifyDialogText.Invoke(doll, new object[] { text });

    /// <summary>
    /// The dialog box is the wide one when the cellphone is closed, or when the state says the phone doesn't matter
    /// (<see cref="IGameState.DialogIgnoresCellphone"/>), and the narrow one beside the open phone.
    /// </summary>
    [HarmonyPatch(nameof(UiDoll.ReadDialogLine))]
    [HarmonyPrefix]
    private static bool ReadDialogLine(UiDoll __instance, DialogLine dialogLine, DialogLineFormat format, int priority)
    {
        var doll = __instance;

        if (DollGirlDefinition(doll) == null) return false;

        if (CurrentDialogLine(doll) != null)
        {
            doll.StopDialogLine(true, true);
        }

        CurrentDialogLine(doll) = dialogLine;

        var yuri = dialogLine.yuri && Game.Persistence.playerFile.settingGender == SettingGender.FEMALE;
        CurrentDialogText(doll) = (yuri ? dialogLine.yuriDialogText : dialogLine.dialogText) + "▸▸▸▸▸";
        CurrentDialogFormat(doll) = format;
        CurrentDialogPriority(doll) = priority;
        ClickSkippedDialog(doll) = false;

        var audioClip = yuri ? dialogLine.yuriAudioClip : dialogLine.audioClip;
        var horny = HasMood(doll) && MoodExpression(doll) == GirlExpressionType.HORNY;

        if (audioClip != null)
        {
            DialogLineAudioLink(doll) = Game.Manager.Audio.Play(AudioCategory.VOICE, audioClip, doll.pauseDefinition,
                horny ? doll.soulGirlDefinition.sexVoiceVolume : doll.soulGirlDefinition.voiceVolume);
        }

        var text = CurrentDialogText(doll);

        DialogTimestamp(doll) = Game.Manager.Time.Lifetime(doll.pauseDefinition);
        DialogDuration(doll) = audioClip != null ? Mathf.Max(audioClip.length, 0.25f) : Mathf.Max(text.Length * 0.05f, 0.25f);
        DialogLetterDuration(doll) = Mathf.Min(DialogDuration(doll) / text.Length, 0.05f);
        DialogExpressionIndex(doll) = 0;

        var state = ModInterface.GameState.CurrentState;
        if (!Game.Session.gameCanvas.cellphone.IsShowing() || (state?.DialogIgnoresCellphone ?? false))
        {
            if (doll.slideLayer.anchoredPosition.x >= 0f)
            {
                ResizeDialogBox(doll, doll.dialogBoxPosValues.y, DialogBoxWidthBig, DialogTextWidthBig);
            }
            else
            {
                ResizeDialogBox(doll, doll.dialogBoxPosValues.z, DialogBoxWidthBig, DialogTextWidthBig);
            }
        }
        else
        {
            ResizeDialogBox(doll, doll.dialogBoxPosValues.x, DialogBoxWidthSmall, DialogTextWidthSmall);
        }

        doll.dialogText.text = PurifyDialogText(doll, InlineStylerStart + text + InlineStylerEnd);

        var boxHeight = Mathf.Max(192f + (doll.dialogText.preferredHeight - 138f), 192f);

        doll.ChangeExpression(dialogLine.startExpression.expressionType, dialogLine.startExpression.eyesClosed);
        Game.Manager.Time.KillTween(DialogBoxSequence(doll), true, true);

        if (!IsDialogBoxShowing(doll) && !IsDialogBoxLocked(doll))
        {
            doll.dialogBox.rectTransform.sizeDelta = new Vector2(doll.dialogBox.rectTransform.sizeDelta.x, boxHeight);
            doll.dialogBox.rectTransform.anchoredPosition = new Vector2(0f, (boxHeight - 192f) * 0.5f);
        }
        else
        {
            doll.dialogBoxLayerCanvasGroup.alpha = horny ? 0f : 0.5f;
            doll.dialogBoxLayer.localScale = Vector3.one * 0.75f;
        }

        var sequence = DOTween.Sequence();
        DialogBoxSequence(doll) = sequence;
        sequence.Insert(0f, doll.dialogBoxLayerCanvasGroup.DOFade(horny ? 0 : 1, 0.25f).SetEase(Ease.Linear));
        sequence.Insert(0f, doll.dialogBoxLayer.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        sequence.Insert(0f, doll.dialogBox.rectTransform.DOSizeDelta(new Vector2(doll.dialogBox.rectTransform.sizeDelta.x, boxHeight), 0.25f, false).SetEase(Ease.OutBack));
        sequence.Insert(0f, doll.dialogBox.rectTransform.DOAnchorPos(new Vector2(0f, (boxHeight - 192f) * 0.5f), 0.25f, false).SetEase(Ease.OutBack));
        Game.Manager.Time.Play(sequence, doll.pauseDefinition, 0f);

        IsDialogBoxShowing(doll) = true;
        IsDialogBoxLocked(doll) = false;

        UiPatchUtil.Raise(doll, nameof(UiDoll.DialogLineStartedEvent), doll);
        return false;
    }

    /// <summary>
    /// A doll's focus button and drop zone follow <see cref="IGameState.DollFocusIsInteractive"/>: when it is true the unfocused
    /// doll can be clicked to take focus and only the focused doll takes gifts, when false the buttons are dead and either takes gifts.
    /// </summary>
    [HarmonyPatch(nameof(UiDoll.SetFocus))]
    [HarmonyPrefix]
    private static bool SetFocus(UiDoll __instance, bool focused, float duration)
    {
        var doll = __instance;

        if (!focused == IsUnfocused(doll)) return false;

        IsUnfocused(doll) = !focused;

        if (ModInterface.GameState.CurrentState?.DollFocusIsInteractive ?? false)
        {
            doll.focusButton.image.raycastTarget = IsUnfocused(doll);
            doll.dropZoneCanvasGroup.blocksRaycasts = !IsUnfocused(doll);
        }
        else
        {
            doll.focusButton.image.raycastTarget = false;
            doll.dropZoneCanvasGroup.blocksRaycasts = true;
        }

        Game.Manager.Time.KillTween(FocusSequence(doll), false, true);

        var sequence = DOTween.Sequence().SetEase(Ease.InOutSine);
        FocusSequence(doll) = sequence;

        var mirror = doll.mirrored ? -1 : 1;

        if (IsUnfocused(doll))
        {
            sequence.Insert(0f, DOTween.To(() => doll.uiEffectGroup.effectFactor, x => doll.uiEffectGroup.effectFactor = x, 0.66f, duration).SetEase(Ease.Linear));
            sequence.Insert(0f, doll.verticalLayer.DOAnchorPosX(100f, duration, false).SetEase(Ease.OutSine));
            sequence.Insert(0f, doll.verticalLayer.DOAnchorPosY(-150f, duration, false).SetEase(Ease.InSine));

            if (doll.unfocusedCover != null)
            {
                sequence.Insert(0f, doll.unfocusedCover.rectTransform.DOScaleX(4 * mirror, duration).SetEase(Ease.OutSine));
                sequence.Insert(0f, doll.unfocusedCover.DOFade(0.3f, duration).SetEase(Ease.Linear));
            }
        }
        else
        {
            sequence.Insert(0f, DOTween.To(() => doll.uiEffectGroup.effectFactor, x => doll.uiEffectGroup.effectFactor = x, 0f, duration).SetEase(Ease.Linear));
            sequence.Insert(0f, doll.verticalLayer.DOAnchorPosX(0f, duration, false).SetEase(Ease.InSine));
            sequence.Insert(0f, doll.verticalLayer.DOAnchorPosY(0f, duration, false).SetEase(Ease.OutSine));

            if (doll.unfocusedCover != null)
            {
                sequence.Insert(0f, doll.unfocusedCover.rectTransform.DOScaleX(mirror, duration).SetEase(Ease.InSine));
                sequence.Insert(0f, doll.unfocusedCover.DOFade(0f, duration).SetEase(Ease.Linear));
            }
        }

        Game.Manager.Time.Play(sequence, doll.pauseDefinition, 0f);

        UiPatchUtil.Raise(doll, nameof(UiDoll.FocusChangeEvent), doll);
        return false;
    }
}

[HarmonyPatch(typeof(UiPuzzleStaminaHint))]
internal static class UiPuzzleStaminaHintStatePatch
{
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, Vector2> OrigSizeDelta = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, Vector2>("_origSizeDelta");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, float> OrigFontSize = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, float>("_origFontSize");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, Sprite> DefaultBgSprite = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, Sprite>("_defaultBgSprite");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, Color> DefaultTextColor = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, Color>("_defaultTextColor");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, Material> DefaultTextMaterial = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, Material>("_defaultTextMaterial");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, int> CurrentCost = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, int>("_currentCost");
    private static readonly AccessTools.FieldRef<UiPuzzleStaminaHint, Sequence> HintSequence = AccessTools.FieldRefAccess<UiPuzzleStaminaHint, Sequence>("_hintSequence");

    /// <summary>
    /// The hint is the stamina a talk or gift would cost where the state says so (<see cref="IGameState.HintsSimStaminaCost"/>),
    /// else the stamina the move in progress costs when a puzzle is being played, else nothing.
    /// </summary>
    [HarmonyPatch("Refresh")]
    [HarmonyPrefix]
    private static bool Refresh(UiPuzzleStaminaHint __instance)
    {
        var hint = __instance;
        var state = ModInterface.GameState.CurrentState;
        var puzzle = Game.Session.Puzzle;

        var stamina = puzzle.puzzleStatus.GetStatusGirl(hint.altGirl).stamina;
        var cost = 0;

        if (state != null && state.HintsSimStaminaCost)
        {
            var talk = Game.Session.Talk;

            if (talk.staminaHintCost != 0
                && (talk.staminaHintDollIndex < 0
                    || (!hint.altGirl && talk.staminaHintDollIndex == 0)
                    || (hint.altGirl && talk.staminaHintDollIndex > 0)))
            {
                cost = talk.staminaHintCost;
            }
            else if (Game.Manager.DragDrop.IsDropZone(Game.Session.gameCanvas.GetDoll(hint.altGirl).dropZone))
            {
                var item = Game.Manager.DragDrop.currentDraggable.GetItemDefinition();

                cost = !item.noStaminaCost
                    && Game.Session.Gift.CheckGiveCondition(Game.Session.gameCanvas.GetDoll(hint.altGirl).girlDefinition, item)
                    && (item.itemType != ItemType.FOOD || puzzle.puzzleStatus.foodGiftCount < 2)
                    ? 1
                    : 0;
            }
        }
        else if (state is IPuzzleState)
        {
            stamina = puzzle.puzzleStatus.girlStatusFocused.stamina;

            if (puzzle.puzzleGrid.state == PuzzleGameState.MOVING
                && puzzle.puzzleGrid.moveMatchSet != null
                && puzzle.puzzleStatus.altGirlFocused == hint.altGirl)
            {
                cost = puzzle.puzzleGrid.moveMatchSet.GetStaminaCost(false, false);
            }
        }

        var gain = cost < 0;
        cost = Mathf.Abs(cost);

        if (cost == CurrentCost(hint)) return false;

        CurrentCost(hint) = cost;
        TweenUtils.KillTween(HintSequence(hint), false, true);

        var sequence = DOTween.Sequence();
        HintSequence(hint) = sequence;

        if (cost > 0)
        {
            hint.rectTransform.localScale = Vector2.one * 0.25f;
            hint.canvasGroup.alpha = 0f;

            if (cost <= stamina || gain)
            {
                hint.image.sprite = DefaultBgSprite(hint);
                hint.textLabel.color = DefaultTextColor(hint);
                hint.textLabel.fontSharedMaterial = DefaultTextMaterial(hint);
            }
            else
            {
                hint.image.sprite = hint.errorBgSprite;
                hint.textLabel.color = hint.errorTextColor;
                hint.textLabel.fontSharedMaterial = hint.errorTextMaterial;
            }

            hint.rectTransform.sizeDelta = OrigSizeDelta(hint) + new Vector2((cost - 1) * 22, (cost - 1) * 4);
            hint.textLabel.fontSize = OrigFontSize(hint) + (cost - 1) * 3;
            hint.textLabel.text = (gain ? "+" : "-") + cost.ToString() + " Stamina";

            sequence.Insert(0f, hint.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
            sequence.Insert(0f, hint.canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear));
        }
        else
        {
            sequence.Insert(0f, hint.rectTransform.DOScale(Vector3.one * 0.5f, 0.1f).SetEase(Ease.Linear));
            sequence.Insert(0f, hint.canvasGroup.DOFade(0f, 0.1f).SetEase(Ease.Linear));
        }

        sequence.Play<Sequence>();
        return false;
    }
}

[HarmonyPatch(typeof(HubManager))]
internal static class HubManagerStatePatch
{
    [HarmonyPatch(nameof(HubManager.CanPause))]
    [HarmonyPrefix]
    private static bool CanPause(out bool __result)
    {
        __result = ModInterface.GameState.CurrentState.AllowsPause;
        return false;
    }
}

[HarmonyPatch(typeof(GiftManager))]
internal static class GiftManagerStatePatch
{
    /// <summary>Why a gift can't be given right now is the state's to say. Items that can never be given directly are the same everywhere.</summary>
    [HarmonyPatch(nameof(GiftManager.GetDirectGiftGiveError))]
    [HarmonyPrefix]
    private static bool GetDirectGiftGiveError(ItemDefinition itemDef, ref string __result)
    {
        if (itemDef == null)
        {
            __result = null;
        }
        else if (itemDef.itemType == ItemType.MISC)
        {
            __result = "This item can't be given as a gift.";
        }
        else if (itemDef.itemType == ItemType.DATE_GIFT)
        {
            __result = "Date gifts can't be given directly from the HunieBee.";
        }
        else
        {
            __result = ModInterface.GameState.CurrentState.GetGiftError(itemDef);
        }

        return false;
    }
}

[HarmonyPatch(typeof(UiCellphoneAppStatus))]
internal static class UiCellphoneAppStatusStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, PuzzleStatus> StatusOf = AccessTools.FieldRefAccess<UiCellphoneAppStatus, PuzzleStatus>("_puzzleStatus");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> RelationshipChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_relationshipChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> BaggageChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_baggageChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> ResourceChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_resourceChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> FocusChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_focusChanged");

    private static readonly MethodInfo m_rollRelationshipSlot = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollRelationshipSlot");
    private static readonly MethodInfo m_rollBaggageSlots = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollBaggageSlots");
    private static readonly MethodInfo m_rollRecoverLines = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollRecoverLines");
    private static readonly MethodInfo m_focusSides = AccessTools.Method(typeof(UiCellphoneAppStatus), "FocusSides");

    /// <summary>The status app has two layouts: the one for a puzzle being played, and the one with the pair and baggage slots.</summary>
    private static bool IsPuzzleLayout => ModInterface.GameState.CurrentState is IPuzzleState;

    [HarmonyPatch(nameof(UiCellphoneAppStatus.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiCellphoneAppStatus __instance)
    {
        var app = __instance;
        var status = StatusOf(app);

        RelationshipChanged(app) = false;
        BaggageChanged(app) = false;
        ResourceChanged(app) = false;
        FocusChanged(app) = false;

        if (!IsPuzzleLayout)
        {
            app.canvasGroupDate.alpha = 1f;
            app.canvasGroupDate.blocksRaycasts = true;

            for (var i = 0; i < app.simCanvasGroups.Length; i++)
            {
                app.simCanvasGroups[i].alpha = 1f;
                app.simCanvasGroups[i].blocksRaycasts = true;
            }

            app.affectionCanvasGroup.alpha = 0f;
            app.affectionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            if (!status.bonusRound)
            {
                app.canvasGroupDate.alpha = 1f;
                app.canvasGroupDate.blocksRaycasts = true;
            }
            else
            {
                app.canvasGroupDate.alpha = 0.5f;
                app.canvasGroupDate.blocksRaycasts = false;
            }

            for (var j = 0; j < app.simCanvasGroups.Length; j++)
            {
                app.simCanvasGroups[j].alpha = 0f;
                app.simCanvasGroups[j].blocksRaycasts = false;
            }

            app.affectionCanvasGroup.alpha = 1f;
            app.affectionCanvasGroup.blocksRaycasts = true;
        }

        app.statusPortraitLeft.Populate(status.girlStatusLeft.girlDefinition, !status.bonusRound);
        app.statusPortraitRight.Populate(status.girlStatusRight.girlDefinition, !status.bonusRound);
        app.affectionMeter.Reset(status.affection, status.affectionGoal);
        app.staminaMeterLeft.Reset(status.girlStatusLeft.stamina, 6);
        app.staminaMeterRight.Reset(status.girlStatusRight.stamina, 6);

        if (!status.bonusRound)
        {
            app.movesRoller.Reset(status.movesRemaining, status.maxMovesRemaining);
            app.passionRollerLeft.Reset(status.girlStatusLeft.passion, 100);
            app.passionRollerRight.Reset(status.girlStatusRight.passion, 100);
            app.sentimentRollerLeft.Reset(status.girlStatusLeft.sentiment, 40);
            app.sentimentRollerRight.Reset(status.girlStatusRight.sentiment, 40);
        }
        else
        {
            app.movesRoller.Lock("");
            app.passionRollerLeft.Lock("");
            app.passionRollerRight.Lock("");
            app.sentimentRollerLeft.Lock("");
            app.sentimentRollerRight.Lock("");
        }

        m_rollRelationshipSlot.Invoke(app, null);
        m_rollBaggageSlots.Invoke(app, null);
        m_rollRecoverLines.Invoke(app, new object[] { true });
        m_focusSides.Invoke(app, new object[] { 0f });

        return false;
    }

    [HarmonyPatch("RollRelationshipSlot")]
    [HarmonyPrefix]
    private static bool RollRelationshipSlot(UiCellphoneAppStatus __instance)
    {
        if (IsPuzzleLayout) return false;

        var pair = Game.Session.Location.currentGirlPair;
        __instance.relationshipSlot.Populate(pair, 0, null, false);
        return false;
    }

    [HarmonyPatch("RollBaggageSlots")]
    [HarmonyPrefix]
    private static bool RollBaggageSlots(UiCellphoneAppStatus __instance)
    {
        if (IsPuzzleLayout) return false;

        var status = StatusOf(__instance);

        for (var i = 0; i < __instance.baggageSlotsLeft.Length; i++)
        {
            __instance.baggageSlotsLeft[i].Populate(status.girlStatusLeft.playerFileGirl);
        }

        for (var j = 0; j < __instance.baggageSlotsRight.Length; j++)
        {
            __instance.baggageSlotsRight[j].Populate(status.girlStatusRight.playerFileGirl);
        }

        return false;
    }
}

/// <summary>
/// The relationship icon pulses when the sex date is available at the slot's time of day, unless the slot is set to wait
/// (<c>hornyDelayed</c>) for the puzzle of a date to start.
/// </summary>
/// <remarks>
/// Runs after any other mod's replacement (low priority) and does nothing when one has already handled the slot
/// (<c>__runOriginal</c> is false), which is how SingleDate takes the slots of single pairs.
/// </remarks>
[HarmonyPatch(typeof(UiAppRelationshipSlot))]
internal static class UiAppRelationshipSlotStatePatch
{
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, Sequence> HornySequence = AccessTools.FieldRefAccess<UiAppRelationshipSlot, Sequence>("_hornySequence");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, PlayerFileGirlPair> PairFileOf = AccessTools.FieldRefAccess<UiAppRelationshipSlot, PlayerFileGirlPair>("_playerFileGirlPair");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, int> DaytimeOffset = AccessTools.FieldRefAccess<UiAppRelationshipSlot, int>("_daytimeOffset");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, GirlDefinition> MainGirlDefinition = AccessTools.FieldRefAccess<UiAppRelationshipSlot, GirlDefinition>("_mainGirlDefinition");
    private static readonly AccessTools.FieldRef<UiAppRelationshipSlot, UiTooltipSimple> Tooltip = AccessTools.FieldRefAccess<UiAppRelationshipSlot, UiTooltipSimple>("_tooltip");

    /// <summary>
    /// Whether the pair is attracted, it is the pair's sex time of day (at the slot's offset) and, for a slot that waits, a
    /// date's puzzle has started (or no date is being played).
    /// </summary>
    private static bool ShowsHornyIcon(UiAppRelationshipSlot slot, PlayerFileGirlPair pair)
        => pair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && (Game.Persistence.playerFile.daytimeElapsed + DaytimeOffset(slot)) % 4 == (int)pair.girlPairDefinition.sexDaytime
            && (!slot.hornyDelayed
                || !(ModInterface.GameState.CurrentState is IPuzzleState)
                || Game.Session.Puzzle.isPuzzleActive);

    [HarmonyPatch(nameof(UiAppRelationshipSlot.Refresh))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool Refresh(UiAppRelationshipSlot __instance, bool __runOriginal)
    {
        if (!__runOriginal) return false;

        var slot = __instance;
        var pair = PairFileOf(slot);

        Game.Manager.Time.KillTween(HornySequence(slot), false, true);
        slot.itemIcon.rectTransform.localScale = Vector3.one;

        if (pair != null)
        {
            if (ShowsHornyIcon(slot, pair))
            {
                slot.itemIcon.sprite = slot.hornyIcon;

                var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
                HornySequence(slot) = sequence;
                sequence.Insert(0f, slot.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.2f, slot.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.4f, slot.itemIcon.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.InOutSine));
                sequence.Insert(0.6f, slot.itemIcon.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
                sequence.AppendInterval(0.25f);
                sequence.PrependInterval(0.25f);
                Game.Manager.Time.Play(sequence, slot.pauseDefinition, 0f);
            }
            else
            {
                slot.itemIcon.sprite = slot.relationshipIcons[(int)pair.relationshipType];
            }

            slot.itemIcon.SetNativeSize();

            if (slot.headIcon != null)
            {
                if (MainGirlDefinition(slot) != null)
                {
                    slot.headIcon.sprite = pair.girlPairDefinition.GetOtherGirlDef(MainGirlDefinition(slot)).GetHead(false);
                    slot.headIcon.SetNativeSize();
                }
                else
                {
                    slot.headIcon.sprite = null;
                    slot.headIcon.rectTransform.sizeDelta = Vector2.zero;
                }
            }

            slot.canvasGroup.alpha = 1f;
            slot.canvasGroup.blocksRaycasts = true;
            return false;
        }

        slot.itemIcon.sprite = null;
        slot.itemIcon.rectTransform.sizeDelta = Vector2.zero;

        if (slot.headIcon != null)
        {
            slot.headIcon.sprite = null;
            slot.headIcon.rectTransform.sizeDelta = Vector2.zero;
        }

        slot.canvasGroup.alpha = slot.multiSlot ? 0.4f : 0f;
        slot.canvasGroup.blocksRaycasts = false;
        return false;
    }

    [HarmonyPatch(nameof(UiAppRelationshipSlot.ShowTooltip))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool ShowTooltip(UiAppRelationshipSlot __instance, bool __runOriginal)
    {
        if (!__runOriginal) return false;

        var slot = __instance;
        var pair = PairFileOf(slot);

        if (pair == null) return false;

        var text = "Relationship";

        if (MainGirlDefinition(slot) != null)
        {
            text += " w/ " + pair.girlPairDefinition.GetOtherGirlDef(MainGirlDefinition(slot)).girlName;
        }
        else
        {
            text += " Status";
        }

        text += ":\n" + pair.relationshipType.ToString();

        if (ShowsHornyIcon(slot, pair))
        {
            text += slot.hornyText;
        }

        Tooltip(slot).Populate(text, 0, 1f, 1920f);
        Tooltip(slot).Show(Vector2.up * 75f, false);
        return false;
    }
}

/// <summary>
/// A finder slot is for a location the finder offers (<see cref="ExpandedLocationDefinition.AllowSim"/>) or for the hub. The base game
/// decided both from the location's type.
/// </summary>
[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotStatePatch
{
    private static readonly AccessTools.FieldRef<UiAppFinderSlot, PlayerFileFinderSlot> PlayerFileFinderSlotOf
        = AccessTools.FieldRefAccess<UiAppFinderSlot, PlayerFileFinderSlot>("_playerFileFinderSlot");

    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPrefix]
    private static bool Populate(UiAppFinderSlot __instance, bool settled)
    {
        var slot = __instance;
        var location = slot.locationDefinition;
        var locationManager = Game.Session.Location;
        var isFinderLocation = location.GetExpansion().AllowSim;

        PlayerFileFinderSlotOf(slot) = Game.Persistence.playerFile.GetPlayerFileFinderSlot(location);
        var playerFileFinderSlot = PlayerFileFinderSlotOf(slot);

        var enabled = true;
        bool dimmed;

        if (isFinderLocation && location == locationManager.currentLocation)
        {
            if (locationManager.currentGirlPair)
            {
                slot.headSlotLeft.Populate(locationManager.currentGirlLeft);
                slot.headSlotRight.Populate(locationManager.currentGirlRight);
                slot.relationshipSlot.Populate(locationManager.currentGirlPair, 0, null, false);
            }

            dimmed = true;
        }
        else if (isFinderLocation && playerFileFinderSlot != null && playerFileFinderSlot.girlPairDefinition != null)
        {
            if (!playerFileFinderSlot.sidesFlipped)
            {
                slot.headSlotLeft.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionOne);
                slot.headSlotRight.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionTwo);
            }
            else
            {
                slot.headSlotLeft.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionTwo);
                slot.headSlotRight.Populate(playerFileFinderSlot.girlPairDefinition.girlDefinitionOne);
            }

            slot.relationshipSlot.Populate(playerFileFinderSlot.girlPairDefinition, 1, null, false);
            dimmed = !settled;
        }
        else if (location == Game.Session.Hub.hubLocationDefinition)
        {
            slot.headSlotLeft.Populate(Game.Session.Hub.hubGirlDefinition);
            slot.headSlotLeft.rectTransform.anchoredPosition = new Vector2(0f, slot.headSlotLeft.rectTransform.anchoredPosition.y);
            slot.headSlotRight.canvasGroup.alpha = 0f;
            slot.relationshipSlot.canvasGroup.alpha = 0f;
            slot.headSlotRight.canvasGroup.blocksRaycasts = false;
            slot.relationshipSlot.canvasGroup.blocksRaycasts = false;

            // the hub can't be picked from the hub
            dimmed = !settled || ModInterface.GameState.CurrentState.ShowsHubMenu;
        }
        else
        {
            dimmed = true;
            enabled = false;
        }

        if (dimmed)
        {
            if (slot.headSlotLeft.canvasGroup.alpha > 0f)
            {
                slot.headSlotLeft.canvasGroup.alpha = 0.25f;
            }

            if (slot.headSlotRight.canvasGroup.alpha > 0f)
            {
                slot.headSlotRight.canvasGroup.alpha = 0.25f;
            }

            if (slot.relationshipSlot.canvasGroup.alpha > 0f)
            {
                slot.relationshipSlot.canvasGroup.alpha = 0.25f;
            }

            if (slot.headSlotLeft.canvasGroup.blocksRaycasts)
            {
                slot.headSlotLeft.canvasGroup.blocksRaycasts = enabled;
            }

            if (slot.headSlotRight.canvasGroup.blocksRaycasts)
            {
                slot.headSlotRight.canvasGroup.blocksRaycasts = enabled;
            }

            if (slot.relationshipSlot.canvasGroup.blocksRaycasts)
            {
                slot.relationshipSlot.canvasGroup.blocksRaycasts = enabled;
            }

            slot.locationIcon.color = ColorUtils.ColorAlpha(slot.locationIcon.color, 0.5f);
            slot.locationLabel.color = ColorUtils.ColorAlpha(slot.locationLabel.color, 0.4f);
            slot.locationButton.Disable();
        }

        slot.locationIcon.sprite = location.finderLocationIcon;
        slot.locationLabel.text = location.locationName.ToUpper();

        var width = Mathf.RoundToInt(slot.locationLabel.preferredWidth);
        if (width % 2 != 0)
        {
            width++;
        }

        slot.locationRectTransform.sizeDelta = new Vector2(90 + width, slot.locationRectTransform.sizeDelta.y);
        return false;
    }
}

/// <summary>The location bar says "NON-STOP DATE" for a date without a pair, which is a puzzle state.</summary>
[HarmonyPatch(typeof(UiBackgroundBar))]
internal static class UiBackgroundBarStatePatch
{
    [HarmonyPatch(nameof(UiBackgroundBar.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiBackgroundBar __instance, LocationDefinition locationDef, int daytimeElapsed, GirlPairDefinition girlPairDef, bool sidesFlipped)
    {
        var locationText = "- " + locationDef.locationName.ToUpper() + " -";
        var daytimeText = ((ClockDaytimeType)(daytimeElapsed % 4)).ToString().ToUpper();
        var hasSpecialLabel = false;

        if (locationDef.specialLabels.Count > 0)
        {
            for (var i = 0; i < locationDef.specialLabels.Count; i++)
            {
                var label = locationDef.specialLabels[i];

                if (StringUtils.IsEmpty(label.flagName)
                    || (!label.inverse && Game.Persistence.playerFile.GetFlagValue(label.flagName) >= 0)
                    || (label.inverse && Game.Persistence.playerFile.GetFlagValue(label.flagName) < 0))
                {
                    daytimeText = label.labelText;
                    hasSpecialLabel = true;
                    break;
                }
            }
        }

        if (!hasSpecialLabel)
        {
            if (ModInterface.GameState.CurrentState is IPuzzleState && girlPairDef == null)
            {
                daytimeText = "NON-STOP DATE";
            }
            else if (girlPairDef != null && !girlPairDef.specialPair)
            {
                var nameOne = girlPairDef.girlDefinitionOne.specialCharacter || Game.Persistence.playerFile.GetPlayerFileGirl(girlPairDef.girlDefinitionOne).playerMet
                    ? girlPairDef.girlDefinitionOne.girlName.ToUpper()
                    : null;

                var nameTwo = girlPairDef.girlDefinitionTwo.specialCharacter || Game.Persistence.playerFile.GetPlayerFileGirl(girlPairDef.girlDefinitionTwo).playerMet
                    ? girlPairDef.girlDefinitionTwo.girlName.ToUpper()
                    : null;

                if (!StringUtils.IsEmpty(nameOne) && !StringUtils.IsEmpty(nameTwo))
                {
                    daytimeText += "  |  " + (!sidesFlipped ? nameOne + " & " + nameTwo : nameTwo + " & " + nameOne);
                }
            }
        }

        __instance.locationLabel.text = locationText;
        __instance.daytimeLabel.text = daytimeText;
        return false;
    }
}