// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a LocationDefinition
    /// </summary>
    public class LocationDataMod : DataMod, IGameDataMod<LocationDefinition>
    {
        public string LocationName;

        public LocationType? LocationType;

        public string NonStopOptionText;

        public List<LocationSpecialLabelSubDefinition> SpecialLabels;

        public IGameDefinitionInfo<AudioKlip> BgMusic;

        public float? BgYOffset;

        public List<IGameDefinitionInfo<Sprite>> Backgrounds;

        public IGameDefinitionInfo<Sprite> FinderLocationIcon;

        public List<IGameDefinitionInfo<LogicBundle>> ArriveBundleList;

        public List<IGameDefinitionInfo<LogicBundle>> DepartBundleList;

        public List<ClockDaytimeType> DateTimes;
        public bool? AllowNonStop;
        public bool? AllowNormal;
        public bool? PostBoss;
        public RelativeId? DefaultStyle;

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

            var expansion = def.Expansion();

            ValidatedSet.SetValue(ref expansion.AllowNormal, AllowNormal);
            ValidatedSet.SetValue(ref expansion.AllowNonStop, AllowNonStop);
            ValidatedSet.SetValue(ref expansion.PostBoss, PostBoss);
            ValidatedSet.SetListValue(ref expansion.DateTimes, DateTimes, InsertStyle);
            ValidatedSet.SetValue(ref expansion.DefaultStyle, DefaultStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Backgrounds?.ForEach(x => x?.RequestInternals(assetProvider));
            ArriveBundleList?.ForEach(x => x?.RequestInternals(assetProvider));
            DepartBundleList?.ForEach(x => x?.RequestInternals(assetProvider));
            BgMusic?.RequestInternals(assetProvider);
            FinderLocationIcon?.RequestInternals(assetProvider);
        }
    }
}
