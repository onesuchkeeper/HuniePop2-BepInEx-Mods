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
    protected override string ShoeCategoryDescription => "Ballet Flats";

    protected override bool HasUiSprites => false;
    protected override RelativeId GirlId => Hp2BaseMod.GirlId.Kyu;
    protected override string GirlAssetFileName => "kyu";

    protected override int Age => 386;

    public override string UnderwearName => "Pink, Bitch";

    protected override RelativeId ItemEnergyId => EnergyId.Flirtation;

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
        (Items.Kyu.Baggage1, 9250, "Baggage A", "Placeholder"), // Magic Wand
        (Items.Kyu.Baggage2, 9251, "Baggage B", "Placeholder"), // Royal Scepter
        (Items.Kyu.Baggage3, 9251, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.ScenicOverlook),//seduce
        (LocationId.Aquarium, LocationIds.FarmersMarket),//nothing i like
        (LocationId.SecludedCabana, LocationIds.ScenicOverlook),//seduce
        (LocationId.PoolsideBar, LocationIds.WaterPark),//leaf bikini
        (LocationId.GolfCourse, LocationIds.TennisCourts),//don't know about this game
        (LocationId.CruiseShip, LocationIds.WaterPark),//leaf bikini
        (LocationId.RooftopLounge, LocationIds.ScenicOverlook),//seduce
        (LocationId.Casino, LocationIds.Casino),//hunie
        (LocationId.PrivateTable, LocationIds.Restaurant),//eating alone
        (LocationId.SecretGrotto, LocationIds.HotSprings),//staple
        (LocationId.StripClub, LocationIds.Carnival),//fit in
        (LocationId.RoyalSuite, LocationIds.ScenicOverlook),//seduce
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),

        (LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.StyleId.Sexy)),

        (LocationId.MassageSpa, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationId.Aquarium, new GirlStyleInfo(Hp2BaseMod.StyleId.Party, Styles.KyuDisguise)),
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

    public KyuConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Kyu);
        if (Plugin.HasKyuOldPhoto) AddSexPhotos([(Photos.KyuOld, RelativeId.Default)]);
        Mod.SpecialCharacter = false;
        hpBody.SpecialEffect = SpecialPartId.KyuWingId;
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(1,3);
}