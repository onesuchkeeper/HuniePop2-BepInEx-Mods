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
    internal static class UiAppStyleSelectList_Properties
    {
        public class UiAppStyleSelectList_Extension
        {
            public UiAppSelectListItem ListItemTemplate;
            public RectTransform ScrollRectTransform;
            public RectTransform ItemContainerRectTransform;
            public bool Started = false;
            public bool IgnoreDestroy = false;
        }

        public static readonly Vector3 _itemSpacing = new Vector3(0, -33.3333f, 0);
        public static readonly FieldInfo item_hidden = AccessTools.Field(typeof(UiAppSelectListItem), "_hidden");
        public static readonly FieldInfo item_unlocked = AccessTools.Field(typeof(UiAppSelectListItem), "_unlocked");

        public static PlayerFileGirl GetGirl(UiAppStyleSelectList __instance) => _playerFileGirl.GetValue(__instance) as PlayerFileGirl;
        public static readonly FieldInfo _playerFileGirl = AccessTools.Field(typeof(UiAppStyleSelectList), "_playerFileGirl");
        public static readonly FieldInfo _purchaseListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_purchaseListItem");
        public static readonly FieldInfo _selectedListItem = AccessTools.Field(typeof(UiAppStyleSelectList), "_selectedListItem");
        public static readonly FieldInfo _origPos = AccessTools.Field(typeof(UiAppStyleSelectList), "_origPos");
        public static readonly FieldInfo _origBgSize = AccessTools.Field(typeof(UiAppStyleSelectList), "_origBgSize");
        public static readonly MethodInfo Start = AccessTools.Method(typeof(UiAppStyleSelectList), "Start");
        private static readonly MethodInfo OnDestroy = AccessTools.Method(typeof(UiAppStyleSelectList), "OnDestroy");

        private static Dictionary<UiAppStyleSelectList, UiAppStyleSelectList_Extension> _extensions
            = new Dictionary<UiAppStyleSelectList, UiAppStyleSelectList_Extension>();

        public static UiAppStyleSelectList_Extension GetExtension(UiAppStyleSelectList __instance)
        {
            if (!_extensions.TryGetValue(__instance, out var extension))
            {
                extension = new UiAppStyleSelectList_Extension();
                _extensions[__instance] = extension;
            }

            return extension;
        }

        public static void DestroyExtension(UiAppStyleSelectList __instance)
        {
            var extension = GetExtension(__instance);

            if (extension.IgnoreDestroy) { return; }

            UnityEngine.Object.Destroy(_extensions[__instance].ListItemTemplate);
            _extensions.Remove(__instance);
        }

        public static void UnhookEvents(UiAppStyleSelectList __instance)
        {
            var extension = GetExtension(__instance);
            extension.IgnoreDestroy = true;
            OnDestroy.Invoke(__instance, null);
            extension.IgnoreDestroy = false;
        }

        public static void RehookEvents(UiAppStyleSelectList __instance)
        {
            Start.Invoke(__instance, null);
        }
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "Awake")]
    internal static class UiAppStyleSelectList_Awake
    {
        public static void Postfix(UiAppStyleSelectList __instance)
        {
            //make a scroll container parented to the instance and 
            //put the instance's list item container inside.
            var extension = UiAppStyleSelectList_Properties.GetExtension(__instance);
            extension.Started = true;

            //put a scroll rect in the same position as the list
            var scroll_GO = new GameObject($"{__instance.name}Scroll");

            extension.ScrollRectTransform = scroll_GO.AddComponent<RectTransform>();
            extension.ScrollRectTransform.pivot = new Vector2(0.5f, 1f);
            extension.ScrollRectTransform.position = __instance.background.position - new Vector3(0, 32);

            var image = scroll_GO.AddComponent<Image>();
            var scroll_ScrollRect = scroll_GO.AddComponent<ScrollRect>();
            var scroll_Mask = scroll_GO.AddComponent<Mask>();

            scroll_GO.transform.SetParent(__instance.transform, true);

            //padding
            var padding_GO = new GameObject($"{__instance.name}Padding");
            padding_GO.transform.SetParent(scroll_GO.transform, true);
            extension.ItemContainerRectTransform = padding_GO.AddComponent<RectTransform>();
            extension.ItemContainerRectTransform.pivot = new Vector2(0.5f, 1f);
            extension.ItemContainerRectTransform.position -= new Vector3(0f, 800f);

            //container
            var itemContainer = __instance.transform.Find("ListItemContainer");
            itemContainer.transform.SetParent(padding_GO.transform);
            var itemContainer_RectTransform = itemContainer.GetComponent<RectTransform>();
            itemContainer_RectTransform.pivot = new Vector2(0.5f, 1f);
            itemContainer_RectTransform.anchorMin = new Vector2(0.5f, 1f);
            itemContainer_RectTransform.anchorMax = new Vector2(0.5f, 1f);
            itemContainer_RectTransform.localPosition = new Vector2(itemContainer_RectTransform.localPosition.x, -20f);

            //settings
            scroll_ScrollRect.scrollSensitivity = 18;
            scroll_ScrollRect.horizontal = false;
            scroll_ScrollRect.content = extension.ItemContainerRectTransform;
            scroll_ScrollRect.verticalNormalizedPosition = 1f;
            scroll_Mask.showMaskGraphic = false;
            scroll_ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            scroll_ScrollRect.elasticity = 0.15f;

            //grab the first list item to use a a template ot make others
            extension.ListItemTemplate = UnityEngine.Object.Instantiate(__instance.listItems[0]);
            extension.ListItemTemplate.transform.SetParent(null, true);
        }
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "OnDestroy")]
    internal static class UiAppStyleSelectList_OnDestroy
    {
        public static void Prefix(UiAppStyleSelectList __instance)
        {
            UiAppStyleSelectList_Properties.DestroyExtension(__instance);
        }
    }

    [HarmonyPatch(typeof(UiAppStyleSelectList), "Refresh")]
    internal static class UiAppStyleSelectList_Refresh
    {
        public static void Prefix(UiAppStyleSelectList __instance)
        {
            var extension = UiAppStyleSelectList_Properties.GetExtension(__instance);

            var def = UiAppStyleSelectList_Properties.GetGirl(__instance).girlDefinition;

            if (def == null)
            {
                return;
            }

            //unsub
            UiAppStyleSelectList_Properties.UnhookEvents(__instance);

            var itemTotal = __instance.alternative
                ? def.outfits.Count
                : def.hairstyles.Count;

            var diff = itemTotal - __instance.listItems.Count;

            if (diff > 0)
            {
                // add missing
                var parent = __instance.transform.Find($"{__instance.name}Scroll/{__instance.name}Padding/ListItemContainer");

                for (var i = diff; i > 0; i--)
                {
                    var newItem = UnityEngine.Object.Instantiate(extension.ListItemTemplate);
                    newItem.rectTransform.SetParent(parent, true);

                    __instance.listItems.Add(newItem);
                }
            }
            else if (diff < 0)
            {
                //remove extras
                for (var i = diff; i < 0; i++)
                {
                    var old = __instance.listItems.Last();
                    __instance.listItems.RemoveAt(__instance.listItems.Count - 1);
                    UnityEngine.Object.Destroy(old.gameObject);
                }
            }

            //resub
            UiAppStyleSelectList_Properties.RehookEvents(__instance);
        }

        public static void Postfix(UiAppStyleSelectList __instance)
        {
            var extension = UiAppStyleSelectList_Properties.GetExtension(__instance);
            if (!extension.Started) { return; }

            extension.ScrollRectTransform.sizeDelta = __instance.background.sizeDelta - new Vector2(24, 42);

            var purchaseItems = new List<UiAppSelectListItem>();
            var shownItems = new List<UiAppSelectListItem>();
            var hiddenItems = new List<UiAppSelectListItem>();

            var i = 0;
            var visibleItemCount = 0;
            foreach (var item in __instance.listItems)
            {
                if ((bool)UiAppStyleSelectList_Properties.item_hidden.GetValue(item))
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

            extension.ItemContainerRectTransform.sizeDelta = new Vector2(278, 20 + 33.3333f * visibleItemCount);

            // reposition in order
            i = 0;
            foreach (var item in shownItems.Concat(purchaseItems).Concat(hiddenItems))
            {
                item.transform.localPosition = i++ * UiAppStyleSelectList_Properties._itemSpacing;
            }

            //fix bg
            if (Game.Persistence.playerFile.storyProgress < 14)
            {
                var origPos = (Vector2)UiAppStyleSelectList_Properties._origPos.GetValue(__instance);
                var origBgSize = (Vector2)UiAppStyleSelectList_Properties._origBgSize.GetValue(__instance);
                var num = 4;//they all have 1 code and 3 purchase items, so I'll just manually set it
                //it'd be weird if random ones just started changing sizes

                __instance.rectTransform.anchoredPosition = origPos + Vector2.down * (float)(26 * num);
                __instance.background.sizeDelta = origBgSize + Vector2.down * (float)(40 * num);
                __instance.canvasGroup.alpha = 0f;
                __instance.canvasGroup.blocksRaycasts = false;
            }
        }

        // public static void TestRefresh(UiAppStyleSelectList __instance)
        // {
        //     ModInterface.Log.LogInfo();

        //     UiAppStyleSelectList_Properties._purchaseListItem.SetValue(__instance, null);
        //     var playerFileGirl = UiAppStyleSelectList_Properties.GetGirl(__instance);
        //     var purchaseListItem = (UiAppSelectListItem)UiAppStyleSelectList_Properties._purchaseListItem.GetValue(__instance);

        //     bool flag = Game.Persistence.playerFile.storyProgress >= 14;
        //     int num = 0;
        //     for (int i = 0; i < __instance.listItems.Count; i++)
        //     {
        //         ModInterface.Log.LogInfo($"list item {i}");

        //         string text = ((!__instance.alternative) ? playerFileGirl.girlDefinition.hairstyles[i].hairstyleName : playerFileGirl.girlDefinition.outfits[i].outfitName);
        //         bool flag2 = ((!__instance.alternative) ? playerFileGirl.IsHairstyleUnlocked(i) : playerFileGirl.IsOutfitUnlocked(i));
        //         bool flag3 = false;
        //         int num2 = ((!__instance.alternative) ? playerFileGirl.hairstyleIndex : playerFileGirl.outfitIndex);

        //         if (!flag2)
        //         {
        //             text = "???";
        //             if (flag)
        //             {
        //                 if (Game.Session.Hub.unlockStylesCode.Contains(i))
        //                 {
        //                     text = "Unlock With Code";
        //                 }
        //                 else if (Game.Session.Hub.unlockStylesBuy.Contains(i) && purchaseListItem == null)
        //                 {
        //                     text = "Purchase:";
        //                     UiAppStyleSelectList_Properties._purchaseListItem.SetValue(__instance, __instance.listItems[i]);
        //                 }
        //             }
        //             else if (Game.Session.Hub.unlockStylesCode.Contains(i) || Game.Session.Hub.unlockStylesBuy.Contains(i))
        //             {
        //                 flag3 = true;
        //             }
        //         }

        //         __instance.listItems[i].Populate(flag2, text, flag3);

        //         if (flag3)
        //         {
        //             num++;
        //         }

        //         if (i == num2)
        //         {
        //             UiAppStyleSelectList_Properties._selectedListItem.SetValue(__instance, __instance.listItems[i]);
        //             __instance.listItems[i].Select(true);
        //         }

        //         else
        //         {
        //             __instance.listItems[i].Select(false);
        //         }

        //         ModInterface.Log.LogInfo($"text:{text}, flag:{flag}, flag2:{flag2}, flag3:{flag3}, num:{num}, num2:{num2}");
        //     }

        //     purchaseListItem = (UiAppSelectListItem)UiAppStyleSelectList_Properties._purchaseListItem.GetValue(__instance);
        //     if (purchaseListItem != null)
        //     {
        //         FruitCategoryInfo fruitCategoryInfo = Game.Session.Gift.GetFruitCategoryInfo((!__instance.alternative) ? playerFileGirl.girlDefinition.leastFavoriteAffectionType : playerFileGirl.girlDefinition.favoriteAffectionType);
        //         int num3 = ((!__instance.alternative) ? Game.Session.Hub.buyCostHairstyles[Game.Session.Hub.unlockStylesBuy.IndexOf(__instance.listItems.IndexOf(purchaseListItem))] : Game.Session.Hub.buyCostOutfits[Game.Session.Hub.unlockStylesBuy.IndexOf(__instance.listItems.IndexOf(purchaseListItem))]);
        //         if (Game.Persistence.playerFile.settingDifficulty == SettingDifficulty.EASY)
        //         {
        //             num3 = Mathf.FloorToInt((float)num3 * 1.5f);
        //         }
        //         else if (Game.Persistence.playerFile.settingDifficulty == SettingDifficulty.HARD)
        //         {
        //             num3 = Mathf.CeilToInt((float)num3 * 0.5f);
        //         }
        //         purchaseListItem.ShowCost(fruitCategoryInfo, num3);
        //         if (Game.Persistence.playerFile.GetFruitCount(fruitCategoryInfo.affectionType) >= num3)
        //         {
        //             __instance.buyButton.Enable();
        //         }
        //         else
        //         {
        //             __instance.buyButton.Disable();
        //         }
        //     }
        //     else
        //     {
        //         __instance.buyButton.Disable();
        //     }

        //     ModInterface.Log.LogInfo($"down here with the rect stuff");
        //     var origPos = (Vector2)UiAppStyleSelectList_Properties._origPos.GetValue(__instance);
        //     var origBgSize = (Vector2)UiAppStyleSelectList_Properties._origBgSize.GetValue(__instance);

        //     if (flag)
        //     {
        //         __instance.rectTransform.anchoredPosition = origPos;
        //         __instance.background.sizeDelta = origBgSize;
        //         __instance.canvasGroup.alpha = 1f;
        //         __instance.canvasGroup.blocksRaycasts = true;
        //         return;
        //     }
        //     __instance.rectTransform.anchoredPosition = origPos + Vector2.down * (float)(26 * num);
        //     __instance.background.sizeDelta = origBgSize + Vector2.down * (float)(40 * num);
        //     __instance.canvasGroup.alpha = 0f;
        //     __instance.canvasGroup.blocksRaycasts = false;
        //     ModInterface.Log.LogInfo($"end");
        // }
    }
}