using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleJessie(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        if (!Plugin.UseHp1JessieStats.Value)
        {
            girlMod.FavoriteAffectionType = null;
            girlMod.LeastFavoriteAffectionType = null;
        }

        if (!Plugin.UseHp1JessieLines.Value)
        {
            girlMod.LinesByDialogTriggerId = null;
        }

        girlMod.FavAnswers = new(){
            {Questions.LastName,                       LastName.Maye},
            {Questions.Education,                      Education.Dropout},
            {Questions.Height,                         Height._5_7},
            {Questions.Weight,                         Weight._126},
            {Questions.Occupation,                     Occupation.PornStar},
            {Questions.CupSize,                        CupSize.G_Cup},
            {Questions.Birthday,                       Birthday.Jan_27},
            {Questions.Hobby,                          FavHobby.Drinking},
            {Questions.FavColour,                      FavColour.Orange},
            {Questions.FavSeason,                      FavSeason.Winter},
            {Questions.FavHangout,                     FavHangout.Bar},
        };

        ((GirlSpecialPartDataMod)body.specialParts[0]).RequiredHairstyles = new List<RelativeId>(){
            Hp2BaseMod.Styles.Relaxing
        };

        //the celebrity front hair has a different anchor point
        //in hp1 from every other doll part, so correct for it here
        var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)body.hairstyles[3]).FrontHairPart;
        celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
        celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;
        body.BackPosition = new VectorInfo(200, 450);
        body.HeadPosition = new VectorInfo(230, 780);

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

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

        //Drinks during day so doesn't have a rejection line
        var rejectSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieReject];
        var acceptSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieAccept];
        var rejectFood = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }
}
