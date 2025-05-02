using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable info to modify an expanded outfit def
    /// </summary>
    public class OutfitDataMod : DataMod, IGirlSubDataMod<ExpandedOutfitDefinition>
    {
        public string Name;

        public RelativeId? OutfitPartId;

        public bool? IsNSFW;
        public bool? IsCodeUnlocked;
        public bool? IsPurchased;

        public bool? HideNipples;

        public bool? TightlyPaired;

        public RelativeId? PairHairstyleId;

        /// <inheritdoc/>
        public OutfitDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public OutfitDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal OutfitDataMod(int index,
                               GirlDefinition girlDef,
                               AssetProvider assetProvider)
            : base(new RelativeId() { SourceId = -1, LocalId = index }, InsertStyle.replace, 0)
        {
            PairHairstyleId = Id;
            IsNSFW = false;
            IsPurchased = Game.Session.Hub.unlockStylesBuy.Contains(index);
            IsCodeUnlocked = Game.Session.Hub.unlockStylesBuy.Contains(index);

            var outfitDef = girlDef.outfits[index];

            Name = outfitDef.outfitName;
            OutfitPartId = new RelativeId(-1, outfitDef.partIndexOutfit);
            HideNipples = outfitDef.hideNipples;
            TightlyPaired = outfitDef.tightlyPaired;
        }

        /// <inheritdoc/>
        public void SetData(ref ExpandedOutfitDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<ExpandedOutfitDefinition>();
            }

            var girlExpansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.outfitName, Name, insertStyle);
            ValidatedSet.SetValue(ref def.IsNSFW, IsNSFW);
            ValidatedSet.SetValue(ref def.IsCodeUnlocked, IsCodeUnlocked);
            ValidatedSet.SetValue(ref def.IsPurchased, IsPurchased);
            ValidatedSet.SetValue(ref def.hideNipples, HideNipples);
            ValidatedSet.SetValue(ref def.pairHairstyleIndex, girlExpansion.HairstyleIdToIndex, PairHairstyleId);
            ValidatedSet.SetValue(ref def.partIndexOutfit, girlExpansion.PartIdToIndex, OutfitPartId);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
