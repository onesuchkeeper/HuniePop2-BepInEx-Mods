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
        ICollection<RelativeId> hairstyles;
        ICollection<RelativeId> outfits;

        if (playerFileGirl == null)
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
                    if (id == RelativeId.Default) ModInterface.Log.LogInfo($"bad index: {index}");
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

        ModInterface.Log.LogInfo(string.Join(",", outfits));

        if (!nsfw)
        {
            outfits = outfits.Where(x => !girlExpansion.GetOutfit(x).Expansion().IsNSFW).ToArray();
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
        var selectedOutfit = girlExpansion.GetOutfit(outfitId);

        var hairStyleId = (!unpaired && selectedOutfit.pairHairstyleIndex != -1)
            ? girlExpansion.HairstyleIndexToId[selectedOutfit.pairHairstyleIndex]
            : hairstyles.ElementAt(UnityEngine.Random.Range(0, hairstyles.Count()));

        style = new GirlStyleInfo()
        {
            OutfitId = outfitId,
            HairstyleId = hairStyleId
        };
    }
}
