using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppFinder))]
    internal static class UiCellphoneAppFinderPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppFinder __instance)
            => ExpandedUiCellphoneFinderApp.Get(__instance).OnStart();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppFinder __instance)
            => ExpandedUiCellphoneFinderApp.Get(__instance).OnDestroy();
    }

    internal class ExpandedUiCellphoneFinderApp
    {
        private readonly static Dictionary<UiCellphoneAppFinder, ExpandedUiCellphoneFinderApp> _expansions
                    = new Dictionary<UiCellphoneAppFinder, ExpandedUiCellphoneFinderApp>();

        public static ExpandedUiCellphoneFinderApp Get(UiCellphoneAppFinder uiCellphoneAppFinder)
        {
            if (!_expansions.TryGetValue(uiCellphoneAppFinder, out var expansion))
            {
                expansion = new ExpandedUiCellphoneFinderApp(uiCellphoneAppFinder);
                _expansions[uiCellphoneAppFinder] = expansion;
            }

            return expansion;
        }

        private static readonly int _finderLocationsPerPage = 8;
        private static readonly FieldInfo f_playerFileFinderSlotAccess = AccessTools.Field(typeof(UiAppFinderSlot), "_playerFileFinderSlot");
        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;
        private int _pageMax;
        private LocationDefinition[] _simLocations;

        private readonly UiCellphoneAppFinder _finderApp;

        public ExpandedUiCellphoneFinderApp(UiCellphoneAppFinder finderApp)
        {
            _finderApp = finderApp ?? throw new ArgumentNullException(nameof(finderApp));
        }

        public void OnStart()
        {
            _simLocations = Game.Data.Locations.GetAll().Where(x => x.locationType == LocationType.SIM).ToArray();

            _pageMax = _simLocations.Length > 1
                ? (_simLocations.Length - 1) / _finderLocationsPerPage
                : 0;

            if (_pageMax != 0)
            {
                var cellphoneButtonPressedKlip = new AudioKlip()
                {
                    clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                    volume = 1f
                };

                _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                    cellphoneButtonPressedKlip);

                _previousPage.GameObject.transform.SetParent(_finderApp.transform, false);
                _previousPage.RectTransform.anchoredPosition = new Vector2(30, -30);
                _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _currentPage--;
                    Refresh();
                };

                _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                    cellphoneButtonPressedKlip);

                _nextPage.GameObject.transform.SetParent(_finderApp.transform, false);
                _nextPage.RectTransform.anchoredPosition = new Vector2(1024, -30);
                _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _currentPage++;
                    Refresh();
                };
            }

            Refresh();
        }

        public void OnDestroy()
        {
            _previousPage?.Destroy();
            _nextPage?.Destroy();
            _expansions.Remove(_finderApp);
        }

        public void Refresh()
        {
            //location slots
            var locationIndex = _currentPage * _finderLocationsPerPage;

            foreach (var slot in _finderApp.finderSlots.Take(_finderLocationsPerPage))
            {
                if (locationIndex < _simLocations.Length)
                {
                    slot.canvasGroup.alpha = 1f;
                    slot.canvasGroup.blocksRaycasts = true;
                    slot.locationDefinition = _simLocations[locationIndex];
                    slot.Populate(true);

                    if (Game.Persistence.playerFile.GetPlayerFileFinderSlot(slot.locationDefinition)?.girlPairDefinition == null
                        || (slot.locationDefinition.locationType == LocationType.SIM
                            && slot.locationDefinition == Game.Session.Location.currentLocation))
                    {
                        slot.headSlotLeft.canvasGroup.alpha = 0.25f;
                        slot.headSlotRight.canvasGroup.alpha = 0.25f;
                        slot.relationshipSlot.canvasGroup.alpha = 0.25f;

                        slot.headSlotLeft.canvasGroup.blocksRaycasts = false;
                        slot.headSlotRight.canvasGroup.blocksRaycasts = false;
                        slot.relationshipSlot.canvasGroup.blocksRaycasts = false;

                        slot.locationIcon.color = ColorUtils.ColorAlpha(slot.locationIcon.color, 0.5f);
                        slot.locationLabel.color = ColorUtils.ColorAlpha(slot.locationLabel.color, 0.4f);
                        slot.locationButton.Disable();
                    }
                    else
                    {
                        slot.headSlotLeft.canvasGroup.alpha = 1f;
                        slot.headSlotRight.canvasGroup.alpha = 1f;
                        slot.relationshipSlot.canvasGroup.alpha = 1f;

                        slot.headSlotLeft.canvasGroup.blocksRaycasts = true;
                        slot.headSlotRight.canvasGroup.blocksRaycasts = true;
                        slot.relationshipSlot.canvasGroup.blocksRaycasts = true;

                        slot.locationIcon.color = ColorUtils.ColorAlpha(slot.locationIcon.color, 1f);
                        slot.locationLabel.color = ColorUtils.ColorAlpha(slot.locationLabel.color, 1f);
                        slot.locationButton.Enable();
                    }

                    locationIndex++;
                }
                else
                {
                    slot.canvasGroup.alpha = 0f;
                    slot.canvasGroup.blocksRaycasts = false;
                }
            }

            foreach (var slot in _finderApp.finderSlots.Skip(_finderLocationsPerPage))
            {
                slot.canvasGroup.alpha = 0f;
                slot.canvasGroup.blocksRaycasts = false;
            }

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
