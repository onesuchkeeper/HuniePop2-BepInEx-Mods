using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseModTweaks;

public static class RandomizeStyles
{
    public static void On_RequestStyleChange(RequestStyleChangeEventArgs args)
    {
        var girlSave = Plugin.Save.GetCurrentFile().GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, args.Def.id));

        if (girlSave.RandomizeStyles
            && (args.Loc.locationType != LocationType.DATE || (Game.Persistence.playerFile.GetPlayerFileGirl(args.Def)?.stylesOnDates ?? false)))
        {
            args.ApplyChance = 1;
            RandomizeStyle(args.Def, girlSave.UnpairRandomStyles, args.Loc.locationType == LocationType.HUB, out args.Style);
        }
    }

    private static void RandomizeStyle(GirlDefinition girl, bool unpaired, bool anyOutfit, out GirlStyleInfo style)
    {
        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girl);
        var girlExpansion = girl.Expansion();
        ICollection<RelativeId> hairstyles;
        ICollection<RelativeId> outfits;

        if (playerFileGirl == null || anyOutfit)
        {
            outfits = girlExpansion.OutfitIdToIndex.Keys;
            hairstyles = girlExpansion.HairstyleIdToIndex.Keys;
        }
        else
        {
            var outfitPool = new HashSet<RelativeId>();
            outfits = outfitPool;
            var hairstylePool = new HashSet<RelativeId>();
            hairstyles = hairstylePool;

            foreach (var index in playerFileGirl.unlockedOutfits)
            {
                if (girlExpansion.OutfitIndexToId.TryGetValue(index, out var id))
                {
                    outfitPool.Add(id);
                }
            }

            foreach (var index in playerFileGirl.unlockedHairstyles)
            {
                if (girlExpansion.HairstyleIndexToId.TryGetValue(index, out var id))
                {
                    hairstylePool.Add(id);
                }
            }
        }

        if (!outfits.Any())
        {
            outfits = [RelativeId.Default];
        }

        if (!hairstyles.Any())
        {
            hairstyles = [RelativeId.Default];
        }

        var outfitId = outfits.ElementAt(UnityEngine.Random.Range(0, outfits.Count()));
        var outfit = girlExpansion.GetOutfit(girl, outfitId);

        var hairStyleId = (!unpaired && outfit.pairHairstyleIndex != -1)
            ? girlExpansion.HairstyleIndexToId[outfit.pairHairstyleIndex]
            : hairstyles.ElementAt(UnityEngine.Random.Range(0, hairstyles.Count()));

        style = new GirlStyleInfo()
        {
            OutfitId = outfitId,
            HairstyleId = hairStyleId
        };
    }
}
