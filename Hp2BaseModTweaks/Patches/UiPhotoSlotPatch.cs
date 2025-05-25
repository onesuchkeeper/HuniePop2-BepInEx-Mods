using HarmonyLib;

[HarmonyPatch(typeof(UiPhotoSlot))]
public static class UiPhotoSlotPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Awake(UiPhotoSlot __instance)
    {
        __instance.thumbnailImage.preserveAspect = true;
        __instance.thumbnailImage.useSpriteMesh = true;
    }
}