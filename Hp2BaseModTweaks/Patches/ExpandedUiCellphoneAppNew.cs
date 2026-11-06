using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps;

[Expansion(typeof(UiCellphoneAppNew))]
public partial class ExpandedUiCellphoneAppNew
{
    [HarmonyPatch(typeof(UiCellphoneAppNew))]
    private static class UiCellphoneAppNewPatch
    {
        // must run before the game's Start so the prefab's slot state can be captured
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreStart(UiCellphoneAppNew __instance)
            => ExpandedUiCellphoneAppNew.Get(__instance).PreStart();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppNew __instance)
            => ExpandedUiCellphoneAppNew.Get(__instance).PostStart();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppNew __instance)
            => ExpandedUiCellphoneAppNew.Destroy(__instance);
    }

    private static readonly FieldInfo f_image = AccessTools.Field(typeof(ButtonBehavior), "_image");

    private static readonly Vector2 PreviousPagePos = new Vector2(214, -46);
    private static readonly Vector2 NextPagePos = new Vector2(840, -46);

    private Hp2ButtonWrapper _previousPage;
    private Hp2ButtonWrapper _nextPage;
    private UiAppFileIconSlot _dummySlot;

    // prefab state, captured before the game's Start touches it
    private readonly Dictionary<UiAppFileIconSlot, float> _defaultAlpha = new Dictionary<UiAppFileIconSlot, float>();
    private GirlDefinition[] _prefabGirls;

    private GirlDefinition[] _girls;
    private HashSet<GirlDefinition> _usedGirls;
    private GirlDefinition _selectedGirl;
    private int _pageSize;
    private int _pageIndex;
    private int _pageMax;

    internal void PreStart()
    {
        foreach (var slot in _core.fileIconSlots)
        {
            _defaultAlpha[slot] = slot.canvasGroup.alpha;
        }

        _prefabGirls = _core.fileIconSlots
            .Select(x => x.girlDefinition)
            .Where(x => x != null)
            .ToArray();
    }

    internal void PostStart()
    {
        _pageSize = Mathf.Max(_core.fileIconSlots.Count, 1);

        // girls already used as the icon of a started profile are blank, but keep their slot (matches vanilla)
        _usedGirls = [.. Game.Persistence.playerData.files
            .Where(x => x.started && x.fileIconGirlDefinition != null)
            .Select(x => x.fileIconGirlDefinition)];

        // vanilla girls keep their prefab order, then everyone else (modded girls) by id
        _girls = _prefabGirls
            .Concat(Game.Data.Girls.GetAll()
                .Where(x => x != null
                    && !x.specialCharacter
                    && x.GetHead(false) != null
                    && !_prefabGirls.Contains(x))
                .OrderBy(x => x.id))
            .ToArray();

        _pageMax = _girls.Length > 1
            ? (_girls.Length - 1) / _pageSize
            : 0;

        // dummy slot keeps a selection alive while its girl is on another page
        var dummy_go = new GameObject("NewAppDummyFileIconSlot");
        var dummy_button = dummy_go.AddComponent<ButtonBehavior>();
        f_image.SetValue(dummy_button, dummy_go.AddComponent<Image>());
        _dummySlot = dummy_go.AddComponent<UiAppFileIconSlot>();
        _dummySlot.button = dummy_button;

        foreach (var slot in _core.fileIconSlots)
        {
            slot.FileIconSlotSelectedEvent += On_FileIconSlotSelected;
        }

        if (_pageMax > 0)
        {
            var pressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                pressedKlip);
            _previousPage.GameObject.transform.SetParent(_core.transform, false);
            _previousPage.RectTransform.anchoredPosition = PreviousPagePos;
            _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
            {
                _pageIndex--;
                Refresh();
            };

            _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                pressedKlip);
            _nextPage.GameObject.transform.SetParent(_core.transform, false);
            _nextPage.RectTransform.anchoredPosition = NextPagePos;
            _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
            {
                _pageIndex++;
                Refresh();
            };
        }

        Refresh();
    }

    // the game's own handler runs first (subscribed in Start) and marks the slot selected,
    // this just remembers which girl it was so it survives page changes
    private void On_FileIconSlotSelected(UiAppFileIconSlot slot)
    {
        _selectedGirl = slot.girlDefinition;
    }

    private void OnDestroy()
    {
        foreach (var slot in _core.fileIconSlots)
        {
            slot.FileIconSlotSelectedEvent -= On_FileIconSlotSelected;
        }

        _previousPage?.Destroy();
        _nextPage?.Destroy();

        if (_dummySlot != null)
        {
            Object.Destroy(_dummySlot.gameObject);
        }
    }

    public void Refresh()
    {
        _pageIndex = Mathf.Clamp(_pageIndex, 0, _pageMax);

        UiAppFileIconSlot selectedSlot = null;
        var girlIndex = _pageIndex * _pageSize;

        foreach (var slot in _core.fileIconSlots)
        {
            var girl = girlIndex < _girls.Length
                ? _girls[girlIndex++]
                : null;

            // empty tail slots and used girls are both left blank, like vanilla
            if (girl == null || _usedGirls.Contains(girl))
            {
                slot.girlDefinition = girl;
                slot.Populate(true);
                continue;
            }

            slot.girlDefinition = girl;
            slot.Populate(false);
            slot.canvasGroup.blocksRaycasts = true;

            // only undo the blocked/hidden state. Anything else is owned by the button's
            // own transitions, which restore the prefab's semi-transparent default
            var wasHidden = Mathf.Approximately(slot.canvasGroup.alpha, 0f);
            if (wasHidden)
            {
                slot.canvasGroup.alpha = _defaultAlpha.TryGetValue(slot, out var alpha) ? alpha : 1f;
            }

            if (girl == _selectedGirl)
            {
                selectedSlot = slot;

                if (!slot.button.disabled)
                {
                    // the disable transition makes it fully opaque
                    slot.button.Disable();
                }
                else if (wasHidden)
                {
                    // was hidden while still marked selected, so no transition will run
                    slot.canvasGroup.alpha = 1f;
                }
            }
            else if (slot.button.disabled)
            {
                slot.button.Enable();
            }
        }

        // keep the game's selection pointing at the right girl
        if (_selectedGirl != null)
        {
            if (selectedSlot == null)
            {
                _dummySlot.girlDefinition = _selectedGirl;
                selectedSlot = _dummySlot;
            }

            f_selectedFileIconSlot.SetValue(_core, selectedSlot);
        }

        if (_pageMax == 0) return;

        if (_pageIndex <= 0)
        {
            _previousPage.ButtonBehavior.Disable();
        }
        else
        {
            _previousPage.ButtonBehavior.Enable();
        }

        if (_pageIndex >= _pageMax)
        {
            _nextPage.ButtonBehavior.Disable();
        }
        else
        {
            _nextPage.ButtonBehavior.Enable();
        }
    }
}
