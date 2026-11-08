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
    protected override string ShoeCategoryDescription => "Feline";

    protected override RelativeId GirlId => Girls.Momo;
    protected override string GirlAssetFileName => "momo";

    protected override int Age => 33;

    protected override RelativeId ItemEnergyId => EnergyId.Romance;

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
        (Items.Momo.Baggage1, 9256, "Baggage A", "Placeholder"), // Laser Pointer
        (Items.Momo.Baggage2, 9257, "Baggage B", "Placeholder"), // Scratch Post
        (Items.Momo.Baggage3, 9257, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.OutdoorLounge),//happy to be here
        (LocationId.Aquarium, LocationIds.Carnival),//fish game
        (LocationId.SecludedCabana, LocationIds.OutdoorLounge),//happy to be here
        (LocationId.PoolsideBar, LocationIds.WaterPark),//ear get wet
        (LocationId.GolfCourse, LocationIds.TennisCourts),//chase ball
        (LocationId.CruiseShip, LocationIds.WaterPark),//ear get wet
        (LocationId.RooftopLounge, LocationIds.ScenicOverlook),
        (LocationId.Casino, LocationIds.Carnival),//ask for id
        (LocationId.PrivateTable, LocationIds.Restaurant),//dessert
        (LocationId.SecretGrotto, LocationIds.HotSprings),//lot of water
        (LocationId.StripClub, LocationIds.Casino),//id
        (LocationId.RoyalSuite, LocationIds.OutdoorLounge),//happy to be here
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),

        (LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.StyleId.Sexy)),

        (LocationId.MassageSpa, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationId.Aquarium, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationId.SecludedCabana, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationId.PoolsideBar, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationId.GolfCourse, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationId.CruiseShip, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationId.RooftopLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationId.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationId.PrivateTable, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationId.SecretGrotto, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationId.RoyalSuite, new GirlStyleInfo(Hp2BaseMod.StyleId.Sexy)),
        (LocationId.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationId.OuterSpace, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationId.StripClub, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationId.VolcanoTop, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
    ];

    public MomoConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);

    /// <inheritdoc/>
    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Momo);
    }
}