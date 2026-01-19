using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleMomo(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//fish game
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//happy tobe here
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//ear get wet
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts];//chase ball
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//ear get wet
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//ask for id
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//dessert
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//lot of water
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//id
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//happy to be here

        girlMod.GirlAge = 33;
        SetCellphoneImages(girlMod, "momo");
        body.BackPosition = new VectorInfo(200, 450);
        body.HeadPosition = new VectorInfo(240, 780);

        girlMod.FavAnswers = new()
        {
            // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
            // {Hp2BaseMod.Favorites.IceCream,         FavIceCream.None},
            // {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
            // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
            // {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
            // {Hp2BaseMod.Favorites.PhoneApp,         FavPhoneApp.Games},
            // {Hp2BaseMod.Favorites.Exercise,         FavExercise.Hunting},
            // {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
            // {Hp2BaseMod.Favorites.ThemeParkRide,    FavThemeParkRide.None},
            // {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
            // {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.SleepIn},
            // {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
            // {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
            // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Fish},
            // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
            // {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
            // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
            // {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Hair},
            // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
            // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},

            {Questions.LastName,                       LastName.NotApplicable},
            {Questions.HomeWorld,                      Homeworld.Unsure},
            {Questions.Height,                         Height._5_2},
            {Questions.Weight,                         Weight._100},
            {Questions.Occupation,                     Occupation.Kitty},
            {Questions.CupSize,                        CupSize.B_Cup},
            {Questions.Birthday,                       Birthday.Oct_2},
            {Questions.Hobby,                          FavHobby.Sleeping},
            {Questions.FavColour,                      FavColour.Gold},
            {Questions.FavSeason,                      FavSeason.Spring},
            {Questions.FavHangout,                     FavHangout.HikingTrail},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
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

        //Drinks during day so doesn't have a rejection line
        var rejectSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieReject];
        var acceptSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieAccept];
        var rejectFood = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }
}
