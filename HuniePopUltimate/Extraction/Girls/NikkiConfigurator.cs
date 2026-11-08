using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

public class NikkiConfigurator : GirlConfiguratorBase
{
    protected override string UniqueCategoryDescription => "Gaming";
    protected override string ShoeCategoryDescription => "Low-Key";

    protected override RelativeId GirlId => Girls.Nikki;
    protected override string GirlAssetFileName => "nikki";

    protected override int Age => 20;

    protected override RelativeId ItemEnergyId => EnergyId.Talent;

    protected override (int x, int y) BackPosition => (195, 440);
    protected override (int x, int y) HeadPosition => (230, 780);

    public override IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static readonly RelativeId[] _favQuestionOrder =
    [
        Questions.Age,
        Questions.FavSeason,
        Questions.Birthday,
        Questions.Height,
        Questions.Education,
        Questions.Weight,
        Questions.Hobby,
        Questions.FavHangout,
        Questions.Occupation,
        Questions.FavColour,
        Questions.CupSize,
        Questions.LastName,
    ];

    protected override IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap
        => _favAnswersMap;
    private static readonly (RelativeId QuestionId, RelativeId AnswerId)[] _favAnswersMap =
    [
        (Questions.LastName, LastName.AnnMarie),
        (Questions.Education, Education.Highschool),
        (Questions.Height, Height._5_0),
        (Questions.Weight, Weight._104),
        (Questions.Occupation, Occupation.Barista),
        (Questions.CupSize, CupSize.C_Cup),
        (Questions.Birthday, Birthday.May_13),
        (Questions.Hobby, FavHobby.Gaming),
        (Questions.FavColour, FavColour.Cream),
        (Questions.FavSeason, FavSeason.Winter),
        (Questions.FavHangout, FavHangout.Home),
    ];

    protected override IEnumerable<(RelativeId, int)> UniqueItemIds
        => _uniqueItemIds;
    private static readonly (RelativeId, int)[] _uniqueItemIds =
    [
        (Items.Nikki.Unique1, 9228), // Retro Controller
        (Items.Nikki.Unique2, 9229), // Arcade Joystick
        (Items.Nikki.Unique3, 9230), // Zappy Gun
        (Items.Nikki.Unique4, 9231), // Gamer Glove
    ];

    protected override IEnumerable<(RelativeId id, string name, string description)> ShoeItems
        => _shoeItems;
    private static readonly (RelativeId id, string name, string description)[] _shoeItems =
    [
        (Items.Nikki.Shoe1, "Converse", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe2, "Hi-Tops", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe3, "Wolf Slippers", "Give to a NIKKI for +1 [[style]@Style] EXP."),
        (Items.Nikki.Shoe4, "Sensible Loafers", "Give to a NIKKI for +1 [[style]@Style] EXP."),
    ];

    protected override IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds
        => _baggageItemIds;

    private static readonly (RelativeId, int, string name, string description)[] _baggageItemIds =
    [
        (Items.Nikki.Baggage1, 9232, "Baggage A", "Placeholder"), // Handheld Game
        (Items.Nikki.Baggage2, 9233, "Baggage B", "Placeholder"), // Arcade Cabinet
        (Items.Nikki.Baggage3, 9233, "Baggage C", "Placeholder"), // TODO
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (LocationId.MassageSpa, LocationIds.OutdoorLounge),//not my scene
        (LocationId.Aquarium, LocationIds.OutdoorLounge),//not my scene
        (LocationId.SecludedCabana, LocationIds.WaterPark),//burn alive
        (LocationId.PoolsideBar, LocationIds.WaterPark),//burn alive
        (LocationId.GolfCourse, LocationIds.BotanicalGarden),//allergies
        (LocationId.CruiseShip, LocationIds.WaterPark),//burn alive
        (LocationId.RooftopLounge, LocationIds.ScenicOverlook),//romantic
        (LocationId.Casino, LocationIds.Casino),//not an arcade
        (LocationId.PrivateTable, LocationIds.Restaurant),//dishes gross
        (LocationId.SecretGrotto, LocationIds.HotSprings),//water nice
        (LocationId.StripClub, LocationIds.IceRink),//might be fun
        (LocationId.RoyalSuite, LocationIds.OutdoorLounge),//not my scene
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)),
        (LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)),

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

    public NikkiConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {
    }

    public override bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex == 3;

    /// <inheritdoc/>
    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        ConfigureGirlDefault(hpBody, assetBundle, sprites, audio, items, Items.Nikki);
    }
}