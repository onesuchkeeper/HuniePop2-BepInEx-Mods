using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

internal static class ItemSensitivitySmoothie
{
    public static RelativeId SmoothieId => _smoothieId;
    private static RelativeId _smoothieId;

    public static RelativeId Exp => _expId;
    private static RelativeId _expId;

    public static RelativeId Level => _levelId;
    private static RelativeId _levelId;

    public static void AddDataMods(AssetBundle assetBundle)
    {
        _smoothieId = new RelativeId(State.ModId, 0);
        _expId = new RelativeId(State.ModId, 1);
        _levelId = new RelativeId(State.ModId, 2);

        var spriteInfo = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>("item_smoothie_sensitivity"));

        ModInterface.DataMod.AddData(ItemGiftHandlers.SensitivitySmoothie, new GiftHandler());

        ModInterface.DataMod.AddDataMod(new ItemDataMod(_smoothieId, InsertStyle.replace)
        {
            ItemSpriteInfo = spriteInfo,
            ItemName = "Sensitivity Smoothie",
            ItemDescription = "+1 [[broken]@Sensitivity] EXP.",
            TooltipColorIndex = 6,
            StoreCost = 5,
            CategoryPrefix = "Smoothie",
            CategoryDescription = "Sensitivity",
            IsPerishable = true,
            EnergyDefinitionID = new RelativeId(-1, 6),
            StoreSectionPreference = false,
            ItemGiftHandlerId = ItemGiftHandlers.SensitivitySmoothie,
            ItemStoreHandlerId = ItemTypes.Smoothie,
        });

        ModInterface.DataMod.AddDataMod(new ItemDataMod(_expId, InsertStyle.replace)
        {
            ItemSpriteInfo = spriteInfo,
            ItemName = "Sensitivity EXP",
            ItemDescription = "Earn [[broken]@Sensitivity] EXP by giving [[broken]@Sensitivity] smoothies to girls.",
            TooltipColorIndex = 6,
            CategoryDescription = "+(NUM0) EXP until Level (NUM1)"
        });

        ModInterface.DataMod.AddDataMod(new ItemDataMod(_levelId, InsertStyle.replace)
        {
            ItemName = "Sensitivity Level (LEVEL)",
            ItemDescription = "[[broken]@Broken Heart] token matches will yield [[broken]-(13-(NUM0))%] Affection.",
            TooltipColorIndex = 6,
            CategoryPrefix = "Affection Level",
            CategoryDescription = "Sensitivity"
        });
    }

    private class GiftHandler : IItemGiftHandler
    {
        private static readonly RelativeId DT_SMOOTHIE_ACCEPT = new RelativeId(-1, 18);
        private static readonly RelativeId DT_SMOOTHIE_FULL = new RelativeId(-1, 19);
        private static readonly RelativeId DT_SMOOTHIE_REJECT = new RelativeId(-1, 20);

        public bool CanGive(ExpandedItemDefinition item, 
            ExpandedGirlDefinition girl, 
            PuzzleStatusGirl statusGirl, 
            PlayerFileGirl fileGirl, 
            bool altGirl)
        {
            if (!Game.Session.Location.AtLocationType(LocationType.SIM)
                || State.SaveFile.SensitivityExp >= 24
                || statusGirl.stamina <= 0)
            {
                return false;
            }

            return true;
        }

        public string OnGiveSucceed(ExpandedItemDefinition item, 
            UiDoll doll, 
            ExpandedGirlDefinition girl, 
            PuzzleStatusGirl statusGirl, 
            PlayerFileGirl playerFileGirl, 
            bool isAltGirl)
        {
            var text = "+1 Sensitivity EXP";

            var affectionLevel = State.GetSensitivityLevel();

            State.SaveFile.SensitivityExp++;

            var updatedAffectionLevel = State.GetSensitivityLevel();
            if (updatedAffectionLevel != affectionLevel)
            {
                doll.notificationBox.Show($"Sensitivity Level {updatedAffectionLevel + 1} achieved!", 0f, false);
            }

            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_ACCEPT), DialogLineFormat.PASSIVE, -1);

            Game.Persistence.playerFile.relationshipPoints++;
            playerFileGirl.relationshipPoints++;

            if (!item.Core.noStaminaCost)
            {
                Game.Session.Puzzle.puzzleStatus.GetExpansion().AddResourceValue(PuzzleResourceId.Stamina, -1, isAltGirl);
            }

            return text;
        }

        public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
        {
            if (State.SaveFile.SensitivityExp >= 24)
            {
                doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_FULL), DialogLineFormat.PASSIVE, -1);
                return;
            }

            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_REJECT), DialogLineFormat.PASSIVE, -1);
        }
    }
}