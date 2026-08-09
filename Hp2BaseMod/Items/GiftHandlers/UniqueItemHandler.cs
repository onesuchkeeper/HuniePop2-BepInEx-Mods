namespace Hp2BaseMod;

public class UniqueItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_UNIQUE_REJECT = new RelativeId(-1, 24);
    private static readonly RelativeId DT_UNIQUE_ACCEPT = new RelativeId(-1, 23);

    public bool CanGive(ExpandedItemDefinition item, ExpandedGirlDefinition girl, PuzzleStatusGirl statusGirl, PlayerFileGirl fileGirl, bool altGirl)
    {
        if (!girl.Core.uniqueItemDefs.Contains(item.Core) 
            || fileGirl.receivedUniques.Contains(girl.Core.uniqueItemDefs.IndexOf(item.Core)) 
            || fileGirl.receivedUniques.Count > fileGirl.learnedBaggage.Count)
        {
            return false;
        }

        if (!item.Core.noStaminaCost && statusGirl.stamina <= 0)
        {
            return false;
        }

        return true;
    }

    public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
    {
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_UNIQUE_REJECT), DialogLineFormat.PASSIVE, -1);
    }

    public string OnGiveSucceed(ExpandedItemDefinition item, 
        UiDoll doll, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl playerFileGirl, 
        bool isAltGirl)
    {
        var preGiveLevel = Game.Persistence.playerFile.GetPassionLevel(false);
        playerFileGirl.ReceiveShoes(item.Core);
        var postGiveLevel = Game.Persistence.playerFile.GetPassionLevel(false);
        var text = "+1 Passion EXP";
        if (postGiveLevel != preGiveLevel)
        {
            doll.notificationBox.Show("Passion Level " + postGiveLevel.ToString() + " achieved!", 0f, false);
        }
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_UNIQUE_ACCEPT), DialogLineFormat.PASSIVE, -1);

        Game.Persistence.playerFile.relationshipPoints++;
        playerFileGirl.relationshipPoints++;
        if (!item.Core.noStaminaCost)
        {
            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, -1, isAltGirl);
        }

        return text;
    }
}
