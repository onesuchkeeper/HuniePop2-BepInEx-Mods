using System;
using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public class AudreyConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Narcotics";
    protected override string ShoeCategoryDescription => "Mary Janes";

    protected override RelativeId GirlId => Girls.Audrey;
    protected override string GirlAssetFileName => "audrey";

    protected override int Age => 21;
    protected override bool HasDrinkRejectLines => false;

    protected override RelativeId ItemEnergyId => EnergyId.Sexuality;

    public override string UnderwearName => "Pure Sin";

    public override (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes
        => (-1, 0, 1);

    protected override (int x, int y) BackPosition => (190, 430);
    protected override (int x, int y) HeadPosition => (210, 800);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.LastName,
        Questions.Education,
        Questions.Weight,
        Questions.CupSize,
        Questions.FavColour,
        Questions.FavHangout,
        Questions.Age,
        Questions.Height,
        Questions.Occupation,
        Questions.Birthday,
        Questions.Hobby,
        Questions.FavSeason,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Belrose),
        (Questions.Education, Education.InCollege),
        (Questions.Height, Height._5_2),
        (Questions.Weight, Weight._102),
        (Questions.Occupation, Occupation.Student),
        (Questions.CupSize, CupSize.B_Cup),
        (Questions.Birthday, Birthday.Apr_6),
        (Questions.Hobby, FavHobby.Shopping),
        (Questions.FavColour, FavColour.Red),
        (Questions.FavSeason, FavSeason.Spring),
        (Questions.FavHangout, FavHangout.Nightclub),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Audrey.Unique1, 9216),// Cigarette Pack
        (Items.Audrey.Unique2, 9217),// Lighter
        (Items.Audrey.Unique3, 9218),// Glass Pipe
        (Items.Audrey.Unique4, 9219),// Glass Bong
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Audrey.Shoe1, "Classic MJ's", "Stuck up shoes for a stuck up bitch. Give to a bitch to gain +1 [[style]@Bitch] EXP. Bitch."),
        (Items.Audrey.Shoe2, "Ribbon MJ's", "Stuck up shoes for a stuck up bitch. Give to a bitch to gain +1 [[style]@Bitch] EXP. Bitch."),
        (Items.Audrey.Shoe3, "Bowed MJ's", "Stuck up shoes for a stuck up bitch. Give to a bitch to gain +1 [[style]@Bitch] EXP. Bitch."),
        (Items.Audrey.Shoe4, "Lacey MJ's", "Stuck up shoes for a stuck up bitch. Give to a bitch to gain +1 [[style]@Bitch] EXP. Bitch."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        (Items.Audrey.Baggage1, 9220, "Baggage A", "Placeholder"), // Blotter Tabs
        (Items.Audrey.Baggage2, 9221, "Baggage B", "Placeholder"), // Happy Pills
        (Items.Audrey.Baggage3, 9221, "Baggage C", "Placeholder"), // Glow Sticks
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.HotSprings),// get used to this
        (LocationId.Aquarium, LocationIds.ScenicOverlook),// gaaaayyy
        (LocationId.SecludedCabana, LocationIds.OutdoorLounge),
        (LocationId.PoolsideBar, LocationIds.IceRink),//cold
        (LocationId.GolfCourse, LocationIds.TennisCourts),//sweaty
        (LocationId.CruiseShip, LocationIds.WaterPark),//sunscreen
        (LocationId.RooftopLounge, LocationIds.Restaurant),//really impressive
        (LocationId.Casino, LocationIds.Casino),//smoke in here
        (LocationId.PrivateTable, LocationIds.Carnival),//idea of a date
        (LocationId.SecretGrotto, LocationIds.HotSprings),//get used to this
        (LocationId.StripClub, LocationIds.Casino),//smoke in here
        (LocationId.RoyalSuite, LocationIds.BotanicalGarden),//im here what now
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
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

    public AudreyConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {

    }

    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Audrey);

        AddSexPhotos([(Photos.Audrey10th, RelativeId.Default)]);

        var hairThingy = (GirlSpecialPartDataMod)hpBody.specialParts[1];
        hairThingy.RequiredHairstyles = new List<RelativeId>()
        {
            Hp2BaseMod.StyleId.Activity
        };

        hairThingy.RequiredOutfits = new List<RelativeId>()
        {
            Hp2BaseMod.StyleId.Activity,
        };

        hpBody.DefaultHairstyleId = Hp2BaseMod.StyleId.Activity;
        hpBody.DefaultOutfitId = Hp2BaseMod.StyleId.Activity;
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => false;
}