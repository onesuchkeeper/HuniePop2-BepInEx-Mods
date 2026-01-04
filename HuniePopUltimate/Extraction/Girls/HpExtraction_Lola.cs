using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleLola(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        if (!Plugin.UseHp1LolaStats.Value)
        {
            girlMod.FavoriteAffectionType = null;
            girlMod.LeastFavoriteAffectionType = null;
        }

        if (!Plugin.UseHp1LolaLines.Value)
        {
            girlMod.LinesByDialogTriggerId = null;
        }

        girlMod.FavAnswers = new() {
            {Questions.LastName,                       LastName.Rembrite},
            {Questions.Education,                      Education.College_4},
            {Questions.Height,                         Height._5_7},
            {Questions.Weight,                         Weight._122},
            {Questions.Occupation,                     Occupation.Stewardess},
            {Questions.CupSize,                        CupSize.E_Cup},
            {Questions.Birthday,                       Birthday.Feb_23},
            {Questions.Hobby,                          FavHobby.Tennis},
            {Questions.FavColour,                      FavColour.Blue},
            {Questions.FavSeason,                      FavSeason.Spring},
            {Questions.FavHangout,                     FavHangout.Cafe},
        };

        body.BackPosition = new VectorInfo(200, 450);
        body.HeadPosition = new VectorInfo(230, 780);

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)},

            {Locations.MassageSpa, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {Locations.Aquarium, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.SecludedCabana, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {Locations.PoolsideBar, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.GolfCourse, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {Locations.CruiseShip, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.RooftopLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {Locations.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {Locations.PrivateTable, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {Locations.SecretGrotto, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.RoyalSuite, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)},
            {Locations.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {Locations.OuterSpace, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
        };
    }
}
