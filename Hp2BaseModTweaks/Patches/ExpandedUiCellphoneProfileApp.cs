using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Ui;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppProfile))]
    internal static class UiCellphoneAppProfilePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreStart(UiCellphoneAppProfile __instance)
            => ExpandedUiCellphoneProfileApp.Get(__instance).PreStart();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppProfile __instance)
            => ExpandedUiCellphoneProfileApp.Get(__instance).PostStart();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppProfile __instance)
            => ExpandedUiCellphoneProfileApp.Get(__instance).OnDestroy();
    }

    internal class ExpandedUiCellphoneProfileApp
    {
        private readonly static Dictionary<UiCellphoneAppProfile, ExpandedUiCellphoneProfileApp> _expansions
                            = new Dictionary<UiCellphoneAppProfile, ExpandedUiCellphoneProfileApp>();

        public static ExpandedUiCellphoneProfileApp Get(UiCellphoneAppProfile uiCellphoneAppProfile)
        {
            if (!_expansions.TryGetValue(uiCellphoneAppProfile, out var expansion))
            {
                expansion = new ExpandedUiCellphoneProfileApp(uiCellphoneAppProfile);
                _expansions[uiCellphoneAppProfile] = expansion;
            }

            return expansion;
        }

        private static readonly FieldInfo f_favQuestionDefinition = AccessTools.Field(typeof(UiAppFavAnswer), "_favQuestionDefinition");

        private static readonly string _profileBackgroundPath = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks/images/ui_app_profile_modded_background.png");
        private static readonly string _profileFavoritesBackgroundPath = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks/images/ui_app_profile_favorites_background.png");
        private static readonly int _pairsPerPage = 4;

        private Hp2ButtonWrapper _previousPage;
        private Hp2ButtonWrapper _nextPage;

        private int _currentPage = 0;

        private readonly UiCellphoneAppProfile _profileApp;

        public ExpandedUiCellphoneProfileApp(UiCellphoneAppProfile profileApp)
        {
            _profileApp = profileApp ?? throw new ArgumentNullException(nameof(profileApp));
        }

        public void PreStart()
        {
            _profileApp.girlHeadIcon.preserveAspect = true;

            var cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            var activeContainer = _profileApp.transform.Find("ActiveContainer");

            var backgroundImage = _profileApp.transform.Find("Background").GetComponent<Image>();
            backgroundImage.sprite = TextureUtility.SpriteFromPng(_profileBackgroundPath, true);
            backgroundImage.SetNativeSize();

            // pairs
            var pairsLine = activeContainer.Find("PairsLine");
            pairsLine.position -= new Vector3(0, 9);
            var pairsLine_RectTransform = pairsLine.GetComponent<RectTransform>();
            pairsLine_RectTransform.sizeDelta -= new Vector2(80, 0);

            var pairsContainer = activeContainer.Find("PairsContainer");
            pairsContainer.position -= new Vector3(0, 16);

            _previousPage = Hp2ButtonWrapper.MakeCellphoneButton("PreviousPage",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                cellphoneButtonPressedKlip);

            _previousPage.GameObject.transform.SetParent(activeContainer, false);
            _previousPage.RectTransform.anchoredPosition = new Vector2(188, -364);
            _previousPage.ButtonBehavior.ButtonPressedEvent += (e) =>
            {
                _currentPage--;
                Refresh();
            };

            _nextPage = Hp2ButtonWrapper.MakeCellphoneButton("NextPage",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                cellphoneButtonPressedKlip);

            _nextPage.GameObject.transform.SetParent(activeContainer, false);
            _nextPage.RectTransform.anchoredPosition = new Vector2(622, -364);
            _nextPage.ButtonBehavior.ButtonPressedEvent += (e) =>
            {
                _currentPage++;
                Refresh();
            };

            // favorites, make scroll and add extra entries
            var favoritesScroll_GO = new GameObject("FavoritesScroll");
            favoritesScroll_GO.transform.SetParent(activeContainer, false);
            var favoritesScroll_RectTransform = favoritesScroll_GO.AddComponent<RectTransform>();
            favoritesScroll_RectTransform.anchorMin = new Vector2(0.5f, 1);
            favoritesScroll_RectTransform.anchorMax = new Vector2(0.5f, 1);
            favoritesScroll_RectTransform.pivot = new Vector2(0.5f, 1);
            favoritesScroll_RectTransform.anchoredPosition = new Vector2(864, 0);
            favoritesScroll_RectTransform.sizeDelta = new Vector2(383, 568);

            var favoriteScroll_Image = favoritesScroll_GO.AddComponent<Image>();
            var contributorsScroll_Mask = favoritesScroll_GO.AddComponent<Mask>();
            contributorsScroll_Mask.showMaskGraphic = false;

            // favorites panel
            var questions = Game.Data.Questions.GetAll();
            var girlDef = Game.Data.Girls.Get(_profileApp.cellphone.GetCellFlag("profile_girl_id"));
            var girlDefExp = girlDef.Expansion();
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);

            var validFavQuestions = Game.Data.Questions.GetAll()
                .Where(x => girlDefExp.FavQuestionIdToAnswerId.ContainsKey(ModInterface.Data.GetDataId(GameDataType.Question, x.id)))
                .ToList();

            var questionPanelHeight = Mathf.Max(28 + (27 * validFavQuestions.Count), 568);

            var favAnswersPanel_GO = new GameObject("FavoritesPanel");
            favAnswersPanel_GO.transform.SetParent(favoritesScroll_GO.transform, false);

            var favoritesPanel_RectTransform = favAnswersPanel_GO.AddComponent<RectTransform>();
            favoritesPanel_RectTransform.anchorMin = new Vector2(0.5f, 1);
            favoritesPanel_RectTransform.anchorMax = new Vector2(0.5f, 1);
            favoritesPanel_RectTransform.pivot = new Vector2(0.5f, 1);
            favoritesPanel_RectTransform.anchoredPosition = new Vector2(0, 0);
            favoritesPanel_RectTransform.sizeDelta = new Vector2(383, questionPanelHeight);

            // favorites bg
            var favoritesPanelBG_GO = new GameObject("FavoritesBG");
            favoritesPanelBG_GO.transform.SetParent(favAnswersPanel_GO.transform, false);

            var favoritesPanelBG_Texture = TextureUtility.LoadFromPng(_profileFavoritesBackgroundPath, true);
            favoritesPanelBG_Texture.wrapMode = TextureWrapMode.Repeat;

            var favoritesPanelBG_Image = favoritesPanelBG_GO.AddComponent<Image>();
            favoritesPanelBG_Image.sprite = TextureUtility.TextureToSprite(favoritesPanelBG_Texture, Vector2.zero);
            favoritesPanelBG_Image.type = Image.Type.Tiled;
            favoritesPanelBG_Image.pixelsPerUnitMultiplier = 1f;
            favoritesPanelBG_Image.rectTransform.anchorMin = new Vector2(0.5f, 1);
            favoritesPanelBG_Image.rectTransform.anchorMax = new Vector2(0.5f, 1);
            favoritesPanelBG_Image.rectTransform.pivot = new Vector2(0.5f, 1);
            favoritesPanelBG_Image.rectTransform.anchoredPosition = new Vector2(0, 0);
            favoritesPanelBG_Image.rectTransform.sizeDelta = new Vector2(383, questionPanelHeight);

            // favorites container
            var favAnswersContainer = activeContainer.Find("FavAnswersContainer");
            var favAnswersContainer_RectTransform = favAnswersContainer.gameObject.GetComponent<RectTransform>();
            favAnswersContainer_RectTransform.anchorMin = new Vector2(0, 1);
            favAnswersContainer_RectTransform.anchorMax = new Vector2(0, 1);
            favAnswersContainer_RectTransform.pivot = new Vector2(0, 1);
            favAnswersContainer.transform.SetParent(favAnswersPanel_GO.transform, true);

            var delta = validFavQuestions.Count - _profileApp.favAnswers.Length;
            if (delta < 0)
            {
                foreach (var answer in _profileApp.favAnswers.Skip(validFavQuestions.Count))
                {
                    answer.transform.SetParent(null);
                    answer.Destroy();
                }

                _profileApp.favAnswers = _profileApp.favAnswers.Take(validFavQuestions.Count).ToArray();
            }
            else if (delta > 0)
            {
                var templateQuestion = _profileApp.favAnswers[0];
                Array.Resize(ref _profileApp.favAnswers, validFavQuestions.Count);

                while (delta-- > 0)
                {
                    var newQuestion_go = UnityEngine.Object.Instantiate(templateQuestion.gameObject);
                    var uiAppFavAnswer = newQuestion_go.GetComponent<UiAppFavAnswer>();
                    newQuestion_go.transform.SetParent(templateQuestion.transform.parent, false);
                    _profileApp.favAnswers[validFavQuestions.Count - delta - 1] = uiAppFavAnswer;
                }
            }

            var i = 0;
            foreach (var (ui, question) in _profileApp.favAnswers
                .Zip(validFavQuestions, (ui, question) => (ui, question)))
            {
                f_favQuestionDefinition.SetValue(ui, question);

                var rectTransform = ui.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector3(0, -27 * i++);

                ui.Populate(playerFileGirl);
            }

            // favorites scroll rect
            var favoritesScroll_ScrollRect = favoritesScroll_GO.AddComponent<ScrollRect>();
            favoritesScroll_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
            favoritesScroll_ScrollRect.scrollSensitivity = 18;
            favoritesScroll_ScrollRect.horizontal = false;
            favoritesScroll_ScrollRect.viewport = favoritesScroll_RectTransform;
            favoritesScroll_ScrollRect.content = favoritesPanel_RectTransform;
            favoritesScroll_ScrollRect.verticalNormalizedPosition = 1f;
            favoritesScroll_ScrollRect.elasticity = 0.15f;

            favoritesPanel_RectTransform.position -= new Vector3(0f, favoritesPanel_RectTransform.sizeDelta.y / 2);
        }

        internal void PostStart() => Refresh();

        public void OnDestroy()
        {
            _previousPage?.Destroy();
            _nextPage?.Destroy();
        }

        public void Refresh()
        {
            var profileGirl = Game.Data.Girls.Get(Game.Session.gameCanvas.cellphone.GetCellFlag("profile_girl_id"));

            var pairs = Game.Persistence.playerFile.metGirlPairs.Where(x => x.HasGirlDef(profileGirl)).ToArray();

            var pageMax = pairs.Length > 1
                ? (pairs.Length - 1) / _pairsPerPage
                : 0;

            // pairs
            var current = _currentPage * _pairsPerPage;

            foreach (var entry in _profileApp.pairSlots.Take(_pairsPerPage))
            {
                if (current < pairs.Length)
                {
                    entry.canvasGroup.alpha = 1f;
                    entry.canvasGroup.blocksRaycasts = true;
                    entry.button.Enable();
                    entry.Populate(pairs[current]);

                    current++;
                }
                else
                {
                    entry.Populate(null, null);
                }
            }

            foreach (var entry in _profileApp.pairSlots.Skip(_pairsPerPage))
            {
                entry.Populate(null, null);
            }

            // buttons
            if (_currentPage <= 0)
            {
                _currentPage = 0;
                _previousPage.ButtonBehavior.Disable();
            }
            else
            {
                _previousPage.ButtonBehavior.Enable();
            }

            if (_currentPage >= pageMax)
            {
                _currentPage = pageMax;
                _nextPage.ButtonBehavior.Disable();
            }
            else
            {
                _nextPage.ButtonBehavior.Enable();
            }
        }
    }
}
