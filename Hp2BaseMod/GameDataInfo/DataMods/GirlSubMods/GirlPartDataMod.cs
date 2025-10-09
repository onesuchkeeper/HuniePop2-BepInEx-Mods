// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlPartSubDefinition"/>.
    /// </summary>
    public class GirlPartDataMod : DataMod, IGirlSubDataMod<GirlPartSubDefinition>
    {
        public GirlPartType? PartType;

        public string PartName;

        public int? X;

        public int? Y;

        public IGirlSubDataMod<GirlPartSubDefinition> MirroredPart;

        public IGirlSubDataMod<GirlPartSubDefinition> AltPart;

        public IGameDefinitionInfo<Sprite> SpriteInfo;

        /// <inheritdoc/>
        public GirlPartDataMod() { }

        public GirlPartDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal GirlPartDataMod(int index, AssetProvider assetProvider, GirlDefinition girlDef)
            : base(new RelativeId(-1, index), InsertStyle.replace, 0)
        {
            var partDef = girlDef.parts[index];

            PartType = partDef.partType;
            PartName = partDef.partName;
            X = partDef.x;
            Y = partDef.y;

            if (partDef.mirroredPartIndex != -1)
            {
                MirroredPart = new GirlPartDataMod(partDef.mirroredPartIndex, assetProvider, girlDef);
            }

            if (partDef.altPartIndex != -1)
            {
                AltPart = new GirlPartDataMod(partDef.altPartIndex, assetProvider, girlDef);
            }

            // Special handling, prefixes id because all the part sprites of one type have the same name
            if (partDef.sprite != null)
            {
                var path = $"{girlDef.id}_{partDef.sprite.name}";
                SpriteInfo = new SpriteInfoInternal(path);

                assetProvider.AddAsset(typeof(Sprite), path, partDef.sprite);
            }
        }

        /// <inheritdoc/>
        public void SetData(ref GirlPartSubDefinition def,
                            GameDefinitionProvider gameDataProvider,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId,
                            GirlDefinition girlDef)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<GirlPartSubDefinition>();
            }

            var girlExpansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.partType, PartType);
            ValidatedSet.SetValue(ref def.partName, PartName, insertStyle);
            ValidatedSet.SetValue(ref def.x, X);
            ValidatedSet.SetValue(ref def.y, Y);

            ValidatedSet.SetValue(ref def.mirroredPartIndex, girlExpansion.PartIdToIndex, MirroredPart?.Id);
            ValidatedSet.SetValue(ref def.altPartIndex, girlExpansion.PartIdToIndex, AltPart?.Id);

            ValidatedSet.SetValue(ref def.sprite, SpriteInfo, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            SpriteInfo?.RequestInternals(assetProvider);
            MirroredPart?.RequestInternals(assetProvider);
            AltPart?.RequestInternals(assetProvider);
        }

        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return MirroredPart;
            yield return AltPart;
        }
    }
}
