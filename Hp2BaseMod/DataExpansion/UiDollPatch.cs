using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    private static readonly FieldInfo _girlDefinition = AccessTools.Field(typeof(UiDoll), "_girlDefinition");
    private static readonly FieldInfo _currentHairstyleIndex = AccessTools.Field(typeof(UiDoll), "_currentHairstyleIndex");
    private static readonly MethodInfo m_LoadPart = AccessTools.Method(typeof(UiDoll), "LoadPart");
    private static readonly MethodInfo m_GetDollPartByType = AccessTools.Method(typeof(UiDoll), "GetDollPartByType");

    [HarmonyPatch(nameof(UiDoll.LoadGirl))]
    [HarmonyPrefix]
    public static void LoadGirl(UiDoll __instance, GirlDefinition girlDef, int expressionIndex, int hairstyleIndex, int outfitIndex, GirlDefinition soulGirlDef)
    {
        var body = girlDef.Expansion().GetBody();

        if (body != null)
        {
            var vecScale = new Vector3(body.Scale, body.Scale, 1f);
            __instance.partsLayer.localScale = vecScale;
            __instance.outlineUiEffectGroup.transform.localScale = vecScale;
        }
    }

    [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
    [HarmonyPrefix]
    public static bool ChangeHairstyle(UiDoll __instance, int hairstyleIndex)
    {
        var girlDef = __instance.girlDefinition;
        if (girlDef == null)
        {
            return true;
        }

        PlayerFileGirl playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);

        var workingIndex = hairstyleIndex == -1
            ? playerFileGirl.hairstyleIndex
            : hairstyleIndex;

        workingIndex = Mathf.Clamp(workingIndex, 0, girlDef.hairstyles.Count - 1);

        _currentHairstyleIndex.SetValue(__instance, workingIndex);

        if (workingIndex < 0 || workingIndex >= girlDef.hairstyles.Count)
        {
            return true;
        }

        var girlHairstyleSubDefinition = girlDef.hairstyles[workingIndex];
        m_LoadPart.Invoke(__instance, [__instance.partBackhair, girlHairstyleSubDefinition.partIndexBackhair, -1f]);
        m_LoadPart.Invoke(__instance, [__instance.partFronthair, girlHairstyleSubDefinition.partIndexFronthair, -1f]);

        foreach (var partSpecial in __instance.partSpecials)
        {
            partSpecial.StopAnimation();
            partSpecial.dollPart.rectTransform.SetSiblingIndex(0);
            partSpecial.dollPart.UnloadPart();
        }

        // if (girlHairstyleSubDefinition.hideSpecials)
        // {
        //     return;
        // }

        //make new parts if needed
        if (girlDef.specialParts.Count > __instance.partSpecials.Length)
        {
            var newParts = new List<UiDollPartSpecial>(__instance.partSpecials);
            var sample = __instance.partSpecials[0];

            for (int i = girlDef.specialParts.Count - __instance.partSpecials.Length; i < 0; i--)
            {
                newParts.Add(GameObject.Instantiate(sample));
            }

            __instance.partSpecials = newParts.ToArray();
        }

        var girlExpansion = girlDef.Expansion();
        var hairId = girlExpansion.HairstyleIndexToId[workingIndex];

        int j = 0;
        foreach (var part in girlDef.specialParts)
        {
            //make sure special part is allowed
            //empty or null allows all, otherwise whitelist
            var specialPartExpansion = part.Expansion();
            if (specialPartExpansion.RequiredHairstyles != null
                && specialPartExpansion.RequiredHairstyles.Any()
                && !specialPartExpansion.RequiredHairstyles.Contains(hairId))
            {
                continue;
            }

            m_LoadPart.Invoke(__instance, [__instance.partSpecials[j].dollPart, part.partIndexSpecial, -1f]);

            if (part.sortingPartType != GirlPartType.SPECIAL1
                && part.sortingPartType != GirlPartType.SPECIAL2
                && part.sortingPartType != GirlPartType.SPECIAL3)
            {
                var sibling = (UiDollPart)m_GetDollPartByType.Invoke(__instance, [part.sortingPartType]);
                __instance.partSpecials[j].dollPart.rectTransform.SetSiblingIndex(sibling.rectTransform.GetSiblingIndex());
            }

            __instance.partSpecials[j].StartAnimation(part.animType);

            j++;
        }

        return false;
    }
}
