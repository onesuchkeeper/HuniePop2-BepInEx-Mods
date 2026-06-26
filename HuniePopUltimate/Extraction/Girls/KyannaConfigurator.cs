using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class KyannaConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Fitness";
    protected override string ShoeCategoryDescription => "Idk";

    protected override RelativeId GirlId => Girls.Kyanna;
    protected override string GirlAssetFileName => "kyanna";

    protected override int Age => 23;

    public override (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes
    => (-1, 0, 1);

    protected override RelativeId ItemEnergyId => EnergyTypes.Talent;

    protected override (int x, int y) BackPosition => (215, 450);
    protected override (int x, int y) HeadPosition => (280, 800);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.LastName,
        Questions.Age,
        Questions.Education,
        Questions.Weight,
        Questions.CupSize,
        Questions.Occupation,
        Questions.Hobby,
        Questions.FavColour,
        Questions.FavSeason,
        Questions.FavHangout,
        Questions.Height,
        Questions.Birthday,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Delrio),
        (Questions.Education, Education.Dropout),
        (Questions.Height, Height._5_6),
        (Questions.Weight, Weight._118),
        (Questions.Occupation, Occupation.Hairdresser),
        (Questions.CupSize, CupSize.DD_Cup),
        (Questions.Birthday, Birthday.Mar_16),
        (Questions.Hobby, FavHobby.WorkingOut),
        (Questions.FavColour, FavColour.Blue),
        (Questions.FavSeason, FavSeason.Summer),
        (Questions.FavHangout, FavHangout.Gym),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        // (Items.Kyanna.Unique1, 9210), // Maracas
        // (Items.Kyanna.Unique2, 9211), // Sombrero
        // (Items.Kyanna.Unique3, 9212), // Poncho
        // (Items.Kyanna.Unique4, 9213), // Luchador
        (Items.Kyanna.Unique1, 9122), // Water Bottle
        (Items.Kyanna.Unique2, 9123), // Cardio Weights
        (Items.Kyanna.Unique3, 9214), // Skipping Rope
        (Items.Kyanna.Unique4, 9215), // Kettle Bell
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Kyanna.Shoe1, "Suede Sandals", "Give to a mom for +1 [[style]@Style] EXP."),
        (Items.Kyanna.Shoe2, "Strappy Sandals", "Give to a mom for +1 [[style]@Style] EXP."),
        (Items.Kyanna.Shoe3, "Textile Sandals", "Give to a mom for +1 [[style]@Style] EXP."),
        (Items.Kyanna.Shoe4, "Bootie Pumps", "Give to a mom for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Kyanna.Baggage1, 9214), // Pinata
        //(Items.Kyanna.Baggage2, 9215), // Vinuela

        //(Items.Kyanna.Baggage1, 9216), // Boxing Gloves
        //(Items.Kyanna.Baggage2, 9217), // Punching Bag
        //(Items.Kyanna.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.Carnival),//don't get to go out
        (Locations.Aquarium, LocationIds.BotanicalGarden),//neat idea
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),
        (Locations.PoolsideBar, LocationIds.HotSprings),//bikini
        (Locations.GolfCourse, LocationIds.TennisCourts),//emasculated
        (Locations.CruiseShip, LocationIds.HotSprings),//bikini
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),
        (Locations.Casino, LocationIds.Casino),//gamble
        (Locations.PrivateTable, LocationIds.Restaurant),//nicer than food court
        (Locations.SecretGrotto, LocationIds.HotSprings),//bikini
        (Locations.StripClub, LocationIds.Restaurant),//nicer than food court?
        (Locations.RoyalSuite, LocationIds.OutdoorLounge),//seduced
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

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

    public KyannaConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex == 2;

    public override bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId)
    {
        if (dialogTriggerId == DialogTriggers.LovesAccept)
        {
            cleanedDialogTriggerId = Hp2BaseMod.DialogTriggers.UniqueAccept;
            return true;
        }

        if (dialogTriggerId == Hp2BaseMod.DialogTriggers.UniqueAccept)
        {
            cleanedDialogTriggerId = default;
            return false;
        }

        cleanedDialogTriggerId = dialogTriggerId;
        return true;
    }
}