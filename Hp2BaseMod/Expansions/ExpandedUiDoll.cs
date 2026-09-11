using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedStyleDefinition"/> fields.
/// </summary>
[Expansion(typeof(UiDoll))]
public partial class ExpandedUiDoll
{
    [HarmonyPatch(typeof(UiDoll))]
    private class Patch
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
        private static void ChangeOutfit(UiDoll __instance, ref int outfitIndex)
            => ExpandedUiDoll.Get(__instance).ChangeOutfit(ref outfitIndex);

        [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
        [HarmonyPostfix()]
        private static void PostChangeOutfit(UiDoll __instance, int outfitIndex)
            => ExpandedUiDoll.Get(__instance).PostChangeOutfit();

        [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
        [HarmonyPrefix()]
        private static bool ChangeHairstyle(UiDoll __instance, ref int hairstyleIndex)
            => ExpandedUiDoll.Get(__instance).ChangeHairstyle(hairstyleIndex);

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix()]
        private static void OnDestroy(UiDoll __instance)
            => ExpandedUiDoll.Destroy(__instance);

        [HarmonyPatch(nameof(UiDoll.ShowEnergySurge))]
        [HarmonyPrefix]
        private static bool ShowEnergySurge(UiDoll __instance, EnergyDefinition energyDef, float duration, bool knockback, bool negative = false, bool silent = false)
        {
            // Safety - Skip showing surge is there is no definition
            return __instance.girlDefinition != null;
        }

        [HarmonyPatch(nameof(UiDoll.ReadDialogTrigger))]
        [HarmonyPrefix()]
        private static void ReadDialogTrigger(UiDoll __instance, DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, ref int lineIndex)
            => ExpandedUiDoll.Get(__instance).ReadDialogTrigger(dialogTriggerDef, format, ref lineIndex);

        [HarmonyPatch(nameof(UiDoll.SetExhaustion))]
        [HarmonyPrefix]
        private static void SetExhaustion_Prefix(UiDoll __instance, bool exhausted, bool upset, ref bool silent)
        {
            // Dialog lines are now handled by the IPuzzleStatusGirlState instances. They will play lines when needed
            silent = true;
        }
    }

    private UiDollSpecialEffect _specialEffect_hold;

    private void LoadGirl(GirlDefinition girlDef)
    {
        // Scale to match body
        var body = girlDef.GetExpansion().GetCurrentBody();

        if (body != null)
        {
            var vecScale = new Vector3(body.Scale, body.Scale, 1f);
            _core.partsLayer.localScale = vecScale;
            _core.outlineUiEffectGroup.transform.localScale = vecScale;
        }

        // Non-special girls won't load special effects by default
        if (_core?.soulGirlDefinition == null
            || _core.soulGirlDefinition.specialEffectPrefab == null)
        {
            return;
        }

        ModInterface.Log.Message($"Loading special effect {_core.soulGirlDefinition.specialEffectPrefab.name} for {_core.soulGirlDefinition.girlName}");

        var specialEffectPrefab = _core.soulGirlDefinition.specialEffectPrefab;

        if (specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectFairyWings))
        {
            _core.soulGirlDefinition.specialEffectOffset = body.BackPos;
        }
        else if (specialEffectPrefab.GetType() == typeof(UiDollSpecialEffectGloWings))
        {
            _core.soulGirlDefinition.specialEffectOffset = body.HeadPos;
        }
        else if (specialEffectPrefab is IUiDollSpecialEffect uiDollSpecialEffect)
        {
            _core.soulGirlDefinition.specialEffectOffset = uiDollSpecialEffect.GetSpecialEffectOffset(girlDef);
        }

        if (_core.soulGirlDefinition.specialCharacter) return;

        var specialEffectInstance = UnityEngine.Object.Instantiate(_core.soulGirlDefinition.specialEffectPrefab);

        _specialEffect = specialEffectInstance;
        specialEffectInstance.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
        specialEffectInstance.Init(_core);

        // outfit is changed before special effects are set, so correct specials here
        if (_core.soulGirlDefinition.outfits[f_currentOutfitIndex.GetValue<int>(_core)].GetExpansion().HideSpecial)
        {
            _specialEffect_hold = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect_hold?.rectTransform.SetParent(null, false);
        }
    }

    private void UnloadGirl()
    {
        if (_specialEffect_hold != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect_hold);
            _specialEffect_hold = null;
        }
    }

    private void ChangeOutfit(ref int outfitIndex)
    {
        if (_core.girlDefinition == null) return;

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

        var expansion = outfit.GetExpansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            ModInterface.Log.Message("Hiding NSFW outfit for censored mode");
            index = _core.girlDefinition.defaultOutfitIndex;
            outfit = _core.girlDefinition.outfits[index];
            expansion = outfit.GetExpansion();
        }

        if (expansion.HideSpecial && _specialEffect_hold == null)
        {
            _specialEffect_hold = f_specialEffect.GetValue<UiDollSpecialEffect>(_core);
            _specialEffect_hold?.rectTransform.SetParent(null, false);
        }
        else if (!expansion.HideSpecial && _specialEffect_hold != null)
        {
            _specialEffect_hold.rectTransform.SetParent(Game.Session.gameCanvas.dollSpecialEffectContainer, false);
            _specialEffect_hold = null;
        }

        outfitIndex = index;
    }

    /// <summary>
    /// After changing outfit, move it to the bottom of its layer
    /// </summary>
    private void PostChangeOutfit()
    {
        var girlExp = _core.girlDefinition.GetExpansion();
        RefreshSpecialParts(girlExp.HairstyleLookup[_core.currentHairstyleIndex],
            girlExp.OutfitLookup[_core.currentOutfitIndex]);
    }

    private bool ChangeHairstyle(int hairstyleIndex)
    {
        if (_core.girlDefinition == null) return true;

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

        var expansion = hairstyle.GetExpansion();

        if (!Game.Persistence.playerData.uncensored && expansion.IsNSFW)
        {
            hairstyleIndex = _core.girlDefinition.defaultHairstyleIndex;
            hairstyle = _core.girlDefinition.hairstyles[hairstyleIndex];
        }

        _currentHairstyleIndex = hairstyleIndex;

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

        var girlExp = _core.girlDefinition.GetExpansion();
        var hairId = girlExp.HairstyleLookup[_core.currentHairstyleIndex];
        var outfitId = girlExp.OutfitLookup[_core.currentOutfitIndex];

        RefreshSpecialParts(hairId, outfitId);

        return false;
    }

    private void RefreshSpecialParts(RelativeId hairId, RelativeId outfitId)
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
            var specialPartExpansion = part.GetExpansion();
            if ((specialPartExpansion.RequiredHairstyles?.Any() == true
                    && !specialPartExpansion.RequiredHairstyles.Contains(hairId))
                || (specialPartExpansion.RequiredOutfits?.Any() == true
                    && !specialPartExpansion.RequiredOutfits.Contains(outfitId)))
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

    private void OnDestroy()
    {
        if (_specialEffect_hold != null)
        {
            UnityEngine.GameObject.Destroy(_specialEffect_hold);
            _specialEffect_hold = null;
        }
    }

    private void ReadDialogTrigger(DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, ref int lineIndex)
    {
        var girlId = _core.girlDefinition.ModId();

        if (!dialogTriggerDef.GetExpansion().TryGetLineSet(dialogTriggerDef, girlId, out var lineSet))
        {
            throw new Exception($"Failed to find dt line set {dialogTriggerDef.ModId()}");
        }

        if (!lineSet.dialogLines.Any())
        {
            throw new Exception($"dt line set {dialogTriggerDef.ModId()} has no lines");
        }

        // the normal index used is the index of the current location in Game.Session.Location.dateLocationDefs
        // instead we go by identifier
        if (lineIndex <= -1
            && dialogTriggerDef.forceType == DialogTriggerForceType.DATE_LOCATION)
        {
            var girlExp = _core.girlDefinition.GetExpansion();
            var dtExp = dialogTriggerDef.GetExpansion();
            var locId = Game.Session.Location.currentLocation.ModId();
            lineIndex = girlExp.DateGreetingLocIdToDtIndex[locId];
            ModInterface.Log.Message($"Using index {lineIndex} for date greeting at location {locId} - {Game.Session.Location.currentLocation.locationName}");
        }

        // when no valid index is specified, pick a random one
        if (lineIndex <= -1
            || lineSet.dialogLines.Count <= lineIndex
            || lineSet.dialogLines[lineIndex] == null)
        {
            var i = 0;
            lineIndex = lineSet.dialogLines.Select(line => (line, i++)).Where(x => x.line != null).ToArray().GetRandom().Item2;
            ModInterface.Log.Message($"Using random index {lineIndex} to replace invalid dt index");
        }
    }
}
