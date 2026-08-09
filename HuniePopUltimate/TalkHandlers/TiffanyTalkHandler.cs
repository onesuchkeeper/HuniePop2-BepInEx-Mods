using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// Prevents Tiffany from talking about baggages that mention hp2 until she has been introduced there
/// </summary>
public class TiffanyTalkHandler : GirlTalkHandler
{
    public override TalkWithType SelectTalkType(PlayerFileGirl playerFileGirl)
    {
        var result = base.SelectTalkType(playerFileGirl);

        if (result == TalkWithType.BAGGAGE_CONVO
            && !Game.Persistence.playerFile.metGirlPairs.Contains(ModInterface.GameData.GetGirlPair(Pairs.TiffanyAudrey)))
        {
            return MathUtils.RandomBool() 
                ? TalkWithType.HER_QUESTION 
                : TalkWithType.FAVORITE_QUESTION;
        }

        return result;
    }
}
