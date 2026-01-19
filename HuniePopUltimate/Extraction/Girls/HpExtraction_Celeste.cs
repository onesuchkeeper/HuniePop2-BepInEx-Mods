using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleCeleste(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//feel amazing
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//beautiful planet
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//too many peeps
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//beautiful
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//too many people
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//thank for invite
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//colors and lights
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//thank
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//feel amazing
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//colors and lights
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//thank for invite

        //hot spring, scenic overlook, casino, outdoor lounge, carnival, restaurant
        girlMod.LocationGreetingDialogLines[LocationIds.BotanicalGarden] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//beautiful
        girlMod.LocationGreetingDialogLines[LocationIds.FarmersMarket] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//too many peeps
        girlMod.LocationGreetingDialogLines[LocationIds.HikingTrail] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//beautiful
        girlMod.LocationGreetingDialogLines[LocationIds.IceRink] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//wut? enviroment like planet
        girlMod.LocationGreetingDialogLines[LocationIds.WaterPark] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//colors
        girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//too many peeps

        girlMod.GirlAge = 34;
        SetCellphoneImages(girlMod, "celeste");
        body.BackPosition = new VectorInfo(200, 450);
        body.HeadPosition = new VectorInfo(220, 820);

        girlMod.FavAnswers = new()
        {
            // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Milk},
            // {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Vanilla},
            // {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
            // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
            // {Hp2BaseMod.Favorites.OnlineActivity,   FavOnlineActivity.Newgrounds},
            // {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
            // {Hp2BaseMod.Favorites.Exercise,         FavExercise.Hunting},
            // {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
            // {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
            // {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
            // {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
            // {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.ColdAndSnowy},
            // {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
            // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
            // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
            // {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
            // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
            // {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
            // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
            // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},

            {Questions.LastName,                       LastName.Luvendass},
            {Questions.HomeWorld,                      Homeworld.Tendricide},
            {Questions.Height,                         Height._5_9},
            {Questions.Weight,                         Weight._130},
            {Questions.Occupation,                     Occupation.Hunter},
            {Questions.CupSize,                        CupSize.DD_Cup},
            {Questions.Birthday,                       Birthday.July_20},
            {Questions.Hobby,                          FavHobby.Swimming},
            {Questions.FavColour,                      FavColour.Silver},
            {Questions.FavSeason,                      FavSeason.Winter},
            {Questions.FavHangout,                     FavHangout.Beach},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},//in case of non-stop dates
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

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
