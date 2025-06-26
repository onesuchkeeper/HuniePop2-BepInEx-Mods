using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal partial class Styles
{
    public static void AddHair(List<IGirlSubDataMod<GirlPartSubDefinition>> parts, List<IGirlSubDataMod<GirlHairstyleSubDefinition>> hairstyles,
        string name, string displayName, string girlName, int frontX, int frontY, int backX, int backY, bool isNSFW, bool isCodeUnlocked, bool isPurchased)
    {
        var frontId = new RelativeId(Ids.ModId, parts.Count);
        var backId = new RelativeId(Ids.ModId, frontId.LocalId + 1);

        parts.Add(new GirlPartDataMod(frontId, InsertStyle.replace)
        {
            PartType = GirlPartType.FRONTHAIR,
            PartName = $"{name}Fronthair{girlName}",
            X = frontX,
            Y = frontY,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_fronthair_{name}.png")))
        });

        parts.Add(new GirlPartDataMod(backId, InsertStyle.replace)
        {
            PartType = GirlPartType.BACKHAIR,
            PartName = $"{name}Backhair{girlName}",
            X = backX,
            Y = backY,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_backhair_{name}.png")))
        });

        hairstyles.Add(new HairstyleDataMod(new RelativeId(Ids.ModId, hairstyles.Count), InsertStyle.replace)
        {
            Name = displayName,
            FrontHairPartId = frontId,
            BackHairPartId = backId,
            IsNSFW = isNSFW,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = false
        });
    }

    public static void AddOutfit(List<IGirlSubDataMod<GirlPartSubDefinition>> parts, List<IGirlSubDataMod<GirlOutfitSubDefinition>> outfits,
        string name, string displayName, string girlName, int x, int y, bool isNSFW, bool isCodeUnlocked, bool isPurchased, bool hideNipples)
    {
        var partId = new RelativeId(Ids.ModId, parts.Count);

        parts.Add(new GirlPartDataMod(partId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = $"{name}Outfit{girlName}",
            X = x,
            Y = y,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_outfit_{name}.png")))
        });

        outfits.Add(new OutfitDataMod(new RelativeId(Ids.ModId, outfits.Count), InsertStyle.replace)
        {
            Name = displayName,
            OutfitPartId = partId,
            IsNSFW = isNSFW,
            HideNipples = hideNipples,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = false,
        });
    }

    public static void AddPair(List<IGirlSubDataMod<GirlPartSubDefinition>> parts,
        List<IGirlSubDataMod<GirlOutfitSubDefinition>> outfits,
        List<IGirlSubDataMod<GirlHairstyleSubDefinition>> hairstyles,
        string name, string displayName, string girlName,
        int outfitX, int outfitY, int frontX, int frontY, int backX, int backY,
        bool isNSFW, bool isCodeUnlocked, bool isPurchased, bool hideNipples)
    {
        var outfitPartId = new RelativeId(Ids.ModId, parts.Count);
        var frontId = new RelativeId(Ids.ModId, outfitPartId.LocalId + 1);
        var backId = new RelativeId(Ids.ModId, frontId.LocalId + 1);

        parts.Add(new GirlPartDataMod(outfitPartId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = $"{name}Outfit{girlName}",
            X = outfitX,
            Y = outfitY,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_outfit_{name}.png")))
        });

        parts.Add(new GirlPartDataMod(frontId, InsertStyle.replace)
        {
            PartType = GirlPartType.FRONTHAIR,
            PartName = $"{name}Fronthair{girlName}",
            X = frontX,
            Y = frontY,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_fronthair_{name}.png")))
        });

        parts.Add(new GirlPartDataMod(backId, InsertStyle.replace)
        {
            PartType = GirlPartType.BACKHAIR,
            PartName = $"{name}Backhair{girlName}",
            X = backX,
            Y = backY,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, $"{girlName}_backhair_{name}.png")))
        });

        var hairId = new RelativeId(Ids.ModId, hairstyles.Count);
        var outfitId = new RelativeId(Ids.ModId, outfits.Count);

        hairstyles.Add(new HairstyleDataMod(hairId, InsertStyle.replace)
        {
            Name = displayName,
            FrontHairPartId = frontId,
            BackHairPartId = backId,
            IsNSFW = isNSFW,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = true,
            PairOutfitId = outfitId
        });

        outfits.Add(new OutfitDataMod(outfitId, InsertStyle.replace)
        {
            Name = displayName,
            OutfitPartId = outfitPartId,
            IsNSFW = isNSFW,
            HideNipples = hideNipples,
            IsCodeUnlocked = isCodeUnlocked,
            IsPurchased = isPurchased,
            TightlyPaired = true,
            PairHairstyleId = hairId
        });
    }
}
