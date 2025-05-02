using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    public class HairstyleDataMod : DataMod, IGirlSubDataMod<ExpandedHairstyleDefinition>
    {
        public string Name;

        public RelativeId? FrontHairPartId;

        public RelativeId? BackHairPartId;

        public bool? IsNSFW;
        public bool? IsCodeUnlocked;
        public bool? IsPurchased;

        public bool? HideSpecials;

        public bool? TightlyPaired;

        public RelativeId? PairOutfitId;

        /// <inheritdoc/>
        public HairstyleDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public HairstyleDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal HairstyleDataMod(int index,
                                  GirlDefinition girlDef,
                                  AssetProvider assetProvider)
            : base(new RelativeId() { SourceId = -1, LocalId = index }, InsertStyle.replace, 0)
        {
            PairOutfitId = Id;
            IsNSFW = false;
            IsPurchased = Game.Session.Hub.unlockStylesBuy.Contains(index);
            IsCodeUnlocked = Game.Session.Hub.unlockStylesBuy.Contains(index);

            var hairstyleDef = girlDef.hairstyles[index];

            Name = hairstyleDef.hairstyleName;
            FrontHairPartId = new RelativeId(-1, hairstyleDef.partIndexFronthair);
            BackHairPartId = new RelativeId(-1, hairstyleDef.partIndexBackhair);
            HideSpecials = hairstyleDef.hideSpecials;
            TightlyPaired = hairstyleDef.tightlyPaired;
        }

        /// <inheritdoc/>
        public void SetData(ref ExpandedHairstyleDefinition def,
                            GameDefinitionProvider gameData,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<ExpandedHairstyleDefinition>();
            }

            var girlExpansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.hairstyleName, Name, insertStyle);
            ValidatedSet.SetValue(ref def.IsNSFW, IsNSFW);
            ValidatedSet.SetValue(ref def.IsCodeUnlocked, IsCodeUnlocked);
            ValidatedSet.SetValue(ref def.IsPurchased, IsPurchased);
            ValidatedSet.SetValue(ref def.hideSpecials, HideSpecials);
            ValidatedSet.SetValue(ref def.pairOutfitIndex, girlExpansion.OutfitIdToIndex, PairOutfitId);
            ValidatedSet.SetValue(ref def.partIndexBackhair, girlExpansion.PartIdToIndex, BackHairPartId);
            ValidatedSet.SetValue(ref def.partIndexFronthair, girlExpansion.PartIdToIndex, FrontHairPartId);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
