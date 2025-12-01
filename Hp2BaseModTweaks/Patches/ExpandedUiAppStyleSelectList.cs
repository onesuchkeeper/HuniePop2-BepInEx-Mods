// Hp2BaseModTweaks 2022, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            => ExpandedUiAppStyleSelectList.Get(__instance).Awake();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).OnDestroy();

        [HarmonyPatch("Refresh")]
        [HarmonyPrefix]
        public static bool Refresh(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).Refresh();

        [HarmonyPatch("OnBuyButtonPressed")]
        [HarmonyPrefix]
        public static bool OnBuyButtonPressed(UiAppStyleSelectList __instance, ButtonBehavior buttonBehavior)
            => ExpandedUiAppStyleSelectList.Get(__instance).OnBuyButtonPressed(buttonBehavior);
    }

    internal class ExpandedUiAppStyleSelectList
    {
        private static Dictionary<UiAppStyleSelectList, ExpandedUiAppStyleSelectList> _extensions
            = new Dictionary<UiAppStyleSelectList, ExpandedUiAppStyleSelectList>();

        public static ExpandedUiAppStyleSelectList Get(UiAppStyleSelectList __instance)
        {
            if (!_extensions.TryGetValue(__instance, out var extension))
            {
                extension = new ExpandedUiAppStyleSelectList(__instance);
                _extensions[__instance] = extension;
            }

            return extension;
        }

        private static readonly FieldInfo f_origPos = AccessTools.Field(typeof(UiAppStyleSelectList), "_origPos");
        private static readonly FieldInfo f_origBgSize = AccessTools.Field(typeof(UiAppStyleSelectList), "_origBgSize");
        private static readonly FieldInfo f_playerFileGirl = AccessTools.Field(typeof(UiAppStyleSelectList), "_playerFileGirl");
        private static readonly FieldInfo f_purchaseListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_purchaseListItem");
        private static readonly FieldInfo f_selectedListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_selectedListItem");

        private static readonly MethodInfo m_onListItemSelected = AccessTools.Method(typeof(UiAppStyleSelectList), "OnListItemSelected");
        private static readonly MethodInfo m_refresh = AccessTools.Method(typeof(UiAppStyleSelectList), "Refresh");

        private static readonly Vector3 _itemSpacing = new Vector3(0, -33.3333f, 0);

        public event UiAppStyleSelectList.UiAppStyleSelectListDelegate ListItemSelectedEvent;

        private UiAppSelectListItem _listItemTemplate;
        private RectTransform _scrollRectTransform;
        private RectTransform _paddingRectTransform;
        private RectTransform _itemContainerRectTransform;
        private List<UiAppSelectListItem> _ownedListItems = new List<UiAppSelectListItem>();

        private int _purchaseCost;
        private bool _initialized;

        private UiAppStyleSelectList _uiAppStyleSelectList;
        private LayoutElement _layoutElement;
        private ExpandedUiAppStyleSelectList(UiAppStyleSelectList decorated)
        {
            _uiAppStyleSelectList = decorated;
        }

        public void Awake()
        {
            if (_initialized) { return; }

            //positions
            var titleShift = new Vector2(0, _uiAppStyleSelectList.titleBar.rectTransform.sizeDelta.y / 2);

            _uiAppStyleSelectList.titleBar.rectTransform.anchoredPosition -= titleShift;
            _uiAppStyleSelectList.background.anchoredPosition -= titleShift;
            _uiAppStyleSelectList.buyButton.transform.position += new Vector3(0, 64);

            //put a scroll rect in the same position as the list
            var scroll_GO = new GameObject($"{_uiAppStyleSelectList.name}Scroll");
            scroll_GO.transform.SetParent(_uiAppStyleSelectList.transform, true);
            _scrollRectTransform = scroll_GO.AddComponent<RectTransform>();
            _scrollRectTransform.pivot = new Vector2(0.5f, 1f);
            _scrollRectTransform.position = _uiAppStyleSelectList.background.position - (2 * new Vector3(0, titleShift.y)) + new Vector3(0, 12);

            scroll_GO.AddComponent<Image>();
            var scroll_ScrollRect = scroll_GO.AddComponent<ScrollRect>();
            var scroll_Mask = scroll_GO.AddComponent<Mask>();

            //padding
            var padding_GO = new GameObject($"{_uiAppStyleSelectList.name}Padding");
            padding_GO.transform.SetParent(scroll_GO.transform, true);
            _paddingRectTransform = padding_GO.AddComponent<RectTransform>();
            _paddingRectTransform.pivot = new Vector2(0.5f, 1f);

            //container
            var itemContainer = _uiAppStyleSelectList.transform.Find("ListItemContainer");
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
            _listItemTemplate = UnityEngine.Object.Instantiate(_uiAppStyleSelectList.listItems[0]);
            _listItemTemplate.transform.SetParent(null, true);

            //layout
            _layoutElement = _uiAppStyleSelectList.gameObject.AddComponent<LayoutElement>();

            _initialized = true;
        }

        public void OnDestroy()
        {
            UnityEngine.Object.Destroy(_listItemTemplate);

            foreach (var item in _ownedListItems)
            {
                item.ListItemSelectedEvent -= On_ListItemSelected;
                UnityEngine.Object.Destroy(item);
            }

            _extensions.Remove(_uiAppStyleSelectList);
        }

        /// <summary>
        /// Completely replaces <see cref="UiAppStyleSelectList.Refresh"/>
        /// Original cannot handle gaps in collections made nesisary by the indexing of parts
        /// </summary>
        /// <returns></returns>
        public bool Refresh()
        {
            if (!_initialized
                || !f_playerFileGirl.TryGetValue<PlayerFileGirl>(_uiAppStyleSelectList, out var playerFileGirl)
                || playerFileGirl.girlDefinition == null)
            {
                return false;
            }

            // get girl def, default to Ashley
            var i = 0;
            var def = Game.Data.Girls.Get(Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id"));

            if (def == null)
            {
                def = ModInterface.GameData.GetGirl(Girls.AshleyId);
                f_playerFileGirl.SetValue(_uiAppStyleSelectList, Game.Persistence.playerFile.GetPlayerFileGirl(def));
            }

            // create missing list items
            var diff = (_uiAppStyleSelectList.alternative
                ? def.outfits.Where(x => x != null).Count()
                : def.hairstyles.Where(x => x != null).Count()) - _uiAppStyleSelectList.listItems.Count;

            if (diff > 0)
            {
                // add missing
                for (i = diff; i > 0; i--)
                {
                    var newItem = UnityEngine.Object.Instantiate(_listItemTemplate);
                    newItem.rectTransform.SetParent(_itemContainerRectTransform, false);

                    newItem.ListItemSelectedEvent += On_ListItemSelected;

                    _ownedListItems.Add(newItem);
                    _uiAppStyleSelectList.listItems.Add(newItem);
                }
            }

            var postGame = Game.Persistence.playerFile.storyProgress >= 14;
            var purchaseItems = new List<UiAppSelectListItem>();
            var codeItems = new List<UiAppSelectListItem>();
            var shownItems = new List<UiAppSelectListItem>();
            var hiddenItems = new List<UiAppSelectListItem>();

            var visibleItemCount = 0;

            var styleEnumerator = _uiAppStyleSelectList.alternative
                ? playerFileGirl.girlDefinition.outfits
                    .Select<GirlOutfitSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => x == null ? (null, null) : (x.outfitName, x.Expansion()))
                    .GetEnumerator()
                : playerFileGirl.girlDefinition.hairstyles
                    .Select<GirlHairstyleSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => x == null ? (null, null) : (x.hairstyleName, x.Expansion()))
                    .GetEnumerator();

            var listItemEnumerator = _uiAppStyleSelectList.listItems.GetEnumerator();

            UiAppSelectListItem purchaseItem = null;
            _purchaseCost = 0;

            while (styleEnumerator.MoveNext() && listItemEnumerator.MoveNext())
            {
                var unlocked = _uiAppStyleSelectList.alternative
                    ? playerFileGirl.IsOutfitUnlocked(i)
                    : playerFileGirl.IsHairstyleUnlocked(i);

                var hideIfLocked = false;
                string text;

                if (styleEnumerator.Current.Expansion == null)
                {
                    hiddenItems.Add(listItemEnumerator.Current);
                    text = string.Empty;
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

                        _purchaseCost = (int)(costMult * (_uiAppStyleSelectList.alternative
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

                    shownItems.Add(listItemEnumerator.Current);
                    visibleItemCount++;
                }

                listItemEnumerator.Current.Populate(unlocked, text, hideIfLocked);

                if ((_uiAppStyleSelectList.alternative && i == playerFileGirl.outfitIndex)
                    || (!_uiAppStyleSelectList.alternative && i == playerFileGirl.hairstyleIndex))
                {
                    listItemEnumerator.Current.Select(true);
                    f_selectedListItem.SetValue(_uiAppStyleSelectList, listItemEnumerator.Current);
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

            f_purchaseListItem.SetValue(_uiAppStyleSelectList, purchaseItem);

            purchaseItem?.ShowCost(
                Game.Session.Gift.GetFruitCategoryInfo(
                    (!_uiAppStyleSelectList.alternative)
                        ? playerFileGirl.girlDefinition.leastFavoriteAffectionType
                        : playerFileGirl.girlDefinition.favoriteAffectionType
                ),
                _purchaseCost
            );

            if (purchaseItem == null)
            {
                _uiAppStyleSelectList.buyButton.Disable();
            }
            else
            {
                _uiAppStyleSelectList.buyButton.Enable();
            }

            _paddingRectTransform.sizeDelta = new Vector2(278, 33.3333f * (visibleItemCount + 1));

            // reposition in proper order
            i = 0;
            foreach (var item in shownItems.Concat(codeItems).Concat(purchaseItems).Concat(hiddenItems))
            {
                item.transform.localPosition = i++ * _itemSpacing;
            }

            //fix bg
            if (postGame)
            {
                _uiAppStyleSelectList.background.sizeDelta = _uiAppStyleSelectList.background.sizeDelta - new Vector2(0, 80);
            }
            else
            {
                var origBgSize = f_origBgSize.GetValue<Vector2>(_uiAppStyleSelectList);

                //they all have 1 code and 3 purchase items, so I'll just manually set it
                //it'd be weird if random ones just started changing sizes
                var postGameStyleCount = 4;
                _uiAppStyleSelectList.background.sizeDelta = origBgSize + Vector2.down * (float)(40 * postGameStyleCount);
                _uiAppStyleSelectList.canvasGroup.alpha = 0f;
                _uiAppStyleSelectList.canvasGroup.blocksRaycasts = false;
            }

            _layoutElement.preferredHeight = _uiAppStyleSelectList.background.sizeDelta.y;

            if (postGame)
            {
                _layoutElement.preferredHeight += _uiAppStyleSelectList.buyButton.rectTransform.sizeDelta.y + 16;
            }

            _scrollRectTransform.sizeDelta = _uiAppStyleSelectList.background.sizeDelta - new Vector2(24, 42);

            //for fun and to show the user that the list can scroll, move the scroll to the bottom and have it
            //scroll up
            _paddingRectTransform.position -= new Vector3(0f, _scrollRectTransform.sizeDelta.y / 2);

            return false;
        }

        private void On_ListItemSelected(UiAppSelectListItem listItem) => m_onListItemSelected.Invoke(_uiAppStyleSelectList, [listItem]);

        internal bool OnBuyButtonPressed(ButtonBehavior buttonBehavior)
        {
            //the original maps the index of the buy slot to a table of prices, which will not work for any type of expansion or
            //re-arranging of slots, so we have to overwrite it
            var purchaseItem = f_purchaseListItem.GetValue<UiAppSelectListItem>(_uiAppStyleSelectList);

            if (purchaseItem == null)
            {
                return true;
            }

            var playerFileGirl = f_playerFileGirl.GetValue<PlayerFileGirl>(_uiAppStyleSelectList);

            var fruitCategoryInfo = Game.Session.Gift.GetFruitCategoryInfo(_uiAppStyleSelectList.alternative
                ? playerFileGirl.girlDefinition.favoriteAffectionType
                : playerFileGirl.girlDefinition.leastFavoriteAffectionType);

            if (Game.Persistence.playerFile.GetFruitCount(fruitCategoryInfo.affectionType) < _purchaseCost)
            {
                return true;
            }

            Game.Persistence.playerFile.AddFruitCount(fruitCategoryInfo.affectionType, -_purchaseCost);

            var itemIndex = _uiAppStyleSelectList.listItems.IndexOf(purchaseItem);

            if (_uiAppStyleSelectList.alternative)
            {
                playerFileGirl.UnlockOutfit(itemIndex);
                playerFileGirl.outfitIndex = itemIndex;
            }
            else
            {
                playerFileGirl.UnlockHairstyle(itemIndex);
                playerFileGirl.hairstyleIndex = itemIndex;
            }

            m_refresh.Invoke(_uiAppStyleSelectList, null);

            ListItemSelectedEvent?.Invoke(_uiAppStyleSelectList, true);

            return false;
        }
    }
}
