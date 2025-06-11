using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiWindowPhotos))]
    internal static class UiWindowPhotosPatch
    {
        [HarmonyPatch("Init")]
        [HarmonyPostfix]
        public static void Init(UiWindowPhotos __instance)
            => ExpandedUiWindowPhotos.Get(__instance).Init();

        [HarmonyPatch("Refresh")]
        [HarmonyPostfix]
        public static void Refresh(UiWindowPhotos __instance)
            => ExpandedUiWindowPhotos.Get(__instance).Refresh();

        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        public static void Show(UiWindowPhotos __instance, Sequence sequence)
            => ExpandedUiWindowPhotos.Get(__instance).Show();

        [HarmonyPatch("RefreshBigPhoto")]
        [HarmonyPostfix]
        public static void RefreshBigPhoto(UiWindowPhotos __instance)
            => ExpandedUiWindowPhotos.Get(__instance).RefreshBigPhoto();
    }

    internal class ExpandedUiWindowPhotos
    {
        private readonly static Dictionary<UiWindowPhotos, ExpandedUiWindowPhotos> _expansions
            = new Dictionary<UiWindowPhotos, ExpandedUiWindowPhotos>();

        public static ExpandedUiWindowPhotos Get(UiWindowPhotos uiWindowPhotos)
        {
            if (!_expansions.TryGetValue(uiWindowPhotos, out var expansion))
            {
                expansion = new ExpandedUiWindowPhotos(uiWindowPhotos);
                _expansions[uiWindowPhotos] = expansion;
            }

            return expansion;
        }

        private static readonly FieldInfo _photoDefinition = AccessTools.Field(typeof(UiPhotoSlot), "_photoDefinition");
        private static readonly FieldInfo _photoViewMode = AccessTools.Field(typeof(UiWindowPhotos), "_photoViewMode");
        private static readonly FieldInfo _singlePhoto = AccessTools.Field(typeof(UiWindowPhotos), "_singlePhoto");
        private static readonly FieldInfo _earnedPhotos = AccessTools.Field(typeof(UiWindowPhotos), "_earnedPhotos");
        private static readonly FieldInfo _bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
        private static readonly int _photosPerPage = 29;
        private static Sprite _emptyPhotoSlot;

        private int _pageIndex;
        private int _pageMax;

        private UiWindowPhotos _photosWindow;
        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;
        private Image _bg_image;

        public ExpandedUiWindowPhotos(UiWindowPhotos photosWindow)
        {
            _photosWindow = photosWindow ?? throw new ArgumentNullException(nameof(photosWindow));

            _photosWindow.bigPhotoImage.useSpriteMesh = true;
            _photosWindow.bigPhotoImage.preserveAspect = true;

            var bg_go = new GameObject();
            bg_go.layer = _photosWindow.bigPhotoImage.gameObject.layer;
            _bg_image = bg_go.AddComponent<Image>();
            _bg_image.rectTransform.sizeDelta = new Vector2(2000, 1160);
            _bg_image.transform.SetParent(_photosWindow.bigPhotoImage.transform.parent);
            _bg_image.transform.localPosition = _photosWindow.bigPhotoImage.transform.localPosition;
            _bg_image.transform.SetAsFirstSibling();
            _bg_image.useSpriteMesh = true;
            _bg_image.material = GameObject.Instantiate<Material>(UiPrefabs.BgBlur);

            var earnedPhotos = ModInterface.RequestPlayerPhotos();

            if (!Game.Persistence.playerData.uncensored)
            {
                earnedPhotos = earnedPhotos.Where(x => x?.GetBigPhotoImage(0) != null);
            }

            var earnedPhotosList = earnedPhotos.ToList();

            ModInterface.Log.LogInfo($"Earned photo count: {earnedPhotosList.Count}");

            _earnedPhotos.SetValue(_photosWindow, earnedPhotosList);

            _pageMax = earnedPhotosList.Count > 1
                ? (earnedPhotosList.Count - 1) / _photosPerPage
                : 0;

            _emptyPhotoSlot = ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoAlbumSlot);

            if (_pageMax > 0)
            {
                var albumContainer = _photosWindow.transform.Find("AlbumContainer");

                var cellphoneButtonPressedKlip = new AudioKlip()
                {
                    clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                    volume = 1f
                };

                _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoButtonLeft),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoButtonLeft),
                    cellphoneButtonPressedKlip);

                _previousPage.GameObject.transform.SetParent(albumContainer, false);
                _previousPage.RectTransform.anchoredPosition = new Vector2(42, 1038);
                _previousPage.ButtonBehavior.ButtonPressedEvent += (_) =>
                {
                    _pageIndex--;
                    Refresh();
                };

                _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoButtonRight),
                    ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoButtonRight),
                    cellphoneButtonPressedKlip);

                _nextPage.GameObject.transform.SetParent(albumContainer, false);
                _nextPage.RectTransform.anchoredPosition = new Vector2(1878, 1038);
                _nextPage.ButtonBehavior.ButtonPressedEvent += (_) =>
                {
                    _pageIndex++;
                    Refresh();
                };
            }
        }

        public void Init()
        {
            if (_singlePhoto.GetValue<bool>(_photosWindow))
            {
                _bg_image.rectTransform.localScale = Vector2.one * 2;
            }
            else
            {
                _bg_image.rectTransform.localScale = Vector2.one * 1;
            }
        }

        public void Show()
        {
            if ((bool)_singlePhoto.GetValue(_photosWindow))
            {
                _previousPage?.ButtonBehavior.Disable();
                _previousPage?.CanvasRenderer.SetAlpha(0f);

                _nextPage?.ButtonBehavior.Disable();
                _nextPage?.CanvasRenderer.SetAlpha(0f);
            }
            else
            {
                _previousPage?.ButtonBehavior.Enable();
                _nextPage?.ButtonBehavior.Enable();
                Refresh();
            }
        }

        public void Refresh()
        {
            var photoViewMode = _photoViewMode.GetValue<int>(_photosWindow);

            if (_singlePhoto.GetValue<bool>(_photosWindow))
            {
                return;
            }

            //photos
            var photoIndex = _pageIndex * _photosPerPage;
            var earnedPhotos = _earnedPhotos.GetValue<List<PhotoDefinition>>(_photosWindow);
            var photoEnumerator = earnedPhotos.Skip(photoIndex).GetEnumerator();

            foreach (var slot in _photosWindow.photoSlots.Take(_photosPerPage))
            {
                if (photoEnumerator.MoveNext())
                {
                    _photoDefinition.SetValue(slot, photoEnumerator.Current);

                    slot.buttonBehavior.Enable();
                    slot.Refresh(photoViewMode);
                }
                else
                {
                    slot.buttonBehavior.Disable();
                    slot.thumbnailImage.sprite = _emptyPhotoSlot;
                }
            }

            foreach (var slot in _photosWindow.photoSlots.Skip(_photosPerPage))
            {
                slot.buttonBehavior.Disable();
                slot.thumbnailImage.sprite = _emptyPhotoSlot;
            }

            //buttons
            if (_pageMax > 0)
            {
                if (_pageIndex <= 0)
                {
                    _pageIndex = 0;
                    _previousPage.ButtonBehavior.Disable();
                }
                else
                {
                    _previousPage.ButtonBehavior.Enable();
                }

                if (_pageIndex >= _pageMax)
                {
                    _pageIndex = _pageMax;
                    _nextPage.ButtonBehavior.Disable();
                }
                else
                {
                    _nextPage.ButtonBehavior.Enable();
                }
            }

            UpdateViewModeButtons();
        }

        internal void RefreshBigPhoto()
        {
            var photoDef = _bigPhotoDefinition.GetValue<PhotoDefinition>(_photosWindow);
            if (photoDef == null)
            {
                return;
            }

            var sprite = GetBigPhotoSprite(photoDef, out var photoViewMode) ?? UiPrefabs.CensoredBig;
            UpdateViewModeButtons(photoViewMode, photoDef);

            _photosWindow.bigPhotoImage.sprite = sprite;
            _bg_image.sprite = sprite;

            if (_bg_image.sprite != null
                && _bg_image.material != null)
            {
                var ratio = (_bg_image.sprite.rect.size.x / _bg_image.sprite.rect.size.y)
                    / (_bg_image.rectTransform.sizeDelta.x / _bg_image.rectTransform.sizeDelta.y);

                _bg_image.material.SetFloat("_AspectRatio", ratio);
            }
        }

        private Sprite GetBigPhotoSprite(PhotoDefinition photoDef, out int usedPhotoViewMode)
        {
            usedPhotoViewMode = _photoViewMode.GetValue<int>(_photosWindow);
            var sprite = photoDef.GetBigPhotoImage(usedPhotoViewMode);

            while (sprite == null && usedPhotoViewMode != 0)
            {
                usedPhotoViewMode--;
                sprite = photoDef.GetBigPhotoImage(usedPhotoViewMode);
            }

            return sprite;
        }

        private void UpdateViewModeButtons()
        {
            var photoDef = _bigPhotoDefinition.GetValue<PhotoDefinition>(_photosWindow);

            if (photoDef == null)
            {
                return;
            }

            GetBigPhotoSprite(photoDef, out var photoViewMode);
            UpdateViewModeButtons(photoViewMode, photoDef);
        }

        private void UpdateViewModeButtons(int photoViewMode, PhotoDefinition photoDef)
        {
            if (photoDef == null)
            {
                return;
            }

            var index = 0;
            foreach (var button in _photosWindow.viewModeButtons)
            {
                if (index == photoViewMode || photoDef.GetBigPhotoImage(index) == null)
                {
                    button.Disable();
                }
                else
                {
                    button.Enable();
                }

                index++;
            }
        }
    }
}
