using System.Collections.Generic;
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
    private static void LoadGirl(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).LoadGirl();

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
    public static void ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
        => ExpandedUiDoll.Get(__instance).ChangeHairstyle(ref hairstyleIndex);

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

    protected UiDoll _core;
    private UiDollSpecialEffect _specialEffect;
    private ExpandedUiDoll(UiDoll core)
    {
        _core = core;
    }

    public void LoadGirl()
    {
        //Non special girls wont load special effects by default
        if (_core?.soulGirlDefinition == null
            || _core.soulGirlDefinition.specialEffectPrefab == null)
        {
            return;
        }

        var expansion = ExpandedGirlDefinition.Get(_core.soulGirlDefinition);
        if (_core.girlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
        {
            _core.girlDefinition.specialEffectOffset = expansion.BackPosition;
        }
        else if (_core.girlDefinition.specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
        {
            _core.girlDefinition.specialEffectOffset = expansion.HeadPosition;
        }

        if (_core.soulGirlDefinition.specialCharacter) { return; }

        var specialEffectInstance = UnityEngine.Object.Instantiate(_core.soulGirlDefinition.specialEffectPrefab);

        f_specialEffect.SetValue(_core, specialEffectInstance);
        specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
        specialEffectInstance.Init(_core);

        //outfit is changed before specials are set, so correct specials here
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

    public void ChangeHairstyle(ref int hairstyleIndex)
    {
        var girlDef = f_girlDefinition.GetValue(_core) as GirlDefinition;

        if (girlDef == null) { return; }

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDef);
        var index = hairstyleIndex == -1
                ? playerFileGirl.outfitIndex
                : hairstyleIndex;

        index = Mathf.Clamp(index, 0, girlDef.hairstyles.Count - 1);

        var outfit = girlDef.hairstyles[index];
        var expansion = outfit.Expansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            hairstyleIndex = girlDef.defaultOutfitIndex;
            outfit = girlDef.hairstyles[hairstyleIndex];
            expansion = outfit.Expansion();
        }
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
