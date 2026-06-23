using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class CelesteConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Outer Space";
    protected override string ShoeCategoryDescription => "Space Gear";

    protected override RelativeId GirlId => Girls.Celeste;
    protected override string GirlAssetFileName => "celeste";

    protected override int Age => 34;

    public override string UnderwearName => "Undergarments";

    protected override RelativeId ItemEnergyId => EnergyTypes.Talent;

    protected override (int x, int y) BackPosition => (200, 450);
    protected override (int x, int y) HeadPosition => (220, 820);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.Hobby,
        Questions.FavHangout,
        Questions.Birthday,
        Questions.FavSeason,
        Questions.CupSize,
        Questions.Height,
        Questions.Occupation,
        Questions.Weight,
        Questions.FavColour,
        Questions.HomeWorld,
        Questions.LastName,
        Questions.Birthday,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Luvendass),
        (Questions.HomeWorld, Homeworld.Tendricide),
        (Questions.Height, Height._5_9),
        (Questions.Weight, Weight._130),
        (Questions.Occupation, Occupation.Hunter),
        (Questions.CupSize, CupSize.DD_Cup),
        (Questions.Birthday, Birthday.July_20),
        (Questions.Hobby, FavHobby.Swimming),
        (Questions.FavColour, FavColour.Silver),
        (Questions.FavSeason, FavSeason.Winter),
        (Questions.FavHangout, FavHangout.Beach),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Celeste.Unique1, 9258), // Model Rocket
        (Items.Celeste.Unique2, 9259), // Mini UFO
        (Items.Celeste.Unique3, 9260), // Armillary Sphere
        (Items.Celeste.Unique4, 9261), // Telescope
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Celeste.Shoe1, "Mission Boots", "Advanced issue footwear. Give to an alien for +1 [[style]@Style] EXP."),
        (Items.Celeste.Shoe2, "Silver Sandals", "Sand permissible footwear. Give to an alien for +1 [[style]@Style] EXP."),
        (Items.Celeste.Shoe3, "Formatic Platforms", "High skill footwear. Give to an alien for +1 [[style]@Style] EXP."),
        (Items.Celeste.Shoe4, "Hyper Flats", "Avant footwear. Give to an alien for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Celeste.Baggage1, 9262), // Space Helmet
        //(Items.Celeste.Baggage2, 9263), // Moonrock
        //(Items.Celeste.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.HotSprings),//feels amazing
        (Locations.Aquarium, LocationIds.ScenicOverlook),//beautiful planet
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),
        (Locations.PoolsideBar, LocationIds.Restaurant),//too many peeps
        (Locations.GolfCourse, LocationIds.ScenicOverlook),//beautiful
        (Locations.CruiseShip, LocationIds.Restaurant),//too many peeps
        (Locations.RooftopLounge, LocationIds.OutdoorLounge),//thanks for invite
        (Locations.Casino, LocationIds.Carnival),//colors and lights
        (Locations.PrivateTable, LocationIds.OutdoorLounge),//thanks for invite
        (Locations.SecretGrotto, LocationIds.HotSprings),//feel amazing
        (Locations.StripClub, LocationIds.Carnival),//colors and lights
        (Locations.RoyalSuite, LocationIds.OutdoorLounge),//thanks for invite
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
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

    public CelesteConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);
}