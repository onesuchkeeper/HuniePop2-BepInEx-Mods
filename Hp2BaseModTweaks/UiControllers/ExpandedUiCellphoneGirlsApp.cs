using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppGirls))]
    public static class UiCellphoneGirlsAppPatch
    {
        private readonly static Dictionary<UiCellphoneAppGirls, ExpandedUiCellphoneGirlsApp> _extendedApps
            = new Dictionary<UiCellphoneAppGirls, ExpandedUiCellphoneGirlsApp>();

        public static Vector2 DefaultSlotContainerPos;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreStart(UiCellphoneAppGirls __instance)
        {
            DefaultSlotContainerPos = __instance.girlSlotsContainer.anchoredPosition;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppGirls __instance)
        {
            if (!_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp = new ExpandedUiCellphoneGirlsApp(__instance);
                _extendedApps[__instance] = extendedApp;
            }

            extendedApp.OnStart();
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void PreDestroy(UiCellphoneAppGirls __instance)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.OnDestroy();
                _extendedApps.Remove(__instance);
            }
        }
    }

    internal class ExpandedUiCellphoneGirlsApp
    {
        private static readonly int _girlsPerPage = 12;

        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;
        private readonly int _pageMax;

        private readonly UiCellphoneAppGirls _girlsApp;
        private readonly PlayerFileGirl[] _playerFileGirls;

        public ExpandedUiCellphoneGirlsApp(UiCellphoneAppGirls girlsApp)
        {
            _girlsApp = girlsApp ?? throw new ArgumentNullException(nameof(girlsApp));

            _playerFileGirls = Game.Persistence.playerFile.girls.Where(x => x.playerMet).OrderBy(x => x.girlDefinition.id).ToArray();

            _pageMax = _playerFileGirls.Length > 1 ? (_playerFileGirls.Length - 1) / _girlsPerPage : 0;

            // extra ui
            if (_pageMax != 0)
            {
                var cellphoneButtonPressedKlip = new AudioKlip()
                {
                    clip = ModInterface.Assets.GetAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                    volume = 1f
                };

                _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                    cellphoneButtonPressedKlip);

                _previousPage.GameObject.transform.SetParent(girlsApp.transform, false);
                _previousPage.RectTransform.anchoredPosition = new Vector2(30, -30);
                _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _currentPage--;
                    Refresh();
                };

                _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                    cellphoneButtonPressedKlip);

                _nextPage.GameObject.transform.SetParent(girlsApp.transform, false);
                _nextPage.RectTransform.anchoredPosition = new Vector2(1024, -30);
                _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _currentPage++;
                    Refresh();
                };
            }

            Refresh();
        }

        public void OnStart()
        {
            Refresh();
        }

        public void OnDestroy()
        {
            _previousPage?.Destroy();
            _nextPage?.Destroy();
        }

        public void Refresh()
        {
            //girls
            var girlIndex = _currentPage * _girlsPerPage;
            var renderCount = 0;

            foreach (var slot in _girlsApp.girlSlots.Take(_girlsPerPage))
            {
                if (girlIndex < _playerFileGirls.Length)
                {
                    slot.girlDefinition = _playerFileGirls[girlIndex++].girlDefinition;
                    slot.rectTransform.anchoredPosition = new Vector2((float)(renderCount % 6) * 172f,
                        Mathf.FloorToInt(renderCount / 6f) * -272f);
                    slot.Populate();

                    renderCount++;
                }
                else
                {
                    slot.Clear();
                }
            }

            foreach (var slot in _girlsApp.girlSlots.Skip(_girlsPerPage))
            {
                slot.Clear();
            }

            _girlsApp.girlSlotsContainer.anchoredPosition = UiCellphoneGirlsAppPatch.DefaultSlotContainerPos
                + new Vector2(Mathf.Min(renderCount - 1, 5) * -86f,
                    Mathf.Max(Mathf.CeilToInt(renderCount / 6f) - 1, 0) * 136f);

            if (_pageMax == 0)
            {
                return;
            }

            //buttons
            if (_currentPage <= 0)
            {
                _currentPage = 0;
                _previousPage.ButtonBehavior.Disable();
            }
            else
            {
                _previousPage.ButtonBehavior.Enable();
            }

            if (_currentPage >= _pageMax)
            {
                _currentPage = _pageMax;
                _nextPage.ButtonBehavior.Disable();
            }
            else
            {
                _nextPage.ButtonBehavior.Enable();
            }
        }
    }
}
