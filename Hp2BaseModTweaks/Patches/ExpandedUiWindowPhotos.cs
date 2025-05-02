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
        private static readonly int _photosPerPage = 29;
        private static Sprite _emptyPhotoSlot;

        private int _pageIndex;
        private int _pageMax;

        private UiWindowPhotos _photosWindow;
        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;
        private bool _init;

        public ExpandedUiWindowPhotos(UiWindowPhotos photosWindow)
        {
            _photosWindow = photosWindow ?? throw new ArgumentNullException(nameof(photosWindow));
        }

        public void Init()
        {
            //instead of completed pairs, let's look at pairs that have reached lovers.
            //that way we don't have to force every pair to be added to complete pairs
            //I think it's weird that completed pairs is a thing anyways, but I digress
            var earnedPhotos = Game.Persistence.playerFile.girlPairs
                .Where(x => x.relationshipType == GirlPairRelationshipType.LOVERS)
                .Select(x => x.girlPairDefinition.photoDefinition)
                .Append(Game.Session.Hub.tutorialPhotoDef);

            if (Game.Persistence.playerFile.storyProgress >= 12)
            {
                earnedPhotos = earnedPhotos.Concat(Game.Session.Hub.nymphojinnPhotoDefs);
            }

            if (Game.Persistence.playerFile.storyProgress >= 13)
            {
                var kyuHole = Game.Persistence.playerFile.GetFlagValue("kyu_hole_selection");

                if (ModInterface.GameData.IsCodeUnlocked(Common.KyuHoleCodeId))
                {
                    earnedPhotos = earnedPhotos.Concat(Game.Session.Hub.kyuPhotoDefs);
                }
                else if (kyuHole >= 0)
                {
                    earnedPhotos = earnedPhotos.Append(Game.Session.Hub.kyuPhotoDefs[Mathf.Clamp(kyuHole, 0, Game.Session.Hub.kyuPhotoDefs.Length - 1)]);
                }
            }

            var earnedPhotosList = earnedPhotos.Distinct().ToList();
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
                _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
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
                _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
                {
                    _pageIndex++;
                    Refresh();
                };
            }

            _init = true;

            Refresh();
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
            if (!_init) { return; }

            var thumbnailIndex = _photoViewMode.GetValue<int>(_photosWindow);

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
                    slot.Refresh(thumbnailIndex);
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
