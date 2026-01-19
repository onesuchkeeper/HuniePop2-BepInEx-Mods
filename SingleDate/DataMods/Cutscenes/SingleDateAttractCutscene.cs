using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

public static class SingleDateAttractCutscene
{
    private static readonly FieldInfo f_currentWindow = AccessTools.Field(typeof(WindowManager), "_currentWindow");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.Attract, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                // big move dialogue
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.BigMove, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
                new ShowDatePhotoCutsceneStep.Info()
            }
        });
    }
}
