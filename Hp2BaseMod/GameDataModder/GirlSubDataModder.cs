using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

internal static class GirlSubDataModder
{
    /// <summary>
    /// by default each has one default (0), 12 for the normal girls, 1 for kyu, 2 for nymphojinn. 1+12+1+2=16
    /// </summary>
    private const int DEFAULT_DT_SET_COUNT = 16;

    public static void GatherSubMods(IEnumerable<IGirlDataMod> girlDataMods,
        out Dictionary<RelativeId, Dictionary<RelativeId, BodyData>> GirlToBodyToMods,
        out Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IDialogLineDataMod>>>> dialogLineModsByIdByDialogTriggerByGirlId)
    {
        GirlToBodyToMods = new();
        dialogLineModsByIdByDialogTriggerByGirlId = new();

        foreach (var girlMod in girlDataMods)
        {
            var bodyDataDict = GirlToBodyToMods.GetOrNew(girlMod.Id);
            if (girlMod.GetBodyMods() is not null and var bodyMods)
            {
                foreach (var bodyMod in bodyMods)
                {
                    var bodyData = bodyDataDict.GetOrNew(bodyMod.Id);
                    bodyData.bodyMods.Add(bodyMod);
                    AddGirlSubMods(bodyMod.GetPartDataMods(), bodyData.partMods);
                    AddGirlSubMods(bodyMod.GetExpressions(), bodyData.expressionMods);
                    AddGirlSubMods(bodyMod.GetOutfits(), bodyData.outfitMods);
                    AddGirlSubMods(bodyMod.GetHairstyles(), bodyData.hairstyleMods);
                    AddGirlSubMods(bodyMod.GetSpecialPartMods(), bodyData.specialPartMods);

                    //parts have mirrors and alts, only 1 deep
                    var subParts = bodyData.partMods.SelectMany(x => x.Value).SelectMany(x => x.GetPartDataMods()).Where(x => x != null).ToArray();
                    if (subParts.Any())
                    {
                        foreach (var subMod in subParts)
                        {
                            bodyData.partMods.GetOrNew(subMod.Id).Add(subMod);
                        }
                    }
                }
            }
        }
    }

    private static void AddGirlSubMods<T>(IEnumerable<IBodySubDataMod<T>> subMods,
            Dictionary<RelativeId, List<IBodySubDataMod<T>>> subModListsById)
    {
        if (subMods != null && subMods.Any())
        {
            foreach (var subMod in subMods.Where(x => x != null))
            {
                var subModList = subModListsById.GetOrNew(subMod.Id);
                subModList.Add(subMod);
            }
        }
    }

    // there are inconsistencies in how dialog triggers are handled, yay.
    // Some are indexed by girl id, others are made specifically for the hub, and there's no easy way to differentiate them
    // so here's just a manual list, maybe add this to default data later. If someone wants to mod these they can make a dll I'm not gonna support it via json
    // greeting = 49
    // valediction = 50
    // nap = 51
    // wakeup = 52
    // wing check = 53
    // pre nymph = 54
    // pos nymph = 55
    // nonstop = 56
    // wardrobe = 57
    // album = 58
    public static bool IsGirlDialogTrigger(DialogTriggerDefinition dt) => dt.id < 49 || dt.id > 58;
}
