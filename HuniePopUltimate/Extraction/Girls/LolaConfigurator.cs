using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public class LolaConfigurator : IGirlConfigurator
{
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

    public string UnderwearName => "Ivory";

    public (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => null;

    public LolaConfigurator()
    {
        _mod = new GirlDataMod(Hp2BaseMod.GirlId.Lola, InsertStyle.append);
        ModInterface.DataMod.AddDataMod(_mod);
    }

    public void ConfigureGirl(GirlBodyDataMod hpBody, AssetBundle assetBundle, HpSpriteCache sprites, HpAudioCache audio, HpItemCache items)
    {
        if (!Plugin.PConfig.UseHp1LolaStats.Value)
        {
            _mod.FavoriteAffectionId = null;
            _mod.LeastFavoriteAffectionId = null;
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
            {LocationIds.BotanicalGarden, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationIds.HikingTrail, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},
            {LocationIds.FarmersMarket, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},

            {LocationIds.IceRink, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationIds.WaterPark, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},
            {LocationIds.TennisCourts, new GirlStyleInfo(Hp2BaseMod.StyleId.Party)},

            {LocationIds.ScenicOverlook, new GirlStyleInfo(Hp2BaseMod.StyleId.Romantic)},
            {LocationIds.Casino, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationIds.HotSprings, new GirlStyleInfo(Hp2BaseMod.StyleId.Water)},

            {LocationIds.OutdoorLounge, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},
            {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.StyleId.Activity)},
            {LocationIds.Restaurant, new GirlStyleInfo(Hp2BaseMod.StyleId.Relaxing)},

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
    }

    public bool IsTextPhotoIndexNsfw(int photoIndex) => photoIndex == 3;

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