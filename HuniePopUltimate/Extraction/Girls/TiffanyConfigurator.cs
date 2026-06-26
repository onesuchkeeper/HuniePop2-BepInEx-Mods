using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class TiffanyConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Cheerleader";
    protected override string ShoeCategoryDescription => "Cute";

    protected override RelativeId GirlId => Girls.Tiffany;
    protected override string GirlAssetFileName => "tiffany";

    protected override int Age => 22;

    protected override RelativeId ItemEnergyId => EnergyTypes.Flirtation;

    protected override (int x, int y) BackPosition => (230, 450);
    protected override (int x, int y) HeadPosition => (290, 800);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.LastName,
        Questions.Education,
        Questions.Birthday,
        Questions.FavHangout,
        Questions.Education,
        Questions.Weight,
        Questions.CupSize,
        Questions.FavSeason,
        Questions.Age,
        Questions.Height,
        Questions.Hobby,
        Questions.FavColour,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Maye),
        (Questions.Education, Education.InCollege),
        (Questions.Height, Height._5_4),
        (Questions.Weight, Weight._112),
        (Questions.Occupation, Occupation.Student),
        (Questions.CupSize, CupSize.C_Cup),
        (Questions.Birthday, Birthday.Dec_22),
        (Questions.Hobby, FavHobby.Cheerleading),
        (Questions.FavColour, FavColour.Pink),
        (Questions.FavSeason, FavSeason.Summer),
        (Questions.FavHangout, FavHangout.Campus),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Tiffany.Unique1, 9198), // Double Hair Bow
        (Items.Tiffany.Unique2, 9199), // Glitter Bottles
        (Items.Tiffany.Unique3, 9200), // Twirly Baton
        (Items.Tiffany.Unique4, 9201), // Megaphone
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Tiffany.Shoe1, "Leather Loafers", "Give to a TIFFANY for +1 [[style]@Style] EXP."),
        (Items.Tiffany.Shoe2, "Ring Sandals", "Give to a TIFFANY for +1 [[style]@Style] EXP."),
        (Items.Tiffany.Shoe3, "Doll MJ's", "Give to a TIFFANY for +1 [[style]@Style] EXP."),
        (Items.Tiffany.Shoe4, "Shearling Boots", "Give to a TIFFANY for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Tiffany.Baggage1, 9202), // Pom-poms
        //(Items.Tiffany.Baggage2, 9203), // Cheerleading Uniform
        //(Items.Tiffany.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.BotanicalGarden),//pretty smells nice
        (Locations.Aquarium, LocationIds.OutdoorLounge),//chill
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),//chill
        (Locations.PoolsideBar, LocationIds.HotSprings),//water nice
        (Locations.GolfCourse, LocationIds.OutdoorLounge),//chill
        (Locations.CruiseShip, LocationIds.HotSprings),//water nice
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),//beautiful
        (Locations.Casino, LocationIds.Casino),//nikki would love
        (Locations.PrivateTable, LocationIds.Restaurant),//expensive fast food
        (Locations.SecretGrotto, LocationIds.HotSprings),//water nice
        (Locations.StripClub, LocationIds.Restaurant),//expensive fast food
        (Locations.RoyalSuite, LocationIds.HikingTrail),//glad asked out
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
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
        (Locations.StripClub, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),

        (Locations.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (Locations.OuterSpace, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (Locations.VolcanoTop, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
    ];

    public TiffanyConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex == 2;
}