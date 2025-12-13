using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlBodySubDefinition"/>.
    /// </summary>
    public class GirlBodyDataMod : DataMod, IGirlBodyDataMod
    {
        public string bodyName;

        public List<IBodySubDataMod<GirlExpressionSubDefinition>> expressions;
        public List<IBodySubDataMod<GirlHairstyleSubDefinition>> hairstyles;
        public List<IBodySubDataMod<GirlOutfitSubDefinition>> outfits;
        public List<IBodySubDataMod<GirlSpecialPartSubDefinition>> specialParts;

        public string SpecialEffectName;

        public IGameDefinitionInfo<Vector2> HeadPosition;
        public IGameDefinitionInfo<Vector2> BackPosition;
        public IGameDefinitionInfo<Vector2> BreathEmitterPos;
        public IGameDefinitionInfo<Vector2> UpsetEmitterPos;

        public IBodySubDataMod<GirlPartSubDefinition> PartBody;
        public IBodySubDataMod<GirlPartSubDefinition> PartNipples;
        public IBodySubDataMod<GirlPartSubDefinition> PartBlushLight;
        public IBodySubDataMod<GirlPartSubDefinition> PartBlushHeavy;
        public IBodySubDataMod<GirlPartSubDefinition> PartBlink;
        public IBodySubDataMod<GirlPartSubDefinition> PartMouthNeutral;

        public RelativeId? DefaultExpressionId;
        public RelativeId? FailureExpressionId;
        public RelativeId? DefaultHairstyleId;
        public RelativeId? DefaultOutfitId;

        public IBodySubDataMod<GirlPartSubDefinition> Phonemes_aeil;
        public IBodySubDataMod<GirlPartSubDefinition> Phonemes_neutral;
        public IBodySubDataMod<GirlPartSubDefinition> Phonemes_oquw;
        public IBodySubDataMod<GirlPartSubDefinition> Phonemes_fv;
        public IBodySubDataMod<GirlPartSubDefinition> Phonemes_other;
        public IBodySubDataMod<GirlPartSubDefinition> PhonemesTeeth_aeil;
        public IBodySubDataMod<GirlPartSubDefinition> PhonemesTeeth_neutral;
        public IBodySubDataMod<GirlPartSubDefinition> PhonemesTeeth_oquw;
        public IBodySubDataMod<GirlPartSubDefinition> PhonemesTeeth_fv;
        public IBodySubDataMod<GirlPartSubDefinition> PhonemesTeeth_other;

        public float? Scale;

        public Dictionary<RelativeId, GirlStyleInfo> LocationIdToStyleInfo;

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

            DefaultExpressionId = new RelativeId(-1, def.defaultExpressionIndex);
            FailureExpressionId = new RelativeId(-1, def.failureExpressionIndex);

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
                    .Select(x => (IBodySubDataMod<GirlExpressionSubDefinition>)new GirlExpressionDataMod(i++, assetProvider, def))
                    .ToList();
            }

            if (def.outfits != null)
            {
                i = 0;
                outfits = def.outfits
                    .Select(x => (IBodySubDataMod<GirlOutfitSubDefinition>)new OutfitDataMod(i++, def, assetProvider))
                    .ToList();
            }

            if (def.hairstyles != null)
            {
                i = 0;
                hairstyles = def.hairstyles
                    .Select(x => (IBodySubDataMod<GirlHairstyleSubDefinition>)new HairstyleDataMod(i++, def, assetProvider))
                    .ToList();
            }

            if (def.specialParts != null)
            {
                i = 0;
                specialParts = def.specialParts
                    .Select(x => (IBodySubDataMod<GirlSpecialPartSubDefinition>)new GirlSpecialPartDataMod(i++, def, assetProvider))
                    .ToList();
            }

            LocationIdToStyleInfo = new(){
                {Locations.MassageSpa, new GirlStyleInfo() { HairstyleId = Styles.Relaxing, OutfitId = Styles.Relaxing}},
                {Locations.Aquarium, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                {Locations.SecludedCabana, new GirlStyleInfo() { HairstyleId = Styles.Relaxing, OutfitId = Styles.Relaxing}},
                {Locations.PoolsideBar, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                {Locations.GolfCourse, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                {Locations.CruiseShip, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                {Locations.RooftopLounge, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic}},
                {Locations.Casino, new GirlStyleInfo() { HairstyleId = Styles.Party, OutfitId = Styles.Party}},
                {Locations.PrivateTable, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic}},
                {Locations.SecretGrotto, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                {Locations.RoyalSuite, new GirlStyleInfo() { HairstyleId = Styles.Sexy, OutfitId = Styles.Sexy}},
                {Locations.AirplaneBathroom, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                {Locations.OuterSpace, new GirlStyleInfo() { HairstyleId = Styles.Sexy, OutfitId = Styles.Sexy}},
            };
        }

        /// <inheritdoc/>
        public void SetData(GirlBodySubDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            RelativeId girlId)
        {
            if (def == null) { return; }
            var expansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.Scale, Scale);
            ValidatedSet.SetValue(ref def.BodyName, bodyName, InsertStyle);
            ValidatedSet.SetValue(ref def.PartIndexBody, def.PartIdToIndex, PartBody?.Id);
            ValidatedSet.SetValue(ref def.PartIndexNipples, def.PartIdToIndex, PartNipples?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlushLight, def.PartIdToIndex, PartBlushLight?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlushHeavy, def.PartIdToIndex, PartBlushHeavy?.Id);
            ValidatedSet.SetValue(ref def.PartIndexBlink, def.PartIdToIndex, PartBlink?.Id);
            ValidatedSet.SetValue(ref def.PartIndexMouthNeutral, def.PartIdToIndex, PartMouthNeutral?.Id);

            ValidatedSet.SetValue(ref def.DefaultExpressionIndex, expansion.ExpressionIdToIndex, DefaultExpressionId);
            ValidatedSet.SetValue(ref def.FailureExpressionIndex, expansion.ExpressionIdToIndex, FailureExpressionId);

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

            ValidatedSet.SetListValue(ref def.PartIndexesPhonemes, partIdsPhonemes
                .Select(x => x != null ? (int?)def.PartIdToIndex[x.Id] : null), InsertStyle.replace);

            var partIdsPhonemesTeeth = new[]
            {
                PhonemesTeeth_aeil,
                PhonemesTeeth_neutral,
                PhonemesTeeth_oquw,
                PhonemesTeeth_fv,
                PhonemesTeeth_other
            };

            ValidatedSet.SetListValue(ref def.PartIndexesPhonemesTeeth, partIdsPhonemesTeeth
                .Select(x => x != null ? (int?)def.PartIdToIndex[x.Id] : null), InsertStyle.replace);

            ValidatedSet.SetValue(ref def.BreathEmitterPos, BreathEmitterPos, InsertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.UpsetEmitterPos, UpsetEmitterPos, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.SpecialEffectOffset, HeadPosition, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.HeadPos, HeadPosition, InsertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.BackPos, BackPosition, InsertStyle, gameData, assetProvider);

            ValidatedSet.SetValue(ref def.SpecialEffectPrefab, assetProvider.GetInternalAsset<UiDollSpecialEffect>(SpecialEffectName), InsertStyle);

            ValidatedSet.SetDictValues(ref def.LocationIdToOutfitId, LocationIdToStyleInfo, InsertStyle);
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

        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods() => LocalPartMods()
            .Concat(expressions.SelectManyNN(x => x.GetPartDataMods()))
            .Concat(outfits.SelectManyNN(x => x.GetPartDataMods()))
            .Concat(hairstyles.SelectManyNN(x => x.GetPartDataMods()))
            .Concat(specialParts.SelectManyNN(x => x.GetPartDataMods()));

        private IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> LocalPartMods()
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

        public IEnumerable<IBodySubDataMod<GirlExpressionSubDefinition>> GetExpressions() => expressions;
        public IEnumerable<IBodySubDataMod<GirlOutfitSubDefinition>> GetOutfits() => outfits;
        public IEnumerable<IBodySubDataMod<GirlHairstyleSubDefinition>> GetHairstyles() => hairstyles;
        public IEnumerable<IBodySubDataMod<GirlSpecialPartSubDefinition>> GetSpecialPartMods() => specialParts;
    }
}
