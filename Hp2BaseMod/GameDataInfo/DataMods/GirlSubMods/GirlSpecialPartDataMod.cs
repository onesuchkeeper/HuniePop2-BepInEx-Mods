// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlPartSubDefinition"/>.
    /// </summary>
    public class GirlSpecialPartDataMod : DataMod, IBodySubDataMod<GirlSpecialPartSubDefinition>
    {
        public string SpecialPartName;

        public IBodySubDataMod<GirlPartSubDefinition> Part;

        public DollPartSpecialAnimType? AnimType;

        public GirlPartType? SortingPartType;

        public bool? IsToggleable;

        /// <summary>
        /// Hairstyles required to be present for part to be shown. If empty will show for all styles
        /// </summary>
        public List<RelativeId> RequiredHairstyles;

        public GirlSpecialPartDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal GirlSpecialPartDataMod(int index,
            GirlDefinition girlDef,
            AssetProvider assetProvider)
            : base(new RelativeId(-1, index), InsertStyle.replace, 0)
        {
            var specialPartDef = girlDef.specialParts[index];

            Part = new GirlPartDataMod(specialPartDef.partIndexSpecial, assetProvider, girlDef);
            AnimType = specialPartDef.animType;
            SortingPartType = specialPartDef.sortingPartType;
            SpecialPartName = specialPartDef.specialPartName;
            IsToggleable = true;
        }

        /// <inheritdoc/>
        public void SetData(GirlSpecialPartSubDefinition def,
                            GameDefinitionProvider gameDataProvider,
                            AssetProvider assetProvider,
                            RelativeId girlId,
                            GirlBodySubDefinition bodyDef)
        {
            if (def == null) { return; }

            var expansion = def.Expansion();

            ValidatedSet.SetValue(ref expansion.RequiredHairstyles, RequiredHairstyles, InsertStyle);

            ValidatedSet.SetValue(ref def.sortingPartType, SortingPartType);
            ValidatedSet.SetValue(ref def.animType, AnimType);

            ValidatedSet.SetValue(ref def.partIndexSpecial, bodyDef.PartIdToIndex, Part?.Id);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Part?.RequestInternals(assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return Part;
        }
    }
}
