namespace Hp2BaseMod;

public class NonGiftItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_SHOES_REJECT = new RelativeId(-1, 26);
    private static readonly RelativeId DT_SHOES_ACCEPT = new RelativeId(-1, 25);

    public bool CanGive(ExpandedItemDefinition item, ExpandedGirlDefinition girl, PuzzleStatusGirl statusGirl, PlayerFileGirl fileGirl, bool altGirl) 
        => false;

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
        ModInterface.Log.Error("Give succeeded on Non-Gift item");
        return null;
    }
}
