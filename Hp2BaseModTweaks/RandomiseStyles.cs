using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseModTweaks;

public static class RandomizeStyles
{
    public static void On_RequestStyleChange(object sender, RequestStyleChangeEventArgs args)
    {
        var girlSave = Plugin.Save.GetCurrentFile().GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, args.Def.id));

        if (girlSave.RandomizeStyles)
        {
            args.ApplyChance = 1;
        }

        RandomizeStyle(args.Def, girlSave.UnpairRandomStyles, out args.Style, args.Loc.locationType == LocationType.HUB);
    }

    private static void RandomizeStyle(GirlDefinition girl, bool unpaired, out GirlStyleInfo style, bool anyOutfit = false)
    {
        try
        {
            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girl);

            IEnumerable<RelativeId> outfits = null;
            IEnumerable<RelativeId> hairstyles = null;

            if (playerFileGirl == null || anyOutfit)
            {
                outfits = ModInterface.Data.GetAllOutfitIds(girlId);
                hairstyles = ModInterface.Data.GetAllHairstyleIds(girlId);
            }
            else
            {
                var outfitPool = new HashSet<RelativeId>();
                outfits = outfitPool;
                var hairstylePool = new HashSet<RelativeId>();
                hairstyles = hairstylePool;

                foreach (var index in playerFileGirl.unlockedOutfits)
                {
                    if (ModInterface.Data.TryGetOutfitId(girlId, index, out var id))
                    {
                        outfitPool.Add(id);
                    }
                }

                foreach (var index in playerFileGirl.unlockedHairstyles)
                {
                    if (ModInterface.Data.TryGetHairstyleId(girlId, index, out var id))
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

            var outfitId = outfits.ElementAt(UnityEngine.Random.Range(0, outfits.Count() - 1));

            //if paired and a paired hairstyle exists, use that, otherwise pick a random one
            if (!(!unpaired && hairstyles.TryGetFirst(x => x == outfitId, out var hairStyleId)))
            {
                hairStyleId = hairstyles.ElementAt(UnityEngine.Random.Range(0, hairstyles.Count() - 1));
            }

            style = new GirlStyleInfo()
            {
                OutfitId = outfitId,
                HairstyleId = hairStyleId
            };
        }
        catch (Exception e)
        {
            ModInterface.Log.LogError($"RandomizeStyle", e);
            style = null;
        }
    }
}
