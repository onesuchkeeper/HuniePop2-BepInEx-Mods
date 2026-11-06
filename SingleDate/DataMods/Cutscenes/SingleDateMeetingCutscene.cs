using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.Meeting"/>) for meeting the girl on a single date: she greets the player.
/// </summary>
public static class SingleDateMeetingCutscene
{
    /// <summary>Adds the cutscene. It replaces the pair's meeting cutscene, and doesn't clean anything up afterwards.</summary>
    internal static void AddDataMods()
    {
        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(CutsceneIds.Meeting, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                // The right (and only) girl says her afternoon greeting.
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.GreetingAfternoon).TargetOrientation(DollOrientationType.RIGHT),
            }
        });
    }
}
