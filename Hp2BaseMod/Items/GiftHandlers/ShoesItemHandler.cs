namespace Hp2BaseMod;

public class ShoesItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_SHOES_REJECT = new RelativeId(-1, 26);
    private static readonly RelativeId DT_SHOES_ACCEPT = new RelativeId(-1, 25);

    public bool CanGive(ExpandedItemDefinition item, ExpandedGirlDefinition girl, PuzzleStatusGirl statusGirl, PlayerFileGirl fileGirl, bool altGirl)
    {
        if (!girl.Core.shoesItemDefs.Contains(item.Core) 
            || fileGirl.receivedShoes.Contains(girl.Core.shoesItemDefs.IndexOf(item.Core)) 
            || fileGirl.receivedShoes.Count > fileGirl.learnedBaggage.Count)
        {
            return false;
        }

        if (statusGirl.stamina <= 0)
        {
            return false;
        }

        return true;
    }

    public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
    {
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SHOES_REJECT), DialogLineFormat.PASSIVE, -1);
    }

    public string OnGiveSucceed(ExpandedItemDefinition item, 
        UiDoll doll, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl playerFileGirl, 
        bool isAltGirl)
    {
        var preGiveLevel = Game.Persistence.playerFile.GetStyleLevel(false);
        playerFileGirl.ReceiveShoes(item.Core);
        var postGiveLevel = Game.Persistence.playerFile.GetStyleLevel(false);
        var text = "+1 Style EXP";
        if (postGiveLevel != preGiveLevel)
        {
            doll.notificationBox.Show("Style Level " + postGiveLevel.ToString() + " achieved!", 0f, false);
        }
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SHOES_ACCEPT), DialogLineFormat.PASSIVE, -1);

        Game.Persistence.playerFile.relationshipPoints++;
        playerFileGirl.relationshipPoints++;
        if (!item.Core.noStaminaCost)
        {
            Game.Session.Puzzle.puzzleStatus.GetExpansion().AddResourceValue(PuzzleResourceId.Stamina, -1, isAltGirl);
        }

        return text;
    }
}
