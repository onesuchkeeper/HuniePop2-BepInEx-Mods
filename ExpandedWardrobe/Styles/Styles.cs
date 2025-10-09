using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal partial class Styles
{
    private static int _partCount = 0;

    public static void AddHair(List<IGirlSubDataMod<GirlHairstyleSubDefinition>> hairstyles,
        string name, string displayName, string girlName, int frontX, int frontY, int backX, int backY, bool isNSFW, bool isCodeUnlocked, bool isPurchased)
    {
        hairstyles.Add(new HairstyleDataMod(new RelativeId(Ids.ModId, hairstyles.Count), InsertStyle.replace)
        {
            Name = displayName,
            FrontHairPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.FRONTHAIR,
                PartName = $"{name}Fronthair{girlName}",
                X = frontX,
                Y = frontY,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_fronthair_{name}.png")))
            },
            BackHairPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.BACKHAIR,
                PartName = $"{name}Backhair{girlName}",
                X = backX,
                Y = backY,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_backhair_{name}.png")))
            },
            IsNSFW = isNSFW,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = false
        });
    }

    public static void AddOutfit(List<IGirlSubDataMod<GirlOutfitSubDefinition>> outfits,
        string name, string displayName, string girlName, int x, int y, bool isNSFW, bool isCodeUnlocked, bool isPurchased, bool hideNipples)
    {
        outfits.Add(new OutfitDataMod(new RelativeId(Ids.ModId, outfits.Count), InsertStyle.replace)
        {
            Name = displayName,
            OutfitPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.OUTFIT,
                PartName = $"{name}Outfit{girlName}",
                X = x,
                Y = y,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_outfit_{name}.png")))
            },
            IsNSFW = isNSFW,
            HideNipples = hideNipples,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = false,
        });
    }

    public static void AddPair(List<IGirlSubDataMod<GirlOutfitSubDefinition>> outfits,
        List<IGirlSubDataMod<GirlHairstyleSubDefinition>> hairstyles,
        string name, string displayName, string girlName,
        int outfitX, int outfitY, int frontX, int frontY, int backX, int backY,
        bool isNSFW, bool isCodeUnlocked, bool isPurchased, bool hideNipples)
    {
        var hairId = new RelativeId(Ids.ModId, hairstyles.Count);
        var outfitId = new RelativeId(Ids.ModId, outfits.Count);

        hairstyles.Add(new HairstyleDataMod(hairId, InsertStyle.replace)
        {
            Name = displayName,
            FrontHairPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.FRONTHAIR,
                PartName = $"{name}Fronthair{girlName}",
                X = frontX,
                Y = frontY,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_fronthair_{name}.png")))
            },
            BackHairPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.BACKHAIR,
                PartName = $"{name}Backhair{girlName}",
                X = backX,
                Y = backY,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_backhair_{name}.png")))
            },
            IsNSFW = isNSFW,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = true,
            PairOutfitId = outfitId
        });

        outfits.Add(new OutfitDataMod(outfitId, InsertStyle.replace)
        {
            Name = displayName,
            OutfitPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
            {
                PartType = GirlPartType.OUTFIT,
                PartName = $"{name}Outfit{girlName}",
                X = outfitX,
                Y = outfitY,
                SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_outfit_{name}.png")))
            },
            IsNSFW = isNSFW,
            HideNipples = hideNipples,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = true,
            PairHairstyleId = hairId
        });
    }
}
