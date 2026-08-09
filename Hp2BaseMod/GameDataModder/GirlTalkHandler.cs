using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

public class GirlTalkHandler : IGirlTalkHandler
{
    private static readonly int[] BAGGAGE_THRESHOLDS_POINT = [
        10,30,50
    ];

    public virtual ItemDefinition GetBaggageItem(PlayerFileGirl playerFileGirl) => GetBaggageItem(playerFileGirl, Enumerable.Empty<ItemDefinition>());

    protected ItemDefinition GetBaggageItem(PlayerFileGirl playerFileGirl, IEnumerable<ItemDefinition> except)
    {
		var pool = playerFileGirl.girlDefinition.baggageItemDefs
            .Except(playerFileGirl.learnedBaggage.Select(x => playerFileGirl.girlDefinition.baggageItemDefs[x]).Concat(except))
            .ToList();

        return pool.Any() 
            ? pool.GetRandom()
            : null;
    }

    public virtual TalkWithType SelectTalkType(PlayerFileGirl playerFileGirl)
    {
        if (playerFileGirl.learnedBaggage.Count < playerFileGirl.girlDefinition.baggageItemDefs.Count 
            && playerFileGirl.relationshipPoints >= BAGGAGE_THRESHOLDS_POINT[Mathf.Clamp(playerFileGirl.learnedBaggage.Count, 0, BAGGAGE_THRESHOLDS_POINT.Length - 1)])
        {
            return TalkWithType.BAGGAGE_CONVO;
        }

        return MathUtils.RandomBool() 
        ? TalkWithType.HER_QUESTION 
        : TalkWithType.FAVORITE_QUESTION;
    }
}