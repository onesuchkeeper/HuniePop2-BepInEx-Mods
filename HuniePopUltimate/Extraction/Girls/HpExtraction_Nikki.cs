using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleNikki(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//not my scene
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//burn alive
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//burn alive
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.BotanicalGarden];//allergies
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//burn alive
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//romantic
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//not an arcade
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//dishes gross
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//water nice
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.IceRink];//might be fun
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//not my scene but nice

        girlMod.GirlAge = 20;
        SetCellphoneImages(girlMod, "nikki");
        body.BackPosition = new VectorInfo(195, 440);
        body.HeadPosition = new VectorInfo(230, 780);

        girlMod.FavAnswers = new()
        {
            // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
            // {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Vanilla},
            // {Hp2BaseMod.Favorites.MusicGenre,       Hp2BaseMod.FavMusicGenre.Techno},
            // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
            // {Hp2BaseMod.Favorites.OnlineActivity,   FavOnlineActivity.Newgrounds},
            // {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
            // {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
            // {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
            // {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
            // {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
            // {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
            // {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
            // {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
            // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
            // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
            // {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
            // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
            // {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
            // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
            // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},

            {Questions.LastName,                       LastName.AnnMarie},
            {Questions.Education,                      Education.Highschool},
            {Questions.Height,                         Height._5_0},
            {Questions.Weight,                         Weight._104},
            {Questions.Occupation,                     Occupation.Barista},
            {Questions.CupSize,                        CupSize.C_Cup},
            {Questions.Birthday,                       Birthday.May_13},
            {Questions.Hobby,                          FavHobby.Gaming},
            {Questions.FavColour,                      FavColour.Cream},
            {Questions.FavSeason,                      FavSeason.Winter},
            {Questions.FavHangout,                     FavHangout.Home},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
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
