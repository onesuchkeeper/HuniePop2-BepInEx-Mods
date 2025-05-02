namespace Hp2BaseMod.GameDataInfo
{
    public class ExpandedHairstyleDefinition : GirlHairstyleSubDefinition, IExpandedStyleDefinition
    {
        public string Name => hairstyleName;

        bool IExpandedStyleDefinition.IsNSFW => IsNSFW;
        public bool IsNSFW;
        bool IExpandedStyleDefinition.IsCodeUnlocked => IsCodeUnlocked;
        public bool IsCodeUnlocked;
        bool IExpandedStyleDefinition.IsPurchased => IsPurchased;
        public bool IsPurchased;

        public ExpandedHairstyleDefinition()
        {

        }

        public ExpandedHairstyleDefinition(GirlHairstyleSubDefinition def, int id)
        {
            hairstyleName = def.hairstyleName;
            partIndexFronthair = def.partIndexFronthair;
            partIndexBackhair = def.partIndexBackhair;
            pairOutfitIndex = def.pairOutfitIndex;
            tightlyPaired = def.tightlyPaired;
            hideSpecials = def.hideSpecials;
            IsNSFW = false;

            IsPurchased = id > 6;
            IsCodeUnlocked = id == 6;
        }
    }
}
