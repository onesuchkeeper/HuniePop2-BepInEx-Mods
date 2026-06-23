using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class MomoConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Cat Toys";
    protected override string ShoeCategoryDescription => "Idk";

    protected override RelativeId GirlId => Girls.Momo;
    protected override string GirlAssetFileName => "momo";

    protected override int Age => 33;

    protected override RelativeId ItemEnergyId => EnergyTypes.Romance;

    public override string UnderwearName => "Satin Strands";

    protected override (int x, int y) BackPosition => (200, 450);
    protected override (int x, int y) HeadPosition => (240, 780);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.HomeWorld,
        Questions.CupSize,
        Questions.FavColour,
        Questions.Occupation,
        Questions.Hobby,
        Questions.Weight,
        Questions.Height,
        Questions.Birthday,
        Questions.FavHangout,
        Questions.LastName,
        Questions.FavSeason,
        Questions.Age,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.NotApplicable),
        (Questions.HomeWorld, Homeworld.Unsure),
        (Questions.Height, Height._5_2),
        (Questions.Weight, Weight._100),
        (Questions.Occupation, Occupation.Kitty),
        (Questions.CupSize, CupSize.B_Cup),
        (Questions.Birthday, Birthday.Oct_2),
        (Questions.Hobby, FavHobby.Sleeping),
        (Questions.FavColour, FavColour.Gold),
        (Questions.FavSeason, FavSeason.Spring),
        (Questions.FavHangout, FavHangout.HikingTrail),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Momo.Unique1, 9252), // Ball of Yarn
        (Items.Momo.Unique2, 9253), // Lattice Ball
        (Items.Momo.Unique3, 9254), // Squeaky Mouse
        (Items.Momo.Unique4, 9255), // Feather Pole
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Momo.Shoe1, "Flip Flops", "Give to a MOMO for +1 [[style]@Style] EXP."),
        (Items.Momo.Shoe2, "Satin Peep Toes", "Give to a MOMO for +1 [[style]@Style] EXP."),
        (Items.Momo.Shoe3, "Geta", "Traditional footwear for those with the dexterity of a cat. +1 [[style]@Style] EXP."),
        (Items.Momo.Shoe4, "Sack Boots", "Give to a MOMO for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Momo.Baggage1, 9256), // Laser Pointer (Distracted, 50% chance for repeated moves to fail)
        //(Items.Momo.Baggage2, 9257), // Scratch Post
        //(Items.Momo.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.OutdoorLounge),//happy to be here
        (Locations.Aquarium, LocationIds.Carnival),//fish game
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),//happy to be here
        (Locations.PoolsideBar, LocationIds.WaterPark),//ear get wet
        (Locations.GolfCourse, LocationIds.TennisCourts),//chase ball
        (Locations.CruiseShip, LocationIds.WaterPark),//ear get wet
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),
        (Locations.Casino, LocationIds.Carnival),//ask for id
        (Locations.PrivateTable, LocationIds.Restaurant),//dessert
        (Locations.SecretGrotto, LocationIds.HotSprings),//lot of water
        (Locations.StripClub, LocationIds.Casino),//id
        (Locations.RoyalSuite, LocationIds.OutdoorLounge),//happy to be here
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
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

    public MomoConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);
}