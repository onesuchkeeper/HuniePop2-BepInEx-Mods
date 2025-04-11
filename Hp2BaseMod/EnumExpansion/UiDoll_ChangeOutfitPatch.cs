// Hp2RepeatThreesomeMod 2021, By onesuchkeeper

using System;
using HarmonyLib;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod.EnumExpansion
{
    [HarmonyPatch(typeof(UiDoll), "ChangeOutfit")]
    class UiDoll_ChangeOutfitPatch
    {
        public static bool Prefix(UiDoll __instance, ref int outfitIndex)
        {
            try
            {
                var girlDef = AccessTools.Field(typeof(UiDoll), "_girlDefinition").GetValue(__instance) as GirlDefinition;

                if (girlDef == null) { return true; }

                if (!Game.Persistence.playerData.uncensored)
                {
                    var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);

                    outfitIndex = (outfitIndex == -1) ? playerFileGirl.outfitIndex : outfitIndex;
                    var uiDollType = typeof(UiDoll);

                    AccessTools.Field(uiDollType, "_currentOutfitIndex").SetValue(__instance, outfitIndex);

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
                                AccessTools.Method(uiDollType, "LoadPart").Invoke(__instance, new object[] { __instance.partOutfit, outfit.partIndexOutfit, -1f });

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
                ModInterface.Log.LogError(e.ToString());
            }

            return true;
        }
    }
}
