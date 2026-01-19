using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void HandleAudrey(GirlDataMod girlMod)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        m_AddGirlSexPhotos?.Invoke(girlMod.Id, [(Photos.Audrey10th, RelativeId.Default)]);

        girlMod.LocationGreetingDialogLines[Locations.MassageSpa] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//get used to this
        girlMod.LocationGreetingDialogLines[Locations.Aquarium] = girlMod.LocationGreetingDialogLines[LocationIds.ScenicOverlook];//gaaaayyy
        girlMod.LocationGreetingDialogLines[Locations.SecludedCabana] = girlMod.LocationGreetingDialogLines[LocationIds.OutdoorLounge];
        girlMod.LocationGreetingDialogLines[Locations.PoolsideBar] = girlMod.LocationGreetingDialogLines[LocationIds.IceRink];//cold
        girlMod.LocationGreetingDialogLines[Locations.GolfCourse] = girlMod.LocationGreetingDialogLines[LocationIds.TennisCourts];//sweaty
        girlMod.LocationGreetingDialogLines[Locations.CruiseShip] = girlMod.LocationGreetingDialogLines[LocationIds.WaterPark];//sunscreen
        girlMod.LocationGreetingDialogLines[Locations.RooftopLounge] = girlMod.LocationGreetingDialogLines[LocationIds.Restaurant];//really impressive
        girlMod.LocationGreetingDialogLines[Locations.Casino] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//smoke in here
        girlMod.LocationGreetingDialogLines[Locations.PrivateTable] = girlMod.LocationGreetingDialogLines[LocationIds.Carnival];//idea of a date
        girlMod.LocationGreetingDialogLines[Locations.SecretGrotto] = girlMod.LocationGreetingDialogLines[LocationIds.HotSprings];//get used to this
        girlMod.LocationGreetingDialogLines[Locations.StripClub] = girlMod.LocationGreetingDialogLines[LocationIds.Casino];//smoke in here
        girlMod.LocationGreetingDialogLines[Locations.RoyalSuite] = girlMod.LocationGreetingDialogLines[LocationIds.BotanicalGarden];//im here what now

        girlMod.GirlAge = 21;
        ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            Hp2BaseMod.Styles.Activity
                };

        SetCellphoneImages(girlMod, "audrey");
        body.BackPosition = new VectorInfo(190, 430);
        body.HeadPosition = new VectorInfo(210, 800);

        girlMod.FavAnswers = new(){
            // {Hp2BaseMod.Favorites.Drink, Hp2BaseMod.FavDrink.Alcohol},
            // {Hp2BaseMod.Favorites.IceCream, Hp2BaseMod.FavIceCream.Strawberry},//tiff
            // {Hp2BaseMod.Favorites.MusicGenre, FavMusicGenre.Rave},//tiff
            // {Hp2BaseMod.Favorites.MovieGenre, Hp2BaseMod.FavMovieGenre.Comedy},
            // {Hp2BaseMod.Favorites.OnlineActivity, Hp2BaseMod.FavOnlineActivity.Shopping},
            // {Hp2BaseMod.Favorites.PhoneApp, Hp2BaseMod.FavPhoneApp.SocialMedia},//tiff
            // {Hp2BaseMod.Favorites.Exercise, Hp2BaseMod.FavExercise.None},
            // {Hp2BaseMod.Favorites.OutdoorActivity, Hp2BaseMod.FavOutdoorActivity.None},
            // {Hp2BaseMod.Favorites.ThemeParkRide, FavThemeParkRide.None},
            // {Hp2BaseMod.Favorites.FridayNight, Hp2BaseMod.FavFridayNight.OutDrinking},
            // {Hp2BaseMod.Favorites.SundayMorning, Hp2BaseMod.FavSundayMorning.SleepIn},
            // {Hp2BaseMod.Favorites.Weather, Hp2BaseMod.FavWeather.SunnyAndClear},
            // {Hp2BaseMod.Favorites.Holiday, Hp2BaseMod.FavHoliday.Halloween},
            // {Hp2BaseMod.Favorites.Pet, Hp2BaseMod.FavPet.None},
            // {Hp2BaseMod.Favorites.Subject, Hp2BaseMod.FavSchoolSubject.None},
            // {Hp2BaseMod.Favorites.Shop, Hp2BaseMod.FavShop.Fashion},
            // {Hp2BaseMod.Favorites.Trait, Hp2BaseMod.FavTrait.Humor},
            // {Hp2BaseMod.Favorites.BodyPart, FavOwnBodyPart.Hair},//tiff
            // {Hp2BaseMod.Favorites.SexPos, Hp2BaseMod.FavSexPos.SixtyNine},//tiff
            // {Hp2BaseMod.Favorites.PornCat, FavPornCategory.Parody},

            {Questions.LastName,                       LastName.Belrose},
            {Questions.Education,                      Education.InCollege},
            {Questions.Height,                         Height._5_2},
            {Questions.Weight,                         Weight._102},
            {Questions.Occupation,                     Occupation.Student},
            {Questions.CupSize,                        CupSize.B_Cup},
            {Questions.Birthday,                       Birthday.Apr_6},
            {Questions.Hobby,                          FavHobby.Shopping},
            {Questions.FavColour,                      FavColour.Red},
            {Questions.FavSeason,                      FavSeason.Spring},
            {Questions.FavHangout,                     FavHangout.Nightclub},
        };

        body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
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

        //Drinks during day so doesn't have a rejection line
        var rejectSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieReject];
        var acceptSmoothie = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieAccept];
        var rejectFood = girlMod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }
}
