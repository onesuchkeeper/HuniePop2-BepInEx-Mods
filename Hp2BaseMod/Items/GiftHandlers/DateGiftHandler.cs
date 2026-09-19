namespace Hp2BaseMod;

public class DateGiftItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_GIFT_REJECT = new RelativeId(-1, 33);
    private static readonly RelativeId DT_GIFT_ACCEPT = new RelativeId(-1, 30);
    private static readonly RelativeId DT_GIFT_WEIRD = new RelativeId(-1, 32);
    private static readonly RelativeId DT_GIFT_SEXY = new RelativeId(-1, 31);

    public bool CanGive(ExpandedItemDefinition item, ExpandedGirlDefinition girl, PuzzleStatusGirl statusGirl, PlayerFileGirl fileGirl, bool altGirl)
    {
        if (ModInterface.GameState.CurrentState.Id != GameStateId.Puzzle
            || !Game.Session.Puzzle.TutorialStepCheck(PuzzleTutorialStepType.GIFT)
            || item.Core.difficultyExclusive && item.Core.difficulty != Game.Persistence.playerFile.settingDifficulty
            || Game.Session.Ailment.Trigger(item.Core).blockGift)
        {
            return false;
        }

        if (item.Core.dateGiftAilment && item.Core.ailmentDefinition != null 
            && !statusGirl.HasAilment(item.Core.ailmentDefinition))
        {
             return true;
        }

        //TODO, split ability preforming from able-to-preform validation
        if (!item.Core.dateGiftAilment && item.Core.abilityDefinition != null 
            && Game.Session.Ability.PerformAbility(item.Core.abilityDefinition, altGirl, null))
        {
            return true;
        }

        return false;
    }

    public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
    {
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_GIFT_REJECT), DialogLineFormat.UNCHECKED, -1);
    }

    public string OnGiveSucceed(ExpandedItemDefinition item, 
        UiDoll doll, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl playerFileGirl, 
        bool isAltGirl)
    {
        Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.SENTIMENT, -item.Core.GetUseCost(), isAltGirl);
        if (item.Core.dateGiftType == ItemDateGiftType.HYGIENE && !doll.soulGirlDefinition.specialCharacter)
        {
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_GIFT_WEIRD), DialogLineFormat.UNCHECKED, -1);
        }
        else if (item.Core.dateGiftType == ItemDateGiftType.SEX_TOYS && !doll.soulGirlDefinition.specialCharacter)
        {
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_GIFT_SEXY), DialogLineFormat.UNCHECKED, -1);
        }
        else
        {
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_GIFT_ACCEPT), DialogLineFormat.UNCHECKED, -1);
        }
        Game.Session.Ailment.Trigger(AilmentTriggerType.POST_GIFT, null);
        Game.Session.Cutscenes.standbyProceed = true;
        return null;
    }
}
