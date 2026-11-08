using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

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
        var status = Game.Persistence.playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED
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
            __instance.ErrorMessage(status.MessageIndex);
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