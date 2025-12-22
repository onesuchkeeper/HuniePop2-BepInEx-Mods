using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void Tweak(GirlDataMod girlMod, int nativeId)
    {
        var emptySpriteInfo = new SpriteInfoInternal("EmptySprite");

        void SetImages(string name)
        {
            girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, $"ui_girl_portrait_{name}.png"), true));
            girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, $"{name}_cellphoneHead.png"), true));
            girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, $"{name}_cellphoneHeadMini.png"), true));
            m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, $"charm_{name}.png"), true)).GetSprite());
        }
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        switch (nativeId)
        {
            case 1://tiffany
                girlMod.GirlAge = 22;
                SetImages("tiffany");
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

                break;
            case 2://aiko
                girlMod.GirlAge = 30;
                SetImages("aiko");
                body.BackPosition = new VectorInfo(240, 450);
                body.HeadPosition = new VectorInfo(300, 800);

                girlMod.FavAnswers = new()
                {
                    // {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Alcohol},
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
                    // {Hp2BaseMod.Favorites.Holiday,          FavHoliday.None},
                    // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    // {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    // {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},

                    {Questions.LastName,                       LastName.Yumi},
                    {Questions.Education,                      Education.College_6},
                    {Questions.Height,                         Height._5_4},
                    {Questions.Weight,                         Weight._109},
                    {Questions.Occupation,                     Occupation.Professor},
                    {Questions.CupSize,                        CupSize.D_Cup},
                    {Questions.Birthday,                       Birthday.Nov_9},
                    {Questions.Hobby,                          FavHobby.Gambling},
                    {Questions.FavColour,                      FavColour.Green},
                    {Questions.FavSeason,                      FavSeason.Autumn},
                    {Questions.FavHangout,                     FavHangout.Casino},
                };

                body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
                    {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
                    {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

                    {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
                    {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
                    {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

                    {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                    {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

                    {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
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

                break;
            case 3://kyanna
                girlMod.GirlAge = 23;
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                    Hp2BaseMod.Styles.Activity,
                    Hp2BaseMod.Styles.Romantic,
                    Hp2BaseMod.Styles.Party,
                    Hp2BaseMod.Styles.Water,
                };

                SetImages("kyanna");
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

                break;
            case 4://audrey
                girlMod.GirlAge = 21;
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            Hp2BaseMod.Styles.Activity
                };

                SetImages("audrey");
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

                break;
            case 5://lola
                girlMod.FavAnswers = new(){
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
                break;
            case 6://nikki
                girlMod.GirlAge = 20;
                SetImages("nikki");
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

                break;
            case 7://jessie
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
                break;
            case 8: //beli
                girlMod.GirlAge = 26;
                SetImages("beli");
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
                break;
            case 9://kyu
                girlMod.GirlAge = 386;
                body.BackPosition = new VectorInfo(245f, 454f);
                body.HeadPosition = new VectorInfo(230, 780);
                girlMod.SpecialCharacter = false;
                body.SpecialEffect = SpecialParts.KyuWingId;

                girlMod.FavAnswers = new()
                {
                    // {Hp2BaseMod.Favorites.Drink,            FavDrink.Juice},
                    // {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    // {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    // {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    // {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    // {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    // {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    // {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    // {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    // {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    // {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.Masturbate},
                    // {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    // {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    // {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    // {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
                    // {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.AdultShop},
                    // {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    // {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Wings},
                    // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Hentai},

                    {Questions.LastName,                       LastName.Sugardust},
                    {Questions.HomeWorld,                      Homeworld.SkyGarden},
                    {Questions.Height,                         Height._5_4},
                    {Questions.Weight,                         Weight._110},
                    {Questions.Occupation,                     Occupation.LoveFairy},
                    {Questions.CupSize,                        CupSize.C_Cup},
                    {Questions.Birthday,                       Birthday.Aug_3},
                    {Questions.Hobby,                          FavHobby.Porn},
                    {Questions.FavColour,                      FavColour.Pink},
                    {Questions.FavSeason,                      FavSeason.Summer},
                    {Questions.FavHangout,                     FavHangout.Bedroom},
                };

                body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                    {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

                    {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
                    {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

                    {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
                    {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
                    {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

                    {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
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

                break;
            case 10://momo
                girlMod.GirlAge = 3;
                SetImages("momo");
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

                break;
            case 11://celeste
                girlMod.GirlAge = 34;
                SetImages("celeste");
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

                break;
            case 12://venus
                girlMod.GirlAge = 10000;
                SetImages("venus");
                body.BackPosition = new VectorInfo(230, 450);
                body.HeadPosition = new VectorInfo(270, 810);

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
                    // {Hp2BaseMod.Favorites.Trait,            Hp2BaseMod.FavTrait.Humor},
                    // {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    // {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    // {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},

                    {Questions.LastName,                       LastName.Venus},
                    {Questions.HomeWorld,                      Homeworld.SkyGarden},
                    {Questions.Height,                         Height._5_8},
                    {Questions.Weight,                         Weight._128},
                    {Questions.Occupation,                     Occupation.Goddess},
                    {Questions.CupSize,                        CupSize.DD_Cup},
                    {Questions.Birthday,                       Birthday.Sep_1},
                    {Questions.Hobby,                          FavHobby.Relaxing},
                    {Questions.FavColour,                      FavColour.White},
                    {Questions.FavSeason,                      FavSeason.Autumn},
                    {Questions.FavHangout,                     FavHangout.HotSpring},
                };

                body.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                    {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
                    {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

                    {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
                    {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                    {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

                    {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
                    {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

                    {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
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

                break;
        }

        // if (nativeId == 12 || nativeId == 11 || nativeId == 10 || nativeId == 9)
        // {
        //     foreach (var herQuestion in girlMod.HerQuestions.Values.OfType<HerQuestionInfo>())
        //     {
        //         using (ModInterface.Log.MakeIndent(herQuestion.DialogLine.DialogText))
        //         {
        //             ModInterface.Log.Error(herQuestion.CorrectAnswer.text);
        //             foreach (var answer in herQuestion.IncorrectAnswers.Values.OfType<HerQuestionAnswerInfo>())
        //             {
        //                 ModInterface.Log.Error(answer.text);
        //                 if (answer.Response != null) ModInterface.Log.Error("\t" + answer.Response?.DialogText);
        //                 else ModInterface.Log.Error("\trandom");
        //             }
        //         }
        //     }
        // }
    }
}
