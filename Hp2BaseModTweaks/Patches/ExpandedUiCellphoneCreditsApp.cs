using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Ui;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    [HarmonyPatch(typeof(UiCellphoneAppCredits))]
    internal class UiCellphoneAppCreditsPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(UiCellphoneAppCredits __instance)
            => ExpandedUiCellphoneCreditsApp.Get(__instance).Start();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        public static void OnDestroy(UiCellphoneAppCredits __instance)
            => ExpandedUiCellphoneCreditsApp.Get(__instance).OnDestroy();
    }

    internal class ExpandedUiCellphoneCreditsApp
    {
        private readonly static Dictionary<UiCellphoneAppCredits, ExpandedUiCellphoneCreditsApp> _expansions
                    = new Dictionary<UiCellphoneAppCredits, ExpandedUiCellphoneCreditsApp>();

        public static ExpandedUiCellphoneCreditsApp Get(UiCellphoneAppCredits uiCellphoneAppCredits)
        {
            if (!_expansions.TryGetValue(uiCellphoneAppCredits, out var expansion))
            {
                expansion = new ExpandedUiCellphoneCreditsApp(uiCellphoneAppCredits);
                _expansions[uiCellphoneAppCredits] = expansion;
            }

            return expansion;
        }

        public Hp2ButtonWrapper ModCycleLeft;
        public Hp2ButtonWrapper ModCycleRight;
        public Image ModLogo;
        public GameObject ContributorsPanel;
        public RectTransform ContributorsPanel_RectTransform;
        private AudioKlip _cellphoneButtonPressedKlip;

        private int _creditsIndex;

        private List<Hp2ButtonWrapper> _contributors = new List<Hp2ButtonWrapper>();

        private UiCellphoneAppCredits _creditsApp;
        private (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)[] _credits;
        private bool _started = false;

        public ExpandedUiCellphoneCreditsApp(UiCellphoneAppCredits creditsApp)
        {
            _creditsApp = creditsApp;

            var backgroundImage = creditsApp.transform.Find("Background").GetComponent<Image>();
            backgroundImage.sprite = UiPrefabs.CreditsBG;
            backgroundImage.SetNativeSize();
            backgroundImage.rectTransform.anchoredPosition = new Vector2(16, -18);

            var modLogoGO = new GameObject("ModLogo");
            modLogoGO.AddComponent<CanvasRenderer>();
            ModLogo = modLogoGO.AddComponent<Image>();
            modLogoGO.transform.SetParent(creditsApp.transform, false);
            ModLogo.rectTransform.anchoredPosition = new Vector2(528, -60);

            var cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            ModCycleLeft = Hp2ButtonWrapper.MakeCellphoneButton("ModCycleLeft",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                cellphoneButtonPressedKlip);
            ModCycleLeft.GameObject.transform.SetParent(creditsApp.transform, false);
            ModCycleLeft.RectTransform.anchoredPosition = new Vector2(528 - 134, -60);
            ModCycleLeft.ButtonBehavior.ButtonPressedEvent += (x) =>
            {
                _creditsIndex--;
                Refresh();
            };

            ModCycleRight = Hp2ButtonWrapper.MakeCellphoneButton("ModCycleRight",
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                cellphoneButtonPressedKlip);
            ModCycleRight.GameObject.transform.SetParent(creditsApp.transform, false);
            ModCycleRight.RectTransform.anchoredPosition = new Vector2(528 + 134, -60);
            ModCycleRight.ButtonBehavior.ButtonPressedEvent += (x) =>
            {
                _creditsIndex++;
                Refresh();
            };

            // contributors scroll
            var contributorsScroll_GO = new GameObject("ContributorsScroll");
            contributorsScroll_GO.transform.SetParent(creditsApp.transform, false);
            var contributorsScroll_RectTransform = contributorsScroll_GO.AddComponent<RectTransform>();
            contributorsScroll_RectTransform.anchorMin = new Vector2(0.5f, 1);
            contributorsScroll_RectTransform.anchorMax = new Vector2(0.5f, 1);
            contributorsScroll_RectTransform.anchoredPosition = new Vector2(528, -318);
            contributorsScroll_RectTransform.sizeDelta = new Vector2(332, 428);

            var contributorsScroll_ScrollRect = contributorsScroll_GO.AddComponent<ScrollRect>();
            contributorsScroll_ScrollRect.scrollSensitivity = 24;
            contributorsScroll_ScrollRect.horizontal = false;
            contributorsScroll_ScrollRect.viewport = contributorsScroll_RectTransform;

            contributorsScroll_GO.AddComponent<Image>();
            var contributorsScroll_Mask = contributorsScroll_GO.AddComponent<Mask>();
            contributorsScroll_Mask.showMaskGraphic = false;

            // contributors panel
            ContributorsPanel = new GameObject("ContributorsPanel");
            ContributorsPanel.transform.SetParent(contributorsScroll_GO.transform, false);

            ContributorsPanel_RectTransform = ContributorsPanel.AddComponent<RectTransform>();
            ContributorsPanel_RectTransform.anchorMin = new Vector2(0.5f, 1);
            ContributorsPanel_RectTransform.anchorMax = new Vector2(0.5f, 1);
            ContributorsPanel_RectTransform.pivot = new Vector2(0.5f, 1);
            contributorsScroll_ScrollRect.content = ContributorsPanel_RectTransform;

            var contributorsPanel_VLG = ContributorsPanel.AddComponent<VerticalLayoutGroup>();

            contributorsPanel_VLG.spacing = 8;
            contributorsPanel_VLG.padding = new RectOffset(4, 4, 4, 4);
            contributorsPanel_VLG.childForceExpandWidth = false;
            contributorsPanel_VLG.childForceExpandHeight = false;

            _cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetInternalAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            Refresh();
        }

        public void Start()
        {
            _started = true;
            _credits = Plugin.GetModCredits().Values.ToArray();
            Refresh();
        }

        public void OnDestroy()
        {
            ModCycleLeft?.Destroy();
            ModCycleRight?.Destroy();
            GameObject.Destroy(ModLogo);
            GameObject.Destroy(ContributorsPanel);
            foreach (var contributor in _contributors)
            {
                Object.Destroy(contributor.GameObject);
            }
            _contributors.Clear();

            _expansions.Remove(_creditsApp);
        }

        public void Refresh()
        {
            if (!_started) { return; }

            if (_creditsIndex <= 0)
            {
                _creditsIndex = 0;
                ModCycleLeft.ButtonBehavior.Disable();
            }
            else
            {
                ModCycleLeft.ButtonBehavior.Enable();
            }

            var maxModIndex = _credits.Length - 1;
            if (_creditsIndex >= maxModIndex)
            {
                _creditsIndex = maxModIndex;
                ModCycleRight.ButtonBehavior.Disable();
            }
            else
            {
                ModCycleRight.ButtonBehavior.Enable();
            }

            // remove old contributors
            foreach (var contributor in _contributors)
            {
                Object.Destroy(contributor.GameObject);
            }
            _contributors.Clear();

            if (!_credits.Any()) { return; }

            var modConfig = _credits[_creditsIndex];

            ModLogo.sprite = TextureUtility.SpriteFromPng(modConfig.ModImagePath);
            ModLogo.SetNativeSize();

            // add new contributors
            ContributorsPanel_RectTransform.sizeDelta = new Vector2(319, (modConfig.CreditEntries.Count * 135) - 10);

            int i = 0;
            foreach (var contributorConfig in modConfig.CreditEntries.OrEmptyIfNull())
            {
                var newContributorButton = Hp2ButtonWrapper.MakeCellphoneButton($"Contributor {i++}",
                    TextureUtility.SpriteFromPng(contributorConfig.CreditButtonPath),
                    TextureUtility.SpriteFromPng(contributorConfig.CreditButtonOverPath),
                    _cellphoneButtonPressedKlip);

                newContributorButton.GameObject.transform.SetParent(ContributorsPanel.transform, false);

                _contributors.Add(newContributorButton);

                newContributorButton.ButtonBehavior.ButtonPressedEvent += (e) => System.Diagnostics.Process.Start(contributorConfig.RedirectLink);
            }
        }
    }
}
