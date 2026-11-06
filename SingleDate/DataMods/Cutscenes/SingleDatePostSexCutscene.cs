using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.PostSex"/>) after the sex date: a moan line, her date success line, then a
/// short wait.
/// </summary>
public static class SingleDatePostSexCutscene
{
    /// <summary>Adds the cutscene, replacing the pair's post sex cutscene.</summary>
    internal static void AddDataMods()
    {
        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(CutsceneIds.PostSex, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                //moan 
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.SexMoans1).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.DateSuccess).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
            }
        });
    }
}
