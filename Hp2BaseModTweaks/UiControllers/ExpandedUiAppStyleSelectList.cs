// Hp2BaseModTweaks 2022, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks
{
    internal class UiAppStyleSelectList_Extension
    {
        private static Dictionary<UiAppStyleSelectList, UiAppStyleSelectList_Extension> _extensions
            = new Dictionary<UiAppStyleSelectList, UiAppStyleSelectList_Extension>();

        public static UiAppStyleSelectList_Extension GetExtension(UiAppStyleSelectList __instance)
        {
            if (!_extensions.TryGetValue(__instance, out var extension))
            {
                extension = new UiAppStyleSelectList_Extension(__instance);
                _extensions[__instance] = extension;
            }

            return extension;
        }

        private static readonly FieldInfo item_hidden = AccessTools.Field(typeof(UiAppSelectListItem), "_hidden");
        private static readonly FieldInfo item_unlocked = AccessTools.Field(typeof(UiAppSelectListItem), "_unlocked");
        private static readonly FieldInfo _purchaseListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_purchaseListItem");
        private static readonly FieldInfo _selectedListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_selectedListItem");
        private static readonly FieldInfo _origPos = AccessTools.Field(typeof(UiAppStyleSelectList), "_origPos");
        private static readonly FieldInfo _origBgSize = AccessTools.Field(typeof(UiAppStyleSelectList), "_origBgSize");
        private static readonly MethodInfo Start = AccessTools.Method(typeof(UiAppStyleSelectList), "Start");
        private static readonly MethodInfo OnDestroy = AccessTools.Method(typeof(UiAppStyleSelectList), "OnDestroy");
        private static readonly FieldInfo _playerFileGirl = AccessTools.Field(typeof(UiAppStyleSelectList), "_playerFileGirl");
        private static readonly MethodInfo _onListItemSelected = AccessTools.Method(typeof(UiAppStyleSelectList), "OnListItemSelected");

        private static readonly Vector3 _itemSpacing = new Vector3(0, -33.3333f, 0);

        private UiAppSelectListItem _listItemTemplate;
        private RectTransform _scrollRectTransform;
        private RectTransform _paddingRectTransform;
        private RectTransform _itemContainerRectTransform;
        private List<UiAppSelectListItem> _ownedListItems = new List<UiAppSelectListItem>();

        public bool Initialized => _initialized;
        private bool _initialized;

        private UiAppStyleSelectList _decorated;
        private UiAppStyleSelectList_Extension(UiAppStyleSelectList decorated)
        {
            _decorated = decorated;
        }

        public void Init()
        {
            if (_initialized) { return; }

            //put a scroll rect in the same position as the list
            var scroll_GO = new GameObject($"{_decorated.name}Scroll");

            _scrollRectTransform = scroll_GO.AddComponent<RectTransform>();
            _scrollRectTransform.pivot = new Vector2(0.5f, 1f);
            _scrollRectTransform.position = _decorated.background.position - new Vector3(0, 32);

            var image = scroll_GO.AddComponent<Image>();
            var scroll_ScrollRect = scroll_GO.AddComponent<ScrollRect>();
            var scroll_Mask = scroll_GO.AddComponent<Mask>();

            scroll_GO.transform.SetParent(_decorated.transform, true);

            //padding
            var padding_GO = new GameObject($"{_decorated.name}Padding");
            padding_GO.transform.SetParent(scroll_GO.transform, true);
            _paddingRectTransform = padding_GO.AddComponent<RectTransform>();
            _paddingRectTransform.pivot = new Vector2(0.5f, 1f);

            //container
            var itemContainer = _decorated.transform.Find("ListItemContainer");
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
            _listItemTemplate = UnityEngine.Object.Instantiate(_decorated.listItems[0]);
            _listItemTemplate.transform.SetParent(null, true);

            _initialized = true;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(_listItemTemplate);

            foreach (var item in _ownedListItems)
            {
                item.ListItemSelectedEvent -= On_ListItemSelected;
                UnityEngine.Object.Destroy(item);
            }

            _extensions.Remove(_decorated);
        }

        public PlayerFileGirl GetGirl() => _playerFileGirl.GetValue(_decorated) as PlayerFileGirl;

        public void SetItemCount(int itemCount)
        {
            var diff = itemCount - _decorated.listItems.Count;

            if (diff > 0)
            {
                // add missing

                for (var i = diff; i > 0; i--)
                {
                    var newItem = UnityEngine.Object.Instantiate(_listItemTemplate);
                    newItem.rectTransform.SetParent(_itemContainerRectTransform, false);

                    newItem.ListItemSelectedEvent += On_ListItemSelected;

                    _ownedListItems.Add(newItem);
                    _decorated.listItems.Add(newItem);
                }
            }
            else if (diff < 0)
            {
                //remove extras
                for (var i = diff; i < 0; i++)
                {
                    var old = _decorated.listItems.Last();
                    _decorated.listItems.RemoveAt(_decorated.listItems.Count - 1);

                    old.ListItemSelectedEvent -= On_ListItemSelected;
                    _ownedListItems.Remove(old);

                    UnityEngine.Object.Destroy(old.gameObject);
                }
            }
        }

        public void SortItems()
        {
            var purchaseItems = new List<UiAppSelectListItem>();
            var shownItems = new List<UiAppSelectListItem>();
            var hiddenItems = new List<UiAppSelectListItem>();

            var i = 0;
            var visibleItemCount = 0;
            foreach (var item in _decorated.listItems)
            {
                if ((bool)item_hidden.GetValue(item))
                {
                    hiddenItems.Add(item);
                }
                else if (Game.Session.Hub.unlockStylesBuy.Contains(i))
                {
                    purchaseItems.Add(item);
                    visibleItemCount++;
                }
                else
                {
                    shownItems.Add(item);
                    visibleItemCount++;
                }
                i++;
            }

            _paddingRectTransform.sizeDelta = new Vector2(278, 33.3333f * (visibleItemCount + 1));

            // reposition in order
            i = 0;
            foreach (var item in shownItems.Concat(purchaseItems).Concat(hiddenItems))
            {
                item.transform.localPosition = i++ * _itemSpacing;
            }

            //fix bg
            if (Game.Persistence.playerFile.storyProgress < 14)
            {
                var origPos = (Vector2)_origPos.GetValue(_decorated);
                var origBgSize = (Vector2)_origBgSize.GetValue(_decorated);

                //they all have 1 code and 3 purchase items, so I'll just manually set it
                //it'd be weird if random ones just started changing sizes
                var num = 4;

                _decorated.rectTransform.anchoredPosition = origPos + Vector2.down * (float)(26 * num);
                _decorated.background.sizeDelta = origBgSize + Vector2.down * (float)(40 * num);
                _decorated.canvasGroup.alpha = 0f;
                _decorated.canvasGroup.blocksRaycasts = false;
            }

            _scrollRectTransform.sizeDelta = _decorated.background.sizeDelta - new Vector2(24, 42);
            _paddingRectTransform.position -= new Vector3(0f, _scrollRectTransform.sizeDelta.y);
        }

        private void On_ListItemSelected(UiAppSelectListItem listItem) => _onListItemSelected.Invoke(_decorated, [listItem]);
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "Awake")]
    internal static class UiAppStyleSelectList_Awake
    {
        public static void Postfix(UiAppStyleSelectList __instance)
        {
            UiAppStyleSelectList_Extension.GetExtension(__instance).Init();
        }
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "OnDestroy")]
    internal static class UiAppStyleSelectList_OnDestroy
    {
        public static void Prefix(UiAppStyleSelectList __instance)
        {
            UiAppStyleSelectList_Extension.GetExtension(__instance).Destroy();
        }
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "Refresh")]
    internal static class UiAppStyleSelectList_Refresh
    {
        public static void Prefix(UiAppStyleSelectList __instance)
        {
            var extension = UiAppStyleSelectList_Extension.GetExtension(__instance);

            if (extension.Initialized)
            {
                var def = extension.GetGirl()?.girlDefinition;
                if (def == null) { return; }

                extension.SetItemCount(__instance.alternative
                    ? def.outfits.Count
                    : def.hairstyles.Count);
            }
        }

        public static void Postfix(UiAppStyleSelectList __instance)
        {
            var extension = UiAppStyleSelectList_Extension.GetExtension(__instance);

            if (extension.Initialized)
            {
                extension.SortItems();
            }
        }
    }
}
