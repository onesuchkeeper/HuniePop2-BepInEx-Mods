using System;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo;

public class GirlStyleInfo
{
    public RelativeId? OutfitId;

    public RelativeId? HairstyleId;

    public GirlStyleInfo()
    {

    }

    public GirlStyleInfo(RelativeId styleId)
    {
        OutfitId = HairstyleId = styleId;
    }

    public GirlStyleInfo(RelativeId outfitId, RelativeId hairId)
    {
        OutfitId = outfitId;
        HairstyleId = hairId;
    }

    public void SetData(ref GirlStyleInfo def)
    {
        if (def == null)
        {
            def = new GirlStyleInfo() { 
                OutfitId = RelativeId.Default, 
                HairstyleId = RelativeId.Default 
            };
        }

        ValidatedSet.SetValue(ref def.OutfitId, OutfitId, InsertStyle.replace);
        ValidatedSet.SetValue(ref def.HairstyleId, HairstyleId, InsertStyle.replace);
    }

    public void ReplaceRelativeIds(Func<RelativeId?, RelativeId?> getNewId)
    {
        OutfitId = getNewId(OutfitId);
        HairstyleId = getNewId(OutfitId);
    }

    public void Apply(UiDoll doll, int defaultOutfitIndex, int defaultHairstyleIndex)
    {
        var def = doll.soulGirlDefinition ?? doll.girlDefinition;
        if (!ModInterface.Data.TryGetDataId(GameDataType.Girl, def.id, out var girlId))
        {
            return;
        }

        var expandedGirl = def.GetExpansion();

        if (OutfitId.HasValue)
        {
            doll.ChangeOutfit(expandedGirl.OutfitLookup.TryGetIndex(OutfitId.Value, out var index)
                ? index
                : defaultOutfitIndex);
        }

        if (HairstyleId.HasValue)
        {
            doll.ChangeHairstyle(expandedGirl.HairstyleLookup.TryGetIndex(HairstyleId.Value, out var index)
                ? index
                : defaultHairstyleIndex);
        }
    }

    public override string ToString() => $"(Hair: {(HairstyleId.HasValue ? HairstyleId.Value : "null")}, Outfit: {(OutfitId.HasValue ? OutfitId.Value : "null")})";
}