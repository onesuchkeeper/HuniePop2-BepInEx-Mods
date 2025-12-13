using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace SingleDate;

internal static class GirlNobody
{
    public static RelativeId Id => _id;
    private static RelativeId _id;

    public static void AddDataMods()
    {
        _id = new RelativeId(State.ModId, 0);

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

        var neutralExpressionId = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL);

        ModInterface.AddDataMod(new GirlDataMod(_id, InsertStyle.replace)
        {
            GirlName = "Nobody",
            SpecialCharacter = true,



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

            CellphoneHead = emptySpriteInfo,
            CellphoneHeadAlt = emptySpriteInfo,
            CellphoneMiniHead = emptySpriteInfo,
            CellphoneMiniHeadAlt = emptySpriteInfo,
            CellphonePortrait = emptySpriteInfo,
            CellphonePortraitAlt = emptySpriteInfo,
        });
    }
}
