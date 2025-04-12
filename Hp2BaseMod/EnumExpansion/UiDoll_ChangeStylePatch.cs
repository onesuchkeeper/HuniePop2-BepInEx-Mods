// Hp2RepeatThreesomeMod 2021, By onesuchkeeper

using System;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod.EnumExpansion
{
    [HarmonyPatch(typeof(UiDoll))]
    class UiDoll_ChangeOutfitPatch
    {
        private static readonly FieldInfo _girlDefinition = AccessTools.Field(typeof(UiDoll), "_girlDefinition");
        private static readonly FieldInfo _currentOutfitIndex = AccessTools.Field(typeof(UiDoll), "_currentOutfitIndex");
        private static readonly FieldInfo _currentHairstyleIndex = AccessTools.Field(typeof(UiDoll), "_currentHairstyleIndex");
        private static readonly MethodInfo _loadPart = AccessTools.Method(typeof(UiDoll), "LoadPart");
        private static readonly MethodInfo _getDollPartByType = AccessTools.Method(typeof(UiDoll), "GetDollPartByType");

        [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
        [HarmonyPrefix()]
        public static bool ChangeOutfit(UiDoll __instance, ref int outfitIndex)
        {
            try
            {
                var girlDef = _girlDefinition.GetValue(__instance) as GirlDefinition;

                if (girlDef == null) { return true; }

                if (!Game.Persistence.playerData.uncensored)
                {
                    var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);

                    outfitIndex = outfitIndex == -1
                        ? playerFileGirl.outfitIndex
                        : outfitIndex;

                    _currentOutfitIndex.SetValue(__instance, outfitIndex);

                    outfitIndex = Mathf.Clamp(outfitIndex, 0, girlDef.outfits.Count - 1);

                    var outfit = girlDef.outfits[outfitIndex];

                    // when censored, don't change to nsfw outfits
                    if (outfit is ExpandedOutfitDefinition expandedOutfit
                        && expandedOutfit.IsNSFW)
                    {
                        // if already nsfw, change to default
                        if (__instance.currentOutfitIndex > -1)
                        {
                            var currentOutfit = girlDef.outfits[__instance.currentOutfitIndex];

                            if (currentOutfit is ExpandedOutfitDefinition currentExpandedOutfit
                                && currentExpandedOutfit.IsNSFW)
                            {
                                outfit = girlDef.outfits[girlDef.defaultOutfitIndex];
                                _loadPart.Invoke(__instance, [__instance.partOutfit, outfit.partIndexOutfit, -1f]);

                                if (outfit.hideNipples)
                                {
                                    __instance.partNipples.Hide();
                                }
                                else
                                {
                                    __instance.partNipples.Show();
                                }
                            }
                        }

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError("Outfit change", e);
            }

            return true;
        }

        [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
        [HarmonyPrefix()]
        public static bool ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
        {
            try
            {
                var girlDefinition = _girlDefinition.GetValue(__instance) as GirlDefinition;

                if (girlDefinition != null)
                {
                    var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDefinition);

                    hairstyleIndex = (hairstyleIndex == -1) ? playerFileGirl.hairstyleIndex : hairstyleIndex;
                    hairstyleIndex = Mathf.Clamp(hairstyleIndex, 0, girlDefinition.hairstyles.Count - 1);

                    _currentHairstyleIndex.SetValue(__instance, hairstyleIndex);

                    var girlHairstyleSubDefinition = girlDefinition.hairstyles[hairstyleIndex];

                    _loadPart.Invoke(__instance, [__instance.partBackhair, girlHairstyleSubDefinition.partIndexBackhair, -1f]);
                    _loadPart.Invoke(__instance, [__instance.partFronthair, girlHairstyleSubDefinition.partIndexFronthair, -1f]);

                    for (int i = 0; i < __instance.partSpecials.Length; i++)
                    {
                        __instance.partSpecials[i].StopAnimation();
                        __instance.partSpecials[i].dollPart.rectTransform.SetSiblingIndex(0);
                        __instance.partSpecials[i].dollPart.UnloadPart();
                    }

                    if (!girlHairstyleSubDefinition.hideSpecials)
                    {
                        for (int j = 0; j < girlDefinition.specialParts.Count; j++)
                        {
                            _loadPart.Invoke(__instance, [__instance.partSpecials[j].dollPart, girlDefinition.specialParts[j].partIndexSpecial, -1f]);
                            if (girlDefinition.specialParts[j].sortingPartType != GirlPartType.SPECIAL1 && girlDefinition.specialParts[j].sortingPartType != GirlPartType.SPECIAL2 && girlDefinition.specialParts[j].sortingPartType != GirlPartType.SPECIAL3)
                            {
                                __instance.partSpecials[j].dollPart.rectTransform.SetSiblingIndex((_getDollPartByType.Invoke(__instance, [girlDefinition.specialParts[j].sortingPartType]) as UiDollPart).rectTransform.GetSiblingIndex());
                            }
                            __instance.partSpecials[j].StartAnimation(girlDefinition.specialParts[j].animType);
                        }
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError("Hairstyle change", e);
            }

            return true;
        }
    }
}
