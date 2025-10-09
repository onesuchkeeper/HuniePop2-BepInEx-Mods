// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlPartSubDefinition"/>.
    /// </summary>
    public class GirlSpecialPartDataMod : DataMod, IGirlSubDataMod<GirlSpecialPartSubDefinition>
    {
        public string SpecialPartName;

        public IGirlSubDataMod<GirlPartSubDefinition> Part;

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
        public void SetData(ref GirlSpecialPartSubDefinition def,
                            GameDefinitionProvider gameDataProvider,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId,
                            GirlDefinition girlDef)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<GirlSpecialPartSubDefinition>();
            }

            var expansion = def.Expansion();

            ValidatedSet.SetValue(ref expansion.RequiredHairstyles, RequiredHairstyles, insertStyle);

            ValidatedSet.SetValue(ref def.sortingPartType, SortingPartType);
            ValidatedSet.SetValue(ref def.animType, AnimType);

            var girlExpansion = girlDef.Expansion();
            ValidatedSet.SetValue(ref def.partIndexSpecial, girlExpansion.PartIdToIndex, Part?.Id);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Part?.RequestInternals(assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return Part;
        }
    }
}
