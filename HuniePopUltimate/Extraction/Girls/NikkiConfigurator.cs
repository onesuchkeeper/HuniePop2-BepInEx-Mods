using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class NikkiConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Gaming";
    protected override string ShoeCategoryDescription => "Low-Key";

    protected override RelativeId GirlId => Girls.Nikki;
    protected override string GirlAssetFileName => "nikki";

    protected override int Age => 20;

    protected override RelativeId ItemEnergyId => EnergyTypes.Talent;

    protected override (int x, int y) BackPosition => (195, 440);
    protected override (int x, int y) HeadPosition => (230, 780);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.Age,
        Questions.FavSeason,
        Questions.Birthday,
        Questions.Height,
        Questions.Education,
        Questions.Weight,
        Questions.Hobby,
        Questions.FavHangout,
        Questions.Occupation,
        Questions.FavColour,
        Questions.CupSize,
        Questions.LastName,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.AnnMarie),
        (Questions.Education, Education.Highschool),
        (Questions.Height, Height._5_0),
        (Questions.Weight, Weight._104),
        (Questions.Occupation, Occupation.Barista),
        (Questions.CupSize, CupSize.C_Cup),
        (Questions.Birthday, Birthday.May_13),
        (Questions.Hobby, FavHobby.Gaming),
        (Questions.FavColour, FavColour.Cream),
        (Questions.FavSeason, FavSeason.Winter),
        (Questions.FavHangout, FavHangout.Home),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Nikki.Unique1, 9228), // Retro Controller
        (Items.Nikki.Unique2, 9229), // Arcade Joystick
        (Items.Nikki.Unique3, 9230), // Zappy Gun
        (Items.Nikki.Unique4, 9231), // Gamer Glove
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Nikki.Shoe1, "Converse", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe2, "Hi-Tops", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe3, "Wolf Slippers", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe4, "Sensible Loafers", "Give to a NIKKI for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Nikki.Baggage1, 9232), // Handheld Game
        //(Items.Nikki.Baggage2, 9233), // Arcade Cabinet
        //(Items.Nikki.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.OutdoorLounge),//not my scene
        (Locations.Aquarium, LocationIds.OutdoorLounge),//not my scene
        (Locations.SecludedCabana, LocationIds.WaterPark),//burn alive
        (Locations.PoolsideBar, LocationIds.WaterPark),//burn alive
        (Locations.GolfCourse, LocationIds.BotanicalGarden),//allergies
        (Locations.CruiseShip, LocationIds.WaterPark),//burn alive
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),//romantic
        (Locations.Casino, LocationIds.Casino),//not an arcade
        (Locations.PrivateTable, LocationIds.Restaurant),//dishes gross
        (Locations.SecretGrotto, LocationIds.HotSprings),//water nice
        (Locations.StripClub, LocationIds.IceRink),//might be fun
        (Locations.RoyalSuite, LocationIds.OutdoorLounge),//not my scene
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),

        (LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)),

        (Locations.MassageSpa, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (Locations.Aquarium, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (Locations.SecludedCabana, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (Locations.PoolsideBar, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (Locations.GolfCourse, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (Locations.CruiseShip, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (Locations.RooftopLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (Locations.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (Locations.PrivateTable, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (Locations.SecretGrotto, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (Locations.RoyalSuite, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)),
        (Locations.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (Locations.OuterSpace, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (Locations.StripClub, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (Locations.VolcanoTop, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
    ];

    public NikkiConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex == 3;
}