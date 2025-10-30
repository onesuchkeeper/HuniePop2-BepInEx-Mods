// Hp2BaseModTweaks 2022, By OneSuchKeeper

using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiTitleCanvas), "OnInitialAnimationComplete")]
    internal static class TitleCanvasPatch
    {
        private static readonly FieldInfo _coverArt = AccessTools.Field(typeof(UiTitleCanvas), "coverArt");

        public static void Prefix(UiTitleCanvas __instance)
        {
            var logoPaths = Plugin.LogoPaths
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(File.Exists)
                .ToArray();

            if (logoPaths.Length > 0)
            {
                var path = logoPaths.GetRandom();

                if (!(_coverArt.GetValue(__instance) is UiCoverArt coverArt))
                {
                    ModInterface.Log.LogWarning("Unable to find title canvas cover art");
                    return;
                }

                if (coverArt.logo.TryGetComponent<Image>(out var LogoImage))
                {
                    var logoTexture = TextureUtility.LoadFromPng(path, true);

                    LogoImage.sprite = TextureUtility.TextureToSprite(logoTexture, new Vector2(logoTexture.width / 2, logoTexture.height / 2));

                    LogoImage.rectTransform.DOSpiral(0.7f, null, SpiralMode.Expand, 5).Play();
                    LogoImage.rectTransform.DOShakeAnchorPos(0.7f, 40, 15, 100).Play();

                    Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Data.Tokens.Get(9).sfxMatch);
                    Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Data.Tokens.Get(7).sfxMatch);
                    Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Data.Tokens.Get(5).sfxMatch);
                }
            }
        }
    }
}
