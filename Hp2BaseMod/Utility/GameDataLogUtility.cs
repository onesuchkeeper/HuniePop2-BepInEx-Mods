using System;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static class GameDataLogUtility
{
    public static void LogCutscene(CutsceneDefinition cutsceneDef)
    {
        ModInterface.Log.LogInfo($"Cleanup Type: {Enum.GetName(typeof(CutsceneCleanUpType), cutsceneDef.cleanUpType)}");
        int i = 0;
        foreach (var step in cutsceneDef.steps)
        {
            ModInterface.Log.LogInfo($"Step {i++}");
            LogCutsceneStep(step);
        }
    }

    public static void LogCutsceneStep(CutsceneStepSubDefinition step)
    {
        using (ModInterface.Log.MakeIndent())
        {
            ModInterface.Log.LogInfo($"Type: {Enum.GetName(typeof(CutsceneStepType), step.stepType)}");
            if (step.stepType == CutsceneStepType.SUB_CUTSCENE)
            {
                ModInterface.Log.LogInfo($"Type: {Enum.GetName(typeof(CutsceneStepSubCutsceneType), step.subCutsceneType)}");
                var currentGirlPair = Game.Session.Location.currentGirlPair;
                CutsceneDefinition subCutscene = null;
                switch (step.subCutsceneType)
                {
                    case CutsceneStepSubCutsceneType.GIRL_PAIR:
                        if (currentGirlPair != null)
                        {
                            var index = Mathf.Clamp((int)step.girlPairRelationshipType, 0, currentGirlPair.relationshipCutsceneDefinitions.Count - 1);
                            subCutscene = currentGirlPair.relationshipCutsceneDefinitions[index];
                        }
                        break;
                }

                if (subCutscene != null)
                {
                    ModInterface.Log.LogInfo($"SubCutscene:");
                    using (ModInterface.Log.MakeIndent())
                    {
                        LogCutscene(subCutscene);
                    }
                }
            }
            ModInterface.Log.LogInfo($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
            ModInterface.Log.LogInfo($"DollTargetType: {Enum.GetName(typeof(CutsceneStepType), step.dollTargetType)}");
            ModInterface.Log.LogInfo($"TargetGirlDef: {step.targetGirlDefinition?.name}");
            ModInterface.Log.LogInfo($"GirlDef: {step.girlDefinition?.name}");
            ModInterface.Log.LogInfo($"DollTargetType: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.targetDollOrientation)}");
        }
    }
}