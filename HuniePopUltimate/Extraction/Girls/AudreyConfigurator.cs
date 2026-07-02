using System;
using System.Collections.Generic;
using Hp2BaseMod;
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

    protected override RelativeId ItemEnergyId => EnergyTypes.Sexuality;

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
        (Items.Audrey.Baggage1, 9220, "Mega Bitch", "Whenever a [[sentiment]@Sentiment] token is matched, the other girl's [[passion]@Passion] is lowered."),// Blotter Tabs
        //(Items.Audrey.Baggage2, 9221),// Happy Pills
        //(Items.Audrey.Baggage3, 9130),// Glow Sticks
    ];

    protected override IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap
        => _locationGreetingMap;
    private static readonly (RelativeId to, RelativeId from)[] _locationGreetingMap =
    [
        (Locations.MassageSpa, LocationIds.HotSprings),// get used to this
        (Locations.Aquarium, LocationIds.ScenicOverlook),// gaaaayyy
        (Locations.SecludedCabana, LocationIds.OutdoorLounge),
        (Locations.PoolsideBar, LocationIds.IceRink),//cold
        (Locations.GolfCourse, LocationIds.TennisCourts),//sweaty
        (Locations.CruiseShip, LocationIds.WaterPark),//sunscreen
        (Locations.RooftopLounge, LocationIds.Restaurant),//really impressive
        (Locations.Casino, LocationIds.Casino),//smoke in here
        (Locations.PrivateTable, LocationIds.Carnival),//idea of a date
        (Locations.SecretGrotto, LocationIds.HotSprings),//get used to this
        (Locations.StripClub, LocationIds.Casino),//smoke in here
        (Locations.RoyalSuite, LocationIds.BotanicalGarden),//im here what now
    ];

    protected override IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap
        => _locationStyleMap;

    private static readonly (RelativeId LocationId, GirlStyleInfo Style)[] _locationStyleMap =
    [
        (LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
        (LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),

        (LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),
        (LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),

        (LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)),
        (LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)),
        (LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)),

        (LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)),
        (LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)),
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

    public AudreyConfigurator(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
        : base(addGirlSexPhotos, setCharmSprite)
    {

    }

    public override void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        base.ConfigureGirl(hpBody, assetBundle, sprites, audio, items);

        AddSexPhotos([(Photos.Audrey10th, RelativeId.Default)]);
        
        ((GirlSpecialPartDataMod)hpBody.specialParts[1]).RequiredHairstyles = new List<RelativeId>()
        {
            Hp2BaseMod.Styles.Activity
        };

        AddMommyIssuesBaggage();
    }

    public override bool IsPhotoIndexNsfw(int photoIndex) => false;

    private void AddMommyIssuesBaggage()
    {
        var ailment = new AilmentDataMod(Items.Audrey.Baggage1, InsertStyle.append)
        {
            ItemDefinitionID = Items.Audrey.Baggage1,
            ScriptedAilmentFactory = (Ailment) => new RomancePoisonAilment(),
        };
        ModInterface.AddDataMod(ailment);

        var cutscene = new CutsceneDataMod(Items.Audrey.Baggage1, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.BrokenRecovered, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                new FunctionalCutsceneStepInfo(completed =>
                {
                    ModInterface.Log.Message("TEST IM HERE IT WORKED");
                    completed.Invoke();
                }),
                CutsceneStepUtility.MakeGameActionInfo(new LogicActionInfo()
                {
                    Type = LogicActionType.SET_FLAG,
                    StringValue = Flags.NOTIFICATION_ITEM_ID,
                    IntValue = ModInterface.Data.GetRuntimeDataId(GameDataType.Item, Items.Audrey.Baggage1)
                }, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.25f),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };
        ModInterface.AddDataMod(cutscene);
    }
}