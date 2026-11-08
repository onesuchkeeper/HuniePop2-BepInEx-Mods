using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.Attract"/>) for when the pair becomes attracted: the girl says her
/// "big move" line, then a photo from her date photo list is shown (see <see cref="ShowDatePhotoCutsceneStep"/>).
/// </summary>
public static class SingleDateAttractCutscene
{
    // NOTE: these three reflection fields aren't used here, the photo window is handled by ShowDatePhotoCutsceneStep.
    private static readonly FieldInfo f_currentWindow = AccessTools.Field(typeof(WindowManager), "_currentWindow");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    /// <summary>Adds the cutscene, replacing the pair's attract cutscene.</summary>
    internal static void AddDataMods()
    {
        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(CutsceneIds.Attract, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                // big move dialogue
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggerId.BigMove).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
                new ShowDatePhotoCutsceneStep.Info()
            }
        });
    }
}
