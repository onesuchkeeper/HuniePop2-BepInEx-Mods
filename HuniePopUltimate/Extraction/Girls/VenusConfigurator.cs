using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class VenusConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Gemstones";
    protected override string ShoeCategoryDescription => "Exalted";

    protected override RelativeId GirlId => Girls.Venus;
    protected override string GirlAssetFileName => "venus";

    protected override int Age => 10000;

    protected override RelativeId ItemEnergyId => EnergyId.Sexuality;

    public override string UnderwearName => "Divine Blessing";

    protected override (int x, int y) BackPosition => (230, 450);
    protected override (int x, int y) HeadPosition => (270, 810);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.LastName,
        Questions.Height,
        Questions.FavSeason,
        Questions.Occupation,
        Questions.FavHangout,
        Questions.Weight,
        Questions.HomeWorld,
        Questions.Hobby,
        Questions.CupSize,
        Questions.Age,
        Questions.Birthday,
        Questions.FavColour,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Venus),
        (Questions.HomeWorld, Homeworld.SkyGarden),
        (Questions.Height, Height._5_8),
        (Questions.Weight, Weight._128),
        (Questions.Occupation, Occupation.Goddess),
        (Questions.CupSize, CupSize.DD_Cup),
        (Questions.Birthday, Birthday.Sep_1),
        (Questions.Hobby, FavHobby.Relaxing),
        (Questions.FavColour, FavColour.White),
        (Questions.FavSeason, FavSeason.Autumn),
        (Questions.FavHangout, FavHangout.HotSpring),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Venus.Unique1, 9264), // Sapphire
        (Items.Venus.Unique2, 9265), // Ruby
        (Items.Venus.Unique3, 9266), // Emerald
        (Items.Venus.Unique4, 9267), // Topaz
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Venus.Shoe1, "Lazuli MJ's", "Give to a VENUS for +1 [[style]@Style] EXP."),
        (Items.Venus.Shoe2, "Divine Gladiators", "Give to a VENUS for +1 [[style]@Style] EXP."),
        (Items.Venus.Shoe3, "Immaculate Chelseas", "Give to a VENUS for +1 [[style]@Style] EXP."),
        (Items.Venus.Shoe4, "Golden Sandals", "Give to a VENUS for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        (Items.Venus.Baggage1, 9268, "Baggage A", "Placeholder"), // Amethyst
        (Items.Venus.Baggage2, 9269, "Baggage B", "Placeholder"), // Diamond
        (Items.Venus.Baggage3, 9269, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.HotSprings),//soothing
        (LocationId.Aquarium, LocationIds.BotanicalGarden),//pretty
        (LocationId.SecludedCabana, LocationIds.OutdoorLounge),//do your worst
        (LocationId.PoolsideBar, LocationIds.HikingTrail),//watch close
        (LocationId.GolfCourse, LocationIds.TennisCourts),//not what I would have selected
        (LocationId.CruiseShip, LocationIds.WaterPark),//fun
        (LocationId.RooftopLounge, LocationIds.OutdoorLounge),//do your worst
        (LocationId.Casino, LocationIds.Casino),//luck
        (LocationId.PrivateTable, LocationIds.FarmersMarket),//woo me
        (LocationId.SecretGrotto, LocationIds.HotSprings),//soothing
        (LocationId.StripClub, LocationIds.ScenicOverlook),//prepared for me
        (LocationId.RoyalSuite, LocationIds.Restaurant),//capable of
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

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

    public VenusConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex == 2;

    /// <inheritdoc/>
    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Venus);
    }
}