using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

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

        if (state.AllowsStore && Game.Persistence.playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED)
        {
            var selected = Game.Persistence.playerFile.GetFlagValue(Flags.STORE_FILTER_BUTTON);
            if (selected < 0)
            {
                selected = 0;
                Game.Persistence.playerFile.SetFlagValue(Flags.STORE_FILTER_BUTTON, selected);
            }

            for (var i = 0; i < store.filterButtons.Count; i++)
            {
                if (i == selected)
                {
                    SelectedFilterButton(store) = store.filterButtons[i];
                    SelectedFilterButton(store).button.Disable();
                }

                EventUtil.Subscribe(store.filterButtons[i], "FilterSelectedEvent", store, "OnFilterSelected");
            }

            for (var j = 0; j < store.storeSections.Length; j++)
            {
                store.storeSections[j].Populate();
            }

            for (var k = 0; k < store.storeSlots.Length; k++)
            {
                store.storeSlots[k].Populate(SelectedFilterButton(store).filterItemType);
                EventUtil.Subscribe(store.storeSlots[k], "ProductPurchasedEvent", store, "OnProductPurchased");
            }

            EventUtil.Subscribe(Game.Persistence.playerFile, "FruitChangedEvent", store, "OnFruitChanged");
            EventUtil.Subscribe(Game.Manager.DragDrop, "DragStartedEvent", store, "OnDragEvent");
            EventUtil.Subscribe(Game.Manager.DragDrop, "DragReleasedEvent", store, "OnDragEvent");
            EventUtil.Subscribe(Game.Manager.DragDrop, "DragEndedEvent", store, "OnDragEvent");

            // the store works but says it can only be browsed
            if (state.StoreShowsNotice)
            {
                store.ErrorMessage(0);
            }
        }
        else
        {
            store.activeCanvasGroup.alpha = 0f;
            store.activeCanvasGroup.blocksRaycasts = false;
            store.inactiveCanvasGroup.alpha = 1f;
            store.inactiveCanvasGroup.blocksRaycasts = true;
            store.ErrorMessage(1);
        }

        return false;
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    private static bool OnDestroy(UiCellphoneAppStore __instance)
    {
        var store = __instance;

        if (ModInterface.GameState.CurrentState.AllowsStore && Game.Persistence.playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED)
        {
            for (var i = 0; i < store.filterButtons.Count; i++)
            {
                EventUtil.Unsubscribe(store.filterButtons[i], "FilterSelectedEvent", store, "OnFilterSelected");
            }

            for (var j = 0; j < store.storeSlots.Length; j++)
            {
                EventUtil.Unsubscribe(store.storeSlots[j], "ProductPurchasedEvent", store, "OnProductPurchased");
            }

            EventUtil.Unsubscribe(Game.Persistence.playerFile, "FruitChangedEvent", store, "OnFruitChanged");
            EventUtil.Unsubscribe(Game.Manager.DragDrop, "DragStartedEvent", store, "OnDragEvent");
            EventUtil.Unsubscribe(Game.Manager.DragDrop, "DragReleasedEvent", store, "OnDragEvent");
            EventUtil.Unsubscribe(Game.Manager.DragDrop, "DragEndedEvent", store, "OnDragEvent");
        }

        return false;
    }
}