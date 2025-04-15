using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppPairs))]
    public class UiCellphoneAppPairsPatch
    {
        private readonly static Dictionary<UiCellphoneAppPairs, ExpandedUiCellphonePairsApp> _extendedApps
                    = new Dictionary<UiCellphoneAppPairs, ExpandedUiCellphonePairsApp>();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppPairs __instance)
        {
            if (!_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp = new ExpandedUiCellphonePairsApp(__instance);
                _extendedApps[__instance] = extendedApp;
            }

            extendedApp.OnStart();
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void PreDestroy(UiCellphoneAppPairs __instance)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.OnDestroy();
                _extendedApps.Remove(__instance);
            }
        }
    }

    internal class ExpandedUiCellphonePairsApp
    {
        private static readonly int _pairsPerPage = 24;

        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;
        private readonly int _pageMax;

        private readonly UiCellphoneAppPairs _pairsApp;
        private readonly GirlPairDefinition[] _metPairs;

        public ExpandedUiCellphonePairsApp(UiCellphoneAppPairs pairsApp)
        {
            _pairsApp = pairsApp ?? throw new ArgumentNullException(nameof(pairsApp));
            _metPairs = Game.Persistence.playerFile.metGirlPairs.ToArray();

            _pageMax = _metPairs.Length > 1 ? (_metPairs.Length - 1) / _pairsPerPage : 0;

            // no need for extra ui
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

                _previousPage.GameObject.transform.SetParent(pairsApp.transform, false);
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

                _nextPage.GameObject.transform.SetParent(pairsApp.transform, false);
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
            // pairs
            var renderCount = 0;

            var current = _currentPage * _pairsPerPage;

            foreach (var entry in _pairsApp.pairSlots.Take(_pairsPerPage))
            {
                if (current < _metPairs.Length)
                {
                    entry.Populate(_metPairs[current++]);
                    entry.canvasGroup.alpha = 1f;
                    entry.canvasGroup.blocksRaycasts = true;
                    entry.button.Enable();
                    entry.rectTransform.anchoredPosition = new Vector2(renderCount % 4 * 256f,
                        Mathf.FloorToInt(renderCount / 4f) * -90f);

                    renderCount++;
                }
                else
                {
                    entry.Populate(null, null);
                }
            }

            foreach (var entry in _pairsApp.pairSlots.Skip(_pairsPerPage))
            {
                entry.Populate(null, null);
            }

            _pairsApp.pairSlotsContainer.anchoredPosition = new Vector2(528 + (Mathf.Min(renderCount - 1, 3) * -128f),
                -284 + (Mathf.Max(Mathf.CeilToInt(renderCount / 4f) - 1, 0) * 45f));

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
