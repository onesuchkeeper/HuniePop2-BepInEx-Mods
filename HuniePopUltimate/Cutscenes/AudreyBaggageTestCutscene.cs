using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class AudreyBaggageTestCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Items.Audrey.Baggage1, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.BrokenRecovered, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                new FunctionalCutsceneStepInfo(completed =>
                {
                    ModInterface.Log.Message("TEST IM HERE IT WORKED");
                    completed.Invoke();
                }),
                CutsceneStepUtility.MakeGameActionInfo(new LogicActionInfo()
                {
                    Type = LogicActionType.SET_FLAG,
                    StringValue = Flags.NOTIFICATION_ITEM_ID,
                    IntValue = ModInterface.Data.GetRuntimeDataId(GameDataType.Item, Items.Audrey.Baggage1)
                }, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.25f),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.AddDataMod(mod);
    }
}
