// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a LocationDefinition
    /// </summary>
    public class LocationDataMod : DataMod, ILocationDataMod
    {
        public string LocationName;

        public LocationType? LocationType;

        public List<(RelativeId, GirlStyleInfo)> GirlStyles;

        public string NonStopOptionText;

        public List<LocationSpecialLabelSubDefinition> SpecialLabels;

        public IGameDefinitionInfo<AudioKlip> BgMusic;

        public float? BgYOffset;

        public List<IGameDefinitionInfo<Sprite>> Backgrounds;

        public IGameDefinitionInfo<Sprite> FinderLocationIcon;

        public List<IGameDefinitionInfo<LogicBundle>> ArriveBundleList;

        public List<IGameDefinitionInfo<LogicBundle>> DepartBundleList;

        /// <inheritdoc/>
        public LocationDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The local id</param>
        /// <param name="modId">The id of the mod declaring this data. -1 if the base game declared it.</param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public LocationDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="def">A collection of all girl definitions.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        internal LocationDataMod(LocationDefinition def, IEnumerable<GirlDefinition> girls, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            LocationName = def.locationName;
            LocationType = def.locationType;
            BgMusic = new AudioKlipInfo(def.bgMusic, assetProvider);
            BgYOffset = def.bgYOffset;
            FinderLocationIcon = new SpriteInfoPath(def.finderLocationIcon, assetProvider);
            NonStopOptionText = def.nonStopOptionText;
            SpecialLabels = def.specialLabels;
            Backgrounds = def.backgrounds.Select(x => (IGameDefinitionInfo<Sprite>)new SpriteInfoPath(x, assetProvider)).ToList();
            ArriveBundleList = def.arriveBundleList.Select(x => (IGameDefinitionInfo<LogicBundle>)new LogicBundleInfo(x, assetProvider)).ToList();
            DepartBundleList = def.departBundleList.Select(x => (IGameDefinitionInfo<LogicBundle>)new LogicBundleInfo(x, assetProvider)).ToList();

            var styleId = new RelativeId(-1, (int)def.dateGirlStyleType);
            GirlStyles = girls.Select(x => (new RelativeId(x),
                                            new GirlStyleInfo()
                                            {
                                                HairstyleId = styleId,
                                                OutfitId = styleId
                                            })).ToList();
        }

        /// <inheritdoc/>
        public void SetData(LocationDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.bgYOffset, BgYOffset);
            ValidatedSet.SetValue(ref def.locationType, LocationType);

            ValidatedSet.SetValue(ref def.locationName, LocationName, InsertStyle);
            ValidatedSet.SetValue(ref def.nonStopOptionText, NonStopOptionText, InsertStyle);
            ValidatedSet.SetValue(ref def.specialLabels, SpecialLabels, InsertStyle);

            ValidatedSet.SetValue(ref def.bgMusic, BgMusic, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.finderLocationIcon, FinderLocationIcon, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetListValue(ref def.backgrounds, Backgrounds, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.arriveBundleList, ArriveBundleList, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.departBundleList, DepartBundleList, InsertStyle, gameDataProvider, assetProvider);
        }

        public IEnumerable<(RelativeId, GirlStyleInfo)> GetStyles() => GirlStyles;

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests() => Backgrounds.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalSpriteRequests())
            .ConcatNN(BgMusic?.GetInternalSpriteRequests())
            .ConcatNN(FinderLocationIcon?.GetInternalSpriteRequests())
            .ConcatNN(ArriveBundleList?.SelectManyNN(x => x.GetInternalSpriteRequests()))
            .ConcatNN(DepartBundleList?.SelectManyNN(x => x.GetInternalSpriteRequests()));

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests() => Backgrounds.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalAudioRequests())
            .ConcatNN(BgMusic?.GetInternalAudioRequests())
            .ConcatNN(FinderLocationIcon?.GetInternalAudioRequests())
            .ConcatNN(ArriveBundleList?.SelectManyNN(x => x.GetInternalAudioRequests()))
            .ConcatNN(DepartBundleList?.SelectManyNN(x => x.GetInternalAudioRequests()));
    }
}
