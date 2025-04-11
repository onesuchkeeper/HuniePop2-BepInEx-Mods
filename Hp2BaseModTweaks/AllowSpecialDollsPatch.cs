// Hp2Sample 2021, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;

namespace Hp2BaseModTweaks
{
    /// <summary>
    /// It didn't like me adding a prefix to load girl for whatever reason, but change outfit works just as well I guess...
    /// </summary>
    [HarmonyPatch(typeof(UiDoll), "ChangeOutfit")]
    public static class AllowSpecialDollsPatch
    {
        private static readonly FieldInfo _specialEffect = AccessTools.Field(typeof(UiDoll), "_specialEffect");
        private static void Postfix(UiDoll __instance)
        {
            if (__instance.soulGirlDefinition?.specialCharacter ?? true) { return; }

            if (_specialEffect.GetValue(__instance) as UiDollSpecialEffect == null
                && __instance.soulGirlDefinition?.specialEffectPrefab != null)
            {
                var specialEffectInstance = UnityEngine.Object.Instantiate(__instance.soulGirlDefinition.specialEffectPrefab);

                _specialEffect.SetValue(__instance, specialEffectInstance);
                specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
                specialEffectInstance.Init(__instance);
            }
        }
    }
}
