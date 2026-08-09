using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppGirls))]
    internal static class UiCellphoneGirlsAppPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreStart(UiCellphoneAppGirls __instance)
            => ExpandedUiCellphoneAppGirls.Get(__instance).PreStart();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppGirls __instance)
            => ExpandedUiCellphoneAppGirls.Get(__instance).PostStart();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppGirls __instance)
            => ExpandedUiCellphoneAppGirls.Destroy(__instance);
    }

    [Expansion(typeof(UiCellphoneAppGirls))]
    public partial class ExpandedUiCellphoneAppGirls
    {
        private const int GIRLS_PER_PAGE = 12;

        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;
        public static Vector2 _defaultSlotContainerPos;
        private int _pageMax;
        private PlayerFileGirl[] _playerFileGirls;

        internal void PreStart()
        {
            _defaultSlotContainerPos = _core.girlSlotsContainer.anchoredPosition;
        }

        internal void PostStart()
        {
            _playerFileGirls = Game.Persistence.playerFile.girls
                .Where(x => x.playerMet && !x.girlDefinition.specialCharacter)
                .OrderBy(x => x.girlDefinition.id)
                .ToArray();

            _pageMax = _playerFileGirls.Length > 1
                ? (_playerFileGirls.Length - 1) / GIRLS_PER_PAGE
                : 0;

            // extra ui
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

        private void OnDestroy()
        {
            _previousPage?.Destroy();
            _nextPage?.Destroy();
        }

        public void Refresh()
        {
            //girls
            var girlIndex = _currentPage * GIRLS_PER_PAGE;
            var renderCount = 0;

            foreach (var slot in _core.girlSlots.Take(GIRLS_PER_PAGE))
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

            foreach (var slot in _core.girlSlots.Skip(GIRLS_PER_PAGE))
            {
                slot.Clear();
            }

            _core.girlSlotsContainer.anchoredPosition = _defaultSlotContainerPos
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
