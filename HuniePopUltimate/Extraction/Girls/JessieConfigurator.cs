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

    public string UnderwearName => "The Real Deal";

    public (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => null;

    public JessieConfigurator()
    {
        _mod = new GirlDataMod(Hp2BaseMod.GirlId.Jessie, InsertStyle.append);
        ModInterface.DataMod.AddDataMod(_mod);
    }

    public  void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        var cowboyHat = (GirlSpecialPartDataMod)hpBody.specialParts[0];
        cowboyHat.RequiredHairstyles = new List<RelativeId>()
        {
            Hp2BaseMod.StyleId.Relaxing
        };

        cowboyHat.RequiredOutfits = new List<RelativeId>()
        {
            Hp2BaseMod.StyleId.Relaxing,
        };

        if (!Plugin.PConfig.UseHp1JessieStats.Value)
        {
            _mod.FavoriteAffectionId = null;
            _mod.LeastFavoriteAffectionId = null;
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

        //the celebrity front hair has a different anchor point
        //in hp1 from every other doll part, so correct for it here
        var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)hpBody.hairstyles[3]).FrontHairPart;
        celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
        celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;

        hpBody.BackPosition = new VectorInfo(200, 450);
        hpBody.HeadPosition = new VectorInfo(230, 780);

        hpBody.LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)},

            {LocationIds.BedRoomDate, new GirlStyleInfo(Hp2BaseMod.StyleId.Sexy)},

            {LocationId.MassageSpa, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationId.Aquarium, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)},
            {LocationId.SecludedCabana, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationId.PoolsideBar, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},
            {LocationId.GolfCourse, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)},
            {LocationId.CruiseShip, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},
            {LocationId.RooftopLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationId.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationId.PrivateTable, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationId.SecretGrotto, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},
            {LocationId.RoyalSuite, new GirlStyleInfo(Hp2BaseMod.StyleId.Sexy)},
            {LocationId.AirplaneBathroom, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationId.OuterSpace, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)},
            {LocationId.StripClub, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationId.VolcanoTop, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)}
        };

        //Drinks during day so doesn't have a rejection line
        var rejectSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggerId.SmoothieReject];
        var acceptSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggerId.SmoothieAccept];
        var rejectFood = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggerId.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }

    public  bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex.InInclusiveRange(2,3);

    public bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId)
    {
        if (dialogTriggerId == DialogTriggers.LovesAccept)
        {
            cleanedDialogTriggerId = default;
            return false;
        }

        cleanedDialogTriggerId = dialogTriggerId;
        return dialogTriggerId != Hp2BaseMod.DialogTriggerId.UniqueAccept 
            && dialogTriggerId != Hp2BaseMod.DialogTriggerId.StaminaExhausted
            && dialogTriggerId != Hp2BaseMod.DialogTriggerId.ShoesAccept;
    }
}