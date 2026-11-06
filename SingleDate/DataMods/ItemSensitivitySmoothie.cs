using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// The sensitivity smoothie, a store item the player gives to a girl in the sim to earn sensitivity EXP, plus the
/// two display only items (EXP and level) used for the sensitivity tooltips. Ids are local to this mod.
/// </summary>
internal static class ItemSensitivitySmoothie
{
    /// <summary>The smoothie the player buys and gives.</summary>
    public static RelativeId SmoothieId => _smoothieId;
    private static RelativeId _smoothieId;

    /// <summary>Display only item, the tooltip for sensitivity EXP.</summary>
    public static RelativeId Exp => _expId;
    private static RelativeId _expId;

    /// <summary>Display only item, the plate for the current sensitivity level.</summary>
    public static RelativeId Level => _levelId;
    private static RelativeId _levelId;

    /// <summary>Adds the smoothie, EXP and level items and registers the smoothie's gift handler.</summary>
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
            // Tooltip color 6 is shared by all the sensitivity items so they match.
            TooltipColorIndex = 6,
            StoreCost = 5,
            CategoryPrefix = "Smoothie",
            CategoryDescription = "Sensitivity",
            IsPerishable = true,
            // Uses the base game's energy definition 6.
            EnergyDefinitionID = new RelativeId(-1, 6),
            // No store section preference.
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

    /// <summary>
    /// Handles giving the smoothie. Giving one adds one sensitivity EXP and costs the girl a stamina, and is only
    /// allowed in the sim while the girl has stamina and sensitivity isn't maxed.
    /// </summary>
    private class GiftHandler : IItemGiftHandler
    {
        // Base game dialog triggers (-1 is the base game source) for a girl accepting, being full of and rejecting a smoothie.
        private static readonly RelativeId DT_SMOOTHIE_ACCEPT = new RelativeId(-1, 18);
        private static readonly RelativeId DT_SMOOTHIE_FULL = new RelativeId(-1, 19);
        private static readonly RelativeId DT_SMOOTHIE_REJECT = new RelativeId(-1, 20);

        /// <summary>True in the sim, when the girl has stamina and sensitivity isn't maxed.</summary>
        public bool CanGive(ExpandedItemDefinition item, 
            ExpandedGirlDefinition girl, 
            PuzzleStatusGirl statusGirl, 
            PlayerFileGirl fileGirl, 
            bool altGirl)
        {
            var state = ModInterface.GameState.CurrentState;
            
            if (!(state.HasGirlsPresent && state is not IPuzzleState)
                || State.SaveFile.SensitivityExp >= State.MaxSensitivityExp
                || statusGirl.stamina <= 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds one EXP, announces the new level when one is reached, adds a relationship point to the player file and the
        /// girl, and takes one of the girl's stamina (unless the item has no stamina cost). Returns the text shown for the gift.
        /// </summary>
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

        /// <summary>Plays the girl's "too full" line when sensitivity is maxed, otherwise her reject line.</summary>
        public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
        {
            if (State.SaveFile.SensitivityExp >= State.MaxSensitivityExp)
            {
                doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_FULL), DialogLineFormat.PASSIVE, -1);
                return;
            }

            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_REJECT), DialogLineFormat.PASSIVE, -1);
        }
    }
}
