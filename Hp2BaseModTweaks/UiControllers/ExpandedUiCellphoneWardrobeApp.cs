using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    internal class ExpandedUiCellphoneWardrobeApp : IUiController
    {
        private static readonly FieldInfo _uiCellphoneWardrobeApp_selectedFileIconSlot = AccessTools.Field(typeof(UiCellphoneAppWardrobe), "_selectedFileIconSlot");
        private static readonly int _girlsPerPage = 12;
        public static readonly int _wardrobeStylesPerPage = 10;

        private int _girlsPage;

        private readonly Hp2ButtonWrapper _girlsLeft;
        private readonly Hp2ButtonWrapper _girlsRight;

        private readonly UiCellphoneAppWardrobe _wardrobeApp;
        private readonly int _girlsPageMax;
        private readonly PlayerFileGirl[] _metGirls;
        private readonly UiAppFileIconSlot _dummyFileIconSlot;

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
                    PostRefresh();
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
                    PostRefresh();
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

            PreRefresh();
            PostRefresh();
        }


        public void PreRefresh()
        {
            ModInterface.Log.LogInfo("Wardrobe pre refresh");
            ModInterface.Log.LogInfo();

            if (_uiCellphoneWardrobeApp_selectedFileIconSlot.GetValue(_wardrobeApp) is not GirlDefinition wardrobeGirlDef)
            {
                var wardrobeGirlId = Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id");
                wardrobeGirlDef = Game.Data.Girls.Get(wardrobeGirlId);
            }

            ModInterface.Log.LogInfo($"Selecting wardrobe girl {wardrobeGirlDef.name}");
        }

        public void PostRefresh()
        {
            ModInterface.Log.LogInfo();
            // head slots
            UiAppFileIconSlot selectedFileIconSlot = null;
            var iconIndex = _girlsPage * _girlsPerPage;
            int renderedCount = 0;
            var wardrobeGirlId = Game.Persistence.playerFile.GetFlagValue("wardrobe_girl_id");

            foreach (var slot in _wardrobeApp.fileIconSlots.Take(_girlsPerPage))
            {
                if (iconIndex < _metGirls.Length)
                {
                    slot.button.Disable();
                    slot.girlDefinition = _metGirls[iconIndex].girlDefinition;
                    slot.Populate(false);
                    slot.canvasGroup.blocksRaycasts = true;
                    slot.canvasGroup.alpha = 1f;
                    //slot.rectTransform.anchoredPosition = new Vector2((float)(renderedCount % 3) * 120f, (float)Mathf.FloorToInt((float)renderedCount / 3f) * -120f);

                    if (slot.girlDefinition.id == wardrobeGirlId)
                    {
                        selectedFileIconSlot = slot;
                    }
                    else
                    {
                        slot.button.Enable();
                    }

                    renderedCount++;
                    iconIndex++;
                }
                else
                {
                    //slot.girlDefinition = null;
                    slot.Populate(true);
                    slot.canvasGroup.blocksRaycasts = false;
                    slot.canvasGroup.alpha = 0f;
                }
            }

            foreach (var slot in _wardrobeApp.fileIconSlots.Skip(_girlsPerPage))
            {
                slot.Populate(true);
                slot.canvasGroup.blocksRaycasts = false;
                slot.canvasGroup.alpha = 0f;
            }

            // if the selected slot is on a different page, use the dummy slot.
            var wardrobeGirlDef = Game.Data.Girls.Get(wardrobeGirlId);
            if (selectedFileIconSlot == null)
            {
                _dummyFileIconSlot.girlDefinition = wardrobeGirlDef;
                _uiCellphoneWardrobeApp_selectedFileIconSlot.SetValue(_wardrobeApp, _dummyFileIconSlot);
            }
            else
            {
                _uiCellphoneWardrobeApp_selectedFileIconSlot.SetValue(_wardrobeApp, selectedFileIconSlot);
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