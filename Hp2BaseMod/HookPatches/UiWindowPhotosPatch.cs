using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

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
}
