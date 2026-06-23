using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public class LolaConfigurator : IGirlConfigurator
{
    public bool ExtractUniqueAcceptDialogLines => false;

    public IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static RelativeId[] _favQuestionOrder = [
        Questions.LastName,
        Questions.Birthday,
        Questions.Age,
        Questions.Education,
        Questions.FavColour,
        Questions.Hobby,
        Questions.FavHangout,
        Questions.CupSize,
        Questions.FavSeason,
        Questions.Weight,
        Questions.Height,
        Questions.Occupation,
    ];

    public (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes => (0,2,3);

    public GirlDataMod Mod => _mod;
    private readonly GirlDataMod _mod;

    public IEnumerable<(RelativeId, int)> ExtractItemIds => Enumerable.Empty<(RelativeId, int)>();

    public string UnderwearName => "Underwear";

    public (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => null;

    public LolaConfigurator()
    {
        _mod = new GirlDataMod(Hp2BaseMod.Girls.Lola, InsertStyle.append);
        ModInterface.AddDataMod(_mod);
    }

    public  void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        if (!Plugin.PConfig.UseHp1LolaStats.Value)
        {
            _mod.FavoriteAffectionType = null;
            _mod.LeastFavoriteAffectionType = null;
        }

        if (!Plugin.PConfig.UseHp1LolaLines.Value)
        {
            _mod.LinesByDialogTriggerId = null;
        }

        _mod.FavAnswers = new(){
            {Questions.LastName, LastName.Rembrite},
            {Questions.Education, Education.College_4},
            {Questions.Height, Height._5_7},
            {Questions.Weight, Weight._122},
            {Questions.Occupation, Occupation.Stewardess},
            {Questions.CupSize, CupSize.E_Cup},
            {Questions.Birthday, Birthday.Feb_23},
            {Questions.Hobby, FavHobby.Tennis},
            {Questions.FavColour, FavColour.Blue},
            {Questions.FavSeason, FavSeason.Spring},
            {Questions.FavHangout, FavHangout.Cafe},
        };

        hpBody.BackPosition = new VectorInfo(200, 450);
        hpBody.HeadPosition = new VectorInfo(230, 780);

        hpBody.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)},

            {Locations.MassageSpa, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {Locations.Aquarium, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {Locations.SecludedCabana, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {Locations.PoolsideBar, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.GolfCourse, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {Locations.CruiseShip, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.RooftopLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {Locations.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {Locations.PrivateTable, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {Locations.SecretGrotto, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {Locations.RoyalSuite, new GirlStyleInfo(Hp2BaseMod.Styles.Sexy)},
            {Locations.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {Locations.OuterSpace, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
            {Locations.StripClub, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {Locations.VolcanoTop, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)}
        };
    }

    public  bool IsPhotoIndexNsfw(int photoIndex) => photoIndex == 3;
}