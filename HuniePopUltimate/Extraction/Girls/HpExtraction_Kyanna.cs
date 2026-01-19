using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleKyanna(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//don't get to go out
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.BotanicalGarden];//neat ideA
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//bikini
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts];//emasculeted
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//bikini
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//gamble
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//nicer than food court
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//bikini
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//uhh nicer that lunch?
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];//seduced

        girlMod.GirlAge = 23;
        ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
            Hp2BaseMod.Styles.Activity,
            Hp2BaseMod.Styles.Romantic,
            Hp2BaseMod.Styles.Party,
            Hp2BaseMod.Styles.Water,
        };

        SetCellphoneImages(girlMod, "kyanna");
        body.BackPosition = new VectorInfo(215, 450);
        body.HeadPosition = new VectorInfo(280, 800);

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

            {Questions.LastName,                       LastName.Delrio},
            {Questions.Education,                      Education.Dropout},
            {Questions.Height,                         Height._5_6},
            {Questions.Weight,                         Weight._118},
            {Questions.Occupation,                     Occupation.Hairdresser},
            {Questions.CupSize,                        CupSize.DD_Cup},
            {Questions.Birthday,                       Birthday.Mar_16},
            {Questions.Hobby,                          FavHobby.WorkingOut},
            {Questions.FavColour,                      FavColour.Blue},
            {Questions.FavSeason,                      FavSeason.Summer},
            {Questions.FavHangout,                     FavHangout.Gym},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
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
