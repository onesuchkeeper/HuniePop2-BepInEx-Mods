namespace Hp2BaseMod.GameDataInfo
{
    public class ExpandedOutfitDefinition : GirlOutfitSubDefinition, IExpandedStyleDefinition
    {
        public string Name => outfitName;
        bool IExpandedStyleDefinition.IsNSFW => IsNSFW;
        public bool IsNSFW;
        bool IExpandedStyleDefinition.IsCodeUnlocked => IsCodeUnlocked;
        public bool IsCodeUnlocked;
        bool IExpandedStyleDefinition.IsPurchased => IsPurchased;
        public bool IsPurchased;

        public ExpandedOutfitDefinition()
        {
        }

        public ExpandedOutfitDefinition(GirlOutfitSubDefinition def, int id)
        {
            outfitName = def.outfitName;
            partIndexOutfit = def.partIndexOutfit;
            pairHairstyleIndex = def.pairHairstyleIndex;
            tightlyPaired = def.tightlyPaired;
            hideNipples = def.hideNipples;

            IsNSFW = false;
            IsPurchased = id > 6;
            IsCodeUnlocked = id == 6;
        }
    }
}
