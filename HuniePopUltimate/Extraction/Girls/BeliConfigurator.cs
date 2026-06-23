using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class BeliConfigurator : GirlConfiguratorBase
{
    public override (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => _meetingCutsceneStyleSequence;
    private readonly static (RelativeId outfit, RelativeId hairstyle)[] _meetingCutsceneStyleSequence = [
        (Hp2BaseMod.Styles.Water, Hp2BaseMod.Styles.Water),
        (Hp2BaseMod.Styles.Water, Hp2BaseMod.Styles.Water),
        (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)
    ];

    protected override string UniqueCategoryDescription => "Nature";
    protected override string ShoeCategoryDescription => "Ornate";

    protected override RelativeId GirlId => Girls.Beli;
    protected override string GirlAssetFileName => "beli";

    protected override int Age => 26;

    protected override RelativeId ItemEnergyId => EnergyTypes.Romance;

    protected override (int x, int y) BackPosition => (200, 450);
    protected override (int x, int y) HeadPosition => (245, 800);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.FavSeason,
        Questions.Weight,
        Questions.Age,
        Questions.CupSize,
        Questions.FavHangout,
        Questions.FavColour,
        Questions.Height,
        Questions.Hobby,
        Questions.Occupation,
        Questions.Education,
        Questions.LastName,
        Questions.Birthday,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Lapran),
        (Questions.Education, Education.College_2),
        (Questions.Height, Height._5_11),
        (Questions.Weight, Weight._132),
        (Questions.Occupation, Occupation.YogaTeacher),
        (Questions.CupSize, CupSize.D_Cup),
        (Questions.Birthday, Birthday.June_25),
        (Questions.Hobby, FavHobby.Meditation),
        (Questions.FavColour, FavColour.Purple),
        (Questions.FavSeason, FavSeason.Autumn),
        (Questions.FavHangout, FavHangout.Park),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Beli.Unique1, 9240), // Acorns
        (Items.Beli.Unique2, 9241), // Maple Leaf
        (Items.Beli.Unique3, 9242), // Pinecone
        (Items.Beli.Unique4, 9243), // Mushrooms
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Beli.Shoe1, "Boho Sandals", "Give to a BELI for +1 [[style]@Style] EXP."),
        (Items.Beli.Shoe2, "T-Bar Sandals", "Give to a BELI for +1 [[style]@Style] EXP."),
        (Items.Beli.Shoe3, "Ornate Loafers", "Give to a BELI for +1 [[style]@Style] EXP."),
        (Items.Beli.Shoe4, "Golden Moorish", "Give to a BELI for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Beli.Baggage1, 9244), // Seashell
        //(Items.Beli.Baggage2, 9245), // Four Leaf Clover
        //(Items.Beli.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.HotSprings),//relaxing
        (Locations.Aquarium, LocationIds.HotSprings),//relaxing
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),
        (Locations.PoolsideBar, LocationIds.Carnival),//junk food
        (Locations.GolfCourse, LocationIds.TennisCourts),//if I win
        (Locations.CruiseShip, LocationIds.WaterPark),//bikini
        (Locations.RooftopLounge, LocationIds.Restaurant),
        (Locations.Casino, LocationIds.TennisCourts),//if i win
        (Locations.PrivateTable, LocationIds.Restaurant),//fancy
        (Locations.SecretGrotto, LocationIds.OutdoorLounge),//private oasis
        (Locations.StripClub, LocationIds.Casino),//carried away
        (Locations.RoyalSuite, LocationIds.Restaurant),//fancy
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
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

    public BeliConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);
}