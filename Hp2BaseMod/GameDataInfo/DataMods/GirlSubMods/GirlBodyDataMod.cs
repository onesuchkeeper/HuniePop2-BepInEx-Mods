using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlBodySubDefinition"/>.
    /// </summary>
    public class GirlBodyDataMod : DataMod, IGirlBodyDataMod
    {
        public string name;

        public List<IGirlSubDataMod<GirlExpressionSubDefinition>> expressions;
        public List<IGirlSubDataMod<GirlHairstyleSubDefinition>> hairstyles;
        public List<IGirlSubDataMod<GirlOutfitSubDefinition>> outfits;
        public List<IGirlSubDataMod<GirlSpecialPartSubDefinition>> specialParts;

        public string SpecialEffectName;

        public IGameDefinitionInfo<Vector2> HeadPosition;
        public IGameDefinitionInfo<Vector2> BackPosition;
        public IGameDefinitionInfo<Vector2> BreathEmitterPos;
        public IGameDefinitionInfo<Vector2> UpsetEmitterPos;

        public IGirlSubDataMod<GirlPartSubDefinition> PartBody;
        public IGirlSubDataMod<GirlPartSubDefinition> PartNipples;
        public IGirlSubDataMod<GirlPartSubDefinition> PartBlushLight;
        public IGirlSubDataMod<GirlPartSubDefinition> PartBlushHeavy;
        public IGirlSubDataMod<GirlPartSubDefinition> PartBlink;
        public IGirlSubDataMod<GirlPartSubDefinition> PartMouthNeutral;

        public int? DefaultExpressionIndex;
        public int? FailureExpressionIndex;
        public RelativeId? DefaultHairstyleId;
        public RelativeId? DefaultOutfitId;

        public IGirlSubDataMod<GirlPartSubDefinition> Phonemes_aeil;
        public IGirlSubDataMod<GirlPartSubDefinition> Phonemes_neutral;
        public IGirlSubDataMod<GirlPartSubDefinition> Phonemes_oquw;
        public IGirlSubDataMod<GirlPartSubDefinition> Phonemes_fv;
        public IGirlSubDataMod<GirlPartSubDefinition> Phonemes_other;
        public IGirlSubDataMod<GirlPartSubDefinition> PhonemesTeeth_aeil;
        public IGirlSubDataMod<GirlPartSubDefinition> PhonemesTeeth_neutral;
        public IGirlSubDataMod<GirlPartSubDefinition> PhonemesTeeth_oquw;
        public IGirlSubDataMod<GirlPartSubDefinition> PhonemesTeeth_fv;
        public IGirlSubDataMod<GirlPartSubDefinition> PhonemesTeeth_other;

        public float? Scale;

        /// <inheritdoc/>
        public GirlBodyDataMod() { }

        public GirlBodyDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal GirlBodyDataMod(GirlDefinition def, AssetProvider assetProvider)
        : base(new RelativeId(-1, 0), InsertStyle.replace, 0)
        {
            assetProvider.NameAndAddAsset(ref SpecialEffectName, def.specialEffectPrefab);

            DefaultExpressionIndex = def.defaultExpressionIndex;
            FailureExpressionIndex = def.failureExpressionIndex;

            PartBody = new GirlPartDataMod(def.partIndexBody, assetProvider, def);
            PartNipples = new GirlPartDataMod(def.partIndexNipples, assetProvider, def);
            PartBlushLight = new GirlPartDataMod(def.partIndexBlushLight, assetProvider, def);
            PartBlushHeavy = new GirlPartDataMod(def.partIndexBlushHeavy, assetProvider, def);
            PartBlink = new GirlPartDataMod(def.partIndexBlink, assetProvider, def);
            PartMouthNeutral = new GirlPartDataMod(def.partIndexMouthNeutral, assetProvider, def);

            if (def.partIndexesPhonemes != null)
            {
                var it = def.partIndexesPhonemes.GetEnumerator();
                it.MoveNext();
                Phonemes_aeil = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                Phonemes_neutral = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                Phonemes_oquw = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                Phonemes_fv = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                Phonemes_other = new GirlPartDataMod(it.Current, assetProvider, def);
            }

            if (def.partIndexesPhonemesTeeth != null)
            {
                var it = def.partIndexesPhonemesTeeth.GetEnumerator();
                it.MoveNext();
                PhonemesTeeth_aeil = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                PhonemesTeeth_neutral = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                PhonemesTeeth_oquw = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                PhonemesTeeth_fv = new GirlPartDataMod(it.Current, assetProvider, def);
                it.MoveNext();
                PhonemesTeeth_other = new GirlPartDataMod(it.Current, assetProvider, def);
            }

            DefaultHairstyleId = new RelativeId(-1, def.defaultHairstyleIndex);
            DefaultOutfitId = new RelativeId(-1, def.defaultOutfitIndex);

            if (def.breathEmitterPos != null) { BreathEmitterPos = new VectorInfo(def.breathEmitterPos); }
            if (def.upsetEmitterPos != null) { UpsetEmitterPos = new VectorInfo(def.upsetEmitterPos); }

            if (def.specialEffectOffset != null)
            {
                //Kyu's specialEffectOffset is for her back not her head
                if (def.id == Girls.KyuId.LocalId)
                {
                    BackPosition = new VectorInfo(def.specialEffectOffset);
                }
                else
                {
                    HeadPosition = new VectorInfo(def.specialEffectOffset);
                }
            }

            int i;
            if (def.expressions != null)
            {
                i = 0;
                expressions = def.expressions
                    .Select(x => (IGirlSubDataMod<GirlExpressionSubDefinition>)new GirlExpressionDataMod(i++, assetProvider, def))
                    .ToList();
            }

            if (def.outfits != null)
            {
                i = 0;
                outfits = def.outfits
                    .Select(x => (IGirlSubDataMod<GirlOutfitSubDefinition>)new OutfitDataMod(i++, def, assetProvider))
                    .ToList();
            }

            if (def.hairstyles != null)
            {
                i = 0;
                hairstyles = def.hairstyles
                    .Select(x => (IGirlSubDataMod<GirlHairstyleSubDefinition>)new HairstyleDataMod(i++, def, assetProvider))
                    .ToList();
            }

            if (def.specialParts != null)
            {
                i = 0;
                specialParts = def.specialParts
                    .Select(x => (IGirlSubDataMod<GirlSpecialPartSubDefinition>)new GirlSpecialPartDataMod(i++, def, assetProvider))
                    .ToList();
            }
        }

        /// <inheritdoc/>
        public void SetData(ref GirlBodySubDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId,
                            GirlDefinition girlDef)
        {
            def ??= new();
            var expansion = girlDef.Expansion();

            ValidatedSet.SetValue(ref def.Scale, Scale);
            ValidatedSet.SetValue(ref def.PartIndexBody, expansion.PartIdToIndex, PartBody?.Id);
            ValidatedSet.SetValue(ref def.PartIndexNipples, expansion.PartIdToIndex, PartNipples?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlushLight, expansion.PartIdToIndex, PartBlushLight?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlushHeavy, expansion.PartIdToIndex, PartBlushHeavy?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlink, expansion.PartIdToIndex, PartBlink?.Id);
            ValidatedSet.SetValue(ref def.PartIndexMouthNeutral, expansion.PartIdToIndex, PartMouthNeutral?.Id);

            ValidatedSet.SetValue(ref def.DefaultExpressionIndex, DefaultExpressionIndex);
            ValidatedSet.SetValue(ref def.FailureExpressionIndex, FailureExpressionIndex);

            ValidatedSet.SetValue(ref def.DefaultHairstyleIndex, expansion.HairstyleIdToIndex, DefaultHairstyleId);
            ValidatedSet.SetValue(ref def.DefaultOutfitIndex, expansion.OutfitIdToIndex, DefaultOutfitId);

            var partIdsPhonemes = new[]
            {
                Phonemes_aeil,
                Phonemes_neutral,
                Phonemes_oquw,
                Phonemes_fv,
                Phonemes_other
            };

            ValidatedSet.SetListValue(ref def.PartIndexesPhonemes, partIdsPhonemes?
                .Select(x => x != null ? (int?)expansion.PartIdToIndex[x.Id] : null), InsertStyle.replace);

            var partIdsPhonemesTeeth = new[]
            {
                PhonemesTeeth_aeil,
                PhonemesTeeth_neutral,
                PhonemesTeeth_oquw,
                PhonemesTeeth_fv,
                PhonemesTeeth_other
            };

            ValidatedSet.SetListValue(ref def.PartIndexesPhonemesTeeth, partIdsPhonemesTeeth
                .Select(x => x != null ? (int?)expansion.PartIdToIndex[x.Id] : null), InsertStyle.replace);

            ValidatedSet.SetValue(ref def.BreathEmitterPos, BreathEmitterPos, InsertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.UpsetEmitterPos, UpsetEmitterPos, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.SpecialEffectOffset, HeadPosition, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.HeadPos, HeadPosition, InsertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.BackPos, BackPosition, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.SpecialEffectPrefab, assetProvider.GetInternalAsset<UiDollSpecialEffect>(SpecialEffectName), InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            expressions?.ForEach(x => x?.RequestInternals(assetProvider));
            hairstyles?.ForEach(x => x?.RequestInternals(assetProvider));
            outfits?.ForEach(x => x?.RequestInternals(assetProvider));
            HeadPosition?.RequestInternals(assetProvider);
            BackPosition?.RequestInternals(assetProvider);
            BreathEmitterPos?.RequestInternals(assetProvider);
            UpsetEmitterPos?.RequestInternals(assetProvider);

            PartBody?.RequestInternals(assetProvider);
            PartNipples?.RequestInternals(assetProvider);
            PartBlushLight?.RequestInternals(assetProvider);
            PartBlushHeavy?.RequestInternals(assetProvider);
            PartBlink?.RequestInternals(assetProvider);
            PartMouthNeutral?.RequestInternals(assetProvider);
            Phonemes_aeil?.RequestInternals(assetProvider);
            Phonemes_neutral?.RequestInternals(assetProvider);
            Phonemes_oquw?.RequestInternals(assetProvider);
            Phonemes_fv?.RequestInternals(assetProvider);
            Phonemes_other?.RequestInternals(assetProvider);
            PhonemesTeeth_aeil?.RequestInternals(assetProvider);
            PhonemesTeeth_neutral?.RequestInternals(assetProvider);
            PhonemesTeeth_oquw?.RequestInternals(assetProvider);
            PhonemesTeeth_fv?.RequestInternals(assetProvider);
            PhonemesTeeth_other?.RequestInternals(assetProvider);
        }

        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return PartBody;
            yield return PartNipples;
            yield return PartBlushLight;
            yield return PartBlushHeavy;
            yield return PartBlink;
            yield return PartMouthNeutral;
            yield return Phonemes_aeil;
            yield return Phonemes_neutral;
            yield return Phonemes_oquw;
            yield return Phonemes_fv;
            yield return Phonemes_other;
            yield return PhonemesTeeth_aeil;
            yield return PhonemesTeeth_neutral;
            yield return PhonemesTeeth_oquw;
            yield return PhonemesTeeth_fv;
            yield return PhonemesTeeth_other;
        }

        public IEnumerable<IGirlSubDataMod<GirlExpressionSubDefinition>> GetExpressions() => expressions;
        public IEnumerable<IGirlSubDataMod<GirlOutfitSubDefinition>> GetOutfits() => outfits;
        public IEnumerable<IGirlSubDataMod<GirlHairstyleSubDefinition>> GetHairstyles() => hairstyles;
        public IEnumerable<IGirlSubDataMod<GirlSpecialPartSubDefinition>> GetSpecialPartMods() => specialParts;
    }
}
