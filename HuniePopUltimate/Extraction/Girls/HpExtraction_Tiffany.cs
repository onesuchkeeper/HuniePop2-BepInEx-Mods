using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleTiffany(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.BotanicalGarden];//pretty smell nice
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//chill
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//chill
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//water nice
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//chill
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//water nice
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//beautiful from up here
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//nikki would love
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//expensive
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//water nice
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//expensive fast food
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.HikingTrail];//glad asked out

        girlMod.GirlAge = 22;
        SetCellphoneImages(girlMod, "tiffany");
        body.BackPosition = new VectorInfo(230, 450);
        body.HeadPosition = new VectorInfo(290, 800);

        girlMod.FavAnswers = new() {
            // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
            // {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},//aud
            // {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},//aud
            // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
            // {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
            // {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},//aud
            // {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
            // {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
            // {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
            // {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
            // {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
            // {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
            // {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
            // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
            // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
            // {Hp2BaseMod.Favorites.Shop,             FavShop.Bakery},
            // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
            // {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Hair},//aud
            // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.SixtyNine},//aud
            // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
            
            {Questions.LastName,                       LastName.Maye},
            {Questions.Education,                      Education.InCollege},
            {Questions.Height,                         Height._5_4},
            {Questions.Weight,                         Weight._112},
            {Questions.Occupation,                     Occupation.Student},
            {Questions.CupSize,                        CupSize.C_Cup},
            {Questions.Birthday,                       Birthday.Dec_22},
            {Questions.Hobby,                          FavHobby.Cheerleading},
            {Questions.FavColour,                      FavColour.Pink},
            {Questions.FavSeason,                      FavSeason.Summer},
            {Questions.FavHangout,                     FavHangout.Campus},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
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
