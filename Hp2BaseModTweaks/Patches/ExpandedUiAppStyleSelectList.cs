// Hp2BaseModTweaks 2022, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiAppStyleSelectList))]
    internal static class UiAppStyleSelectListPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).Awake_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Destroy(__instance);

        [HarmonyPatch("Refresh")]
        [HarmonyPrefix]
        public static bool Refresh(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).Refresh_Prefix();

        [HarmonyPatch("OnBuyButtonPressed")]
        [HarmonyPrefix]
        public static bool OnBuyButtonPressed(UiAppStyleSelectList __instance, ButtonBehavior buttonBehavior)
            => ExpandedUiAppStyleSelectList.Get(__instance).OnBuyButtonPressed_Prefix(buttonBehavior);
    }

    [Expansion(typeof(UiAppStyleSelectList))]
    public partial class ExpandedUiAppStyleSelectList
    {
        private static readonly Vector3 ITEM_SPACING = new Vector3(0, -33.3333f, 0);

        public event UiAppStyleSelectList.UiAppStyleSelectListDelegate ListItemSelectedEvent;

        private UiAppSelectListItem _listItemTemplate;
        private RectTransform _scrollRectTransform;
        private RectTransform _paddingRectTransform;
        private RectTransform _itemContainerRectTransform;
        private List<UiAppSelectListItem> _ownedListItems = new List<UiAppSelectListItem>();

        private int _purchaseCost;
        private bool _initialized;

        private LayoutElement _layoutElement;

        internal void Awake_Postfix()
        {
            if (_initialized) return;

            //positions
            var titleShift = new Vector2(0, _core.titleBar.rectTransform.sizeDelta.y / 2);

            _core.titleBar.rectTransform.anchoredPosition -= titleShift;
            _core.background.anchoredPosition -= titleShift;
            _core.buyButton.transform.position += new Vector3(0, 64);

            //put a scroll rect in the same position as the list
            var scroll_GO = new GameObject($"{_core.name}Scroll");
            scroll_GO.transform.SetParent(_core.transform, true);
            _scrollRectTransform = scroll_GO.AddComponent<RectTransform>();
            _scrollRectTransform.pivot = new Vector2(0.5f, 1f);
            _scrollRectTransform.position = _core.background.position - (2 * new Vector3(0, titleShift.y)) + new Vector3(0, 12);

            scroll_GO.AddComponent<Image>();
            var scroll_ScrollRect = scroll_GO.AddComponent<ScrollRect>();
            var scroll_Mask = scroll_GO.AddComponent<Mask>();

            //padding
            var padding_GO = new GameObject($"{_core.name}Padding");
            padding_GO.transform.SetParent(scroll_GO.transform, true);
            _paddingRectTransform = padding_GO.AddComponent<RectTransform>();
            _paddingRectTransform.pivot = new Vector2(0.5f, 1f);

            //container
            var itemContainer = _core.transform.Find("ListItemContainer");
            itemContainer.transform.SetParent(_paddingRectTransform, true);
            _itemContainerRectTransform = itemContainer.GetComponent<RectTransform>();
            _itemContainerRectTransform.pivot = new Vector2(0.5f, 1f);
            _itemContainerRectTransform.anchorMin = new Vector2(0.5f, 1f);
            _itemContainerRectTransform.anchorMax = new Vector2(0.5f, 1f);
            _itemContainerRectTransform.localPosition = new Vector2(
                _itemContainerRectTransform.localPosition.x,
                -20f
            );

            //settings
            scroll_ScrollRect.scrollSensitivity = 18;
            scroll_ScrollRect.horizontal = false;
            scroll_ScrollRect.content = _paddingRectTransform;
            scroll_ScrollRect.verticalNormalizedPosition = 1f;
            scroll_Mask.showMaskGraphic = false;
            scroll_ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            scroll_ScrollRect.elasticity = 0.15f;

            //grab the first list item to use a a template ot make others
            _listItemTemplate = UnityEngine.Object.Instantiate(_core.listItems[0]);
            _listItemTemplate.transform.SetParent(null, true);

            //layout
            _layoutElement = _core.gameObject.AddComponent<LayoutElement>();

            _initialized = true;
        }

        private void OnDestroy()
        {
            UnityEngine.Object.Destroy(_listItemTemplate);

            foreach (var item in _ownedListItems)
            {
                item.ListItemSelectedEvent -= On_ListItemSelected;
                UnityEngine.Object.Destroy(item);
            }
        }

        /// <summary>
        /// Completely replaces <see cref="UiAppStyleSelectList.Refresh"/>
        /// Original cannot handle gaps in collections made nesisary by the indexing of parts
        /// </summary>
        /// <returns></returns>
        internal bool Refresh_Prefix()
        {
            if (!_initialized
                || !f_playerFileGirl.TryGetValue<PlayerFileGirl>(_core, out var playerFileGirl)
                || playerFileGirl.girlDefinition == null)
            {
                return false;
            }

            // defaults
            var i = 0;
            var def = Game.Data.Girls.Get(Game.Persistence.playerFile.GetFlagValue(Flags.WARDROBE_GIRL_ID));

            // create missing list items
            var diff = (_core.alternative
                ? def.outfits.Count()
                : def.hairstyles.Count()) - _core.listItems.Count;

            if (diff > 0)
            {
                for (i = diff; i > 0; i--)
                {
                    var newItem = UnityEngine.Object.Instantiate(_listItemTemplate);
                    newItem.rectTransform.SetParent(_itemContainerRectTransform, false);

                    newItem.ListItemSelectedEvent += On_ListItemSelected;

                    _ownedListItems.Add(newItem);
                    _core.listItems.Add(newItem);
                }
            }

            var postGame = Game.Persistence.playerFile.IsProgressComplete();
            var purchaseItems = new List<UiAppSelectListItem>();
            var codeItems = new List<UiAppSelectListItem>();
            var shownItems = new List<UiAppSelectListItem>();
            var hiddenItems = new List<UiAppSelectListItem>();
            var nsfwItems = new List<UiAppSelectListItem>();

            var visibleItemCount = 0;

            var styleEnumerator = _core.alternative
                ? playerFileGirl.girlDefinition.outfits
                    .Select<GirlOutfitSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => (x?.outfitName, x?.GetExpansion()))
                    .GetEnumerator()
                : playerFileGirl.girlDefinition.hairstyles
                    .Select<GirlHairstyleSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => (x?.hairstyleName, x?.GetExpansion()))
                    .GetEnumerator();

            var listItemEnumerator = _core.listItems.GetEnumerator();

            UiAppSelectListItem purchaseItem = null;
            _purchaseCost = 0;

            while (styleEnumerator.MoveNext() && listItemEnumerator.MoveNext())
            {
                var unlocked = _core.alternative
                    ? playerFileGirl.IsOutfitUnlocked(i)
                    : playerFileGirl.IsHairstyleUnlocked(i);

                var hideIfLocked = false;
                string text;

                if (styleEnumerator.Current.Expansion == null)
                {
                    hiddenItems.Add(listItemEnumerator.Current);
                    i++;
                    continue;
                }
                else if (styleEnumerator.Current.Expansion.IsCodeUnlocked)
                {
                    text = unlocked
                        ? styleEnumerator.Current.Name
                        : "Unlock with code";

                    hideIfLocked = !postGame;

                    if (hideIfLocked && !unlocked)
                    {
                        hiddenItems.Add(listItemEnumerator.Current);
                        i++;
                        continue;
                    }
                    else
                    {
                        codeItems.Add(listItemEnumerator.Current);
                        visibleItemCount++;
                    }
                }
                else if (styleEnumerator.Current.Expansion.IsPurchased)
                {
                    hideIfLocked = !postGame;

                    if (hideIfLocked && !unlocked)
                    {
                        hiddenItems.Add(listItemEnumerator.Current);
                        i++;
                        continue;
                    }
                    else
                    {
                        purchaseItems.Add(listItemEnumerator.Current);
                        visibleItemCount++;
                    }

                    if (!unlocked && purchaseItem == null)
                    {
                        purchaseItem = listItemEnumerator.Current;
                        text = "Purchase:";

                        var costMult = Game.Persistence.playerFile.settingDifficulty == SettingDifficulty.EASY
                            ? 1.5f
                            : Game.Persistence.playerFile.settingDifficulty == SettingDifficulty.HARD
                                ? 0.5f
                                : 1f;

                        _purchaseCost = (int)(costMult * (_core.alternative
                            ? Mathf.Min(30, 10 * purchaseItems.Count)
                            : Mathf.Min(15, 5 * purchaseItems.Count)));
                    }
                    else
                    {
                        text = unlocked
                            ? styleEnumerator.Current.Name
                            : "???";
                    }
                }
                else
                {
                    text = unlocked
                        ? styleEnumerator.Current.Name
                        : "???";

                    if (styleEnumerator.Current.Expansion.IsNSFW)
                    {
                        nsfwItems.Add(listItemEnumerator.Current);
                    }
                    else
                    {
                        shownItems.Add(listItemEnumerator.Current);
                    }

                    visibleItemCount++;
                }

                listItemEnumerator.Current.Populate(unlocked, text, hideIfLocked);

                if ((_core.alternative && i == playerFileGirl.outfitIndex)
                    || (!_core.alternative && i == playerFileGirl.hairstyleIndex))
                {
                    listItemEnumerator.Current.Select(true);
                    f_selectedListItem.SetValue(_core, listItemEnumerator.Current);
                }
                else
                {
                    listItemEnumerator.Current.Select(false);
                }

                i++;
            }

            while (listItemEnumerator.MoveNext())
            {
                hiddenItems.Add(listItemEnumerator.Current);
            }

            foreach (var hiddenItem in hiddenItems)
            {
                hiddenItem.Populate(false, string.Empty, true);
            }

            f_purchaseListItem.SetValue(_core, purchaseItem);

            purchaseItem?.ShowCost(
                Game.Session.Gift.GetFruitCategoryInfo(
                    (!_core.alternative)
                        ? playerFileGirl.girlDefinition.leastFavoriteAffectionType
                        : playerFileGirl.girlDefinition.favoriteAffectionType
                ),
                _purchaseCost
            );

            if (purchaseItem == null)
            {
                _core.buyButton.Disable();
            }
            else
            {
                _core.buyButton.Enable();
            }

            _paddingRectTransform.sizeDelta = new Vector2(278, 33.3333f * (visibleItemCount + 1));

            // reposition in proper order
            i = 0;
            foreach (var item in shownItems.Concat(nsfwItems).Concat(codeItems).Concat(purchaseItems).Concat(hiddenItems))
            {
                var position = i * ITEM_SPACING;
                item.transform.localPosition = i++ * ITEM_SPACING;
            }

            // fix bg
            var origBgSize = f_origBgSize.GetValue<Vector2>(_core);

            if (postGame)
            {
                _core.background.sizeDelta = origBgSize - new Vector2(0, 80);
            }
            else
            {
                // they all have 1 code and 3 purchase items, so I'll just manually set it
                // it'd be weird if random ones just started changing sizes
                var postGameStyleCount = 4;
                _core.background.sizeDelta = origBgSize + Vector2.down * (40 * postGameStyleCount);
                _core.canvasGroup.alpha = 0f;
                _core.canvasGroup.blocksRaycasts = false;
            }

            _layoutElement.preferredHeight = _core.background.sizeDelta.y;

            if (postGame)
            {
                _layoutElement.preferredHeight += _core.buyButton.rectTransform.sizeDelta.y + 16;
            }

            _scrollRectTransform.sizeDelta = _core.background.sizeDelta - new Vector2(24, 42);

            // for fun and to show the user that the list can scroll, move the scroll to the bottom and have it
            //scroll up
            _paddingRectTransform.position -= new Vector3(0f, _scrollRectTransform.sizeDelta.y / 2);

            return false;
        }

        private void On_ListItemSelected(UiAppSelectListItem listItem) => m_OnListItemSelected.Invoke(_core, [listItem]);

        internal bool OnBuyButtonPressed_Prefix(ButtonBehavior buttonBehavior)
        {
            //the original maps the index of the buy slot to a table of prices, which will not work for any type of expansion or
            //re-arranging of slots, so we have to overwrite it
            var purchaseItem = f_purchaseListItem.GetValue<UiAppSelectListItem>(_core);

            if (purchaseItem == null)
            {
                return true;
            }

            var playerFileGirl = f_playerFileGirl.GetValue<PlayerFileGirl>(_core);

            var fruitCategoryInfo = Game.Session.Gift.GetFruitCategoryInfo(_core.alternative
                ? playerFileGirl.girlDefinition.favoriteAffectionType
                : playerFileGirl.girlDefinition.leastFavoriteAffectionType);

            if (Game.Persistence.playerFile.GetFruitCount(fruitCategoryInfo.affectionType) < _purchaseCost)
            {
                return true;
            }

            Game.Persistence.playerFile.AddFruitCount(fruitCategoryInfo.affectionType, -_purchaseCost);

            var itemIndex = _core.listItems.IndexOf(purchaseItem);

            if (_core.alternative)
            {
                playerFileGirl.UnlockOutfit(itemIndex);
                playerFileGirl.outfitIndex = itemIndex;
            }
            else
            {
                playerFileGirl.UnlockHairstyle(itemIndex);
                playerFileGirl.hairstyleIndex = itemIndex;
            }

            m_Refresh.Invoke(_core, null);

            ListItemSelectedEvent?.Invoke(_core, true);

            return false;
        }
    }
}
