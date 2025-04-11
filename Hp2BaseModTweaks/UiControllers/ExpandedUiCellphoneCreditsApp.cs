using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Ui;
using Hp2BaseMod.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks.CellphoneApps
{
    internal class ExpandedUiCellphoneCreditsApp : IUiController
    {
        private static readonly string _creditsBackgroundPath = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks\\images\\ui_app_credits_modded_background.png");

        public Hp2ButtonWrapper ModCycleLeft;
        public Hp2ButtonWrapper ModCycleRight;
        public Image ModLogo;
        public GameObject ContributorsPanel;
        public RectTransform ContributorsPanel_RectTransform;
        private AudioKlip _cellphoneButtonPressedKlip;

        private int _creditsIndex;

        private List<Hp2ButtonWrapper> _contributors = new List<Hp2ButtonWrapper>();

        public ExpandedUiCellphoneCreditsApp(UiCellphoneAppCredits creditsApp)
        {
            if (File.Exists(_creditsBackgroundPath))
            {
                var backgroundImage = creditsApp.transform.Find("Background").GetComponent<Image>();
                backgroundImage.sprite = TextureUtility.SpriteFromPath(_creditsBackgroundPath);
                backgroundImage.SetNativeSize();
                backgroundImage.rectTransform.anchoredPosition = new Vector2(16, -18);
            }
            else
            {
                ModInterface.Log.LogError($"{_creditsBackgroundPath} not found");
            }

            var modLogoGO = new GameObject("ModLogo");
            modLogoGO.AddComponent<CanvasRenderer>();
            ModLogo = modLogoGO.AddComponent<Image>();
            modLogoGO.transform.SetParent(creditsApp.transform, false);
            ModLogo.rectTransform.anchoredPosition = new Vector2(528, -60);

            var cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            ModCycleLeft = Hp2ButtonWrapper.MakeCellphoneButton("ModCycleLeft",
                ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
                ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
                cellphoneButtonPressedKlip);
            ModCycleLeft.GameObject.transform.SetParent(creditsApp.transform, false);
            ModCycleLeft.RectTransform.anchoredPosition = new Vector2(528 - 134, -60);
            ModCycleLeft.ButtonBehavior.ButtonPressedEvent += (x) =>
            {
                _creditsIndex--;
                PostRefresh();
            };

            ModCycleRight = Hp2ButtonWrapper.MakeCellphoneButton("ModCycleRight",
                ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRight),
                ModInterface.Assets.GetAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
                cellphoneButtonPressedKlip);
            ModCycleRight.GameObject.transform.SetParent(creditsApp.transform, false);
            ModCycleRight.RectTransform.anchoredPosition = new Vector2(528 + 134, -60);
            ModCycleRight.ButtonBehavior.ButtonPressedEvent += (x) =>
            {
                _creditsIndex++;
                PostRefresh();
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
            contributorsPanel_VLG.spacing = 0;
            contributorsPanel_VLG.childForceExpandWidth = false;
            contributorsPanel_VLG.childForceExpandHeight = false;

            _cellphoneButtonPressedKlip = new AudioKlip()
            {
                clip = ModInterface.Assets.GetAsset<AudioClip>(Common.Sfx_PhoneAppButtonPressed),
                volume = 1f
            };

            PostRefresh();
        }

        public void PreRefresh()
        {
            //noop
        }

        public void PostRefresh()
        {
            ModInterface.Log.LogInfo("Expanded Credits App Post Refresh");

            if (_creditsIndex <= 0)
            {
                _creditsIndex = 0;
                ModCycleLeft.ButtonBehavior.Disable();
            }
            else
            {
                ModCycleLeft.ButtonBehavior.Enable();
            }

            if (_creditsIndex >= Common.Mods.Count - 1)
            {
                _creditsIndex = Common.Mods.Count - 1;
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

            ModInterface.Log.LogInfo("Expanded Credits App Post Refresh");

            if (!Common.Mods.Any()) { return; }

            ModInterface.Log.LogInfo($"Setting up credits for mod index {_creditsIndex}");

            var modConfig = Common.Mods[_creditsIndex];

            ModLogo.sprite = TextureUtility.SpriteFromPath(modConfig.ModImagePath);
            ModLogo.SetNativeSize();

            // add new contributors
            ContributorsPanel_RectTransform.sizeDelta = new Vector2(319, (modConfig.CreditsEntries.Count * 135) - 10);

            int i = 0;
            foreach (var contributorConfig in modConfig.CreditsEntries.OrEmptyIfNull())
            {
                ModInterface.Log.LogInfo($"Adding contributor {i}");
                var newContributorButton = Hp2ButtonWrapper.MakeCellphoneButton($"Contributor {i++}",
                    TextureUtility.SpriteFromPath(contributorConfig.CreditButtonImagePath),
                    TextureUtility.SpriteFromPath(contributorConfig.CreditButtonImageOverPath),
                    _cellphoneButtonPressedKlip);

                newContributorButton.GameObject.transform.SetParent(ContributorsPanel.transform, false);

                _contributors.Add(newContributorButton);

                newContributorButton.ButtonBehavior.ButtonPressedEvent += (e) => System.Diagnostics.Process.Start(contributorConfig.RedirectLink);
            }
        }
    }
}
