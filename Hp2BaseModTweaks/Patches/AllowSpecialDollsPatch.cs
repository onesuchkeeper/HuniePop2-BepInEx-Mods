// Hp2Sample 2021, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiDoll), nameof(UiDoll.LoadGirl))]
    internal static class AllowSpecialDollsPatch
    {
        private static readonly FieldInfo _specialEffect = AccessTools.Field(typeof(UiDoll), "_specialEffect");
        private static void Postfix(UiDoll __instance)
        {
            if (__instance?.soulGirlDefinition == null
                || __instance.soulGirlDefinition.specialEffectPrefab == null)
            {
                return;
            }

            var expansion = ExpandedGirlDefinition.Get(__instance.soulGirlDefinition);
            if (__instance.girlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
            {
                __instance.girlDefinition.specialEffectOffset = expansion.BackPosition;
            }
            else if (__instance.girlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
            {
                __instance.girlDefinition.specialEffectOffset = expansion.HeadPosition;
            }

            if (__instance.soulGirlDefinition.specialCharacter) { return; }

            var specialEffectInstance = UnityEngine.Object.Instantiate(__instance.soulGirlDefinition.specialEffectPrefab);

            _specialEffect.SetValue(__instance, specialEffectInstance);
            specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
            specialEffectInstance.Init(__instance);
        }
    }
}
