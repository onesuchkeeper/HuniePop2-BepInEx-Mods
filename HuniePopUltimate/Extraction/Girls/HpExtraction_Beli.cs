using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleBeli(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//relaxing
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//relaxing
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//junk food
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts];//if I win
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//bikini
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts];//if I win
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//fancy
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//private oasis
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//carried away
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//fancy

        girlMod.GirlAge = 26;
        SetCellphoneImages(girlMod, "beli");
        body.BackPosition = new VectorInfo(200, 450);
        body.HeadPosition = new VectorInfo(245, 800);

        girlMod.FavAnswers = new()
        {
            // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
            // {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
            // {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
            // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
            // {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
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

            {Questions.LastName,                       LastName.Lapran},
            {Questions.Education,                      Education.College_2},
            {Questions.Height,                         Height._5_11},
            {Questions.Weight,                         Weight._132},
            {Questions.Occupation,                     Occupation.YogaTeacher},
            {Questions.CupSize,                        CupSize.D_Cup},
            {Questions.Birthday,                       Birthday.June_25},
            {Questions.Hobby,                          FavHobby.Meditation},
            {Questions.FavColour,                      FavColour.Purple},
            {Questions.FavSeason,                      FavSeason.Autumn},
            {Questions.FavHangout,                     FavHangout.Park},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

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
