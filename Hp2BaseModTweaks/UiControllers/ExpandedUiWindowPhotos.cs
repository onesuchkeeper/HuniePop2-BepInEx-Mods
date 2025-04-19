using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using UnityEngine;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiWindowPhotos))]
    public static class UiWindowPhotosPatch
    {
        private readonly static Dictionary<UiWindowPhotos, ExpandedUiWindowPhotos> _extendedApps
                            = new Dictionary<UiWindowPhotos, ExpandedUiWindowPhotos>();

        [HarmonyPatch("Init")]
        [HarmonyPostfix]
        public static void PostStart(UiWindowPhotos __instance)
        {
            if (!_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp = new ExpandedUiWindowPhotos(__instance);
                _extendedApps[__instance] = extendedApp;
            }

            extendedApp.OnStart();
        }

        [HarmonyPatch("Refresh")]
        [HarmonyPostfix]
        public static void PostRefresh(UiWindowPhotos __instance)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.Refresh();
            }
        }

        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        public static void Show(UiWindowPhotos __instance, Sequence sequence)
        {
            if (_extendedApps.TryGetValue(__instance, out var extendedApp))
            {
                extendedApp.OnShow();
            }
        }
    }

    public class ExpandedUiWindowPhotos
    {
        private static readonly FieldInfo _photoDefinition = AccessTools.Field(typeof(UiPhotoSlot), "_photoDefinition");
        private static readonly FieldInfo _earnedPhotos = AccessTools.Field(typeof(UiWindowPhotos), "_earnedPhotos");
        private static readonly FieldInfo _singlePhoto = AccessTools.Field(typeof(UiWindowPhotos), "_singlePhoto");
        private static readonly int _photosPerPage = 29;
        private static Sprite _emptyPhotoSlot;

        private int _pageIndex;
        private int _pageMax;

        private readonly UiWindowPhotos _photosWindow;
        private readonly PhotoDefinition[] _photosArray;
        private readonly Hp2ButtonWrapper _previousPage;
        private readonly Hp2ButtonWrapper _nextPage;

        public ExpandedUiWindowPhotos(UiWindowPhotos photosWindow)
        {
            _photosWindow = photosWindow ?? throw new ArgumentNullException(nameof(photosWindow));

            _photosArray = (_earnedPhotos.GetValue(photosWindow) as List<PhotoDefinition>).ToArray();
            _pageMax = _photosArray.Length > 1 ? (_photosArray.Length - 1) / _photosPerPage : 0;
            _emptyPhotoSlot = ModInterface.Assets.GetAsset<Sprite>(Common.Ui_PhotoAlbumSlot);

            if (_pageMax > 0)
            {
                var albumContainer = photosWindow.transform.Find("AlbumContainer");

                var cellphoneButtonPressedKlip = new AudioKlip()
                {
                    clip = ModInterface.Assets.GetAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                    volume = 1f
                };

                _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_PhotoButtonLeft),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_PhotoButtonLeft),
                    cellphoneButtonPressedKlip);

                _previousPage.GameObject.transform.SetParent(albumContainer, false);
                _previousPage.RectTransform.anchoredPosition = new Vector2(42, 1038);
                _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _pageIndex--;
                    Refresh();
                };

                _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_PhotoButtonRight),
                    ModInterface.Assets.GetAsset<Sprite>(Common.Ui_PhotoButtonRight),
                    cellphoneButtonPressedKlip);

                _nextPage.GameObject.transform.SetParent(albumContainer, false);
                _nextPage.RectTransform.anchoredPosition = new Vector2(1878, 1038);
                _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _pageIndex++;
                    Refresh();
                };
            }

            Refresh();
        }

        public void OnStart()
        {
            Refresh();
        }

        public void OnShow()
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
            if ((bool)_singlePhoto.GetValue(_photosWindow))
            {
                return;
            }

            //photos
            var photoIndex = _pageIndex * _photosPerPage;

            foreach (var slot in _photosWindow.photoSlots.Take(_photosPerPage))
            {
                if (photoIndex < _photosArray.Length)
                {
                    _photoDefinition.SetValue(slot, _photosArray[photoIndex]);
                    slot.buttonBehavior.Enable();
                    slot.Refresh(1);
                    photoIndex++;
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
        }
    }
}
