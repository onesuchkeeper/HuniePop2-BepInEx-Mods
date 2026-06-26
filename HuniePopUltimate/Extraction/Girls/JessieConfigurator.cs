using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public class JessieConfigurator : IGirlConfigurator
{
    public IEnumerable<RelativeId> FavQuestionOrder => _favQuestionOrder;
    private static RelativeId[] _favQuestionOrder = [
        Questions.Weight,
        Questions.FavColour,
        Questions.Education,
        Questions.Birthday,
        Questions.Occupation,
        Questions.CupSize,
        Questions.FavHangout,
        Questions.Age,
        Questions.Height,
        Questions.FavSeason,
        Questions.Hobby,
        Questions.LastName,
    ];

    public (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes => (0,2,3);

    public GirlDataMod Mod => _mod;
    private readonly GirlDataMod _mod;

    public IEnumerable<(RelativeId, int)> ExtractItemIds => Enumerable.Empty<(RelativeId, int)>();

    public string UnderwearName => "Underwear";

    public (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => null;

    public JessieConfigurator()
    {
        _mod = new GirlDataMod(Hp2BaseMod.Girls.Jessie, InsertStyle.append);
        ModInterface.AddDataMod(_mod);
    }

    public  void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        if (!Plugin.PConfig.UseHp1JessieStats.Value)
        {
            _mod.FavoriteAffectionType = null;
            _mod.LeastFavoriteAffectionType = null;
        }

        if (!Plugin.PConfig.UseHp1JessieLines.Value)
        {
            _mod.LinesByDialogTriggerId = null;
        }

        _mod.FavAnswers = new(){
            {Questions.LastName, LastName.Maye},
            {Questions.Education, Education.Dropout},
            {Questions.Height, Height._5_7},
            {Questions.Weight, Weight._126},
            {Questions.Occupation, Occupation.PornStar},
            {Questions.CupSize, CupSize.G_Cup},
            {Questions.Birthday, Birthday.Jan_27},
            {Questions.Hobby, FavHobby.Drinking},
            {Questions.FavColour, FavColour.Orange},
            {Questions.FavSeason, FavSeason.Winter},
            {Questions.FavHangout, FavHangout.Bar},
        };

        ((GirlSpecialPartDataMod)hpBody.specialParts[0]).RequiredHairstyles = new List<RelativeId>(){
            Hp2BaseMod.Styles.Relaxing
        };

        //the celebrity front hair has a different anchor point
        //in hp1 from every other doll part, so correct for it here
        var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)hpBody.hairstyles[3]).FrontHairPart;
        celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
        celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;

        hpBody.BackPosition = new VectorInfo(200, 450);
        hpBody.HeadPosition = new VectorInfo(230, 780);

        hpBody.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.Styles.Romantic)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.Styles.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.Styles.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Relaxing)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},

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

        //Drinks during day so doesn't have a rejection line
        var rejectSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieReject];
        var acceptSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieAccept];
        var rejectFood = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }

    public  bool IsPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);

    public bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId)
    {
        if (dialogTriggerId == DialogTriggers.LovesAccept)
        {
            cleanedDialogTriggerId = default;
            return false;
        }

        cleanedDialogTriggerId = dialogTriggerId;
        return dialogTriggerId != Hp2BaseMod.DialogTriggers.UniqueAccept;
    }
}