using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiWindowPhotos))]
internal static class UiWindowPhotosPatch
{
    private static FieldInfo _singlePhoto = AccessTools.Field(typeof(UiWindowPhotos), "_singlePhoto");
    private static FieldInfo _bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static FieldInfo _nextPhotos = AccessTools.Field(typeof(UiWindowPhotos), "_nextPhotos");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    [HarmonyPatch("Init")]
    [HarmonyPostfix]
    private static void Init(UiWindowPhotos __instance)
    {
        if (!_singlePhoto.GetValue<bool>(__instance))
        {
            return;
        }

        var args = new SinglePhotoDisplayArgs()
        {
            BigPhotoId = ModInterface.Data.GetDataId(_bigPhotoDefinition.GetValue<PhotoDefinition>(__instance)),
            NextPhotos = _nextPhotos.GetValue<List<PhotoDefinition>>(__instance).Select(ModInterface.Data.GetDataId).ToList()
        };

        ModInterface.Events.NotifySinglePhotoDisplayed(args);
        ModInterface.Log.LogInfo($"Single photo with id {args.BigPhotoId} being displayed");

        var photo = ModInterface.GameData.GetPhoto(args.BigPhotoId);
        if (photo != null)
        {
            _bigPhotoDefinition.SetValue(__instance, photo);
            m_refreshBigPhoto.Invoke(__instance, null);
        }

        if (args.NextPhotos != null)
        {
            _nextPhotos.SetValue(__instance, args.NextPhotos
                .Select(ModInterface.GameData.GetPhoto)
                .Where(x => x != null)
                .ToList());
        }
    }
}