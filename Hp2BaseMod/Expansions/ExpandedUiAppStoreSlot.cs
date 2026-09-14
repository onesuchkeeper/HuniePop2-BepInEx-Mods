using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiAppStoreSlot))]
public partial class ExpandedUiAppStoreSlot
{
    [HarmonyPatch(typeof(UiAppStoreSlot))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(UiAppStoreSlot __instance)
            => ExpandedUiAppStoreSlot.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiAppStoreSlot __instance)
            => ExpandedUiAppStoreSlot.Destroy(__instance);

        [HarmonyPatch(nameof(UiAppStoreSlot.Refresh))]
        [HarmonyPostfix]
        private static void Refresh(UiAppStoreSlot __instance, ItemType highlightedItemType)
            => ExpandedUiAppStoreSlot.Get(__instance).Refresh_Postfix(highlightedItemType);
    }

    private void Refresh_Postfix(ItemType highlightedItemType)
    {
        if (_playerFileStoreProduct == null) return;

		if (_playerFileStoreProduct.itemDefinition != null)
		{
            var itemExp = _playerFileStoreProduct.itemDefinition.GetExpansion();

			if (!Game.Persistence.playerFile.IsInventoryFull() 
                && Game.Persistence.playerFile.GetFruitCount(_core.affectionType) >= _playerFileStoreProduct.itemCost 
                && (highlightedItemType == ItemType.MISC 
                    || itemExp.StoreHandler.ShowWithStoreFilter(highlightedItemType)))
			{
				_core.button.Enable();
				_core.itemSlot.slotBgCanvasGroup.alpha = 1f;
				_core.itemSlot.itemIcon.color = ColorUtils.ColorAlpha(_core.itemSlot.itemIcon.color, 1f);
				_core.countLabelPro.alpha = 1f;
			}
            //Keeping this here just in case, but item type has been depreciated. All items should have a custom category and description string
			else if (highlightedItemType == ItemType.MISC || highlightedItemType == _playerFileStoreProduct.itemDefinition.itemType)
			{
				_core.button.Enable();
				_core.itemSlot.slotBgCanvasGroup.alpha = 0.5f;
				_core.itemSlot.itemIcon.color = ColorUtils.ColorAlpha(_core.itemSlot.itemIcon.color, 0.5f);
				_core.countLabelPro.alpha = 0.75f;
			}
		}
    }

    private void Start_Postfix()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    private void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }
}
