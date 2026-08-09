using UnityEngine;

namespace Hp2BaseMod;

public class SmoothieItemHandler : IItemGiftHandler
{
    private static readonly RelativeId DT_SMOOTHIE_ACCEPT = new RelativeId(-1, 18);
    private static readonly RelativeId DT_SMOOTHIE_FULL = new RelativeId(-1, 19);
    private static readonly RelativeId DT_SMOOTHIE_REJECT = new RelativeId(-1, 20);

    private readonly RelativeId _affectionId;
    public SmoothieItemHandler(RelativeId affectionId)
    {
        _affectionId = affectionId;
    }

    public bool CanGive(ExpandedItemDefinition item, 
        ExpandedGirlDefinition girl, 
        PuzzleStatusGirl statusGirl, 
        PlayerFileGirl fileGirl, 
        bool altGirl)
    {
        if (item.Affection == girl.LeastFavAffection
            || Game.Persistence.playerFile.GetAffectionLevelExp(_affectionId, false) >= 24
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
        var initialLevel = Game.Persistence.playerFile.GetAffectionLevel(_affectionId, false);
        var expAmount = Mathf.Min(
            (_affectionId == girl.FavAffection.Id) 
                ? 2
                : 1,
            24 - Game.Persistence.playerFile.GetAffectionLevelExp(_affectionId, false));
        Game.Persistence.playerFile.AddAffectionLevelExp(_affectionId, expAmount);
        var updatedLevel = Game.Persistence.playerFile.GetAffectionLevel(_affectionId, false);
        var text = string.Concat(
        [
            "+",
            expAmount.ToString(),
            " ",
            StringUtils.Titleize(item.Affection.Name),
            " EXP"
        ]);

        if (updatedLevel != initialLevel)
        {
            doll.notificationBox.Show(StringUtils.Titleize(item.Affection.Name) + " Level " + updatedLevel.ToString() + " achieved!", 0f, false);
        }
        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_ACCEPT), DialogLineFormat.PASSIVE, -1);

        Game.Persistence.playerFile.relationshipPoints++;
        playerFileGirl.relationshipPoints++;
        if (!item.Core.noStaminaCost)
        {
            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, -1, isAltGirl);
        }

        return text;
    }

    public void OnGiveFailed(ExpandedItemDefinition item, ExpandedGirlDefinition girl, UiDoll doll)
    {
        if (item.Affection != girl.LeastFavAffection 
            && Game.Persistence.playerFile.GetAffectionLevelExp(_affectionId, false) >= 24)
        {
            doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_FULL), DialogLineFormat.PASSIVE, -1);
            return;
        }

        doll.ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(DT_SMOOTHIE_REJECT), DialogLineFormat.PASSIVE, -1);
    }
}
