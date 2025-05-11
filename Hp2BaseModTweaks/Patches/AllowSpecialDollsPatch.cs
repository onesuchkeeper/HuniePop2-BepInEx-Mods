// Hp2Sample 2021, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;

namespace Hp2BaseModTweaks
{
    [HarmonyPatch(typeof(UiDoll), nameof(UiDoll.LoadGirl))]
    internal static class AllowSpecialDollsPatch
    {
        //the special effects always read the def's offset field, which is annoying
        //because we can't store two different fields in the case of Kyu for her wings
        private static readonly FieldInfo _specialEffect = AccessTools.Field(typeof(UiDoll), "_specialEffect");
        private static void Postfix(UiDoll __instance)
        {
            if (__instance?.soulGirlDefinition == null
                || __instance.soulGirlDefinition.specialEffectPrefab == null)
            {
                return;
            }

            var expansion = ExpandedGirlDefinition.Get(__instance.soulGirlDefinition);
            if (__instance.soulGirlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
            {
                __instance.soulGirlDefinition.specialEffectOffset = expansion.BackPosition;
            }
            else if (__instance.soulGirlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
            {
                __instance.soulGirlDefinition.specialEffectOffset = expansion.HeadPosition;
            }

            if (__instance.soulGirlDefinition.specialCharacter) { return; }

            var specialEffectInstance = UnityEngine.Object.Instantiate(__instance.soulGirlDefinition.specialEffectPrefab);

            _specialEffect.SetValue(__instance, specialEffectInstance);
            specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
            specialEffectInstance.Init(__instance);
        }
    }
}
