using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppWardrobe))]
    internal static class UiCellphoneAppWardrobePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreStart(UiCellphoneAppWardrobe __instance)
            => ExpandedUiCellphoneWardrobeApp.Get(__instance).PreStart();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppWardrobe __instance)
            => ExpandedUiCellphoneWardrobeApp.Get(__instance).OnDestroy();

        [HarmonyPatch("Refresh")]
        [HarmonyPostfix]
        public static void Refresh(UiCellphoneAppWardrobe __instance)
            => ExpandedUiCellphoneWardrobeApp.Get(__instance).Refresh();
    }

    internal class ExpandedUiCellphoneWardrobeApp
    {
        private readonly static Dictionary<UiCellphoneAppWardrobe, ExpandedUiCellphoneWardrobeApp> _expansions
            = new Dictionary<UiCellphoneAppWardrobe, ExpandedUiCellphoneWardrobeApp>();

        public static ExpandedUiCellphoneWardrobeApp Get(UiCellphoneAppWardrobe uiCellphoneAppWardrobe)
        {
            if (!_expansions.TryGetValue(uiCellphoneAppWardrobe, out var expansion))
            {
                expansion = new ExpandedUiCellphoneWardrobeApp(uiCellphoneAppWardrobe);
                _expansions[uiCellphoneAppWardrobe] = expansion;
            }

            return expansion;
        }

        private static readonly FieldInfo f_selectedFileIconSlot = AccessTools.Field(typeof(UiCellphoneAppWardrobe), "_selectedFileIconSlot");
        private static readonly FieldInfo f_wardrobeDoll = AccessTools.Field(typeof(UiCellphoneAppWardrobe), "_wardrobeDoll");

        private static readonly FieldInfo f_image = AccessTools.Field(typeof(ButtonBehavior), "_image");
        private static readonly MethodInfo m_onListItemSelected = AccessTools.Method(typeof(UiCellphoneAppWardrobe), "OnListItemSelected");

        private const int GIRLS_PER_PAGE = 12;

        private UiAppCheckBox _randomizeStylesCheckBox;
        private UiAppCheckBox _unpairRandomizeStylesCheckBox;
        private UiAppCheckBox _allowNsfwCheckBox;
        private UiCellphoneBodySelector _bodySelector;

        private Hp2ButtonWrapper _girlsLeft;
        private Hp2ButtonWrapper _girlsRight;
        private int _girlsPageIndex;
        private int _girlsPageMax;

        private RectTransform _rightLayout_rt;

        private PlayerFileGirl[] _metGirls;
        private UiAppFileIconSlot _dummyFileIconSlot;
        private UiAppFileIconSlot[] _subscribedSlots;
        private bool _started;
        private UiCellphoneAppWardrobe _wardrobeApp;
        private TweaksSaveGirl _girlSave;
        private int _initialWardrobeGirlId;

        public ExpandedUiCellphoneWardrobeApp(UiCellphoneAppWardrobe wardrobeApp)
        {
            _wardrobeApp = wardrobeApp ?? throw new ArgumentNullException(nameof(wardrobeApp));
        }

        public void PreStart()
        {
            //If the initial girl is modded it is not handled properly, so default to lola
            //and repopulate afterwards
            _initialWardrobeGirlId = Game.Persistence.playerFile.GetFlagValue(Flags.WARDROBE_GIRL_ID);
            Game.Persistence.playerFile.SetFlagValue(Flags.WARDROBE_GIRL_ID, Girls.AshleyId.LocalId);
            _wardrobeApp.StartCoroutine(Start());
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();

            Game.Persistence.playerFile.SetFlagValue(Flags.WARDROBE_GIRL_ID, _initialWardrobeGirlId);
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(_initialWardrobeGirlId));
            _wardrobeApp.selectListHairstyle.Populate(playerFileGirl);
            _wardrobeApp.selectListOutfit.Populate(playerFileGirl);

            var dummySlot_go = new GameObject();
            var dummySlot_buttonBehavior = dummySlot_go.AddComponent<ButtonBehavior>();
            var dummySlot_image = dummySlot_go.AddComponent<Image>();
            f_image.SetValue(dummySlot_buttonBehavior, dummySlot_image);
            _dummyFileIconSlot = dummySlot_go.AddComponent<UiAppFileIconSlot>();
            _dummyFileIconSlot.button = dummySlot_buttonBehavior;
            f_selectedFileIconSlot.SetValue(_wardrobeApp, _dummyFileIconSlot);

            _metGirls = Game.Persistence.playerFile.girls.Where(x => x.playerMet).OrderBy(x => x.girlDefinition.id).ToArray();

            _girlsPageMax = _metGirls.Length > 1
                ? (_metGirls.Length - 1) / GIRLS_PER_PAGE
                : 0;

            // girl selector
            var cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            if (_girlsPageMax > 0)
            {
                _girlsLeft = Hp2ButtonWrapper.MakeCellphoneButton("GirlsLeft",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                    cellphoneButtonPressedKlip);

                _girlsLeft.GameObject.transform.SetParent(_wardrobeApp.transform, false);
                _girlsLeft.RectTransform.anchoredPosition = new Vector2(34, -34);
                _girlsLeft.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _girlsPageIndex--;
                    Refresh();
                };

                _girlsRight = Hp2ButtonWrapper.MakeCellphoneButton("GirlsRight",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                    cellphoneButtonPressedKlip);

                _girlsRight.GameObject.transform.SetParent(_wardrobeApp.transform, false);
                _girlsRight.RectTransform.anchoredPosition = new Vector2(352, -34);
                _girlsRight.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _girlsPageIndex++;
                    Refresh();
                };

                //go to correct page
                var index = 0;

                foreach (var girl in _metGirls)
                {
                    if (girl.girlDefinition.id == _initialWardrobeGirlId)
                    {
                        break;
                    }

                    index++;
                }

                _girlsPageIndex = index / GIRLS_PER_PAGE;
            }

            //shift other ui down a bit for the buttons to better fit
            _wardrobeApp.transform.Find("FileIconSlotsContainer").GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 16);
            var wearOnDatesCheckBox = _wardrobeApp.transform.Find("WearOnDatesCheckBox");
            wearOnDatesCheckBox.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 16);

            //right layout area
            var rightLayout_go = new GameObject();
            _rightLayout_rt = rightLayout_go.AddComponent<RectTransform>();
            _rightLayout_rt.SetParent(_wardrobeApp.transform, false);
            _rightLayout_rt.pivot = new Vector2(0.5f, 0.5f);
            _rightLayout_rt.sizeDelta = new Vector2(666, 566);
            _rightLayout_rt.anchoredPosition = new Vector2(723, -283);
            //rightLayout_go.AddComponent<Image>().color = Color.magenta;

            //right column
            var rightColumn_go = new GameObject();
            var rightColumn_rt = rightColumn_go.AddComponent<RectTransform>();
            rightColumn_rt.SetParent(_rightLayout_rt, false);
            rightColumn_rt.anchorMin = Vector2.zero;
            rightColumn_rt.anchorMax = new Vector2(1f, 0f);
            rightColumn_rt.sizeDelta = new Vector2(666, 0);
            rightColumn_rt.pivot = new Vector2(0.5f, 0f);
            rightColumn_rt.anchoredPosition = Vector2.zero;
            rightColumn_rt.sizeDelta = Vector2.zero;

            var rightColumn = rightColumn_go.AddComponent<VerticalLayoutGroup>();
            rightColumn.childAlignment = TextAnchor.MiddleCenter;
            rightColumn.childControlWidth = true;
            rightColumn.childControlHeight = true;
            rightColumn.childForceExpandWidth = false;
            rightColumn.childForceExpandHeight = true;
            rightColumn.spacing = 8f;

            var rightColumn_Fitter = rightColumn_go.AddComponent<ContentSizeFitter>();
            rightColumn_Fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rightColumn_Fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            //rightColumn_go.AddComponent<Image>().color = Color.blue;

            MakeStyleListContainer(rightColumn_rt);
            MakeRandomizeStyleUi(rightColumn_rt);

            //only slots where the player has met the girlDef are subscribed to
            _subscribedSlots = _wardrobeApp.fileIconSlots.Where(x => Game.Persistence.playerFile.GetPlayerFileGirl(x.girlDefinition).playerMet).ToArray();

            //sub to expansions
            ExpandedUiAppStyleSelectList.Get(_wardrobeApp.selectListOutfit).ListItemSelectedEvent += On_ListItemSelected;
            ExpandedUiAppStyleSelectList.Get(_wardrobeApp.selectListHairstyle).ListItemSelectedEvent += On_ListItemSelected;

            //body selection ui
            _bodySelector = UiCellphoneBodySelector.Create(wearOnDatesCheckBox.GetComponent<UiAppCheckBox>().valueLabel.font, cellphoneButtonPressedKlip);
            var bodySelectParent = Game.Session.gameCanvas.cellphoneContainer.Find("Cellphone");
            _bodySelector.transform.SetParent(bodySelectParent, false);
            _bodySelector.BodyChanged += On_BodySelector_BodyChanged;
            _bodySelector.transform.SetAsFirstSibling();
            _bodySelector.RectTransform.anchoredPosition = new Vector2(195, 616);

            _started = true;
            Refresh();
        }

        private void MakeRandomizeStyleUi(Transform parent)
        {
            _randomizeStylesCheckBox = CreateCheckbox("Randomize Styles");
            _randomizeStylesCheckBox.CheckBoxChangedEvent += On_RandomizeStylesCheckBox_CheckBoxChangedEvent;

            _unpairRandomizeStylesCheckBox = CreateCheckbox("Unpair Random Styles");
            _unpairRandomizeStylesCheckBox.CheckBoxChangedEvent += On_UnpairRandomizeStylesCheckBox_CheckBoxChangedEvent;

            _allowNsfwCheckBox = CreateCheckbox("NSFW Random Styles");
            _allowNsfwCheckBox.CheckBoxChangedEvent += On_AllowRandomNsfwStylesCheckBox_CheckBoxChangedEvent;

            //I tried so many times to get some kind of dynamic ui working or even just a static GridLayoutGroup 
            //but you have defeated me unity UI. You monster. I'mma just hard code it.
            var randomizeStyleContainer_go = new GameObject();

            var randomizeStyleContainer_rt = randomizeStyleContainer_go.AddComponent<RectTransform>();
            randomizeStyleContainer_rt.SetParent(parent, false);
            randomizeStyleContainer_rt.sizeDelta = new Vector2(666, 92);
            randomizeStyleContainer_rt.pivot = new Vector2(0.5f, 1f);
            randomizeStyleContainer_rt.anchorMin = new Vector2(0.5f, 1f);
            randomizeStyleContainer_rt.anchorMax = new Vector2(0.5f, 1f);

            var layoutElement = randomizeStyleContainer_go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = randomizeStyleContainer_rt.sizeDelta.x;
            layoutElement.preferredHeight = randomizeStyleContainer_rt.sizeDelta.y;

            _randomizeStylesCheckBox.rectTransform.SetParent(randomizeStyleContainer_rt, false);
            _randomizeStylesCheckBox.rectTransform.anchoredPosition = new Vector2(-309, 20);

            _unpairRandomizeStylesCheckBox.rectTransform.SetParent(randomizeStyleContainer_rt, false);
            _unpairRandomizeStylesCheckBox.rectTransform.anchoredPosition = new Vector2(16, 20);

            _allowNsfwCheckBox.rectTransform.SetParent(randomizeStyleContainer_rt, false);
            _allowNsfwCheckBox.rectTransform.anchoredPosition = new Vector2(-309, -20);

            //test
            //randomizeStyleContainer_go.AddComponent<Image>().color = Color.green;
        }

        private void MakeStyleListContainer(Transform parent)
        {
            var styleListLayout_GO = new GameObject();
            var styleListLayout_rt = styleListLayout_GO.AddComponent<RectTransform>();
            styleListLayout_GO.transform.SetParent(parent, false);

            styleListLayout_rt.anchorMin = Vector2.zero;
            styleListLayout_rt.anchorMax = Vector2.one;
            styleListLayout_rt.offsetMin = Vector2.one;
            styleListLayout_rt.offsetMax = Vector2.one;

            var styleListLayout = styleListLayout_GO.AddComponent<HorizontalLayoutGroup>();
            styleListLayout.childAlignment = TextAnchor.UpperCenter;
            styleListLayout.childControlWidth = true;
            styleListLayout.childControlHeight = true;
            styleListLayout.childForceExpandWidth = true;
            styleListLayout.childForceExpandHeight = true;

            var styleListLayout_Fitter = styleListLayout_GO.AddComponent<ContentSizeFitter>();
            styleListLayout_Fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            styleListLayout_Fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            _wardrobeApp.selectListHairstyle.transform.SetParent(styleListLayout_GO.transform, false);
            _wardrobeApp.selectListOutfit.transform.SetParent(styleListLayout_GO.transform, false);

            //test
            //styleListLayout_GO.AddComponent<Image>().color = Color.red;
        }

        private UiAppCheckBox CreateCheckbox(string text)
        {
            var checkbox = GameObject.Instantiate(_wardrobeApp.wearOnDatesCheckBox);
            checkbox.valueLabel.text = text;
            checkbox.Toggle(true);

            return checkbox;
        }

        public void OnDestroy()
        {
            if (!_started) { return; }

            _girlsLeft?.Destroy();
            _girlsRight?.Destroy();
            UnityEngine.Object.Destroy(_dummyFileIconSlot);

            _randomizeStylesCheckBox.CheckBoxChangedEvent -= On_RandomizeStylesCheckBox_CheckBoxChangedEvent;
            _unpairRandomizeStylesCheckBox.CheckBoxChangedEvent -= On_UnpairRandomizeStylesCheckBox_CheckBoxChangedEvent;
            _allowNsfwCheckBox.CheckBoxChangedEvent -= On_AllowRandomNsfwStylesCheckBox_CheckBoxChangedEvent;

            ExpandedUiAppStyleSelectList.Get(_wardrobeApp.selectListOutfit).ListItemSelectedEvent -= On_ListItemSelected;
            ExpandedUiAppStyleSelectList.Get(_wardrobeApp.selectListHairstyle).ListItemSelectedEvent -= On_ListItemSelected;

            _bodySelector.BodyChanged -= On_BodySelector_BodyChanged;
            UnityEngine.Object.Destroy(_bodySelector.gameObject);

            _expansions.Remove(_wardrobeApp);
        }

        public void Refresh()
        {
            // check valid data
            if (!_started) { return; }

            var wardrobeGirlId = Game.Persistence.playerFile.GetFlagValue(Flags.WARDROBE_GIRL_ID);
            var wardrobeGirlDef = Game.Data.Girls.Get(wardrobeGirlId);

            if (wardrobeGirlDef == null)
            {
                wardrobeGirlId = ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, Girls.AshleyId);
                wardrobeGirlDef = ModInterface.GameData.GetGirl(Girls.AshleyId);
                Game.Persistence.playerFile.SetFlagValue(Flags.WARDROBE_GIRL_ID, wardrobeGirlId);

            }
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(wardrobeGirlDef);

            if (!playerFileGirl.outfitIndex.InInclusiveRange(playerFileGirl.girlDefinition.outfits.Count - 1))
            {
                playerFileGirl.outfitIndex = playerFileGirl.girlDefinition.defaultOutfitIndex;
            }

            if (!playerFileGirl.hairstyleIndex.InInclusiveRange(playerFileGirl.girlDefinition.hairstyles.Count - 1))
            {
                playerFileGirl.hairstyleIndex = playerFileGirl.girlDefinition.defaultHairstyleIndex;
            }

            // head slots
            UiAppFileIconSlot selectedFileIconSlot = null;
            var iconIndex = _girlsPageIndex * GIRLS_PER_PAGE;
            int renderedCount = 0;

            foreach (var slot in _subscribedSlots.Take(GIRLS_PER_PAGE))
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

            foreach (var slot in _subscribedSlots.Skip(GIRLS_PER_PAGE))
            {
                slot.hideIfBlocked = true;
                slot.girlDefinition = null;
                slot.Populate(true);
            }

            // if the selected slot is on a different page, use the dummy slot
            if (selectedFileIconSlot == null)
            {
                _dummyFileIconSlot.girlDefinition = wardrobeGirlDef;
                f_selectedFileIconSlot.SetValue(_wardrobeApp, _dummyFileIconSlot);
            }
            else
            {
                f_selectedFileIconSlot.SetValue(_wardrobeApp, selectedFileIconSlot);
            }

            //buttons
            if (_girlsPageMax > 0)
            {
                if (_girlsPageIndex <= 0)
                {
                    _girlsPageIndex = 0;
                    _girlsLeft.ButtonBehavior.Disable();
                }
                else
                {
                    _girlsLeft.ButtonBehavior.Enable();
                }

                if (_girlsPageIndex >= _girlsPageMax)
                {
                    _girlsPageIndex = _girlsPageMax;
                    _girlsRight.ButtonBehavior.Disable();
                }
                else
                {
                    _girlsRight.ButtonBehavior.Enable();
                }
            }

            //toggles
            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, wardrobeGirlId);
            _girlSave = Plugin.Save.GetCurrentFile().GetGirl(girlId);
            _randomizeStylesCheckBox.Populate(_girlSave.RandomizeStyles);
            _unpairRandomizeStylesCheckBox.Populate(_girlSave.UnpairRandomStyles);
            _allowNsfwCheckBox.Populate(_girlSave.AllowNsfwRandomStyles);
            UpdateStyleUi();

            //body selector
            _bodySelector.SetGirl(wardrobeGirlDef);
        }

        private void UpdateStyleUi()
        {
            var alpha = 0.5f;
            var blocksRaycasts = false;

            if (_girlSave.RandomizeStyles)
            {
                alpha = 1f;
                blocksRaycasts = true;
            }

            _unpairRandomizeStylesCheckBox.canvasGroup.alpha = alpha;
            _unpairRandomizeStylesCheckBox.canvasGroup.blocksRaycasts = blocksRaycasts;

            _allowNsfwCheckBox.canvasGroup.alpha = alpha;
            _allowNsfwCheckBox.canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        private void On_RandomizeStylesCheckBox_CheckBoxChangedEvent(UiAppCheckBox checkBox)
        {
            _girlSave.RandomizeStyles = checkBox.isChecked;
            UpdateStyleUi();
        }

        private void On_UnpairRandomizeStylesCheckBox_CheckBoxChangedEvent(UiAppCheckBox checkBox)
        {
            _girlSave.UnpairRandomStyles = checkBox.isChecked;
        }

        private void On_AllowRandomNsfwStylesCheckBox_CheckBoxChangedEvent(UiAppCheckBox checkBox)
        {
            _girlSave.AllowNsfwRandomStyles = checkBox.isChecked;
        }

        private void On_ListItemSelected(UiAppStyleSelectList selectList, bool unlocked)
            => m_onListItemSelected.Invoke(_wardrobeApp, [selectList, unlocked]);

        private void On_BodySelector_BodyChanged()
        {
            var doll = f_wardrobeDoll.GetValue<UiDoll>(_wardrobeApp);
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(doll.girlDefinition);
            f_wardrobeDoll.GetValue<UiDoll>(_wardrobeApp).LoadGirl(doll.girlDefinition);

            _wardrobeApp.selectListHairstyle.Populate(playerFileGirl);
            _wardrobeApp.selectListOutfit.Populate(playerFileGirl);
            _wardrobeApp.wearOnDatesCheckBox.Populate(playerFileGirl.stylesOnDates);

            Refresh();
        }
    }
}
