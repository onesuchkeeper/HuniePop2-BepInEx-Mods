using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppWardrobe))]
    public static class UiCellphoneAppWardrobePatch
    {
        private readonly static Dictionary<UiCellphoneAppWardrobe, ExpandedUiCellphoneWardrobeApp> _extendedApps
                            = new Dictionary<UiCellphoneAppWardrobe, ExpandedUiCellphoneWardrobeApp>();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppWardrobe __instance)
        {
            if (!_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp = new ExpandedUiCellphoneWardrobeApp(__instance);
                _extendedApps[__instance] = extendedApp;
            }

            extendedApp.OnStart();
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void PreDestroy(UiCellphoneAppWardrobe __instance)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.OnDestroy();
                _extendedApps.Remove(__instance);
            }
        }

        [HarmonyPatch("Refresh")]
        [HarmonyPostfix]
        public static void PostRefresh(UiCellphoneAppWardrobe __instance)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.Refresh();
            }
        }
    }

    internal class ExpandedUiCellphoneWardrobeApp
    {
        private static readonly FieldInfo _selectedFileIconSlot = AccessTools.Field(typeof(UiCellphoneAppWardrobe), "_selectedFileIconSlot");
        private static readonly int _girlsPerPage = 12;

        private int _girlsPage;

        private readonly Hp2ButtonWrapper _girlsLeft;
        private readonly Hp2ButtonWrapper _girlsRight;

        private readonly UiCellphoneAppWardrobe _wardrobeApp;
        private readonly int _girlsPageMax;
        private readonly PlayerFileGirl[] _metGirls;
        private readonly UiAppFileIconSlot _dummyFileIconSlot;

        private UiAppFileIconSlot[] _subscribedSlots;

        public ExpandedUiCellphoneWardrobeApp(UiCellphoneAppWardrobe wardrobeApp)
        {
            _wardrobeApp = wardrobeApp ?? throw new ArgumentNullException(nameof(wardrobeApp));

            _metGirls = Game.Persistence.playerFile.girls.Where(x => x.playerMet).OrderBy(x => x.girlDefinition.id).ToArray();

            _girlsPageMax = _metGirls.Length > 1
                ? (_metGirls.Length - 1) / _girlsPerPage
                : 0;

            if (_girlsPageMax > 0)
            {
                // Buttons
                var cellphoneButtonPressedKlip = new AudioKlip()
                {
                    clip = ModInterface.Assets.GetAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                    volume = 1f
                };

                _girlsLeft = Hp2ButtonWrapper.MakeCellphoneButton("GirlsLeft",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                    cellphoneButtonPressedKlip);

                _girlsLeft.GameObject.transform.SetParent(_wardrobeApp.transform, false);
                _girlsLeft.RectTransform.anchoredPosition = new Vector2(30, -30);
                _girlsLeft.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _girlsPage--;
                    Refresh();
                };

                _girlsRight = Hp2ButtonWrapper.MakeCellphoneButton("GirlsRight",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                    cellphoneButtonPressedKlip);

                _girlsRight.GameObject.transform.SetParent(_wardrobeApp.transform, false);
                _girlsRight.RectTransform.anchoredPosition = new Vector2(356, -30);
                _girlsRight.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _girlsPage++;
                    Refresh();
                };

                _dummyFileIconSlot = new UiAppFileIconSlot() { button = new ButtonBehavior() };

                //shift other stuff down a bit for the buttons to better fit
                _wardrobeApp.transform.Find("FileIconSlotsContainer").GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 32);
                _wardrobeApp.transform.Find("WearOnDatesCheckBox").GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 16);

                //go to correct page
                var wardrobeGirlId = Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id");
                var index = 0;

                foreach (var girl in _metGirls)
                {
                    if (girl.girlDefinition.id == wardrobeGirlId)
                    {
                        break;
                    }

                    index++;
                }

                _girlsPage = index / _girlsPerPage;
            }
        }

        public void OnStart()
        {
            //only slots where the player has met the girldef are subscribed to
            _subscribedSlots = _wardrobeApp.fileIconSlots.Where(x => Game.Persistence.playerFile.GetPlayerFileGirl(x.girlDefinition).playerMet).ToArray();
            Refresh();
        }

        public void OnDestroy()
        {
            _girlsLeft?.Destroy();
            _girlsRight?.Destroy();
            UnityEngine.Object.Destroy(_dummyFileIconSlot);
        }

        public void Refresh()
        {
            // head slots
            UiAppFileIconSlot selectedFileIconSlot = null;
            var iconIndex = _girlsPage * _girlsPerPage;
            int renderedCount = 0;
            var wardrobeGirlId = Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id");

            foreach (var slot in _subscribedSlots.Take(_girlsPerPage))
            {
                if (iconIndex < _metGirls.Length)
                {
                    slot.button.Disable();
                    slot.girlDefinition = _metGirls[iconIndex++].girlDefinition;
                    slot.Populate(false);
                    slot.canvasGroup.blocksRaycasts = true;
                    slot.canvasGroup.alpha = 1f;
                    slot.rectTransform.anchoredPosition = new Vector2((renderedCount % 3) * 120f, Mathf.FloorToInt(renderedCount / 3f) * -120f);

                    if (slot.girlDefinition.id == wardrobeGirlId)
                    {
                        selectedFileIconSlot = slot;
                    }
                    else
                    {
                        slot.button.Enable();
                    }

                    renderedCount++;
                }
                else
                {
                    slot.hideIfBlocked = true;
                    slot.girlDefinition = null;
                    slot.Populate(true);
                }
            }

            foreach (var slot in _subscribedSlots.Skip(_girlsPerPage))
            {
                slot.hideIfBlocked = true;
                slot.girlDefinition = null;
                slot.Populate(true);
            }

            // if the selected slot is on a different page, use the dummy slot
            var wardrobeGirlDef = Game.Data.Girls.Get(wardrobeGirlId);
            if (selectedFileIconSlot == null)
            {
                _dummyFileIconSlot.girlDefinition = wardrobeGirlDef;
                _selectedFileIconSlot.SetValue(_wardrobeApp, _dummyFileIconSlot);
            }
            else
            {
                _selectedFileIconSlot.SetValue(_wardrobeApp, selectedFileIconSlot);
            }

            //buttons
            if (_girlsPageMax > 0)
            {
                if (_girlsPage <= 0)
                {
                    _girlsPage = 0;
                    _girlsLeft.ButtonBehavior.Disable();
                }
                else
                {
                    _girlsLeft.ButtonBehavior.Enable();
                }

                if (_girlsPage >= _girlsPageMax)
                {
                    _girlsPage = _girlsPageMax;
                    _girlsRight.ButtonBehavior.Disable();
                }
                else
                {
                    _girlsRight.ButtonBehavior.Enable();
                }
            }
        }
    }
}
