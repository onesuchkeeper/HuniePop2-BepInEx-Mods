using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public ItemDataMod ExtractItem(OrderedDictionary def, RelativeId id, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw textureInfo)
    {
        if (def.TryGetValue("iconName", out string iconName)
            && def.TryGetValue("description", out string description)
            && spriteLookup.TryGetValue(iconName, out var iconSpriteDef)
            && TryMakeSpriteInfo(iconSpriteDef, textureInfo, out var spriteInfo)
            && def.TryGetValue("name", out string name))
        {
            return new ItemDataMod(id, Hp2BaseMod.Utility.InsertStyle.append, 0)
            {
                ItemSpriteInfo = spriteInfo,
                ItemName = name,
                ItemDescription = description,
            };
        }

        throw new Exception();
    }

    public void ExtractWeirdThing(OrderedDictionary def, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw textureInfo)
    {
        var mod = ExtractItem(def, Items.WeirdThing, spriteLookup, textureInfo);
        mod.StoreCost = 30;
        mod.AffectionType = PuzzleAffectionType.TALENT;
        mod.ItemType = ItemType.MISC;
        mod.CategoryDescription = "Special Item";
        mod.TooltipColorIndex = 0;
        mod.StoreSectionPreference = true;
        ModInterface.AddDataMod(mod);
    }

    public void ExtractGoldfish(OrderedDictionary def, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw textureInfo)
    {
        var mod = ExtractItem(def, Items.Goldfish, spriteLookup, textureInfo);
        mod.StoreCost = 30;
        mod.AffectionType = PuzzleAffectionType.ROMANCE;
        mod.ItemType = (ItemType)(-1);
        mod.CategoryDescription = "Special Item";
        mod.TooltipColorIndex = 0;
        mod.StoreSectionPreference = true;
        ModInterface.AddDataMod(mod);
    }
}
