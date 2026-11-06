using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace SingleDate;

/// <summary>
/// Defines "Nobody", the empty girl used as girl one of every single pair. She is a special character with every
/// sprite empty, so she is never visible, and she is skipped by things like save data (see
/// <see cref="SingleSaveFile.GetGirl(RelativeId)"/>).
/// </summary>
internal static class GirlNobody
{
    /// <summary>Adds the Nobody girl with one body, expression, hairstyle and outfit that are all empty.</summary>
    internal static void AddDataMods()
    {
        // One empty part reused for every slot of the girl, with local id 0.
        var emptyPartId = new RelativeId(State.ModId, 0);
        var emptySpriteInfo = new SpriteInfoInternal("EmptySprite");

        var emptyPart = new GirlPartDataMod(emptyPartId, InsertStyle.replace)
        {
            X = 0,
            Y = 0,
            PartType = GirlPartType.BODY,
            PartName = "Body",
            SpriteInfo = emptySpriteInfo
        };

        // Base game's neutral expression id, used as Nobody's only expression.
        var neutralExpressionId = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL);

        ModInterface.DataMod.AddDataMod(new GirlDataMod(Girls.Nobody, InsertStyle.replace)
        {
            GirlName = "Nobody",
            // Marks her as a special character, this mod's save data skips these (see SingleSaveFile.GetGirl).
            SpecialCharacter = true,

            // Favorite and least favorite have to be something, Talent is arbitrary.
            FavoriteAffectionId = Hp2BaseMod.PuzzleAffectionId.Talent,
            LeastFavoriteAffectionId = Hp2BaseMod.PuzzleAffectionId.Talent,

            bodies = new List<IGirlBodyDataMod>()
            {
                new GirlBodyDataMod(new RelativeId(State.ModId,0), InsertStyle.append)
                {
                    PartBody = emptyPart,
                    PartBlink = emptyPart,
                    PartBlushHeavy = emptyPart,
                    PartBlushLight = emptyPart,
                    PartMouthNeutral = emptyPart,
                    PartNipples = emptyPart,
                    DefaultExpressionId = neutralExpressionId,
                    DefaultHairstyleId = emptyPartId,
                    DefaultOutfitId = emptyPartId,

                    expressions = new List<IBodySubDataMod<GirlExpressionSubDefinition>>(){
                        new GirlExpressionDataMod(neutralExpressionId, InsertStyle.replace)
                        {
                            ExpressionType = GirlExpressionType.NEUTRAL,
                            PartEyebrows = emptyPart,
                            PartEyes = emptyPart,
                            PartEyesGlow = emptyPart,
                            PartMouthClosed = emptyPart,
                            PartMouthOpen = emptyPart
                        }
                    },
                    hairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>(){
                        new HairstyleDataMod(emptyPartId, InsertStyle.replace){
                            Name = string.Empty,
                            FrontHairPart = emptyPart,
                            BackHairPart = emptyPart
                        }
                    },
                    outfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>()
                    {
                        new OutfitDataMod(emptyPartId, InsertStyle.replace)
                        {
                            Name = string.Empty,
                            OutfitPart = emptyPart
                        }
                    }
                }
            },

            HasAltStyles = false,

            // Empty sprites everywhere she could be shown in the cellphone.
            CellphoneHead = emptySpriteInfo,
            CellphoneHeadAlt = emptySpriteInfo,
            CellphoneMiniHead = emptySpriteInfo,
            CellphoneMiniHeadAlt = emptySpriteInfo,
            CellphonePortrait = emptySpriteInfo,
            CellphonePortraitAlt = emptySpriteInfo,
        });
    }
}
