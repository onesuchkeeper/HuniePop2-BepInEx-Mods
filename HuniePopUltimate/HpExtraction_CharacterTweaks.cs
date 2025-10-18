using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void Tweak(GirlDataMod girlMod, int nativeId)
    {
        void SetImages(string name)
        {
            girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, $"ui_girl_portrait_{name}.png")));
            girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, $"{name}_cellphoneHead.png")));
            girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, $"{name}_cellphoneHeadMini.png")));
            m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, $"charm_{name}.png"))).GetSprite());
        }
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        switch (nativeId)
        {
            case 1://tiffany
                SetImages("tiffany");
                body.BackPosition = new VectorInfo(230, 450);
                body.HeadPosition = new VectorInfo(290, 800);

                girlMod.FavAnswers = new(){
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},//aud
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},//aud
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},//aud
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             FavShop.Bakery},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Hair},//aud
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.SixtyNine},//aud
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 2://aiko
                SetImages("aiko");
                body.BackPosition = new VectorInfo(240, 450);
                body.HeadPosition = new VectorInfo(300, 800);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Alcohol},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          FavHoliday.None},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 3://kyanna
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0),
                            new RelativeId(Plugin.ModId, 2),
                            new RelativeId(Plugin.ModId, 3),
                            new RelativeId(Plugin.ModId, 4),
                        };

                SetImages("kyanna");
                body.BackPosition = new VectorInfo(215, 450);
                body.HeadPosition = new VectorInfo(280, 800);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 4://audrey
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0)
                };

                SetImages("audrey");
                body.BackPosition = new VectorInfo(190, 430);
                body.HeadPosition = new VectorInfo(210, 800);

                girlMod.FavAnswers = new(){
                    {Hp2BaseMod.Favorites.Drink, Hp2BaseMod.FavDrink.Alcohol},
                    {Hp2BaseMod.Favorites.IceCream, Hp2BaseMod.FavIceCream.Strawberry},//tiff
                    {Hp2BaseMod.Favorites.MusicGenre, FavMusicGenre.Rave},//tiff
                    {Hp2BaseMod.Favorites.MovieGenre, Hp2BaseMod.FavMovieGenre.Comedy},
                    {Hp2BaseMod.Favorites.OnlineActivity, Hp2BaseMod.FavOnlineActivity.Shopping},
                    {Hp2BaseMod.Favorites.PhoneApp, Hp2BaseMod.FavPhoneApp.SocialMedia},//tiff
                    {Hp2BaseMod.Favorites.Exercise, Hp2BaseMod.FavExercise.None},
                    {Hp2BaseMod.Favorites.OutdoorActivity, Hp2BaseMod.FavOutdoorActivity.None},
                    {Hp2BaseMod.Favorites.ThemeParkRide, FavThemeParkRide.None},
                    {Hp2BaseMod.Favorites.FridayNight, Hp2BaseMod.FavFridayNight.OutDrinking},
                    {Hp2BaseMod.Favorites.SundayMorning, Hp2BaseMod.FavSundayMorning.SleepIn},
                    {Hp2BaseMod.Favorites.Weather, Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday, Hp2BaseMod.FavHoliday.Halloween},
                    {Hp2BaseMod.Favorites.Pet, Hp2BaseMod.FavPet.None},
                    {Hp2BaseMod.Favorites.Subject, Hp2BaseMod.FavSchoolSubject.None},
                    {Hp2BaseMod.Favorites.Shop, Hp2BaseMod.FavShop.Fashion},
                    {Hp2BaseMod.Favorites.Trait, Hp2BaseMod.FavTrait.Humor},
                    {Hp2BaseMod.Favorites.BodyPart, FavOwnBodyPart.Hair},//tiff
                    {Hp2BaseMod.Favorites.SexPos, Hp2BaseMod.FavSexPos.SixtyNine},//tiff
                    {Hp2BaseMod.Favorites.PornCat, FavPornCategory.Parody},
                };

                break;
            case 5://lola
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(230, 780);
                break;
            case 6://nikki
                SetImages("nikki");
                body.BackPosition = new VectorInfo(195, 440);
                body.HeadPosition = new VectorInfo(230, 780);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Vanilla},
                    {Hp2BaseMod.Favorites.MusicGenre,       Hp2BaseMod.FavMusicGenre.Techno},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   FavOnlineActivity.Newgrounds},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 7://jessie
                ((GirlSpecialPartDataMod)body.specialParts[0]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 1)
                        };
                var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)body.hairstyles[3]).FrontHairPart;
                celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
                celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(230, 780);
                break;
            case 8: //beli
                SetImages("beli");
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(245, 800);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 9://kyu
                body.BackPosition = new VectorInfo(225f, 454f);
                body.HeadPosition = new VectorInfo(230, 780);
                girlMod.SpecialCharacter = false;
                body.SpecialEffectName = "FairyWingsKyu";

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            FavDrink.Juice},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.Masturbate},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.AdultShop},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Wings},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Hentai},
                };

                break;
            case 10://momo
                SetImages("momo");
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(240, 780);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         FavIceCream.None},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         FavPhoneApp.Games},
                    {Hp2BaseMod.Favorites.Exercise,         FavExercise.Hunting},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    FavThemeParkRide.None},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.SleepIn},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Fish},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         FavOwnBodyPart.Hair},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 11://celeste
                SetImages("celeste");
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(220, 820);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Milk},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Vanilla},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   FavOnlineActivity.Newgrounds},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         FavExercise.Hunting},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.ColdAndSnowy},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.None},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            FavTrait.Reliable},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
            case 12://venus
                SetImages("venus");
                body.BackPosition = new VectorInfo(230, 450);
                body.HeadPosition = new VectorInfo(270, 810);

                girlMod.FavAnswers = new()
                {
                    {Hp2BaseMod.Favorites.Drink,            Hp2BaseMod.FavDrink.Boba},
                    {Hp2BaseMod.Favorites.IceCream,         Hp2BaseMod.FavIceCream.Strawberry},
                    {Hp2BaseMod.Favorites.MusicGenre,       FavMusicGenre.Rave},
                    {Hp2BaseMod.Favorites.MovieGenre,       Hp2BaseMod.FavMovieGenre.Horror},
                    {Hp2BaseMod.Favorites.OnlineActivity,   Hp2BaseMod.FavOnlineActivity.Videos},
                    {Hp2BaseMod.Favorites.PhoneApp,         Hp2BaseMod.FavPhoneApp.SocialMedia},
                    {Hp2BaseMod.Favorites.Exercise,         Hp2BaseMod.FavExercise.Yoga},
                    {Hp2BaseMod.Favorites.OutdoorActivity,  Hp2BaseMod.FavOutdoorActivity.Hiking},
                    {Hp2BaseMod.Favorites.ThemeParkRide,    Hp2BaseMod.FavThemeParkRide.SwingRide},
                    {Hp2BaseMod.Favorites.FridayNight,      Hp2BaseMod.FavFridayNight.OutDancing},
                    {Hp2BaseMod.Favorites.SundayMorning,    Hp2BaseMod.FavSundayMorning.EatBreakfast},
                    {Hp2BaseMod.Favorites.Weather,          Hp2BaseMod.FavWeather.SunnyAndClear},
                    {Hp2BaseMod.Favorites.Holiday,          Hp2BaseMod.FavHoliday.NewYears},
                    {Hp2BaseMod.Favorites.Pet,              Hp2BaseMod.FavPet.Cat},
                    {Hp2BaseMod.Favorites.Subject,          Hp2BaseMod.FavSchoolSubject.HomeEc},
                    {Hp2BaseMod.Favorites.Shop,             Hp2BaseMod.FavShop.BookStore},
                    {Hp2BaseMod.Favorites.Trait,            Hp2BaseMod.FavTrait.Humor},
                    {Hp2BaseMod.Favorites.BodyPart,         Hp2BaseMod.FavOwnBodyPart.Face},
                    {Hp2BaseMod.Favorites.SexPos,           Hp2BaseMod.FavSexPos.Missionary},
                    {Hp2BaseMod.Favorites.PornCat,          Hp2BaseMod.FavPornCategory.Vanilla},
                };

                break;
        }
    }
}