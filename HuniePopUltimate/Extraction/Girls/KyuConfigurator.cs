using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class KyuConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Sex Toys";
    protected override string ShoeCategoryDescription => "Idk";

    protected override bool HasUiSprites => false;
    protected override RelativeId GirlId => Hp2BaseMod.Girls.Kyu;
    protected override string GirlAssetFileName => "kyu";

    protected override int Age => 386;

    public override string UnderwearName => "Pink, Bitch";

    protected override RelativeId ItemEnergyId => EnergyTypes.Flirtation;

    protected override (int x, int y) BackPosition => (245, 454);
    protected override (int x, int y) HeadPosition => (230, 780);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.FavHangout,
        Questions.Height,
        Questions.FavColour,
        Questions.LastName,
        Questions.HomeWorld,
        Questions.FavSeason,
        Questions.Weight,
        Questions.Birthday,
        Questions.Occupation,
        Questions.Age,
        Questions.CupSize,
        Questions.Hobby,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.Sugardust),
        (Questions.HomeWorld, Homeworld.SkyGarden),
        (Questions.Height, Height._5_4),
        (Questions.Weight, Weight._110),
        (Questions.Occupation, Occupation.LoveFairy),
        (Questions.CupSize, CupSize.C_Cup),
        (Questions.Birthday, Birthday.Aug_3),
        (Questions.Hobby, FavHobby.Porn),
        (Questions.FavColour, FavColour.Pink),
        (Questions.FavSeason, FavSeason.Summer),
        (Questions.FavHangout, FavHangout.Bedroom),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Kyu.Unique1, 9246), // Endurance Ring
        (Items.Kyu.Unique2, 9247), // Pocket Vibe
        (Items.Kyu.Unique3, 9248), // Fairy's Tail
        (Items.Kyu.Unique4, 9249), // Bliss Beads
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Kyu.Shoe1, "Fae Ballet Flats", "Give to a KYU for +1 [[style]@Style] EXP."),
        (Items.Kyu.Shoe2, "Foliage Ballet Flats", "Give to a KYU for +1 [[style]@Style] EXP."),
        (Items.Kyu.Shoe3, "Icey Ballet Flats", "Give to a KYU for +1 [[style]@Style] EXP."),
        (Items.Kyu.Shoe4, "Bleeding Heard Ballet Flats", "Give to a KYU for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        //(Items.Kyu.Baggage1, 9250), // Magic Wand
        //(Items.Kyu.Baggage2, 9251), // Royal Scepter
        //(Items.Kyu.Baggage3, ), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.ScenicOverlook),//seduce
        (Locations.Aquarium, LocationIds.FarmersMarket),//nothing i like
        (Locations.SecludedCabana, LocationIds.ScenicOverlook),//seduce
        (Locations.PoolsideBar, LocationIds.WaterPark),//leaf bikini
        (Locations.GolfCourse, LocationIds.TennisCourts),//don't know about this game
        (Locations.CruiseShip, LocationIds.WaterPark),//leaf bikini
        (Locations.RooftopLounge, LocationIds.ScenicOverlook),//seduce
        (Locations.Casino, LocationIds.Casino),//hunie
        (Locations.PrivateTable, LocationIds.Restaurant),//eating alone
        (Locations.SecretGrotto, LocationIds.HotSprings),//staple
        (Locations.StripClub, LocationIds.Carnival),//fit in
        (Locations.RoyalSuite, LocationIds.ScenicOverlook),//seduce
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),

        (LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)),

        (Locations.MassageSpa, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (Locations.Aquarium, new GirlStyleInfo(Hp2BaseMod.Styles.Party, Styles.KyuDisguise)),
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

    public KyuConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        base.ConfigureGirl(hpBody, assetBundle, sprites, audio, items);
        Mod.SpecialCharacter = false;
        hpBody.SpecialEffect = SpecialParts.KyuWingId;
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(1,3);
}