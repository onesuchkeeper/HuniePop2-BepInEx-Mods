using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void AddHp2GirlLocMods()
    {
        foreach (var girl in Hp2BaseMod.Girls.NormalGirls.Append(Hp2BaseMod.Girls.KyuId))
        {
            var body = new GirlBodyDataMod(new RelativeId(-1, 0), Hp2BaseMod.Utility.InsertStyle.append)
            {
                LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.FarmersMarket, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity }},
                    {LocationIds.Casino, new GirlStyleInfo() { HairstyleId = Styles.Party, OutfitId = Styles.Party }},
                    {LocationIds.OutdoorLounge, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic }},
                    {LocationIds.BotanicalGarden, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity }},
                    {LocationIds.HotSprings, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water }},
                    {LocationIds.HikingTrail, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity }},
                    {LocationIds.IceRink, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity }},
                    {LocationIds.WaterPark, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water }},
                    {LocationIds.TennisCourts, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity }},
                    {LocationIds.ScenicOverlook, new GirlStyleInfo() { HairstyleId = Styles.Relaxing, OutfitId = Styles.Relaxing }},
                    {LocationIds.Restaurant, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic }},
                    {LocationIds.BedRoomDate, new GirlStyleInfo() { HairstyleId = Styles.Sexy, OutfitId = Styles.Sexy }},
                    {LocationIds.Carnival, new GirlStyleInfo(Styles.Activity)},
                }
            };

            if (girl == Hp2BaseMod.Girls.LolaId)
            {
                body.LocationIdToStyleInfo[LocationIds.TennisCourts] = new GirlStyleInfo(Styles.Bonus2);
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
