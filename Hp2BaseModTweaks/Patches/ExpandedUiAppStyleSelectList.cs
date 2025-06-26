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
        public static void PreRefresh(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).PreRefresh();

        [HarmonyPatch("Refresh")]
        [HarmonyPostfix]
        public static void PostRefresh(UiAppStyleSelectList __instance)
            => ExpandedUiAppStyleSelectList.Get(__instance).PostRefresh();

        //OnBuyButtonPressed(ButtonBehavior buttonBehavior)
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

        private static readonly FieldInfo _origPos = AccessTools.Field(typeof(UiAppStyleSelectList), "_origPos");
        private static readonly FieldInfo _origBgSize = AccessTools.Field(typeof(UiAppStyleSelectList), "_origBgSize");
        private static readonly FieldInfo _playerFileGirl = AccessTools.Field(typeof(UiAppStyleSelectList), "_playerFileGirl");
        private static readonly FieldInfo _purchaseListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_purchaseListItem");
        private static readonly FieldInfo _selectedListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_selectedListItem");


        private static readonly MethodInfo _onListItemSelected = AccessTools.Method(typeof(UiAppStyleSelectList), "OnListItemSelected");
        private static readonly MethodInfo _refresh = AccessTools.Method(typeof(UiAppStyleSelectList), "Refresh");
        private static readonly MethodInfo _onButtonPressed = AccessTools.Method(typeof(UiAppSelectListItem), "OnButtonPressed");

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
        private ExpandedUiAppStyleSelectList(UiAppStyleSelectList decorated)
        {
            _uiAppStyleSelectList = decorated;
        }

        public void Awake()
        {
            if (_initialized) { return; }

            //put a scroll rect in the same position as the list
            var scroll_GO = new GameObject($"{_uiAppStyleSelectList.name}Scroll");

            _scrollRectTransform = scroll_GO.AddComponent<RectTransform>();
            _scrollRectTransform.pivot = new Vector2(0.5f, 1f);
            _scrollRectTransform.position = _uiAppStyleSelectList.background.position - new Vector3(0, 32);

            var image = scroll_GO.AddComponent<Image>();
            var scroll_ScrollRect = scroll_GO.AddComponent<ScrollRect>();
            var scroll_Mask = scroll_GO.AddComponent<Mask>();

            scroll_GO.transform.SetParent(_uiAppStyleSelectList.transform, true);

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
            _itemContainerRectTransform.localPosition = new Vector2(_itemContainerRectTransform.localPosition.x, -20f);

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

            //move the buy buttons up to make room for toggles
            _uiAppStyleSelectList.buyButton.transform.position = _uiAppStyleSelectList.buyButton.transform.position + new Vector3(0, 32);

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

        public void PreRefresh()
        {
            //if the selected wardrobe is on another page, the wardrobe will populate this with a null
            //and will throw on refresh. So manually set the playerfile girl from the flag or lola for default
            var def = Game.Data.Girls.Get(Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id")) ?? Game.Data.Girls.Get(1);

            _playerFileGirl.SetValue(_uiAppStyleSelectList, Game.Persistence.playerFile.GetPlayerFileGirl(def));

            if (_initialized)
            {
                if (def == null) { return; }

                var diff = (_uiAppStyleSelectList.alternative
                        ? def.outfits.Count
                        : def.hairstyles.Count)
                    - _uiAppStyleSelectList.listItems.Count;

                if (diff > 0)
                {
                    // add missing

                    for (var i = diff; i > 0; i--)
                    {
                        var newItem = UnityEngine.Object.Instantiate(_listItemTemplate);
                        newItem.rectTransform.SetParent(_itemContainerRectTransform, false);

                        newItem.ListItemSelectedEvent += On_ListItemSelected;

                        _ownedListItems.Add(newItem);
                        _uiAppStyleSelectList.listItems.Add(newItem);
                    }
                }
                else if (diff < 0)
                {
                    //remove extras
                    for (var i = diff; i < 0; i++)
                    {
                        var old = _uiAppStyleSelectList.listItems.Last();
                        _uiAppStyleSelectList.listItems.RemoveAt(_uiAppStyleSelectList.listItems.Count - 1);

                        old.ListItemSelectedEvent -= On_ListItemSelected;
                        _ownedListItems.Remove(old);

                        UnityEngine.Object.Destroy(old.gameObject);
                    }
                }
            }
        }

        public void PostRefresh()
        {
            if (!_initialized)
            {
                return;
            }

            if (!_playerFileGirl.TryGetValue<PlayerFileGirl>(_uiAppStyleSelectList, out var playerFileGirl)
                            || playerFileGirl.girlDefinition == null)
            {
                return;
            }

            var postGame = Game.Persistence.playerFile.storyProgress >= 14;

            var purchaseItems = new List<UiAppSelectListItem>();
            var codeItems = new List<UiAppSelectListItem>();
            var shownItems = new List<UiAppSelectListItem>();
            var hiddenItems = new List<UiAppSelectListItem>();

            var visibleItemCount = 0;

            var styleEnumerator = _uiAppStyleSelectList.alternative
                ? playerFileGirl.girlDefinition.outfits.Select<GirlOutfitSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => (x.outfitName, x.Expansion())).GetEnumerator()
                : playerFileGirl.girlDefinition.hairstyles.Select<GirlHairstyleSubDefinition, (string Name, ExpandedStyleDefinition Expansion)>(x => (x.hairstyleName, x.Expansion())).GetEnumerator();

            var listItemEnumerator = _uiAppStyleSelectList.listItems.GetEnumerator();

            UiAppSelectListItem purchaseItem = null;
            _purchaseCost = 0;

            var i = 0;
            while (styleEnumerator.MoveNext() && listItemEnumerator.MoveNext())
            {
                var unlocked = _uiAppStyleSelectList.alternative
                    ? playerFileGirl.IsOutfitUnlocked(i)
                    : playerFileGirl.IsHairstyleUnlocked(i);

                var hideIfLocked = false;
                string text;

                if (styleEnumerator.Current.Expansion.IsCodeUnlocked)
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
                    _selectedListItem.SetValue(_uiAppStyleSelectList, listItemEnumerator.Current);
                }
                else
                {
                    listItemEnumerator.Current.Select(false);
                }

                i++;
            }

            _purchaseListItem.SetValue(_uiAppStyleSelectList, purchaseItem);

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
                _uiAppStyleSelectList.background.sizeDelta = _uiAppStyleSelectList.background.sizeDelta - new Vector2(0, 32);
            }
            else
            {
                var origPos = _origPos.GetValue<Vector2>(_uiAppStyleSelectList);
                var origBgSize = _origBgSize.GetValue<Vector2>(_uiAppStyleSelectList);

                //they all have 1 code and 3 purchase items, so I'll just manually set it
                //it'd be weird if random ones just started changing sizes
                var num = 4;

                _uiAppStyleSelectList.rectTransform.anchoredPosition = origPos + Vector2.down * (float)(26 * num);
                _uiAppStyleSelectList.background.sizeDelta = origBgSize + Vector2.down * (float)(40 * num);
                _uiAppStyleSelectList.canvasGroup.alpha = 0f;
                _uiAppStyleSelectList.canvasGroup.blocksRaycasts = false;
            }

            _scrollRectTransform.sizeDelta = _uiAppStyleSelectList.background.sizeDelta - new Vector2(24, 42);

            //for fun and to show the user that the list can scroll, move the scroll to the bottom and have it
            //scroll up
            _paddingRectTransform.position -= new Vector3(0f, _scrollRectTransform.sizeDelta.y);
        }

        private void On_ListItemSelected(UiAppSelectListItem listItem) => _onListItemSelected.Invoke(_uiAppStyleSelectList, [listItem]);

        internal bool OnBuyButtonPressed(ButtonBehavior buttonBehavior)
        {
            //the original maps the index of the buy slot to a table of prices, which will not work for any type of expansion or
            //re-arranging of slots, so we have to overwrite it
            var purchaseItem = _purchaseListItem.GetValue<UiAppSelectListItem>(_uiAppStyleSelectList);

            if (purchaseItem == null)
            {
                return true;
            }

            var playerFileGirl = _playerFileGirl.GetValue<PlayerFileGirl>(_uiAppStyleSelectList);

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

            _refresh.Invoke(_uiAppStyleSelectList, null);

            ListItemSelectedEvent?.Invoke(_uiAppStyleSelectList, true);

            return false;
        }
    }
}
