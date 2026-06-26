using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class AikoConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Toys";
    protected override string ShoeCategoryDescription => "Idk";

    protected override RelativeId GirlId => Girls.Aiko;
    protected override string GirlAssetFileName => "aiko";

    protected override int Age => 30;
    protected override bool HasDrinkRejectLines => false;

    protected override RelativeId ItemEnergyId => EnergyTypes.Sexuality;

    public override string UnderwearName => "Naughty Teacher";

    public override (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes
        => (-1, 0, 1);

    protected override (int x, int y) BackPosition => (240, 450);
    protected override (int x, int y) HeadPosition => (300, 800);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.LastName,
        Questions.Height,
        Questions.Birthday,
        Questions.FavColour,
        Questions.Age,
        Questions.Weight,
        Questions.FavSeason,
        Questions.Education,
        Questions.Occupation,
        Questions.CupSize,
        Questions.Hobby,
        Questions.FavHangout,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Yumi),
        (Questions.Education, Education.College_6),
        (Questions.Height, Height._5_4),
        (Questions.Weight, Weight._109),
        (Questions.Occupation, Occupation.Professor),
        (Questions.CupSize, CupSize.D_Cup),
        (Questions.Birthday, Birthday.Nov_9),
        (Questions.Hobby, FavHobby.Gambling),
        (Questions.FavColour, FavColour.Green),
        (Questions.FavSeason, FavSeason.Autumn),
        (Questions.FavHangout, FavHangout.Casino),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        // (Items.Aiko.Unique1, 9204), // Chopsticks
        // (Items.Aiko.Unique2, 9205), // Rice Balls
        // (Items.Aiko.Unique3, 9206), // Bonsai Tree
        // (Items.Aiko.Unique4, 9207), // Wooden Sandals
        (Items.Aiko.Unique1, 9121), // ChessSet
        (Items.Aiko.Unique2, 9117), // Puzzle Cube
        (Items.Aiko.Unique3, 9118), // Sudoku Books
        (Items.Aiko.Unique4, 9119), // Dart Board
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Aiko.Shoe1, "Red Kittens", "Give to an AIKO for +1 [[style]@Style] EXP."),
        (Items.Aiko.Shoe2, "Peeptoe Kittens", "Give to an AIKO for +1 [[style]@Style] EXP."),
        (Items.Aiko.Shoe3, "Clog Sandals", "Give to an AIKO for +1 [[style]@Style] EXP."),
        (Items.Aiko.Shoe4, "Cutout Kittens", "Give to an AIKO for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Aiko.Baggage1, 9208), // Kimono
        //(Items.Aiko.Baggage2, 9209), // Samurai Helmet

        //(Items.Aiko.Baggage1, 9120), // Board Game
        //(Items.Aiko.Baggage2, 9116), // Old Fashioned Yoyo
        //(Items.Aiko.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.BotanicalGarden),
        (Locations.Aquarium, LocationIds.ScenicOverlook),
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),
        (Locations.PoolsideBar, LocationIds.HotSprings),
        (Locations.GolfCourse, LocationIds.BotanicalGarden),
        (Locations.CruiseShip, LocationIds.WaterPark),
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),
        (Locations.Casino, LocationIds.Carnival),
        (Locations.PrivateTable, LocationIds.OutdoorLounge),
        (Locations.SecretGrotto, LocationIds.HotSprings),
        (Locations.StripClub, LocationIds.HikingTrail),
        (Locations.RoyalSuite, LocationIds.Restaurant),
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
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

    public AikoConfigurator(
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