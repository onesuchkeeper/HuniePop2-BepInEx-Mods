// Hp2BaseMod 2026, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using DG.Tweening;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Notifies when a photo is displayed to allow it to be overwritten by mods.
/// </summary>
[HarmonyPatch(typeof(UiWindowPhotos))]
internal static class UiWindowPhotosPatch
{
    private static readonly FieldInfo f_singlePhoto = AccessTools.Field(typeof(UiWindowPhotos), "_singlePhoto");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly FieldInfo f_nextPhotos = AccessTools.Field(typeof(UiWindowPhotos), "_nextPhotos");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    [HarmonyPatch("Init")]
    [HarmonyPostfix]
    private static void Init(UiWindowPhotos __instance)
    {
        if (!f_singlePhoto.GetValue<bool>(__instance))
        {
            return;
        }

        var args = new SinglePhotoDisplayArgs()
        {
            BigPhotoId = ModInterface.Data.GetDataId(f_bigPhotoDefinition.GetValue<PhotoDefinition>(__instance)),
            NextPhotos = f_nextPhotos.GetValue<List<PhotoDefinition>>(__instance).Select(ModInterface.Data.GetDataId).ToList()
        };

        ModInterface.Events.NotifySinglePhotoDisplayed(args);

        var photo = ModInterface.GameData.GetPhoto(args.BigPhotoId);
        if (photo != null)
        {
            f_bigPhotoDefinition.SetValue(__instance, photo);
            m_refreshBigPhoto.Invoke(__instance, null);
        }

        if (args.NextPhotos != null)
        {
            f_nextPhotos.SetValue(__instance, args.NextPhotos
                .Select(ModInterface.GameData.GetPhoto)
                .Where(x => x != null)
                .ToList());
        }
    }

    // Closing the big photo over a date puts the dolls back into the mood the puzzle has them in, which is the puzzle state's call.
    [HarmonyPatch(nameof(UiWindowPhotos.Hide))]
    [HarmonyPrefix]
    private static bool Hide(UiWindowPhotos __instance, Sequence sequence)
    {
        if (!f_singlePhoto.GetValue<bool>(__instance))
        {
            for (int i = 0; i < __instance.photoSlots.Count; i++)
            {
                var slot = __instance.photoSlots[i];
                var delay = (__instance.photoSlots.Count - 1 - i) * 0.02f;
                sequence.Insert(delay, slot.rectTransform.DOAnchorPosY(slot.origY - 100f, 0.44f, false).SetEase(Ease.InBack));
                sequence.Insert(delay, slot.rectTransform.DOLocalRotate(new Vector3(0f, 0f, -6f), 0.44f, RotateMode.Fast).SetEase(Ease.InSine));
                sequence.Insert(delay + 0.11f, slot.thumbnailImage.DOFade(0f, 0.33f).SetEase(Ease.Linear));
            }

            sequence.Insert(0f, __instance.albumCloseButton.rectTransform.DOScale(0.5f, 0.25f).SetEase(Ease.InBack));
            sequence.Insert(0.05f, __instance.albumCloseButtonImage.DOFade(0f, 0.2f).SetEase(Ease.Linear));
            sequence.Insert(0.5f, __instance.frameBottom.DOAnchorPosY(-100f, 0.5f, false).SetEase(Ease.InOutCubic));
            sequence.Insert(0.5f, Game.Manager.Windows.windowOverlay.DOFade(0f, 0.5f).SetEase(Ease.Linear));
            return false;
        }

        (ModInterface.GameState.CurrentState as IPuzzleState)?.OnPhotoWindowClosed();

        sequence.Insert(0f, __instance.bigPhotoContainer.DOScale(2f, 0.25f).SetEase(Ease.OutSine));
        sequence.Insert(0.1f, __instance.bigPhotoCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.Linear));
        Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxZoomOut, __instance.pauseDefinition);
        return false;
    }
}
