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

        ModInterface.AddDataMod(new GirlDataMod(_id, InsertStyle.replace)
        {
            GirlName = "Nobody",
            SpecialCharacter = true,

            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(emptyPartId, InsertStyle.replace) {
                    X = 0,
                    Y = 0,
                    PartType = GirlPartType.BODY,
                    PartName = "Body",
                    SpriteInfo = emptySpriteInfo
                }
            },

            PartIdBody = emptyPartId,
            PartIdBlink = emptyPartId,
            PartIdBlushHeavy = emptyPartId,
            PartIdBlushLight = emptyPartId,
            PartIdMouthNeutral = emptyPartId,
            PartIdNipples = emptyPartId,
            HasAltStyles = false,

            CellphoneHead = emptySpriteInfo,
            CellphoneHeadAlt = emptySpriteInfo,
            CellphoneMiniHead = emptySpriteInfo,
            CellphoneMiniHeadAlt = emptySpriteInfo,
            CellphonePortrait = emptySpriteInfo,
            CellphonePortraitAlt = emptySpriteInfo,

            DefaultExpressionIndex = 0,
            DefaultHairstyleId = emptyPartId,
            DefaultOutfitId = emptyPartId,

            expressions = new List<IGirlSubDataMod<GirlExpressionSubDefinition>>(){
                new GirlExpressionDataMod(emptyPartId, InsertStyle.replace)
                {
                    ExpressionType = GirlExpressionType.NEUTRAL,
                    PartIdEyebrows = RelativeId.Default,
                    PartIdEyes = RelativeId.Default,
                    PartIdEyesGlow = RelativeId.Default,
                    PartIdMouthClosed = RelativeId.Default,
                    PartIdMouthOpen = RelativeId.Default
                }
            },

            hairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>(){
                new HairstyleDataMod(emptyPartId, InsertStyle.replace){
                    Name = string.Empty,
                    FrontHairPartId = RelativeId.Default,
                    BackHairPartId = RelativeId.Default
                }
            },

            outfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>()
            {
                new OutfitDataMod(emptyPartId, InsertStyle.replace)
                {
                    Name = string.Empty,
                    OutfitPartId = RelativeId.Default
                }
            }
        });
    }
}
