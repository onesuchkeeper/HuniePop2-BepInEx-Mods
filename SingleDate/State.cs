using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

public static class State
{
    public static Vector3 DefaultPuzzleGridPosition;
    public static int ModId;

    public static bool IsLocationPairSingle() => IsSingle(Game.Session.Location.currentGirlPair);

    public static bool IsSingle(GirlPairDefinition def)
    {
        if (def == null)
        {
            return false;
        }

        var id = ModInterface.Data.GetDataId(GameDataType.GirlPair, def.id);

        return id.SourceId == ModId;
    }
}