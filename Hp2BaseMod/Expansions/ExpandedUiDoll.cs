using System;
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

    [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
    [HarmonyPostfix()]
    public static void PostChangeOutfit(UiDoll __instance, int outfitIndex)
        => ExpandedUiDoll.Get(__instance).PostChangeOutfit();

    [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
    [HarmonyPrefix()]
    public static bool ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
        => ExpandedUiDoll.Get(__instance).ChangeHairstyle(hairstyleIndex);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix()]
    public static void OnDestroy(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).OnDestroy();

    [HarmonyPatch(nameof(UiDoll.ShowEnergySurge))]
    [HarmonyPrefix]
    public static bool ShowEnergySurge(UiDoll __instance, EnergyDefinition energyDef, float duration, bool knockback, bool negative = false, bool silent = false)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiDoll.ReadDialogTrigger))]
    [HarmonyPrefix()]
    public static void ReadDialogTrigger(UiDoll __instance, DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, ref int lineIndex)
        => ExpandedUiDoll.Get(__instance).ReadDialogTrigger(dialogTriggerDef, ref lineIndex);
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

        ModInterface.Log.Message($"Loading special effect {_core.soulGirlDefinition.specialEffectPrefab.name} for {_core.soulGirlDefinition.girlName}");

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

        // outfit is changed before special effects are set, so correct specials here
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
        if (_core.girlDefinition == null) { return; }

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_core.girlDefinition);
        var index = outfitIndex == -1
                ? playerFileGirl.outfitIndex
                : outfitIndex;

        if (index < 0 || index >= _core.girlDefinition.outfits.Count)
        {
            ModInterface.Log.Message($"out of range outfit index {index}/{_core.girlDefinition.outfits.Count} changed to default {_core.girlDefinition.defaultOutfitIndex}.");
            index = _core.girlDefinition.defaultOutfitIndex;
        }

        var outfit = _core.girlDefinition.outfits[index];

        if (outfit == null)
        {
            index = _core.girlDefinition.defaultOutfitIndex;
            outfit = _core.girlDefinition.outfits[index];
        }

        var expansion = outfit.Expansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            ModInterface.Log.Message("Hiding NSFW outfit for censored mode");
            index = _core.girlDefinition.defaultOutfitIndex;
            outfit = _core.girlDefinition.outfits[index];
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

        outfitIndex = index;
    }

    /// <summary>
    /// After changing outfit, move it to the bottom of its layer
    /// </summary>
    public void PostChangeOutfit()
    {
        RefreshSpecialParts(_core.girlDefinition.Expansion().HairstyleLookup[_core.currentHairstyleIndex]);
    }

    public bool ChangeHairstyle(int hairstyleIndex)
    {
        if (_core.girlDefinition == null) { return true; }

        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_core.girlDefinition);

        // clean input
        hairstyleIndex = hairstyleIndex == -1
            ? playerFileGirl.hairstyleIndex
            : hairstyleIndex;

        if (hairstyleIndex < 0 || hairstyleIndex >= _core.girlDefinition.hairstyles.Count)
        {
            ModInterface.Log.Message($"out of range hairstyle index {hairstyleIndex}/{_core.girlDefinition.hairstyles.Count} changed to default {_core.girlDefinition.defaultHairstyleIndex}");
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
        }

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

            for (int i = _core.girlDefinition.specialParts.Count - _core.partSpecials.Length; i > 0; i--)
            {
                newParts.Add(GameObject.Instantiate(sample));
            }

            _core.partSpecials = newParts.ToArray();
        }

        var hairId = _core.girlDefinition.Expansion().HairstyleLookup[_core.currentHairstyleIndex];

        RefreshSpecialParts(hairId);

        return false;
    }

    private void RefreshSpecialParts(RelativeId hairId)
    {
        int i;
        for (i = 0; i < _core.partSpecials.Length; i++)
        {
            _core.partSpecials[i].StopAnimation();
            _core.partSpecials[i].dollPart.rectTransform.SetSiblingIndex(0);
            _core.partSpecials[i].dollPart.UnloadPart();
        }

        i = 0;
        foreach (var part in _core.girlDefinition.specialParts)
        {
            // make sure special part is allowed
            // empty or null allows all, otherwise whitelist
            var specialPartExpansion = part.Expansion();
            if (specialPartExpansion.RequiredHairstyles != null
                && specialPartExpansion.RequiredHairstyles.Any()
                && !specialPartExpansion.RequiredHairstyles.Contains(hairId))
            {
                continue;
            }

            m_LoadPart.Invoke(_core, [_core.partSpecials[i].dollPart, part.partIndexSpecial, -1f]);

            if (part.sortingPartType != GirlPartType.SPECIAL1
                && part.sortingPartType != GirlPartType.SPECIAL2
                && part.sortingPartType != GirlPartType.SPECIAL3)
            {
                var sibling = (UiDollPart)m_GetDollPartByType.Invoke(_core, [part.sortingPartType]);
                _core.partSpecials[i].dollPart.rectTransform.SetSiblingIndex(sibling.rectTransform.GetSiblingIndex());
            }

            _core.partSpecials[i].StartAnimation(part.animType);

            i++;
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

    internal void ReadDialogTrigger(DialogTriggerDefinition dialogTriggerDef, ref int lineIndex)
    {
        // when no index is specified, pick a random one that isn't null
        if (!dialogTriggerDef.Expansion()
            .TryGetLineSet(dialogTriggerDef, ModInterface.Data.GetDataId(GameDataType.Girl, _core.girlDefinition.id), out var lineSet))
        {
            throw new Exception("Failed to find dt line set");
        }

        if (lineIndex > -1
            && lineSet.dialogLines.Count > lineIndex
            && lineSet.dialogLines[lineIndex] != null)
        {
            return;
        }

        var i = 0;
        lineIndex = lineSet.dialogLines.Select(line => (line, i++)).Where(x => x.line != null).ToArray().GetRandom().Item2;
    }
}
