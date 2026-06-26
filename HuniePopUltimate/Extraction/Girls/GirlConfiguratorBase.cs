using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public abstract class GirlConfiguratorBase : IGirlConfigurator
{
    protected abstract string UniqueCategoryDescription {get;}
    protected abstract string ShoeCategoryDescription {get;}
    protected virtual bool HasUiSprites => true;
    protected abstract IEnumerable<(RelativeId, int, string name, string description)> BaggageItemIds {get;}
    protected abstract IEnumerable<(RelativeId id, string name, string description)> ShoeItems {get;}
    protected abstract IEnumerable<(RelativeId, int)> UniqueItemIds {get;}
    protected abstract IEnumerable<(RelativeId LocationId, GirlStyleInfo Style)> LocationStyleMap {get;}
    protected virtual bool HasDrinkRejectLines => true;
    protected abstract IEnumerable<(RelativeId QuestionId, RelativeId AnswerId)> FavAnswersMap {get;}
    protected abstract (int x, int y) BackPosition {get;}
    protected abstract (int x, int y) HeadPosition {get;}
    protected abstract IEnumerable<(RelativeId to, RelativeId from)> LocationGreetingMap {get;}
    protected abstract int Age {get;}

    /// <summary>
    /// Order the favorite questions are encoded in hp1
    /// </summary>
    public abstract IEnumerable<RelativeId> FavQuestionOrder {get;}

    /// <summary>
    /// Id of the energy used for items
    /// </summary>
    protected abstract RelativeId ItemEnergyId {get;}

    /// <summary>
    /// The id of the girl
    /// </summary>
    protected abstract RelativeId GirlId {get;}

    /// <summary>
    /// The name of the girl used in common asset file naming
    /// </summary>
    protected abstract string GirlAssetFileName {get;}

    /// <inheritdoc/>
    public virtual string UnderwearName => "Underwear";

    /// <inheritdoc/>
    public virtual (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence => null;
    private static readonly (RelativeId outfit, RelativeId hairstyle)[] _meetingCutsceneStyleSequence = [
        (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity),
        (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity),
        (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity),
    ];

    /// <inheritdoc/>
    public virtual IEnumerable<(RelativeId, int)> ExtractItemIds => UniqueItemIds.Concat(BaggageItemIds.Select(x => (x.Item1, x.Item2)));

    /// <inheritdoc/>
    public GirlDataMod Mod => _mod;
    private GirlDataMod _mod;

    /// <inheritdoc/>
    public virtual (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes => (0,2,3);

    private readonly Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> _addGirlSexPhotos;
    private readonly Action<RelativeId, Sprite> _setCharmSprite;

    protected GirlConfiguratorBase(
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, Sprite> setCharmSprite)
    {
        _mod = new GirlDataMod(GirlId, InsertStyle.append)
        {
            BaggageItemDefIDs = new(),
            UniqueItemDefIDs = new(),
            ShoesItemDefIDs = new()
        };

        ModInterface.AddDataMod(_mod);
        _addGirlSexPhotos = addGirlSexPhotos;
        _setCharmSprite = setCharmSprite;
    }

    /// <inheritdoc/>
    public virtual bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId)
    {
        if (dialogTriggerId == DialogTriggers.LovesAccept)
        {
            cleanedDialogTriggerId = default;
            return false;
        }

        cleanedDialogTriggerId = dialogTriggerId;
        return true;
    }

    /// <inheritdoc/>
    public virtual void ConfigureGirl(
        GirlBodyDataMod hpBody,
        AssetBundle assetBundle,
        HpSpriteCache sprites,
        HpAudioCache audio,
        HpItemCache items)
    {
        Mod.GirlAge = Age;

        var lines = Mod.LocationGreetingDialogLines;
        foreach (var (to,from) in LocationGreetingMap)
        {
            lines[to] = lines[from];
        }

        if (HasUiSprites) SetCellphoneImages(assetBundle);

        hpBody.BackPosition = new VectorInfo(BackPosition.x, BackPosition.y);
        hpBody.HeadPosition = new VectorInfo(HeadPosition.x, HeadPosition.y);

        Mod.FavAnswers ??= new();
        var favAnswers = Mod.FavAnswers;
        foreach (var (questionId, answerId) in FavAnswersMap)
        {
            favAnswers[questionId] = answerId;
        }

        if (!HasDrinkRejectLines)
        {
            RemapSmoothieRejectionLines();
        }

        hpBody.LocationIdToStyleInfo ??= new Dictionary<RelativeId, GirlStyleInfo>();
        var locationIdToStyleInfo = hpBody.LocationIdToStyleInfo;
        foreach (var (locationId, style) in LocationStyleMap)
        {
            locationIdToStyleInfo[locationId] = style;
        }

        foreach ((RelativeId modId, int nativeId) in UniqueItemIds)
        {
            if (!items.Mods.TryGetValue(modId, out var mod))
            {
                throw new Exception($"Unique item {modId} wasn't found in cache");
            }

            ConfigureUniqueMod(mod);
        }

        foreach ((RelativeId modId, int nativeId, string name, string description) in BaggageItemIds)
        {
            if (!items.Mods.TryGetValue(modId, out var mod))
            {
                throw new Exception($"Baggage item {modId} wasn't found in cache");
            }

            ConfigureBaggageItemMod(mod, name, description);
        }

        int i = 1;
        foreach ((RelativeId modId, string name, string description) in ShoeItems)
        {
            AddShoeMod(assetBundle, modId, name, description, i++);
        }
    }

    /// <summary>
    /// Loads and assigns all cellphone UI sprites for a girl, using the
    /// conventional asset naming scheme (<c>ui_girl_portrait_{name}</c> etc.).
    /// Also fires the charm sprite callback.
    /// </summary>
    protected void SetCellphoneImages(AssetBundle assetBundle)
    {
        _mod.CellphonePortrait = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>($"ui_girl_portrait_{GirlAssetFileName}"));
        _mod.CellphoneHead = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>($"{GirlAssetFileName}_cellphoneHead"));
        _mod.CellphoneMiniHead = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>($"{GirlAssetFileName}_cellphoneHeadMini"));
        _setCharmSprite?.Invoke(_mod.Id, assetBundle.LoadAsset<Sprite>($"charm_{GirlAssetFileName}"));
    }

    /// <summary>
    /// Registers additional sex photos for a girl beyond those extracted from
    /// the HP1 data (e.g. 10th anniversary photos).
    /// </summary>
    protected void AddSexPhotos(IEnumerable<(RelativeId, RelativeId)> photos)
        => _addGirlSexPhotos?.Invoke(GirlId, photos);

    private void AddShoeMod(AssetBundle assetBundle,
        RelativeId itemId, 
        string itemName,
        string itemDescription,
        int index)
    {
        ModInterface.AddDataMod(new ItemDataMod(itemId, InsertStyle.append)
        {
            ItemName = itemName,
            CategoryDescription = ShoeCategoryDescription,
            ItemDescription = itemDescription,
            ItemType = ItemType.SHOES,
            EnergyDefinitionID = ItemEnergyId,
            GirlDefinitionID = GirlId,
            ItemSpriteInfo = new SpriteInfoSprite(assetBundle.LoadAsset<Sprite>($"{GirlAssetFileName}_shoes_{index}")),
            StoreCost = 4,
            StoreSectionPreference = true,
            AffectionType = _mod.FavoriteAffectionType
        });

        _mod.ShoesItemDefIDs.Add(itemId);
    }

    private void ConfigureUniqueMod(
        ItemDataMod mod)
    {
        mod.ItemType = ItemType.UNIQUE_GIFT;
        mod.EnergyDefinitionID = ItemEnergyId;
        mod.GirlDefinitionID = GirlId;
        mod.StoreCost = 4;
        mod.ItemDescription = mod.ItemDescription.Replace(" Earn an additional +50% [[Hunie]hunie] while talking with her.", "") + " +1 [[passion]@Passion] EXP.";
        mod.CategoryDescription = UniqueCategoryDescription;
        mod.StoreSectionPreference = true;
        mod.AffectionType = _mod.FavoriteAffectionType;

        _mod.UniqueItemDefIDs.Add(mod.Id);
    }

    private void ConfigureBaggageItemMod(ItemDataMod mod, string name, string description)
    {
        mod.ItemType = ItemType.BAGGAGE;
        mod.EnergyDefinitionID = ItemEnergyId;
        mod.GirlDefinitionID = GirlId;
        mod.CutsceneDefinitionID = mod.Id;//Just using the same Id for all 3 for now
        mod.AilmentDefinitionID = mod.Id;
        mod.ItemName = name;
        mod.ItemDescription = description;

        _mod.BaggageItemDefIDs.Add(mod.Id);
    }

    /// <inheritdoc/>
    public abstract bool IsPhotoIndexNsfw(int photoIndex);

    /// <summary>
    /// Some girls drink during the day and have no drink rejection line.
    /// Rejection lines move to accept, food rejection covers the gap.
    /// </summary>
    private void RemapSmoothieRejectionLines()
    {
        var rejectSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieReject];
        var acceptSmoothie = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.SmoothieAccept];
        var rejectFood = _mod.LinesByDialogTriggerId[Hp2BaseMod.DialogTriggers.FoodReject];
        acceptSmoothie.AddRange(rejectSmoothie);
        rejectSmoothie.Clear();
        rejectSmoothie.AddRange(rejectFood);
    }
}