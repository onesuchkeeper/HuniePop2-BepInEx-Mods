using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseModTweaks;

public class KyuBodyDataMod : IGirlBodyDataMod
{
    public RelativeId Id => _decorated.Id;

    public int LoadPriority => _decorated.LoadPriority;

    private readonly IGirlBodyDataMod _decorated;
    public KyuBodyDataMod(IGirlBodyDataMod decorated)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public IEnumerable<IBodySubDataMod<GirlExpressionSubDefinition>> GetExpressions() => _decorated.GetExpressions();

    public IEnumerable<IBodySubDataMod<GirlHairstyleSubDefinition>> GetHairstyles() => _decorated.GetHairstyles();

    public IEnumerable<IBodySubDataMod<GirlOutfitSubDefinition>> GetOutfits() => _decorated.GetOutfits();

    public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods() => _decorated.GetPartDataMods();

    public IEnumerable<IBodySubDataMod<GirlSpecialPartSubDefinition>> GetSpecialPartMods() => _decorated.GetSpecialPartMods();

    public void RequestInternals(AssetProvider assetProvider) => _decorated.RequestInternals(assetProvider);

    public void SetData(GirlBodySubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, GirlDefinition girlDef)
    {
        _decorated.SetData(def, gameData, assetProvider, girlDef);
        
        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Activity).outfitName = "Fairy Uniform";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Activity).hairstyleName = "Wing Clips";

        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Relaxing).outfitName = "Studio Rep";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Relaxing).hairstyleName = "Cum Slut";

        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Party).outfitName= "Sailor Skank";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Party).hairstyleName = "Power Band";

        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Romantic).outfitName = "Star Saviour";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Romantic).hairstyleName = "Platinum Princess";
        
        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Sexy).outfitName = "Aurora Mist";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Sexy).hairstyleName = "Such A Vibe";
            
        girlDef.GetExpansion().GetOutfit(def, Hp2BaseMod.Styles.Water).outfitName = "Floral Fairy";
        girlDef.GetExpansion().GetHairstyle(def, Hp2BaseMod.Styles.Water).hairstyleName = "Canopy";
            
    }
}
