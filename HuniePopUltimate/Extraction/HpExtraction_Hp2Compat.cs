using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void AddHp2GirlMods()
    {
        foreach (var girl in Hp2BaseMod.Girls.NormalGirls.Append(Hp2BaseMod.Girls.Kyu))
        {
            var body = new GirlBodyDataMod(new RelativeId(-1, 0), Hp2BaseMod.Utility.InsertStyle.append)
            {
                LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.FarmersMarket, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.Casino, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Party, OutfitId = Hp2BaseMod.Styles.Party }},
                    {LocationIds.OutdoorLounge, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Romantic, OutfitId = Hp2BaseMod.Styles.Romantic }},
                    {LocationIds.BotanicalGarden, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.HotSprings, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Water, OutfitId = Hp2BaseMod.Styles.Water }},
                    {LocationIds.HikingTrail, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.IceRink, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.WaterPark, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Water, OutfitId = Hp2BaseMod.Styles.Water }},
                    {LocationIds.TennisCourts, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.ScenicOverlook, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Relaxing, OutfitId = Hp2BaseMod.Styles.Relaxing }},
                    {LocationIds.Restaurant, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Romantic, OutfitId = Hp2BaseMod.Styles.Romantic }},
                    {LocationIds.BedRoomDate, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Sexy, OutfitId = Hp2BaseMod.Styles.Sexy }},
                    {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                }
            };

            if (girl == Hp2BaseMod.Girls.Lola)
            {
                body.LocationIdToStyleInfo[LocationIds.TennisCourts] = new GirlStyleInfo(Hp2BaseMod.Styles.Bonus2);
            }

            ModInterface.AddDataMod(new GirlDataMod(girl, Hp2BaseMod.Utility.InsertStyle.append)
            {
                bodies = new List<Hp2BaseMod.GameDataInfo.Interface.IGirlBodyDataMod>(){
                    body
                }
            });
        }
    }
}
