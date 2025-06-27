// Hp2BaseMod 2021, By OneSuchKeeper

using System;
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

        public RelativeId? MirroredPartId;

        public RelativeId? AltPartId;

        public IGameDefinitionInfo<Sprite> SpriteInfo;

        /// <inheritdoc/>
        public GirlPartDataMod() { }

        public GirlPartDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal GirlPartDataMod(int index, AssetProvider assetProvider, GirlDefinition girlDef)
            : base(new RelativeId() { SourceId = -1, LocalId = index }, InsertStyle.replace, 0)
        {
            var partDef = girlDef.parts[index];

            PartType = partDef.partType;
            PartName = partDef.partName;
            X = partDef.x;
            Y = partDef.y;

            MirroredPartId = new RelativeId(-1, partDef.mirroredPartIndex);
            AltPartId = new RelativeId(-1, partDef.altPartIndex);

            // Special handling, prefixes id because all the part sprites of one type have the same name
            if (partDef.sprite != null)
            {
                var path = girlDef.id.ToString() + "_" + partDef.sprite.name;
                SpriteInfo = new SpriteInfoInternal(path);

                assetProvider.AddAsset(typeof(Sprite), path, partDef.sprite);
            }
        }

        /// <inheritdoc/>
        public void SetData(ref GirlPartSubDefinition def,
                            GameDefinitionProvider gameDataProvider,
                            AssetProvider assetProvider,
                            InsertStyle insertStyle,
                            RelativeId girlId)
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

            ValidatedSet.SetValue(ref def.mirroredPartIndex, girlExpansion.PartIdToIndex, MirroredPartId);
            ValidatedSet.SetValue(ref def.altPartIndex, girlExpansion.PartIdToIndex, AltPartId);

            ValidatedSet.SetValue(ref def.sprite, SpriteInfo, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            SpriteInfo?.RequestInternals(assetProvider);
        }
    }
}
