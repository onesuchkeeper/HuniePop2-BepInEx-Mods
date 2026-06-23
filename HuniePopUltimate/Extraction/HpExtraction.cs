using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;

namespace HuniePopUltimate;

public class HpExtraction : BaseExtraction
{
    public const string DATA_DIR = "HuniePop_Data";
    public static readonly string ASSEMBLY_DIR = Path.Combine(DATA_DIR, "Managed");

    public IReadOnlyDictionary<RelativeId, SingleDatePairData> SingleDatePairData => _singleDatePairData;

    private readonly Dictionary<RelativeId, SingleDatePairData> _singleDatePairData = new();
    private readonly Dictionary<int, IGirlConfigurator> _hpGirlConfigs;
    private readonly Action<RelativeId, IEnumerable<(RelativeId, float)>> _addGirlDatePhotos;
    private readonly Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> _addGirlSexPhotos;
    private readonly IBodySubDataMod<GirlPartSubDefinition> _nudeOutfitPart;
    private readonly UnityEngine.AssetBundle _assetBundle;

    public HpExtraction(
        string dir,
        Action<RelativeId, IEnumerable<(RelativeId, float)>> addGirlDatePhotos,
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, UnityEngine.Sprite> setCharmSprite,
        IBodySubDataMod<GirlPartSubDefinition> nudeOutfitPart,
        UnityEngine.AssetBundle assetBundle)
        : base(Path.Combine(dir, DATA_DIR), Path.Combine(dir, ASSEMBLY_DIR))
    {
        _addGirlDatePhotos = addGirlDatePhotos;
        _addGirlSexPhotos = addGirlSexPhotos;
        _nudeOutfitPart = nudeOutfitPart;
        _assetBundle = assetBundle;

        _hpGirlConfigs = new()
        {
            {1, new TiffanyConfigurator(addGirlSexPhotos, setCharmSprite)},
            {2, new AikoConfigurator(addGirlSexPhotos, setCharmSprite)},
            {3, new KyannaConfigurator(addGirlSexPhotos, setCharmSprite)},
            {4, new AudreyConfigurator(addGirlSexPhotos, setCharmSprite)},
            {5, new LolaConfigurator()},
            {6, new NikkiConfigurator(addGirlSexPhotos, setCharmSprite)},
            {7, new JessieConfigurator()},
            {8, new BeliConfigurator(addGirlSexPhotos, setCharmSprite)},
            {9, new KyuConfigurator(addGirlSexPhotos, setCharmSprite)},
            {10, new MomoConfigurator(addGirlSexPhotos, setCharmSprite)},
            {11, new CelesteConfigurator(addGirlSexPhotos, setCharmSprite)},
            {12, new VenusConfigurator(addGirlSexPhotos, setCharmSprite)},
        };
    }

    public void Extract(string audioCacheFilePath)
    {
        var audioCache = new HpAudioCache(audioCacheFilePath, _extractor.SerializedFiles);
        var spriteCache = new HpSpriteCache(_extractor);

        var collectionData = new Dictionary<string, (SerializedFile, OrderedDictionary)>();
        foreach (var file_behaviorList in _extractor.ExtractMonoBehaviors("tk2dSpriteCollectionData"))
        {
            foreach (var behavior in file_behaviorList.Value)
            {
                if (behavior.TryGetValue("spriteCollectionName", out string name))
                {
                    collectionData[name] = (file_behaviorList.Key, behavior);
                }
            }
        }

        var parts = new HpPartsExtractor(spriteCache);

        var dialog = new HpDialogExtractor(audioCache, _hpGirlConfigs);

        var cutscenes = new HpCutsceneExtractor(dialog, _hpGirlConfigs, _extractor);

        var thumbnailCollection = collectionData["AllPhotoIconsSpriteCollection"];

        var itemCache = new HpItemCache(_extractor, spriteCache);

        var itemFile = _extractor.SerializedFiles.FirstOrDefault(x => x.fileName == "sharedassets0.assets");

        if (itemFile != null
            && collectionData.TryGetValue("ItemIconsSpriteCollection", out var itemIconSpriteCollection))
        {
            itemCache.Extract(itemFile, _hpGirlConfigs.Values.SelectMany(x => x.ExtractItemIds).Concat([(Items.WeirdThing, 9167)]), itemIconSpriteCollection);
        }
        else
        {
            ModInterface.Log.Warning("Failed to find sharedassets0.assets");
        }

        if (itemCache.Mods.TryGetValue(Items.WeirdThing, out var weirdThing))
        {
            ConfigureWeirdThing(weirdThing);
        }
        else
        {
            ModInterface.Log.Warning("Failed to configure 'Weird Thing'");
        }

        var girls = new HpGirlExtractor(
            _extractor,
            parts,
            spriteCache,
            audioCache,
            itemCache,
            dialog,
            cutscenes,
            _hpGirlConfigs,
            _nudeOutfitPart,
            _assetBundle,
            _singleDatePairData,
            _addGirlDatePhotos,
            _addGirlSexPhotos,
            thumbnailCollection);

        var locations = new HpLocationExtractor(spriteCache, audioCache, _assetBundle);

        foreach (var file_dialogTriggers in _extractor.ExtractMonoBehaviors("DialogTriggerDefinition"))
        {
            foreach (var dtDef in file_dialogTriggers.Value)
            {
                dialog.ExtractDialogTrigger(dtDef, file_dialogTriggers.Key);
            }
        }

        if (Plugin.PConfig.AddCharacters.Value)
        {
            foreach (var file_girlDefs in _extractor.ExtractMonoBehaviors("GirlDefinition"))
            {
                foreach (var girlDef in file_girlDefs.Value)
                {
                    girls.ExtractGirl(file_girlDefs.Key, girlDef, collectionData);
                }
            }
        }

        foreach (var file_locationDef in _extractor.ExtractMonoBehaviors("LocationDefinition"))
        {
            foreach (var locationDef in file_locationDef.Value)
            {
                locations.ExtractLocation(file_locationDef.Key, locationDef, collectionData);
            }
        }

        AddHp2GirlMods();

        spriteCache.FinalizeTextures();
    }

    public void AddHp2GirlMods()
    {
        foreach (var girl in Hp2BaseMod.Girls.NormalGirls.Append(Hp2BaseMod.Girls.Kyu))
        {
            var body = new GirlBodyDataMod(new RelativeId(-1, 0), Hp2BaseMod.Utility.InsertStyle.append)
            {
                LocationIdToStyleInfo = new Dictionary<RelativeId, GirlStyleInfo>() {
                    {LocationIds.FarmersMarket, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.Casino, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Party, OutfitId = Hp2BaseMod.Styles.Party }},
                    {LocationIds.OutdoorLounge, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Romantic, OutfitId = Hp2BaseMod.Styles.Romantic }},
                    {LocationIds.BotanicalGarden, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.HotSprings, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Water, OutfitId = Hp2BaseMod.Styles.Water }},
                    {LocationIds.HikingTrail, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.IceRink, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.WaterPark, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Water, OutfitId = Hp2BaseMod.Styles.Water }},
                    {LocationIds.TennisCourts, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Activity, OutfitId = Hp2BaseMod.Styles.Activity }},
                    {LocationIds.ScenicOverlook, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Relaxing, OutfitId = Hp2BaseMod.Styles.Relaxing }},
                    {LocationIds.Restaurant, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Romantic, OutfitId = Hp2BaseMod.Styles.Romantic }},
                    {LocationIds.BedRoomDate, new GirlStyleInfo() { HairstyleId = Hp2BaseMod.Styles.Sexy, OutfitId = Hp2BaseMod.Styles.Sexy }},
                    {LocationIds.Carnival, new GirlStyleInfo(Hp2BaseMod.Styles.Activity)},
                }
            };

            if (girl == Hp2BaseMod.Girls.Lola)
            {
                body.LocationIdToStyleInfo[LocationIds.TennisCourts] = new GirlStyleInfo(Hp2BaseMod.Styles.Bonus2);
            }

            ModInterface.AddDataMod(new GirlDataMod(girl, Hp2BaseMod.Utility.InsertStyle.append)
            {
                bodies = new(){
                    body
                }
            });
        }
    }

    public void ConfigureWeirdThing(ItemDataMod mod)
    {
        mod.StoreCost = 30;
        mod.AffectionType = PuzzleAffectionType.TALENT;
        mod.ItemType = ItemType.MISC;
        mod.CategoryDescription = "Special Item";
        mod.TooltipColorIndex = 0;
        mod.StoreSectionPreference = true;
        ModInterface.AddDataMod(mod);
    }
}