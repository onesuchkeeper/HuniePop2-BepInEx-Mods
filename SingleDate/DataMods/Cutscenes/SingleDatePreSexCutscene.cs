using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.PreSex"/>) before the sex date. Currently only the girl's moan line.
/// </summary>
public static class SingleDatePreSexCutscene
{
    /// <summary>Adds the cutscene, replacing the pair's pre sex cutscene.</summary>
    internal static void AddDataMods()
    {
        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(CutsceneIds.PreSex, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                //moan
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.SexMoans1).TargetOrientation(DollOrientationType.RIGHT),
            }
        });
    }
}
