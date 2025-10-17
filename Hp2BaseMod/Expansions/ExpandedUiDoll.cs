using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;
[HarmonyPatch(typeof(UiDoll))]
class UiDoll_ChangeStyle
{
    [HarmonyPatch(nameof(UiDoll.LoadGirl))]
    [HarmonyPostfix]
    private static void LoadGirl(UiDoll __instance, GirlDefinition girlDef, int expressionIndex, int hairstyleIndex, int outfitIndex, GirlDefinition soulGirlDef)
        => ExpandedUiDoll.Get(__instance).LoadGirl(girlDef);

    [HarmonyPatch(nameof(UiDoll.UnloadGirl))]
    [HarmonyPostfix]
    private static void UnloadGirl(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).UnloadGirl();

    [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
    [HarmonyPrefix()]
    public static void ChangeOutfit(UiDoll __instance, ref int outfitIndex)
        => ExpandedUiDoll.Get(__instance).ChangeOutfit(ref outfitIndex);

    [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
    [HarmonyPrefix()]
    public static bool ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
        => ExpandedUiDoll.Get(__instance).ChangeHairstyle(hairstyleIndex);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix()]
    public static void OnDestroy(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).OnDestroy();
}

/// <summary>
/// Handles <see cref="ExpandedStyleDefinition"/> fields.
/// </summary>
public class ExpandedUiDoll
{
    private static Dictionary<UiDoll, ExpandedUiDoll> _expansions
        = new Dictionary<UiDoll, ExpandedUiDoll>();

    public static ExpandedUiDoll Get(UiDoll core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiDoll(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_specialEffect = AccessTools.Field(typeof(UiDoll), "_specialEffect");
    private static readonly FieldInfo f_girlDefinition = AccessTools.Field(typeof(UiDoll), "_girlDefinition");
    private static readonly FieldInfo f_currentOutfitIndex = AccessTools.Field(typeof(UiDoll), "_currentOutfitIndex");
    private static readonly FieldInfo f_currentHairstyleIndex = AccessTools.Field(typeof(UiDoll), "_currentHairstyleIndex");

    private static readonly MethodInfo m_LoadPart = AccessTools.Method(typeof(UiDoll), "LoadPart");
    private static readonly MethodInfo m_GetDollPartByType = AccessTools.Method(typeof(UiDoll), "GetDollPartByType");

    protected UiDoll _core;
    private UiDollSpecialEffect _specialEffect;
    private ExpandedUiDoll(UiDoll core)
    {
        _core = core;
    }

    public void LoadGirl(GirlDefinition girlDef)
    {
        //scale to match body
        var body = girlDef.Expansion().GetCurrentBody();

        if (body != null)
        {
            var vecScale = new Vector3(body.Scale, body.Scale, 1f);
            _core.partsLayer.localScale = vecScale;
            _core.outlineUiEffectGroup.transform.localScale = vecScale;
        }

        //Non special girls wont load special effects by default
        if (_core?.soulGirlDefinition == null
            || _core.soulGirlDefinition.specialEffectPrefab == null)
        {
            return;
        }

        if (_core.soulGirlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
        {
            _core.soulGirlDefinition.specialEffectOffset = body.BackPos;
        }
        else if (_core.soulGirlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
        {
            _core.soulGirlDefinition.specialEffectOffset = body.HeadPos;
        }

        if (_core.soulGirlDefinition.specialCharacter) { return; }

        var specialEffectInstance = UnityEngine.Object.Instantiate(_core.soulGirlDefinition.specialEffectPrefab);

        f_specialEffect.SetValue(_core, specialEffectInstance);
        specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
        specialEffectInstance.Init(_core);

        //outfit is changed before special effects are set, so correct specials here
        if (_core.soulGirlDefinition.outfits[f_currentOutfitIndex.GetValue<int>(_core)].Expansion().HideSpecial)
        {
            _specialEffect = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect?.rectTransform.SetParent(null, false);
        }
    }

    internal void UnloadGirl()
    {
        if (_specialEffect != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect);
            _specialEffect = null;
        }
    }

    public void ChangeOutfit(ref int outfitIndex)
    {
        var girlDef = f_girlDefinition.GetValue(_core) as GirlDefinition;

        if (girlDef == null) { return; }

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);
        var index = outfitIndex == -1
                ? playerFileGirl.outfitIndex
                : outfitIndex;

        index = Mathf.Clamp(index, 0, girlDef.outfits.Count - 1);

        var outfit = girlDef.outfits[index];

        if (outfit == null)
        {
            index = girlDef.defaultOutfitIndex;
            outfit = girlDef.outfits[index];
        }

        var expansion = outfit.Expansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            outfitIndex = girlDef.defaultOutfitIndex;
            outfit = girlDef.outfits[outfitIndex];
            expansion = outfit.Expansion();
        }

        if (expansion.HideSpecial && _specialEffect == null)
        {
            _specialEffect = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect?.rectTransform.SetParent(null, false);
        }
        else if (!expansion.HideSpecial && _specialEffect != null)
        {
            _specialEffect.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
            _specialEffect = null;
        }
    }

    public bool ChangeHairstyle(int hairstyleIndex)
    {
        if (_core.girlDefinition == null) { return true; }

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_core.girlDefinition);

        // clean input
        hairstyleIndex = hairstyleIndex == -1
            ? playerFileGirl.outfitIndex
            : hairstyleIndex;

        hairstyleIndex = Mathf.Clamp(hairstyleIndex, 0, _core.girlDefinition.hairstyles.Count - 1);

        var hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];

        if (hairstyle == null)
        {
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
            hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];
        }

        var expansion = hairstyle.Expansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
            hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];
        }

        f_currentHairstyleIndex.SetValue(_core, hairstyleIndex);

        // load hair
        m_LoadPart.Invoke(_core, [_core.partBackhair, hairstyle.partIndexBackhair, -1f]);
        m_LoadPart.Invoke(_core, [_core.partFronthair, hairstyle.partIndexFronthair, -1f]);

        // special parts
        // make new parts if needed
        if (_core.girlDefinition.specialParts.Count > _core.partSpecials.Length)
        {
            var newParts = new List<UiDollPartSpecial>(_core.partSpecials);
            var sample = _core.partSpecials[0];

            for (int i = _core.girlDefinition.specialParts.Count - _core.partSpecials.Length; i < 0; i--)
            {
                newParts.Add(GameObject.Instantiate(sample));
            }

            _core.partSpecials = newParts.ToArray();
        }

        var hairId = _core.girlDefinition.Expansion().HairstyleIndexToId[_core.currentHairstyleIndex];

        int j = 0;
        foreach (var part in _core.girlDefinition.specialParts)
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

            m_LoadPart.Invoke(_core, [_core.partSpecials[j].dollPart, part.partIndexSpecial, -1f]);

            if (part.sortingPartType != GirlPartType.SPECIAL1
                && part.sortingPartType != GirlPartType.SPECIAL2
                && part.sortingPartType != GirlPartType.SPECIAL3)
            {
                var sibling = (UiDollPart)m_GetDollPartByType.Invoke(_core, [part.sortingPartType]);
                _core.partSpecials[j].dollPart.rectTransform.SetSiblingIndex(sibling.rectTransform.GetSiblingIndex());
            }

            _core.partSpecials[j].StartAnimation(part.animType);

            j++;
        }

        return false;
    }

    internal void OnDestroy()
    {
        if (_specialEffect != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect);
            _specialEffect = null;
        }

        _expansions.Remove(_core);
    }
}
