// Hp2Sample 2022, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiWindowPhotos), "GetDefaultPhotoViewMode")]
    public static class UiWindowPhotos_GetDefaultPhotoViewMode_Postfix
    {
        public static void Postfix(ref int __result)
        {
            if (__result == 1
                && ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.FemaleJizzToggleCodeID))
            {
                __result = 2;
            }
        }
    }

    [HarmonyPatch(typeof(UiWindowPhotos), "Refresh")]
    public static class UiWindowPhotos_Refresh_Prefix
    {
        private static readonly FieldInfo _bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");

        public static void Prefix(UiWindowPhotos __instance)
        {
            var photoDef = _bigPhotoDefinition.GetValue<PhotoDefinition>(__instance);
            if (photoDef != null)
            {
                for (int i = 0; i < __instance.viewModeButtons.Count; i++)
                {
                    if (i >= photoDef.bigPhotoImages.Count
                        || photoDef.bigPhotoImages[i] == null)
                    {
                        __instance.viewModeButtons[i].Disable();
                    }
                    else
                    {
                        __instance.viewModeButtons[i].Enable();
                    }
                }
            }

            if (!Game.Persistence.playerData.uncensored
                || !ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.FemaleJizzToggleCodeID))
            {
                return;
            }

            __instance.bpButtonJizzCanvasGroup.blocksRaycasts = true;
            __instance.bpButtonJizzCanvasGroup.alpha = 1f;
        }
    }
}
