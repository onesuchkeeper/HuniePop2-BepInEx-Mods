using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

public static class EnergyTrailBehavior_Ext
{
    private static FieldInfo f_inited = AccessTools.Field(typeof(EnergyTrailBehavior), "_inited");
    private static FieldInfo f_itemDefinition = AccessTools.Field(typeof(EnergyTrailBehavior), "_itemDefinition");
    private static FieldInfo f_dynamicEndPosition = AccessTools.Field(typeof(EnergyTrailBehavior), "_dynamicEndPosition");
    private static FieldInfo f_targetDoll = AccessTools.Field(typeof(EnergyTrailBehavior), "_targetDoll");
    private static FieldInfo f_negative = AccessTools.Field(typeof(EnergyTrailBehavior), "_negative");
    private static FieldInfo f_endPositionOffset = AccessTools.Field(typeof(EnergyTrailBehavior), "_endPositionOffset");

    public static void Init(this EnergyTrailBehavior core, EnergyTrailFormat format, IPuzzleReward reward, UiPuzzleSlot puzzleSlot)
    {
        if (f_inited.GetValue<bool>(core)) return;

            var (splashText, burstText) = reward.GetLabelText();

            f_itemDefinition.SetValue(core, reward.FruitDefinition);
            f_dynamicEndPosition.SetValue(core, true);
            var targetDoll = Game.Session.gameCanvas.GetDoll(reward.Girl.altGirl);
            f_targetDoll.SetValue(core, targetDoll);
            f_negative.SetValue(core, reward.Negative);
            f_endPositionOffset.SetValue(core, Game.Manager.gameCamera.mainCamera.WorldToScreenPoint(
                new Vector3(
                    UnityEngine.Random.Range(0f - targetDoll.currentTargetZone.sizeDelta.x * 0.5f, targetDoll.currentTargetZone.sizeDelta.x * 0.5f), 
                    UnityEngine.Random.Range(0f - targetDoll.currentTargetZone.sizeDelta.y * 0.5f, targetDoll.currentTargetZone.sizeDelta.y * 0.5f), 
                    0f)
                )
            );

            core.Init(format, reward.TokenDefinition.Core.energyDefinition, puzzleSlot.rectTransform.position, targetDoll, splashText, burstText);
    }
}