// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a GirlPartSubDefinition
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

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        /// <param name="GirlId">The runtime id of the <see cref="GirlDefinition"/> this part is for.</param>
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
                SpriteInfo = new SpriteInfoPath()
                {
                    Path = path,
                    IsExternal = false
                };
                assetProvider.AddAsset(path, partDef.sprite);
            }
        }

        public IEnumerable<string> GetInternalAudioRequests() => IEnumerableExtension.OrEmptyIfNull(SpriteInfo?.GetInternalAudioRequests());

        public IEnumerable<string> GetInternalSpriteRequests() => IEnumerableExtension.OrEmptyIfNull(SpriteInfo?.GetInternalSpriteRequests());

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

            ValidatedSet.SetValue(ref def.partType, PartType);
            ValidatedSet.SetValue(ref def.partName, PartName, insertStyle);
            ValidatedSet.SetValue(ref def.x, X);
            ValidatedSet.SetValue(ref def.y, Y);
            ValidatedSet.SetValue(ref def.mirroredPartIndex, ModInterface.Data.GetPartIndex(girlId, MirroredPartId));
            ValidatedSet.SetValue(ref def.altPartIndex, ModInterface.Data.GetPartIndex(girlId, AltPartId));
            ValidatedSet.SetValue(ref def.sprite, SpriteInfo, insertStyle, gameDataProvider, assetProvider);
        }
    }
}
