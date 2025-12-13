using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.RepeatThreesome", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.ExtraLocations", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : Hp2BaseModPlugin
{
    public const string DATA_DIR = "HuniePop_Data";
    public const string DAC_DIR = "Digital Art Collection";
    public const string REPEAT_THREESOME_UUID = "OSK.BepInEx.RepeatThreesome";
    public const string SINGLE_DATE_UUID = "OSK.BepInEx.SingleDate";
    public const string EXTRA_LOCATIONS_UUID = "OSK.BepInEx.ExtraLocations";

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");
    public static readonly string ASSEMBLY_DIR = Path.Combine(DATA_DIR, "Managed");

    public static ConfigEntry<bool> UnlockStyles => _unlockStyles;
    private static ConfigEntry<bool> _unlockStyles;

    public static ConfigEntry<bool> UnlockPhotos => _unlockPhotos;
    private static ConfigEntry<bool> _unlockPhotos;

    public static ConfigEntry<string> HuniePopDir => _huniePopDir;
    private static ConfigEntry<string> _huniePopDir;

    /// <summary>
    /// Ids of locations where single data bonus rounds can take place.
    /// </summary>
    public static IEnumerable<RelativeId> SexLocs => _instance._sexLocs;
    private RelativeId[] _sexLocs;

    /// <summary>
    /// Id of SingleDate's nobody character.
    /// </summary>
    public static RelativeId SingleDateNobodyId => _instance._singleDateNobodyId;
    private RelativeId _singleDateNobodyId;

    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    internal static int _photoModCount = 0;

    private static Plugin _instance;

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        base.Awake();

        var audioManager_GO = new GameObject();
        var audioManager = audioManager_GO.AddComponent<AudioMemoryMonitor>();
        audioManager.gameObject.transform.SetParent(this.transform);

        _instance = this;
        AssetStudio.Logger.Default = new Logger(new TextWriter());

        _unlockStyles = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockStyles), false, "If all Hp1 outfit and hairstyles should auto-unlock.");
        _unlockPhotos = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockPhotos), false, "If all Hp1 photos should auto-unlock.");
        _huniePopDir = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(HuniePopDir), Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the HuniePop install directory.");

        if (!(!string.IsNullOrWhiteSpace(HuniePopDir.Value)
                && Directory.Exists(HuniePopDir.Value)))
        {
            ModInterface.Log.Error("HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return;
        }

        var dataDir = Path.Combine(HuniePopDir.Value, DATA_DIR);
        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop data directory");
            return;
        }

        var assemblyDir = Path.Combine(HuniePopDir.Value, ASSEMBLY_DIR);
        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.Error("HuniePop Ultimate failed to find HuniePop assembly directory");
            return;
        }

        var nudeOutfitPart = Chainloader.PluginInfos.ContainsKey(REPEAT_THREESOME_UUID)
            ? new GirlPartDataMod(new RelativeId(ModInterface.GetSourceId(REPEAT_THREESOME_UUID), 0), InsertStyle.replace)
            {
                PartType = GirlPartType.OUTFIT,
                PartName = "nudeOutfit",
                X = 0,
                Y = 0,
                SpriteInfo = new SpriteInfoInternal("EmptySprite")
            }
            : null;

        var sexLocs = new List<RelativeId>()
        {
            LocationIds.BedRoomDate,
            Locations.RoyalSuite,
            Locations.HotelRoom,
        };

        if (Chainloader.PluginInfos.ContainsKey(EXTRA_LOCATIONS_UUID))
        {
            var extraLocationsId = ModInterface.GetSourceId(EXTRA_LOCATIONS_UUID);
            sexLocs.Add(new RelativeId(extraLocationsId, 2));
            sexLocs.Add(new RelativeId(extraLocationsId, 7));
        }

        _sexLocs = sexLocs.ToArray();

        var singleDateId = ModInterface.GetSourceId(SINGLE_DATE_UUID);

        if (ModInterface.TryGetInterModValue(singleDateId, "AddGirlDatePhotos", out Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos)
            && ModInterface.TryGetInterModValue(singleDateId, "AddGirlSexPhotos", out Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> m_AddGirlSexPhotos)
            && ModInterface.TryGetInterModValue(singleDateId, "SetGirlCharm", out Action<RelativeId, Sprite> m_SetCharmSprite)
            && ModInterface.TryGetInterModValue(singleDateId, "AddSexLocationBlackList", out Action<RelativeId, IEnumerable<RelativeId>> m_AddSexLocationBlackList)
            && ModInterface.TryGetInterModValue(singleDateId, "SetCutsceneSuccessAttracted", out Action<RelativeId, RelativeId> m_SetCutsceneSuccessAttracted)
            && ModInterface.TryGetInterModValue(singleDateId, "SetBonusRoundSuccessCutscene", out Action<RelativeId, RelativeId> m_SetBonusRoundSuccessCutscene))
        {
            var defaultGirlStyle = new GirlStyleInfo()
            {
                HairstyleId = RelativeId.Default,
                OutfitId = RelativeId.Default,
            };

            var defaultPairStyle = new PairStyleInfo()
            {
                MeetingGirlOne = defaultGirlStyle,
                MeetingGirlTwo = defaultGirlStyle,
                SexGirlOne = defaultGirlStyle,
                SexGirlTwo = defaultGirlStyle,
            };

            // var questions = new List<IGameDefinitionInfo<GirlPairFavQuestionSubDefinition>>();
            // for (int i = 1; i <= 20; i++)
            // {
            //     questions.Add(new GirlPairFavQuestionInfo()
            //     {
            //         GirlResponseIndexOne = 1,
            //         GirlResponseIndexTwo = 1,
            //         QuestionDefinitionID = new RelativeId(-1, i)
            //     });
            // }

            var defaultPhoto = new RelativeId(singleDateId, 0);
            _singleDateNobodyId = new RelativeId(singleDateId, 0);

            void AddPairMod(ClockDaytimeType sexTime, RelativeId girlId, RelativeId pairId)
            {
                ModInterface.AddDataMod(new GirlPairDataMod(pairId, InsertStyle.assignNull, 1)
                {
                    GirlDefinitionOneID = SingleDateNobodyId,
                    GirlDefinitionTwoID = girlId,
                    SpecialPair = false,
                    PhotoDefinitionID = defaultPhoto,
                    IntroductionPair = false,
                    IntroSidesFlipped = false,
                    HasMeetingStyleOne = false,
                    HasMeetingStyleTwo = false,
                    MeetingLocationDefinitionID = new RelativeId(-1, 1 + (girlId.LocalId % 8)),//temp
                    SexDayTime = sexTime,
                    Styles = defaultPairStyle,
                    //FavQuestions = questions,
                    SexLocationDefinitionID = null,

                    BonusSuccessCutsceneDefinitionID = Cutscenes.BonusRoundSuccess,
                    AttractSuccessCutsceneDefinitionID = Cutscenes.SuccessAttracted,
                    SuccessCutsceneDefinitionID = new RelativeId(singleDateId, 4),

                    IntroRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 0),
                    AttractRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 1),
                    PreSexRelationshipCutsceneDefinitionID = Cutscenes.PreSex,
                    PostSexRelationshipCutsceneDefinitionID = Cutscenes.PostSex,
                });

                m_AddSexLocationBlackList(girlId, SexLocs);
                m_SetCutsceneSuccessAttracted(girlId, Cutscenes.SuccessAttracted);
                m_SetBonusRoundSuccessCutscene(girlId, Cutscenes.BonusRoundSuccess);
            }

            IEnumerable<RelativeId> AllHp1NormalSingleDates()
            {
                yield return Girls.Tiffany;
                yield return Girls.Aiko;
                yield return Girls.Kyanna;
                yield return Girls.Audrey;
                yield return Girls.Nikki;
                yield return Girls.Beli;
                yield return Hp2BaseMod.Girls.KyuId;
                yield return Girls.Celeste;
                yield return Girls.Venus;
            }
            ;// ide why do you force this on the newline? What did I ever do to you?

            foreach (var girl in AllHp1NormalSingleDates())
            {
                AddPairMod(ClockDaytimeType.NIGHT, girl, girl);
            }

            AddPairMod(ClockDaytimeType.EVENING, Girls.Momo, Girls.Momo);
            AddPairMod(ClockDaytimeType.NIGHT, Hp2BaseMod.Girls.LolaId, new RelativeId(singleDateId, Hp2BaseMod.Girls.LolaId.LocalId));
            AddPairMod(ClockDaytimeType.NIGHT, Hp2BaseMod.Girls.JessieId, new RelativeId(singleDateId, Hp2BaseMod.Girls.JessieId.LocalId));
        }
        else
        {
            m_AddGirlDatePhotos = null;
            m_AddGirlSexPhotos = null;
            m_SetCharmSprite = null;
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        ModInterface.Log.Message("Loading HuniePop assembly (this may take a bit)");
        var hpExtraction = new HpExtraction(HuniePopDir.Value, m_AddGirlDatePhotos, m_AddGirlSexPhotos, m_SetCharmSprite, nudeOutfitPart);
        using (ModInterface.Log.MakeIndent("HuniePop assembly loaded successfully, beginning import:"))
        {
            hpExtraction.Extract();
        }

        var whiteVal = 248f / 255f;
        var whiteCol = new Color(whiteVal, whiteVal, whiteVal);

        var greyVal = 153f / 255f;
        var greyCol = new Color(greyVal, greyVal, greyVal);

        ITextureRenderStep[] hp1ThumbSteps = [
            new TextureRsScale(new Vector2(156f/2400, 112f/1800f)),
            new TextureRsPad(2, whiteCol),
            new TextureRsPad(1),
        ];

        // kyu old photo
        var kyuPhotoDir = Path.Combine(HuniePopDir.Value, DAC_DIR, "Photos", "Kyu");
        if (Directory.Exists(kyuPhotoDir))
        {
            var censoredTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom1.jpg"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom2.jpg"), true);

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(ModId, _photoModCount++), InsertStyle.replace)
            {
                BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
                ThumbnailCensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "kyu_old_thumb_censored.png"), new TextureInfoRender(censoredTexture, false, hp1ThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "kyu_old_thumb_wet.png"), new TextureInfoRender(wetTexture, false, hp1ThumbSteps))),
            });
        }

        // audrey 10th photo
        {
            ITextureRenderStep[] audreyThumbSteps = [
                new TextureRsScale(new Vector2(156f/1440f, 112f/1080f)),
                new TextureRsPad(2, whiteCol),
                new TextureRsPad(1),
            ];

            var UncensoredTexture = new TextureInfoExternal(Path.Combine(IMAGES_DIR, "hp_10th_anniversary_audrey_dry.png"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(IMAGES_DIR, "hp_10th_anniversary_audrey_wet.png"), true);

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(ModId, _photoModCount++), InsertStyle.replace)
            {
                BigPhotoUncensored = new SpriteInfoTexture(UncensoredTexture),
                ThumbnailUncensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "audrey_10th_thumb_uncensored.png"), new TextureInfoRender(UncensoredTexture, false, audreyThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(IMAGES_DIR, "audrey_10th_thumb_wet.png"), new TextureInfoRender(wetTexture, false, audreyThumbSteps))),
            });
        }

        // favorites
        FavDrink.AddDataMods();
        FavExercise.AddDataMods();
        FavFridayNight.AddDataMods();
        FavHoliday.AddDataMods();
        FavIceCream.AddDataMods();
        FavMovieGenre.AddDataMods();
        FavMusicGenre.AddDataMods();
        FavOnlineActivity.AddDataMods();
        FavOutdoorActivity.AddDataMods();
        FavOwnBodyPart.AddDataMods();
        FavPet.AddDataMods();
        FavPhoneApp.AddDataMods();
        FavPornCategory.AddDataMods();
        FavSchoolSubject.AddDataMods();
        FavSexPos.AddDataMods();
        FavShop.AddDataMods();
        FavSundayMorning.AddDataMods();
        FavThemeParkRide.AddDataMods();
        FavTrait.AddDataMods();
        FavWeather.AddDataMods();

        Birthday.AddDataMods();
        CupSize.AddDataMods();
        Education.AddDataMods();
        FavColour.AddDataMods();
        FavHangout.AddDataMods();
        FavHobby.AddDataMods();
        FavSeason.AddDataMods();
        Height.AddDataMods();
        Homeworld.AddDataMods();
        LastName.AddDataMods();
        Occupation.AddDataMods();
        Weight.AddDataMods();
        Age.AddDataMods();

        //dts
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PreBedroom, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PreSex, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });
        ModInterface.AddDataMod(new DialogTriggerDataMod(DialogTriggers.PostSex, InsertStyle.append)
        {
            ForceType = DialogTriggerForceType.NONE,
        });

        //cutscenes
        PreSexCutscene.AddDataMods();
        PostSexCutscene.AddDataMods();
        SuccessAttractedCutscene.AddDataMods();
        BonusRoundSuccessCutscene.AddDataMods();

        ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;

        ModInterface.Events.FinderSlotsPopulate += On_FinderSlotsPopulate;
    }

    private void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

        switch (time)
        {
            case ClockDaytimeType.MORNING:
                args.RemoveGirlFromAllPools(Girls.Audrey);
                args.RemoveGirlFromAllPools(Girls.Celeste);
                break;
            case ClockDaytimeType.AFTERNOON:
                args.RemoveGirlFromAllPools(Girls.Celeste);
                break;
            case ClockDaytimeType.EVENING:
                args.RemoveGirlFromAllPools(Girls.Momo);
                break;
            case ClockDaytimeType.NIGHT:
                break;
        }
    }

    private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        if (!UnlockPhotos.Value) return;

        args.UnlockedPhotos ??= new List<PhotoDefinition>();

        for (int i = 0; i < _photoModCount; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(ModId, i)));
        }
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        if (!UnlockStyles.Value) return;

        using (ModInterface.Log.MakeIndent())
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                var expansion = ExpandedGirlDefinition.Get(girlId);

                foreach (var outfitId_Index in expansion.OutfitIdToIndex.Where(x => x.Key.SourceId == ModId))
                {
                    fileGirl.UnlockOutfit(outfitId_Index.Value);
                }

                foreach (var hairstyleId_index in expansion.HairstyleIdToIndex.Where(x => x.Key.SourceId == ModId))
                {
                    fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                }
            }
        }
    }
}
