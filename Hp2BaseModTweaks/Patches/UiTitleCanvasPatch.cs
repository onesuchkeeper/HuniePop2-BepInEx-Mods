// Hp2BaseModTweaks 2022, By OneSuchKeeper

using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiTitleCanvas), "OnInitialAnimationComplete")]
    internal static class TitleCanvasPatch
    {
        private static readonly FieldInfo f_coverArt = AccessTools.Field(typeof(UiTitleCanvas), "coverArt");

        public static void Prefix(UiTitleCanvas __instance)
        {
            if (Plugin.LogoSprites.Any())
            {
                if (!(f_coverArt.GetValue(__instance) is UiCoverArt coverArt))
                {
                    ModInterface.Log.Warning("Unable to find title canvas cover art");
                    return;
                }

                if (coverArt.logo.TryGetComponent<Image>(out var LogoImage))
                {
                    LogoImage.sprite = Plugin.LogoSprites.GetRandom();

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
