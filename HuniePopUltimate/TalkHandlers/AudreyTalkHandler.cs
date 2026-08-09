using System.Linq;
using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// Prevents Audrey from talking about baggages that referance hp2 until she has been introduced there
/// </summary>
public class AudreyTalkHandler : GirlTalkHandler
{
    public override ItemDefinition GetBaggageItem(PlayerFileGirl playerFileGirl)
    {
        return Game.Persistence.playerFile.metGirlPairs.Contains(ModInterface.GameData.GetGirlPair(Pairs.TiffanyAudrey)) 
            ? base.GetBaggageItem(playerFileGirl)
            : base.GetBaggageItem(playerFileGirl, [ModInterface.GameData.GetItem(Items.Audrey.Baggage3)]);
    }

    public override TalkWithType SelectTalkType(PlayerFileGirl playerFileGirl)
    {
        var result = base.SelectTalkType(playerFileGirl);

        if (result == TalkWithType.BAGGAGE_CONVO
            && !Game.Persistence.playerFile.metGirlPairs.Contains(ModInterface.GameData.GetGirlPair(Pairs.TiffanyAudrey)))
        {
            var hasValidBaggages = playerFileGirl.girlDefinition.baggageItemDefs
                .Except(playerFileGirl.learnedBaggage.Select(x => playerFileGirl.girlDefinition.baggageItemDefs[x]))
                .Any(x => x.ModId() != Items.Audrey.Baggage3);
            
            if (!hasValidBaggages)
            {
                return MathUtils.RandomBool() 
                    ? TalkWithType.HER_QUESTION 
                    : TalkWithType.FAVORITE_QUESTION;
            }
        }

        return result;
    }
}
