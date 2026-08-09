// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a ItemDefinition
    /// </summary>
    public class ItemDataMod : DataMod, IGameDataMod<ItemDefinition>
    {
        // these three are only used to generate the category description,
        // which we're doing manually for the expansion
        //public EditorDialogTriggerTab? baggageGirl;
        //public ItemShoesType? shoesType;
        //public ItemUniqueType? uniqueType;

        public RelativeId? ItemGiftHandlerId;
        public RelativeId? ItemStoreHandlerId;

        public ItemDateGiftType? DateGiftType;

        public ItemFoodType? FoodType;

        public IGameDefinitionInfo<Sprite> ItemSpriteInfo;

        public string ItemName;

        public string ItemDescription;

        public int? TooltipColorIndex;

        public bool? StoreSectionPreference;

        public int? StoreCost;

        public RelativeId? CutsceneDefinitionID;

        public RelativeId? AilmentDefinitionID;

        public bool? DifficultyExclusive;

        public SettingDifficulty? Difficulty;

        public RelativeId? EnergyDefinitionID;

        public RelativeId? AffectionId;

        public ItemGiveConditionType? GiveConditionType;

        public RelativeId? GirlDefinitionID;

        public bool? NoStaminaCost;

        public bool? DateGiftAilment;

        public RelativeId? AbilityDefinitionID;

        public int? UseCost;

        public string CategoryDescription;

        public string CostDescription;

        public int? NotifierHeaderIndex;

        /// <inheritdoc/>
        public ItemDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public ItemDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the asset referenced by the definition.</param>
#pragma warning disable HP001 // Deprecated member usage        
        internal ItemDataMod(ItemDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            ItemName = def.itemName;

            switch (def.itemType)
            {
                case ItemType.DATE_GIFT:
                    ItemStoreHandlerId = Hp2BaseMod.ItemTypes.DateGift;
                    ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.DateGift;
                    break;
                case ItemType.FOOD:
                    ItemStoreHandlerId = Hp2BaseMod.ItemTypes.Food;
                    ItemGiftHandlerId = def.noStaminaCost 
                        ? Hp2BaseMod.ItemGiftHandlerId.StaminaFood
                        : Hp2BaseMod.ItemGiftHandlerId.Food;
                    break;
                case ItemType.SMOOTHIE:
                    ItemStoreHandlerId = Hp2BaseMod.ItemTypes.Smoothie;
                    switch (def.affectionType)
                    {
                        case PuzzleAffectionType.TALENT:
                            ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.SmoothieTalent;
                            break;
                        case PuzzleAffectionType.FLIRTATION:
                            ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.SmoothieFlirtation;
                            break;
                        case PuzzleAffectionType.ROMANCE:
                            ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.SmoothieRomance;
                            break;
                        case PuzzleAffectionType.SEXUALITY:
                            ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.SmoothieSexuality;
                            break;
                    }
                    break;
                case ItemType.UNIQUE_GIFT:
                    ItemStoreHandlerId = Hp2BaseMod.ItemTypes.Unique;
                    ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.Uniques;
                    break;
                case ItemType.SHOES:
                    ItemStoreHandlerId = Hp2BaseMod.ItemTypes.Shoe;
                    ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.Shoes;
                    break;
                case ItemType.BAGGAGE:
                case ItemType.FRUIT:
                case ItemType.MISC:
                    ItemGiftHandlerId = Hp2BaseMod.ItemGiftHandlerId.Misc;
                    break;
            }

            ItemDescription = def.itemDescription;

            switch (def.affectionType)
            {
                case PuzzleAffectionType.TALENT:
                    AffectionId = PuzzleAffectionId.Talent;
                    break;
                case PuzzleAffectionType.ROMANCE:
                    AffectionId = PuzzleAffectionId.Romance;
                    break;
                case PuzzleAffectionType.SEXUALITY:
                    AffectionId = PuzzleAffectionId.Sexuality;
                    break;
                case PuzzleAffectionType.FLIRTATION:
                    AffectionId = PuzzleAffectionId.Flirtation;
                    break;
            }

            TooltipColorIndex = def.tooltipColorIndex;
            GiveConditionType = def.giveConditionType;
            DifficultyExclusive = def.difficultyExclusive;
            Difficulty = def.difficulty;
            StoreSectionPreference = def.storeSectionPreference;
            StoreCost = def.storeCost;
            FoodType = def.foodType;
            NoStaminaCost = def.noStaminaCost;
            DateGiftType = def.dateGiftType;
            DateGiftAilment = def.dateGiftAilment;
            UseCost = def.useCost;
            CostDescription = def.costDescription;
            NotifierHeaderIndex = def.notifierHeaderIndex;

            // category description will now be prioritized, so I'm setting it in the defaults to emphasize
            switch (def.itemType)
            {
                case ItemType.FRUIT:
                case ItemType.SMOOTHIE:
                    CategoryDescription = StringUtils.Titleize(def.affectionType.ToString());
                    break;
                case ItemType.FOOD:
                    CategoryDescription = StringUtils.Titleize(def.foodType.ToString());
                    break;
                case ItemType.SHOES:
                    CategoryDescription = StringUtils.Titleize(def.shoesType.ToString());
                    break;
                case ItemType.UNIQUE_GIFT:
                    CategoryDescription = StringUtils.Titleize(def.uniqueType.ToString());
                    break;
                case ItemType.DATE_GIFT:
                    CategoryDescription = StringUtils.Titleize(def.dateGiftType.ToString());
                    break;
                case ItemType.BAGGAGE:
                    CategoryDescription = StringUtils.Titleize(def.baggageGirl.ToString());
                    break;
                case ItemType.MISC:
                    CategoryDescription = def.categoryDescription;
                    break;
            }

            EnergyDefinitionID = new RelativeId(def.energyDefinition);
            GirlDefinitionID = new RelativeId(def.girlDefinition);
            AbilityDefinitionID = new RelativeId(def.abilityDefinition);
            CutsceneDefinitionID = new RelativeId(def.cutsceneDefinition);
            AilmentDefinitionID = new RelativeId(def.ailmentDefinition);

            if (def.itemSprite != null) 
            { 
                ItemSpriteInfo = new SpriteInfoInternal(def.itemSprite, assetProvider); 
            }
        }
#pragma warning restore HP001 // Deprecated member usage

        /// <inheritdoc/>
        public void SetData(ItemDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            var expansion = def.GetExpansion();

            ValidatedSet.SetValue(ref def.notifierHeaderIndex, NotifierHeaderIndex);

            if (ItemGiftHandlerId.HasValue) expansion.GiftHandler = gameDataProvider.GetItemGiftHandler(ItemGiftHandlerId);
            if (ItemStoreHandlerId.HasValue) expansion.StoreHandler = gameDataProvider.GetItemStoreHandler(ItemStoreHandlerId);
            if (AffectionId.HasValue) expansion.Affection = gameDataProvider.GetAffection(AffectionId);
            ValidatedSet.SetValue(ref def.tooltipColorIndex, TooltipColorIndex);
            ValidatedSet.SetValue(ref def.giveConditionType, GiveConditionType);
            ValidatedSet.SetValue(ref def.difficultyExclusive, DifficultyExclusive);
            ValidatedSet.SetValue(ref def.difficulty, Difficulty);
            ValidatedSet.SetValue(ref def.storeSectionPreference, StoreSectionPreference);
            ValidatedSet.SetValue(ref def.storeCost, StoreCost);
            ValidatedSet.SetValue(ref def.foodType, FoodType);
            ValidatedSet.SetValue(ref def.noStaminaCost, NoStaminaCost);
            ValidatedSet.SetValue(ref def.dateGiftType, DateGiftType);
            ValidatedSet.SetValue(ref def.dateGiftAilment, DateGiftAilment);
            ValidatedSet.SetValue(ref def.useCost, UseCost);
            ValidatedSet.SetValue(ref def.cutsceneDefinition, gameDataProvider.GetCutscene(CutsceneDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.ailmentDefinition, gameDataProvider.GetAilment(AilmentDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.energyDefinition, gameDataProvider.GetEnergy(EnergyDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.girlDefinition, gameDataProvider.GetGirl(GirlDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.abilityDefinition, gameDataProvider.GetAbility(AbilityDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.itemName, ItemName, InsertStyle);
            ValidatedSet.SetValue(ref def.itemDescription, ItemDescription, InsertStyle);
            ValidatedSet.SetValue(ref def.costDescription, CostDescription, InsertStyle);
            ValidatedSet.SetValue(ref def.itemSprite, ItemSpriteInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.categoryDescription, CategoryDescription, InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            ItemSpriteInfo?.RequestInternals(assetProvider);
        }
    }
}
