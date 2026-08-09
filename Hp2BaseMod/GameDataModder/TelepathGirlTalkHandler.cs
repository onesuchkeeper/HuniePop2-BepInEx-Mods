using System.Linq;

namespace Hp2BaseMod;

public class TelepathGirlTalkHandler : GirlTalkHandler
{
    public override TalkWithType SelectTalkType(PlayerFileGirl playerFileGirl)
    {
        var result = base.SelectTalkType(playerFileGirl);

        //must meet all girls before using zoey's her_questions
        if (result != TalkWithType.HER_QUESTION 
            || Game.Data.Girls.GetAllBySpecial(false).All(x => Game.Persistence.playerFile.GetPlayerFileGirl(x).playerMet))
        {
            return result;
        }

        return TalkWithType.FAVORITE_QUESTION;
    }
}