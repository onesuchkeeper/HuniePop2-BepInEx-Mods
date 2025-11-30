using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseModTweaks;
using UnityEngine;

[HarmonyPatch(typeof(UiPhotoSlot))]
public static class UiPhotoSlotPatch
{
    private static readonly FieldInfo f_photoDefinition = AccessTools.Field(typeof(UiPhotoSlot), "_photoDefinition");

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Awake(UiPhotoSlot __instance)
    {
        __instance.thumbnailImage.preserveAspect = true;
        __instance.thumbnailImage.useSpriteMesh = true;
        __instance.slotType = PhotoSlotType.NORMAL;
    }

    [HarmonyPatch(nameof(UiPhotoSlot.Refresh))]
    [HarmonyPostfix]
    private static void Refresh(UiPhotoSlot __instance, int thumbnailIndex)
    {
        var photoDef = f_photoDefinition.GetValue<PhotoDefinition>(__instance);

        if (photoDef != null)
        {
            //if no thumb for current photo mode, use next existing at a lower mode. If none use censored.
            var index = Game.Persistence.playerData.uncensored ? thumbnailIndex : 0;

            var sprite = photoDef.GetThumbnailImage(index);
            while (sprite == null && index != 0)
            {
                index--;
                sprite = photoDef.GetThumbnailImage(index);
            }

            __instance.thumbnailImage.sprite = sprite ?? UiPrefabs.CensoredThumb;
        }
        else
        {
            __instance.thumbnailImage.sprite = ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_PhotoAlbumSlot);
        }
    }
}
