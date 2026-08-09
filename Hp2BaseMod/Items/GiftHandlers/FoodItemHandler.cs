namespace Hp2BaseMod;

public class FoodItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_FOOD_REJECT = new RelativeId(-1, 22);
    private static readonly RelativeId DT_FOOD_ACCEPT = new RelativeId(-1, 21);

    public bool CanGive(ExpandedItemDefinition item, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl fileGirl, 
        bool altGirl)
    {
        if (item.Core.foodType == girl.Core.badFoodTypes[0] 
            || statusGirl.stamina <= 0
            || Game.Session.Puzzle.puzzleStatus.foodGiftCount >= 2)
        {
            return false;
        }

        switch (item.Core.giveConditionType)
        {
            case ItemGiveConditionType.BAGGAGE:
                return fileGirl.learnedBaggage.Count > 0;
            case ItemGiveConditionType.UNIQUE_GIFTS:
                return fileGirl.receivedUniques.Count > 0;
            case ItemGiveConditionType.SHOES:
                return fileGirl.receivedShoes.Count > 0;
            case ItemGiveConditionType.PASSION:
                return Game.Session.Puzzle.puzzleStatus.GetResourceValue(PuzzleResourceType.PASSION, altGirl, false) > 0;
            case ItemGiveConditionType.SENTIMENT:
                return Game.Session.Puzzle.puzzleStatus.GetResourceValue(PuzzleResourceType.SENTIMENT, altGirl, false) > 0;
            case ItemGiveConditionType.TALENT_LVL:
                return Game.Persistence.playerFile.GetAffectionLevel(PuzzleAffectionType.TALENT, true) > 0;
            case ItemGiveConditionType.FLIRTATION_LVL:
                return Game.Persistence.playerFile.GetAffectionLevel(PuzzleAffectionType.FLIRTATION, true) > 0;
            case ItemGiveConditionType.ROMANCE_LVL:
                return Game.Persistence.playerFile.GetAffectionLevel(PuzzleAffectionType.ROMANCE, true) > 0;
            case ItemGiveConditionType.SEXUALITY_LVL:
                return Game.Persistence.playerFile.GetAffectionLevel(PuzzleAffectionType.SEXUALITY, true) > 0;
        }

        if (item.Core.abilityDefinition == null 
            || !Game.Session.Ability.PerformAbility(item.Core.abilityDefinition, altGirl, null))
        {
            return false;
        }

        return true;
    }

    public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
    {
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_FOOD_REJECT), DialogLineFormat.PASSIVE, 0);
    }

    public string OnGiveSucceed(ExpandedItemDefinition item, 
        UiDoll doll, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl playerFileGirl, 
        bool isAltGirl)
    {
        statusGirl.GiveFood(item.Core);
        if (item.Core.noStaminaCost)
        {
            Game.Persistence.playerFile.staminaFoodLimit--;
        }
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_FOOD_ACCEPT), DialogLineFormat.PASSIVE, -1);

        Game.Persistence.playerFile.relationshipPoints++;
        playerFileGirl.relationshipPoints++;
        if (!item.Core.noStaminaCost)
        {
            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, -1, isAltGirl);
        }
        return null;
    }
}
