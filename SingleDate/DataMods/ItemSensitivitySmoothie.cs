using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

internal static class ItemSensitivitySmoothie
{
    public static RelativeId SmoothieId => _smoothieId;
    private static RelativeId _smoothieId;

    public static RelativeId Exp => _expId;
    private static RelativeId _expId;

    public static RelativeId Level => _levelId;
    private static RelativeId _levelId;

    public static void AddDataMods(AssetBundle assetBundle)
    {
        _smoothieId = new RelativeId(State.ModId, 0);
        _expId = new RelativeId(State.ModId, 1);
        _levelId = new RelativeId(State.ModId, 2);

        var spriteInfo = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>("item_smoothie_sensitivity"));

        ModInterface.AddDataMod(new ItemDataMod(_smoothieId, InsertStyle.replace)
        {
            ItemType = ItemType.SMOOTHIE,
            ItemSpriteInfo = spriteInfo,
            ItemName = "Sensitivity Smoothie",
            ItemDescription = "+1 [[broken]@Sensitivity] EXP.",
            TooltipColorIndex = 6,
            StoreCost = 5,
            CategoryDescription = "Sensitivity",
            EnergyDefinitionID = new RelativeId(-1, 6),
            StoreSectionPreference = false,
        });

        ModInterface.AddDataMod(new ItemDataMod(_expId, InsertStyle.replace)
        {
            ItemType = ItemType.MISC,
            ItemSpriteInfo = spriteInfo,
            ItemName = "Sensitivity EXP",
            ItemDescription = "Earn [[broken]@Sensitivity] EXP by giving [[broken]@Sensitivity] smoothies to girls.",
            TooltipColorIndex = 6,
            CategoryDescription = "+(NUM0) EXP until Level (NUM1)"
        });

        ModInterface.AddDataMod(new ItemDataMod(_levelId, InsertStyle.replace)
        {
            ItemType = ItemType.MISC,
            ItemName = "Sensitivity Level (LEVEL)",
            ItemDescription = "[[broken]@Broken Heart] token matches will yield [[broken]-(13-(NUM0))%] Affection.",
            TooltipColorIndex = 6,
            CategoryDescription = "Affection Level • Sensitivity"
        });
    }
}
