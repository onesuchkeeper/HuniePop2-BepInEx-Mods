using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseModTweaks;

public static class RandomizeStyles
{
    public static void On_RequestStyleChange(RequestStyleChangeEventArgs args)
    {
        var girlSave = Plugin.Save.GetCurrentFile().GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, args.Def.id));

        if (girlSave.RandomizeStyles)
        {
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(args.Def);

            if (args.Loc.locationType != LocationType.DATE
                || (playerFileGirl != null && playerFileGirl.stylesOnDates))
            {
                args.ApplyChance = 1;
                RandomizeStyle(args.Def.Expansion(),
                    playerFileGirl,
                    girlSave.UnpairRandomStyles,
                    girlSave.AllowNsfwRandomStyles && (!Game.Persistence.playerData.censoredMode),
                    out args.Style);
            }
        }
    }

    private static void RandomizeStyle(ExpandedGirlDefinition girlExpansion, PlayerFileGirl playerFileGirl, bool unpaired, bool nsfw, out GirlStyleInfo style)
    {
        IEnumerable<RelativeId> hairstyles;
        IEnumerable<RelativeId> outfits;

        if (playerFileGirl == null)
        {
            outfits = girlExpansion.OutfitLookup.Ids;
            hairstyles = girlExpansion.HairstyleLookup.Ids;
        }
        else
        {
            var outfitPool = new HashSet<RelativeId>();
            outfits = outfitPool;
            var hairstylePool = new HashSet<RelativeId>();
            hairstyles = hairstylePool;

            foreach (var index in playerFileGirl.unlockedOutfits)
            {
                if (girlExpansion.OutfitLookup.TryGetId(index, out var id))
                {
                    outfitPool.Add(id);
                }
            }

            foreach (var index in playerFileGirl.unlockedHairstyles)
            {
                if (girlExpansion.HairstyleLookup.TryGetId(index, out var id))
                {
                    hairstylePool.Add(id);
                }
            }
        }

        outfits = outfits.Where(x =>
        {
            var outfit = girlExpansion.GetOutfit(x);
            if (outfit == null) return false;

            return nsfw || !outfit.Expansion().IsNSFW;
        }).ToArray();

        if (!outfits.Any())
        {
            outfits = [RelativeId.Default];
        }

        if (!hairstyles.Any())
        {
            hairstyles = [RelativeId.Default];
        }

        var outfitId = outfits.ElementAt(UnityEngine.Random.Range(0, outfits.Count()));
        var selectedOutfit = girlExpansion.GetOutfit(outfitId);

        RelativeId hairStyleId;
        if (!unpaired && selectedOutfit.pairHairstyleIndex != -1)
        {
            hairStyleId = girlExpansion.HairstyleLookup[selectedOutfit.pairHairstyleIndex];
        }
        else
        {
            var pool = hairstyles.Where(x =>
            {
                var hairstyle = girlExpansion.GetHairstyle(x);
                if (hairstyle == null) return false;

                return nsfw || !hairstyle.Expansion().IsNSFW;
            }).ToArray();

            hairStyleId = pool.GetRandom();
        }

        style = new GirlStyleInfo()
        {
            OutfitId = outfitId,
            HairstyleId = hairStyleId
        };
    }
}
