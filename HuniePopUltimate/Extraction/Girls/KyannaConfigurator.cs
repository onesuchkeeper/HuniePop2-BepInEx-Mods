using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class KyannaConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Fitness";
    protected override string ShoeCategoryDescription => "Strapped";

    protected override RelativeId GirlId => Girls.Kyanna;
    protected override string GirlAssetFileName => "kyanna";

    public override string UnderwearName => "Ebony";

    protected override int Age => 23;

    public override (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes
    => (-1, 0, 1);

    protected override RelativeId ItemEnergyId => EnergyId.Talent;

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
        (Items.Kyanna.Unique3, 9124), // Skipping Rope
        (Items.Kyanna.Unique4, 9125), // Kettle Bell
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
        (Items.Kyanna.Baggage1, 9216, "Baggage A", "Placeholder"), // Boxing Gloves
        (Items.Kyanna.Baggage2, 9217, "Baggage B", "Placeholder"), // Punching Bag
        (Items.Kyanna.Baggage3, 9217, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.Carnival),//don't get to go out
        (LocationId.Aquarium, LocationIds.BotanicalGarden),//neat idea
        (LocationId.SecludedCabana, LocationIds.OutdoorLounge),//seduced
        (LocationId.PoolsideBar, LocationIds.HotSprings),//bikini
        (LocationId.GolfCourse, LocationIds.TennisCourts),//emasculated
        (LocationId.CruiseShip, LocationIds.HotSprings),//bikini
        (LocationId.RooftopLounge, LocationIds.ScenicOverlook),
        (LocationId.Casino, LocationIds.Casino),//gamble
        (LocationId.PrivateTable, LocationIds.Restaurant),//nicer than food court
        (LocationId.SecretGrotto, LocationIds.HotSprings),//bikini
        (LocationId.StripClub, LocationIds.Restaurant),//nicer than food court
        (LocationId.RoyalSuite, LocationIds.OutdoorLounge),//seduced
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
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

    public KyannaConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex == 2;

    public override bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId)
    {
        if (dialogTriggerId == DialogTriggers.LovesAccept)
        {
            cleanedDialogTriggerId = Hp2BaseMod.DialogTriggerId.UniqueAccept;
            return true;
        }

        if (dialogTriggerId == Hp2BaseMod.DialogTriggerId.UniqueAccept)
        {
            cleanedDialogTriggerId = default;
            return false;
        }

        cleanedDialogTriggerId = dialogTriggerId;
        return true;
    }

    /// <inheritdoc/>
    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Kyanna);
    }
}