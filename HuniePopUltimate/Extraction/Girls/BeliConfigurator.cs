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
        (Hp2BaseMod.StyleId.Water, Hp2BaseMod.StyleId.Water),
        (Hp2BaseMod.StyleId.Water, Hp2BaseMod.StyleId.Water),
        (Hp2BaseMod.StyleId.Activity, Hp2BaseMod.StyleId.Activity)
    ];

    protected override string UniqueCategoryDescription => "Nature";
    protected override string ShoeCategoryDescription => "Ornate";

    protected override RelativeId GirlId => Girls.Beli;
    protected override string GirlAssetFileName => "beli";

    protected override int Age => 26;

    protected override RelativeId ItemEnergyId => EnergyId.Romance;

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
        (Items.Beli.Baggage1, 9244, "Baggage A", "Placeholder"), // Seashell
        (Items.Beli.Baggage2, 9245, "Baggage B", "Placeholder"), // Four Leaf Clover
        (Items.Beli.Baggage3, 9245, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.HotSprings),//relaxing
        (LocationId.Aquarium, LocationIds.HotSprings),//relaxing
        (LocationId.SecludedCabana, LocationIds.OutdoorLounge),
        (LocationId.PoolsideBar, LocationIds.Carnival),//junk food
        (LocationId.GolfCourse, LocationIds.TennisCourts),//if I win
        (LocationId.CruiseShip, LocationIds.WaterPark),//bikini
        (LocationId.RooftopLounge, LocationIds.Restaurant),
        (LocationId.Casino, LocationIds.TennisCourts),//if i win
        (LocationId.PrivateTable, LocationIds.Restaurant),//fancy
        (LocationId.SecretGrotto, LocationIds.OutdoorLounge),//private oasis
        (LocationId.StripClub, LocationIds.Casino),//carried away
        (LocationId.RoyalSuite, LocationIds.Restaurant),//fancy
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
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

    public BeliConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);

    /// <inheritdoc/>
    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Beli);
    }
}