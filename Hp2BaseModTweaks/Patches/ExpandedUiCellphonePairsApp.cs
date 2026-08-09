using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppPairs))]
    internal class UiCellphoneAppPairsPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(UiCellphoneAppPairs __instance)
            => ExpandedUiCellphoneAppPairs.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppPairs __instance)
            => ExpandedUiCellphoneAppPairs.Destroy(__instance);
    }

    [Expansion(typeof(UiCellphoneAppPairs))]
    public partial class ExpandedUiCellphoneAppPairs
    {
        private const int PAIRS_PER_PAGE = 24;

        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;
        private int _pageMax;
        private GirlPairDefinition[] _metPairs;

        private void OnInit()
        {
            _metPairs = Game.Persistence.playerFile.metGirlPairs.ToArray();

            _pageMax = _metPairs.Length > 1
                ? (_metPairs.Length - 1) / PAIRS_PER_PAGE
                : 0;

            // no need for extra ui
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

                _previousPage.GameObject.transform.SetParent(_core.transform, false);
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

                _nextPage.GameObject.transform.SetParent(_core.transform, false);
                _nextPage.RectTransform.anchoredPosition = new Vector2(1024, -30);
                _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _currentPage++;
                    Refresh();
                };
            }

            Refresh();
        }

        internal void Start_Postfix()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            _previousPage?.Destroy();
            _nextPage?.Destroy();
            _expansions.Remove(_core);
        }

        public void Refresh()
        {
            // pairs
            var renderCount = 0;

            var current = _currentPage * PAIRS_PER_PAGE;

            foreach (var entry in _core.pairSlots.Take(PAIRS_PER_PAGE))
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

            foreach (var entry in _core.pairSlots.Skip(PAIRS_PER_PAGE))
            {
                entry.Populate(null, null);
            }

            _core.pairSlotsContainer.anchoredPosition = new Vector2(528 + (Mathf.Min(renderCount - 1, 3) * -128f),
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
