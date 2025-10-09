using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlOutfitSubDefinition"/>.
    /// </summary>
    public class OutfitDataMod : DataMod, IGirlSubDataMod<GirlOutfitSubDefinition>
    {
        public string Name;

        public IGirlSubDataMod<GirlPartSubDefinition> OutfitPart;

        public bool? IsNSFW;
        public bool? IsCodeUnlocked;
        public bool? IsPurchased;

        public bool? HideNipples;
        public bool? HideSpecial;

        public bool? TightlyPaired;

        public RelativeId? PairHairstyleId;

        /// <inheritdoc/>
        public OutfitDataMod() { }

        public OutfitDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal OutfitDataMod(int index,
                               GirlDefinition girlDef,
                               AssetProvider assetProvider)
            : base(new RelativeId(-1, index), InsertStyle.replace, 0)
        {
            PairHairstyleId = Id;
            IsNSFW = false;
            IsPurchased = Game.Session.Hub.unlockStylesBuy.Contains(index);
            IsCodeUnlocked = Game.Session.Hub.unlockStylesBuy.Contains(index);

            var outfitDef = girlDef.outfits[index];

            Name = outfitDef.outfitName;
            OutfitPart = new GirlPartDataMod(outfitDef.partIndexOutfit, assetProvider, girlDef);
            HideNipples = outfitDef.hideNipples;
            TightlyPaired = outfitDef.tightlyPaired;
        }

        /// <inheritdoc/>
        public void SetData(ref GirlOutfitSubDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId,
                            GirlDefinition girlDef)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<GirlOutfitSubDefinition>();
            }

            var expansion = def.Expansion();
            var girlExpansion = girlDef.Expansion();

            ValidatedSet.SetValue(ref def.outfitName, Name, insertStyle);
            ValidatedSet.SetValue(ref expansion.IsNSFW, IsNSFW);
            ValidatedSet.SetValue(ref expansion.IsCodeUnlocked, IsCodeUnlocked);
            ValidatedSet.SetValue(ref expansion.IsPurchased, IsPurchased);
            ValidatedSet.SetValue(ref def.hideNipples, HideNipples);
            ValidatedSet.SetValue(ref expansion.HideSpecial, HideSpecial);
            ValidatedSet.SetValue(ref def.pairHairstyleIndex, girlExpansion.HairstyleIdToIndex, PairHairstyleId);
            ValidatedSet.SetValue(ref def.partIndexOutfit, girlExpansion.PartIdToIndex, OutfitPart?.Id);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            OutfitPart?.RequestInternals(assetProvider);
        }

        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return OutfitPart;
        }
    }
}
