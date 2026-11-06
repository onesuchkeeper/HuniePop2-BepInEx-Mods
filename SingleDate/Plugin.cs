using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Entry point for SingleDate, which adds HuniePop 1 style single dates (one girl, no partner) to HuniePop 2.
/// </summary>
/// <remarks>
/// <para>Any pair that has <see cref="GirlNobody"/> (LocalId 0 of this mod) as girl one is treated as a single date
/// pair, with the real girl as girl two. This mod adds a pair like that for each base game girl.</para>
/// <para>The sex location of a single date is chosen from the locations of the girl's sex photos set by
/// <see cref="AddGirlSexPhotos"/>. If the location for the sex photo is <see cref="RelativeId.Default"/>, the sex
/// location of the pair is used instead. If the pair's sex location is <see cref="RelativeId.Default"/> it will be
/// randomized, use <see cref="AddSexLocationBlackList"/> to limit the pool if needed. (maybe add multiple loc support
/// to the base mod? Could be cool)</para>
/// <para>All data mods are added with a priority of zero.</para>
/// <para>Methods marked <see cref="InteropMethodAttribute"/> are the API other mods use to configure their girls
/// for single dates, see the README for how to call them.</para>
/// </remarks>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.1")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal partial class Plugin : Hp2BaseModPlugin
{
    // BepInEx config category that all of this mod's settings are in.
    private const string GENERAL_CONFIG_CAT = "general";
    // Folder of this mod inside the BepInEx plugins folder, holds the asset bundle and the images folder.
    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    // Folder charm images are cached in after being scaled (see UiPrefabs.GetCharmSprite).
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    /// <summary>Config: if upset hints are shown on single dates. NOTE: not currently read by any code in this mod.</summary>
    public static ConfigEntry<bool> ShowSingleUpsetHint => _showSingleUpsetHint;
    private static ConfigEntry<bool> _showSingleUpsetHint;

    /// <summary>Config: if baggage is active on single dates. NOTE: not currently read by any code in this mod.</summary>
    public static ConfigEntry<bool> SingleDateBaggage => _singleDateBaggage;
    private static ConfigEntry<bool> _singleDateBaggage;

    /// <summary>Config: if both girls of a double date pair must be lovers (at max single date level) before a threesome can occur.</summary>
    public static ConfigEntry<bool> RequireLoversBeforeThreesome => _requireLoversBeforeThreesome;
    private static ConfigEntry<bool> _requireLoversBeforeThreesome;

    /// <summary>
    /// Config: the number of single date levels a girl goes through, which is tracked separately from the base game's
    /// relationship level so extra stages can be configured. Reaching the max level means lovers, one below max is
    /// the level where the sex date is available.
    /// </summary>
    public static ConfigEntry<int> MaxSingleGirlRelationshipLevel => _maxSingleGirlRelationshipLevel;
    private static ConfigEntry<int> _maxSingleGirlRelationshipLevel;

    /// <summary>
    /// Config: the highest sensitivity level. Each level takes <see cref="State.ExpPerLevel"/> EXP, and the max level
    /// gives the full reduction of the broken heart affection penalty no matter what this is set to.
    /// </summary>
    public static ConfigEntry<int> MaxSensitivityLevel => _maxSensitivityLevel;
    private static ConfigEntry<int> _maxSensitivityLevel;

    /// <summary>The mod's asset bundle (sprites for the UI, photos, items and credits).</summary>
    public static AssetBundle AssetBundle => _instance._assetBundle;
    private AssetBundle _assetBundle;

    /// <summary>The registry the charm idle animations are built from, see <see cref="RegisterCharmAnimation"/>.</summary>
    public static CharmAnimationRegistry CharmAnimationRegistry => _instance._charmAnimationRegistry;
    private CharmAnimationRegistry _charmAnimationRegistry;

    // Ailments that are never enabled on single dates, mostly because they require switching focus between the two
    // girls, which can't happen with one girl. Other mods can add to this with AddSingleDateDisabledAilment.
    internal static readonly HashSet<RelativeId> _singleDateDisabledAilments = [
        new RelativeId(-1, 28),//Ashley - Commitment Issues - disabled because after 4 turns all matches turn negative
        new RelativeId(-1, 31)//Abia - Self Effacing - disables because doesn't allow gifts at all on single dates
    ];

    // The source id the base mod assigned this plugin, hides Hp2BaseModPlugin.ModId so it can be used statically.
    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    // Per girl single date configuration (photos, charm, cutscenes...), filled by the interop methods.
    private Dictionary<RelativeId, SingleDateGirl> _singleDateGirls = new();
    // Static access to the plugin for the static interop methods.
    private static Plugin _instance;
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    /// <summary>
    /// Sets up the mod: config, data mods (the Nobody girl, cutscenes, items, photos, ailment), the UI assets,
    /// event subscriptions and the Harmony patches.
    /// </summary>
    protected override void Awake()
    {
        _instance = this;
        _charmAnimationRegistry = new();
        base.Awake();

        _assetBundle = AssetBundle.LoadFromFile(Path.Combine(ROOT_DIR, "singledate_assetbundle")); 

        // Add the mod's credits button if the (soft dependency) Tweaks mod is installed.
        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
            out Action<Sprite, IEnumerable<(Sprite creditButtonPath, Sprite creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(_assetBundle.LoadAsset<Sprite>("CreditsLogo"), [
                (
                    _assetBundle.LoadAsset<Sprite>("onesuchkeeper_credits_art"),
                    _assetBundle.LoadAsset<Sprite>("onesuchkeeper_credits_art_over"),
                    "https://linktr.ee/onesuchkeeper"
                ),
            ]);
        }

        // config
        _showSingleUpsetHint = Config.Bind(GENERAL_CONFIG_CAT, nameof(ShowSingleUpsetHint), false, "If upset hints are shown on single dates.");
        _singleDateBaggage = Config.Bind(GENERAL_CONFIG_CAT, nameof(SingleDateBaggage), true, "If baggage is active on single dates.");
        _requireLoversBeforeThreesome = Config.Bind(GENERAL_CONFIG_CAT, nameof(RequireLoversBeforeThreesome), false, "If both characters must reach lovers on single dates before a threesome can occur.");
        _maxSingleGirlRelationshipLevel = Config.Bind(GENERAL_CONFIG_CAT, nameof(MaxSingleGirlRelationshipLevel), 3, "Maximum relationship level for single dates. Maximum level must be reached for lovers status.");
        _maxSensitivityLevel = Config.Bind(GENERAL_CONFIG_CAT, nameof(MaxSensitivityLevel), 4, "Maximum level for sensitivity.");

        // Must be before anything that uses State.ModId.
        State.On_Plugin_Awake();

        // girls
        GirlNobody.AddDataMods();

        // The sensitivity smoothie and the items used to display sensitivity EXP and level.
        ItemSensitivitySmoothie.AddDataMods(_assetBundle);

        // Cutscenes the single pairs use, see CutsceneIds.
        SingleDateMeetingCutscene.AddDataMods();
        SingleDateAttractCutscene.AddDataMods();
        SingleDatePreSexCutscene.AddDataMods();
        SingleDatePostSexCutscene.AddDataMods();
        SingleDateSuccessCutscene.AddDataMods();
        BonusRoundSuccessCutscene.AddDataMods();

        // Sex photos, the default is used by the single pairs and the others are each girl's own.
        AddPhotoMod(Photos.DefaultSex, "default");
        AddPhotoMod(Photos.AbiaSex, "abia");
        AddPhotoMod(Photos.BrookeSex, "brooke");
        AddPhotoMod(Photos.CandaceSex, "candace");
        AddPhotoMod(Photos.LailaniSex, "lailani");
        AddPhotoMod(Photos.LillianSex, "lillian");
        AddPhotoMod(Photos.NoraSex, "nora");
        AddPhotoMod(Photos.PollySex, "polly", true);
        AddPhotoMod(Photos.SarahSex, "sarah");
        AddPhotoMod(Photos.ZoeySex, "zoey");

        // Register the sex photos with each girl, all taking place in the Royal Suite.
        AddGirlSexPhotos(Hp2BaseMod.Girls.Abia, [(Photos.AbiaSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Brooke, [(Photos.BrookeSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Candace, [(Photos.CandaceSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Lailani, [(Photos.LailaniSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Lillian, [(Photos.LillianSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Nora, [(Photos.NoraSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Polly, [(Photos.PollySex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Sarah, [(Photos.SarahSex, Locations.RoyalSuite)]);
        AddGirlSexPhotos(Hp2BaseMod.Girls.Zoey, [(Photos.ZoeySex, Locations.RoyalSuite)]);

        // Load UI sprites and charms.
        UiPrefabs.InitExternals(_assetBundle);

        // Adds the single pairs when data mods are about to be applied.
        ModInterface.Events.PreDataMods += On_PreDataMods;

        // Save/load, the mod's data is stored in the base mod's per-source save string.
        ModInterface.Events.PreGameSave += State.On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += State.On_PostPersistenceReset;
        ModInterface.Events.SaveFileReset += State.On_SaveFileReset;

        // Gameplay events, see ModEventHandles.
        ModInterface.Events.PuzzleRoundOver += ModEventHandles.On_PuzzleRoundOver;
        ModInterface.Events.FinderSlotsPopulate += ModEventHandles.On_FinderSlotsPopulate;
        ModInterface.Events.DateLocationSelected += ModEventHandles.On_DateLocationSelected;
        ModInterface.Events.RequestUnlockedPhotos += ModEventHandles.On_RequestUnlockedPhotos;
        ModInterface.Events.PreDateDollReset += ModEventHandles.On_PreDateDollsRefresh;
        ModInterface.Events.ResolveStatePayload += ModEventHandles.On_ResolveStatePayload;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    /// <summary>
    /// Interop: prevents the ailment from being enabled during single dates. Use this for ailments that can't work
    /// with one girl, for example ones that need the player to switch focus between girls.
    /// </summary>
    /// <param name="ailmentId">The id of the ailment to disable on single dates.</param>
    [InteropMethod]
    public static void AddSingleDateDisabledAilment(RelativeId ailmentId)
        => _singleDateDisabledAilments.Add(ailmentId);

    /// <summary>
    /// Interop: adds sex photos for a girl. When the sex date occurs, one of the girl's photos is shown, preferring ones
    /// she hasn't unlocked yet and ones that match the location of the date. The photo's location is also where the sex
    /// date will take place.
    /// </summary>
    /// <param name="girlId">The girl the photos belong to.</param>
    /// <param name="photoIds">Pairs of (photo id, location id). The location can be <see cref="RelativeId.Default"/> to use the pair's sex location.</param>
    [InteropMethod]
    public static void AddGirlSexPhotos(RelativeId girlId, IEnumerable<(RelativeId, RelativeId)> photoIds)
        => _instance._singleDateGirls.GetOrNew(girlId).SexPhotos.AddRange(photoIds.Select(x => new SexPhotoData() { PhotoId = x.Item1, LocationId = x.Item2 }));

    /// <summary>
    /// Interop: adds photos for a girl that can be shown after a successful single date. A photo can be shown once the
    /// girl's single date level, as a fraction of <see cref="MaxSingleGirlRelationshipLevel"/>, has reached its percentage.
    /// </summary>
    /// <param name="girlId">The girl the photos belong to.</param>
    /// <param name="photoIds">Pairs of (photo id, relationship percentage from 0 to 1 the photo unlocks at).</param>
    [InteropMethod]
    public static void AddGirlDatePhotos(RelativeId girlId, IEnumerable<(RelativeId, float)> photoIds)
            => _instance._singleDateGirls.GetOrNew(girlId).DatePhotos.AddRange(photoIds.Select(x => new DatePhotoData() { PhotoId = x.Item1, RelationshipPercentage = x.Item2 }));

    /// <summary>
    /// Interop: adds locations that should never be randomly chosen for the girl's sex date.
    /// Only used when no sex photo location or pair sex location is set.
    /// </summary>
    /// <param name="girlId">The girl the blacklist belongs to.</param>
    /// <param name="photoIds">The ids of the locations to blacklist. (Named photoIds historically, these are location ids.)</param>
    [InteropMethod]
    public static void AddSexLocationBlackList(RelativeId girlId, IEnumerable<RelativeId> photoIds)
    {
        var singleDateGirl = _instance._singleDateGirls.GetOrNew(girlId);
        singleDateGirl.SexLocBlackList.UnionWith(photoIds);
    }

    /// <summary>
    /// Interop: sets the cutscene that plays when a single date succeeds and the girl levels up to attracted.
/// NOTE: the value is stored in <see cref="SingleDateGirl.CutsceneSuccessAttracted"/> but nothing reads it yet.
    /// </summary>
    /// <param name="girlId">The girl the cutscene is for.</param>
    /// <param name="CutsceneId">The id of the cutscene.</param>
    [InteropMethod]
    public static void SetCutsceneSuccessAttracted(RelativeId girlId, RelativeId CutsceneId)
        => _instance._singleDateGirls.GetOrNew(girlId).CutsceneSuccessAttracted = CutsceneId;

    /// <summary>
    /// Interop: sets the cutscene that plays when the sex date (bonus round) succeeds.
    /// NOTE: the value is stored in <see cref="SingleDateGirl.CutsceneSuccessBonus"/> but nothing reads it yet.
    /// </summary>
    /// <param name="girlId">The girl the cutscene is for.</param>
    /// <param name="CutsceneId">The id of the cutscene.</param>
    [InteropMethod]
    public static void SetBonusRoundSuccessCutscene(RelativeId girlId, RelativeId CutsceneId)
        => _instance._singleDateGirls.GetOrNew(girlId).CutsceneSuccessBonus = CutsceneId;

    /// <summary>
    /// Interop: swaps all of the single date configuration of two girls with each other. Intended for randomizer mods
    /// that swap which girl is which.
    /// </summary>
    /// <param name="girlIdA">The first girl.</param>
    /// <param name="girlIdB">The second girl.</param>
    [InteropMethod]
    public static void SwapGirls(RelativeId girlIdA, RelativeId girlIdB)
    {
        var aData = _instance._singleDateGirls.GetOrNew(girlIdA);
        var bData = _instance._singleDateGirls.GetOrNew(girlIdB);

        _instance._singleDateGirls[girlIdA] = bData;
        _instance._singleDateGirls[girlIdB] = aData;
    }

    /// <summary>
    /// Interop: sets the charm sprite shown for the girl in the single date status app.
    /// Girls with no charm set use the default charm.
    /// </summary>
    /// <param name="girlId">The girl the charm is for.</param>
    /// <param name="charmSprite">The sprite to display.</param>
    [InteropMethod]
    public static void SetGirlCharm(RelativeId girlId, Sprite charmSprite)
        => _instance._singleDateGirls.GetOrNew(girlId).CharmSprite = charmSprite;

    /// <summary>
    /// Interop: if the girl is at the level where the sex date is available, one level below
    /// <see cref="MaxSingleGirlRelationshipLevel"/>.
    /// </summary>
    /// <param name="girlId">The girl to check.</param>
    [InteropMethod]
    public static bool IsSexDateValid(RelativeId girlId) => State.SaveFile.GetGirl(girlId).RelationshipLevel >= (MaxSingleGirlRelationshipLevel.Value - 1);

    /// <summary>
    /// Interop: creates a cutscene step that shows one of the girl's sex photos. For use in other mods' cutscenes.
    /// </summary>
    [InteropMethod]
    public static IGameDefinitionInfo<CutsceneStepSubDefinition> MakeSexPhotoCutsceneStep() => new ShowSexPhotoCutsceneStep.Info();

    /// <summary>
    /// Interop: creates a cutscene step that shows one of the girl's date photos, those that have unlocked at the girl's
    /// current level. For use in other mods' cutscenes.
    /// </summary>
    [InteropMethod]
    public static IGameDefinitionInfo<CutsceneStepSubDefinition> MakeDatePhotoCutsceneStep() => new ShowDatePhotoCutsceneStep.Info();

    /// <summary>
    /// Interop: adds an idle animation to the pool the charm in the single date status app picks from.
    /// </summary>
    /// <param name="weight">How likely the animation is picked relative to the others in the pool.</param>
    /// <param name="cost">How much of the sequence's time budget (10 seconds, see
    /// <c>ExpandedUiCellphoneAppStatus.MakeCharmSequence</c>) the animation uses. This is an arbitrary value that is
    /// tuned by feel against the animation's actual duration.</param>
    /// <param name="build">Appends the animation to the sequence. Receives the charm's transform, whether it currently faces
    /// the positive direction, and the girl's id. Return true to flip the direction for the animations after it.</param>
    /// <param name="allowedCharacters">The girls the animation is limited to, or null for all girls.</param>
    [InteropMethod]
        public void RegisterCharmAnimation(
        float weight, 
        float cost,
        Func<Sequence, (RectTransform transform, bool dir, RelativeId girlId), bool> build,
        HashSet<RelativeId> allowedCharacters) 
            => _charmAnimationRegistry.Register(weight, cost, build, allowedCharacters);

    /// <summary>Gets the girl's single date configuration, creating an empty one if she doesn't have any yet.</summary>
    public static SingleDateGirl GetSingleDateGirl(RelativeId girlId)
            => _instance._singleDateGirls.GetOrNew(girlId);

    /// <summary>Gets the girl's single date configuration if any has been set for her, without creating one.</summary>
    public static bool TryGetSingleDateGirl(RelativeId girlId, out SingleDateGirl singleDateGirl)
        => _instance._singleDateGirls.TryGetValue(girlId, out singleDateGirl);

    /// <summary>
    /// Runs once just before data mods are applied (<see cref="ModEvents.PreDataMods"/>). Adds the single date pairs
    /// and registers the single date upset girl state.
    /// </summary>
    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        // Styles: RelativeId.Default means the girl's default hairstyle/outfit.
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

        // One single pair for each of the 12 base game girls (local ids 1-12 from source -1, the base game). Girl one is
        // Nobody, which is what makes it a single pair, and girl two is the real girl.
        // base game girl pairs
        for (int i = 1; i < 13; i++)
        {
            var mod = new GirlPairDataMod(new RelativeId(State.ModId, i), InsertStyle.replace)
            {
                GirlDefinitionOneID = Girls.Nobody,
                GirlDefinitionTwoID = new RelativeId(-1, i),
                SpecialPair = false,
                PhotoDefinitionID = Photos.DefaultSex,
                IntroductionPair = false,
                IntroSidesFlipped = false,
                HasMeetingStyleOne = false,
                HasMeetingStyleTwo = false,
                // Spread the first meeting locations over the base game location ids 1-8 so all the girls don't meet at the same place.
                MeetingLocationDefinitionID = new RelativeId(-1, 1 + (i % 8)),
                // The sex date is at night in the Royal Suite unless a girl's sex photos say otherwise (see AddGirlSexPhotos).
                SexDayTime = ClockDaytimeType.NIGHT,
                SexLocationDefinitionID = Locations.RoyalSuite,
                IntroRelationshipCutsceneDefinitionID = CutsceneIds.Meeting,
                AttractRelationshipCutsceneDefinitionID = CutsceneIds.Attract,
                PreSexRelationshipCutsceneDefinitionID = CutsceneIds.PreSex,
                PostSexRelationshipCutsceneDefinitionID = CutsceneIds.PostSex,
                SuccessCutsceneDefinitionID = CutsceneIds.Success,
                BonusSuccessCutsceneDefinitionID = CutsceneIds.BonusSuccess,
                // Pairs use the default outfit and hairstyle in every situation unless another mod changes them.
                Styles = defaultPairStyle
            };

            ModInterface.DataMod.AddDataMod(mod);
        }

        // states
        ModInterface.DataMod.AddData(PuzzleStatusGirlStateId.SingleDateUpset, new SingleDateUpsetGirlState());

        // Shows sensitivity EXP in the cellphone's EXP display.
        ModInterface.DataMod.AddExp(new SensitivityExp());

        // states
        ModInterface.DataMod.AddData(PuzzleStateIds.SingleDatePuzzle, new SingleDatePuzzleState.Factory());
        ModInterface.DataMod.AddData(GameStateIds.SingleDateSim, new SingleDateSim.Factory());
    }

    /// <summary>
    /// Adds a photo data mod from the asset bundle. The photo sprite is named photo_{name}_1 and its thumbnail
    /// photo_{name}_1_thumb. With <paramref name="hasAlts"/> the alternate photo (photo_{name}_1_alt and
    /// photo_{name}_1_alt_thumb) is added as well.
    /// </summary>
    /// <param name="id">The id the photo is added as.</param>
    /// <param name="name">The name used in the sprite names.</param>
    /// <param name="hasAlts">If the photo has an alternate version.</param>
    private void AddPhotoMod(RelativeId id, string name, bool hasAlts = false)
    {
        if (!(
                TryGetSprite($"photo_{name}_1", out var photoSprite)
                && TryGetSprite($"photo_{name}_1_thumb", out var photoThumbSprite)
            ))
        {
            ModInterface.Log.Warning($"Failed to get photo for {name}");
            return;
        }

        var photoMod = new PhotoDataMod(id, InsertStyle.replace)
        {
            HasAlts = false,

            //BigPhotoCensored = photoInfo,
            BigPhotoUncensored = new SpriteInfoSprite(photoSprite),
            //BigPhotoWet = photoInfo,

            //ThumbnailCensored = thumbInfo,
            ThumbnailUncensored = new SpriteInfoSprite(photoThumbSprite),
            //ThumbnailWet = thumbInfo,
        };

        ModInterface.DataMod.AddDataMod(photoMod);

        if (hasAlts)
        {
            if (!(
                    TryGetSprite($"photo_{name}_1_alt", out var photoSpriteAlt)
                    && TryGetSprite($"photo_{name}_1_alt_thumb", out var photoThumbSpriteAlt)
                ))
            {
                ModInterface.Log.Warning($"Failed to get alt photo for {name}");
                return;
            }

            photoMod.HasAlts = true;
            photoMod.BigPhotoUncensoredAlt = new SpriteInfoSprite(photoSpriteAlt);
            photoMod.ThumbnailUncensoredAlt = new SpriteInfoSprite(photoThumbSpriteAlt);
        }
    }

    /// <summary>Loads a sprite from the asset bundle, returns false if it isn't there.</summary>
    private bool TryGetSprite(string assetName, out Sprite sprite)
    {
        sprite = _assetBundle.LoadAsset<Sprite>(assetName);
        return sprite != null;
    }
}
