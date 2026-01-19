using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlHairstyleSubDefinition"/>.
    /// </summary>
    public class HairstyleDataMod : DataMod, IBodySubDataMod<GirlHairstyleSubDefinition>
    {
        public string Name;

        public IBodySubDataMod<GirlPartSubDefinition> FrontHairPart;

        public IBodySubDataMod<GirlPartSubDefinition> BackHairPart;

        public bool? IsNSFW;
        public bool? IsCodeUnlocked;
        public bool? IsPurchased;
        public bool? HideSpecial;

        public bool? TightlyPaired;

        public RelativeId? PairOutfitId;

        /// <inheritdoc/>
        public HairstyleDataMod() { }

        public HairstyleDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal HairstyleDataMod(int index,
                                  GirlDefinition girlDef,
                                  AssetProvider assetProvider)
            : base(new RelativeId(-1, index), InsertStyle.replace, 0)
        {
            PairOutfitId = Id;
            IsNSFW = false;
            IsPurchased = Game.Session.Hub.unlockStylesBuy.Contains(index);
            IsCodeUnlocked = Game.Session.Hub.unlockStylesBuy.Contains(index);

            var hairstyleDef = girlDef.hairstyles[index];

            Name = hairstyleDef.hairstyleName;
            FrontHairPart = new GirlPartDataMod(hairstyleDef.partIndexFronthair, assetProvider, girlDef);
            BackHairPart = new GirlPartDataMod(hairstyleDef.partIndexBackhair, assetProvider, girlDef);
            TightlyPaired = hairstyleDef.tightlyPaired;
        }

        /// <inheritdoc/>
        public void SetData(GirlHairstyleSubDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            RelativeId girlId,
                            GirlBodySubDefinition bodyDef)
        {
            if (def == null) { return; }

            var expansion = def.Expansion();
            var girlExpansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.hairstyleName, Name, InsertStyle);
            ValidatedSet.SetValue(ref expansion.IsNSFW, IsNSFW);
            ValidatedSet.SetValue(ref expansion.IsCodeUnlocked, IsCodeUnlocked);
            ValidatedSet.SetValue(ref expansion.IsPurchased, IsPurchased);

            ValidatedSet.SetValue(ref def.pairOutfitIndex, girlExpansion.OutfitLookup, PairOutfitId);
            ValidatedSet.SetValue(ref def.partIndexBackhair, bodyDef.PartLookup, BackHairPart?.Id);
            ValidatedSet.SetValue(ref def.partIndexFronthair, bodyDef.PartLookup, FrontHairPart?.Id);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            FrontHairPart?.RequestInternals(assetProvider);
            BackHairPart?.RequestInternals(assetProvider);
        }

        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return FrontHairPart;
            yield return BackHairPart;
        }
    }
}
