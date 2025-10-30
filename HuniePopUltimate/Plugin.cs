using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
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
public class Plugin : BaseUnityPlugin
{
    private static readonly string _configGeneralName = "general";
    private static readonly string _configUnlockStylesName = "unlockStyles";
    private static readonly string _configUnlockPhotosName = "unlockPhotos";

    private static readonly string _dataDir = "HuniePop_Data";
    private static readonly string _dacDir = "Digital Art Collection";
    private static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    public static string RootDir => _dir;
    private static string _dir;

    public static string ImgDir => Path.Combine(_dir, "images");

    public static int ModId => _modId;
    private static int _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

    public static int PhotoModCount = 0;

    private void Awake()
    {
        _dir = Path.GetDirectoryName(Info.Location);
        var imagesDir = Path.Combine(_dir, "images");

        this.Config.Bind(_configGeneralName, _configUnlockStylesName, false, "If All Hp1 outfit and hairstyles should auto-unlock.");
        this.Config.Bind(_configGeneralName, _configUnlockPhotosName, false, "If All Hp1 photos should auto-unlock.");
        this.Config.Bind("General", "HuniePopDir", Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the HuniePop install directory.");

        var singleDateId = ModInterface.GetSourceId("OSK.BepInEx.SingleDate");

        if (ModInterface.TryGetInterModValue(singleDateId, "AddGirlDatePhotos", out Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos)
            && ModInterface.TryGetInterModValue(singleDateId, "AddGirlSexPhotos", out Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> m_AddGirlSexPhotos)
            && ModInterface.TryGetInterModValue(singleDateId, "SetGirlCharm", out Action<RelativeId, Sprite> m_SetCharmSprite)
            )
        {
            ModInterface.Log.LogInfo("Successfully found single date interop");

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

            var questions = new List<IGameDefinitionInfo<GirlPairFavQuestionSubDefinition>>();
            for (int i = 1; i <= 20; i++)
            {
                questions.Add(new GirlPairFavQuestionInfo()
                {
                    GirlResponseIndexOne = 1,
                    GirlResponseIndexTwo = 1,
                    QuestionDefinitionID = new RelativeId(-1, i)
                });
            }

            var defaultPhoto = new RelativeId(singleDateId, 0);

            foreach (var girl in Girls.AllHp1)
            {
                ModInterface.AddDataMod(new GirlPairDataMod(girl, InsertStyle.replace)
                {
                    GirlDefinitionOneID = new RelativeId(singleDateId, 0),
                    GirlDefinitionTwoID = girl,
                    SpecialPair = false,
                    PhotoDefinitionID = defaultPhoto,
                    IntroductionPair = false,
                    IntroSidesFlipped = false,
                    HasMeetingStyleOne = false,
                    HasMeetingStyleTwo = false,
                    MeetingLocationDefinitionID = new RelativeId(-1, 1 + (girl.LocalId % 8)),//temp
                    SexDayTime = ClockDaytimeType.NIGHT,
                    SexLocationDefinitionID = new RelativeId(-1, 20),//royal suite
                    IntroRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 0),
                    AttractRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 1),
                    PreSexRelationshipCutsceneDefinitionID = new RelativeId(ModId, 0),//new RelativeId(singleDateId, 2),
                    PostSexRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 3),
                    Styles = defaultPairStyle,
                    FavQuestions = questions
                });
            }
        }
        else
        {
            m_AddGirlDatePhotos = null;
            m_AddGirlSexPhotos = null;
            m_SetCharmSprite = null;
        }

        if (!(this.Config.TryGetEntry<string>("General", "HuniePopDir", out var hpDirConfig)
                && !string.IsNullOrWhiteSpace(hpDirConfig.Value)
                && Directory.Exists(hpDirConfig.Value)))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return;
        }

        var dataDir = Path.Combine(hpDirConfig.Value, _dataDir);
        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop data directory");
            return;
        }

        var assemblyDir = Path.Combine(hpDirConfig.Value, _assemblyDir);
        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop assembly directory");
            return;
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        ModInterface.Log.LogInfo("Loading HuniePop assembly (this may take a bit)");

        using (var hpExtraction = new HpExtraction(hpDirConfig.Value, m_AddGirlDatePhotos, m_AddGirlSexPhotos, m_SetCharmSprite))
        {
            using (ModInterface.Log.MakeIndent("HuniePop assembly loaded successfully, beginning import:"))
            {
                hpExtraction.Extract();
            }
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

        //kyu old photo
        var kyuPhotoDir = Path.Combine(hpDirConfig.Value, _dacDir, "Photos", "Kyu");
        if (Directory.Exists(kyuPhotoDir))
        {
            var censoredTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom1.jpg"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(kyuPhotoDir, "Old Bedroom2.jpg"), true);

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(_modId, PhotoModCount++), InsertStyle.replace)
            {
                BigPhotoCensored = new SpriteInfoTexture(censoredTexture),
                ThumbnailCensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "kyu_old_thumb_censored.png"), new TextureInfoRender(censoredTexture, false, hp1ThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "kyu_old_thumb_wet.png"), new TextureInfoRender(wetTexture, false, hp1ThumbSteps))),
            });
        }

        //audrey 10th photo
        {
            ITextureRenderStep[] audreyThumbSteps = [
                new TextureRsScale(new Vector2(156f/1440f, 112f/1080f)),
                new TextureRsPad(2, whiteCol),
                new TextureRsPad(1),
            ];

            var UncensoredTexture = new TextureInfoExternal(Path.Combine(imagesDir, "hp_10th_anniversary_audrey_dry.png"), true);
            var wetTexture = new TextureInfoExternal(Path.Combine(imagesDir, "hp_10th_anniversary_audrey_wet.png"), true);

            ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(_modId, PhotoModCount++), InsertStyle.replace)
            {
                BigPhotoUncensored = new SpriteInfoTexture(UncensoredTexture),
                ThumbnailUncensored = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "audrey_10th_thumb_uncensored.png"), new TextureInfoRender(UncensoredTexture, false, audreyThumbSteps))),

                BigPhotoWet = new SpriteInfoTexture(wetTexture),
                ThumbnailWet = new SpriteInfoTexture(new TextureInfoCache(Path.Combine(imagesDir, "audrey_10th_thumb_wet.png"), new TextureInfoRender(wetTexture, false, audreyThumbSteps))),
            });
        }

        //favorites
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
        if (!Config.TryGetEntry<bool>(_configGeneralName, _configUnlockPhotosName, out var config)
            || !config.Value)
        {
            return;
        }

        args.UnlockedPhotos ??= new List<PhotoDefinition>();

        for (int i = 0; i < PhotoModCount; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(ModId, i)));
        }
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        if (!Config.TryGetEntry<bool>(_configGeneralName, _configUnlockStylesName, out var config)
            || !config.Value)
        {
            return;
        }

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