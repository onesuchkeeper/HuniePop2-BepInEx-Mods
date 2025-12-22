// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlPartSubDefinition"/>.
    /// </summary>
    public class GirlPartDataMod : DataMod, IBodySubDataMod<GirlPartSubDefinition>
    {
        public GirlPartType? PartType;

        public string PartName;

        public int? X;

        public int? Y;

        public IBodySubDataMod<GirlPartSubDefinition> MirroredPart;

        public IBodySubDataMod<GirlPartSubDefinition> AltPart;

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
        public void SetData(GirlPartSubDefinition def,
                            GameDefinitionProvider gameDataProvider,
                            AssetProvider assetProvider,
                            RelativeId girlId,
                            GirlBodySubDefinition bodyDef)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));

            ValidatedSet.SetValue(ref def.partType, PartType);
            ValidatedSet.SetValue(ref def.partName, PartName, InsertStyle);
            ValidatedSet.SetValue(ref def.x, X);
            ValidatedSet.SetValue(ref def.y, Y);

            ValidatedSet.SetValue(ref def.mirroredPartIndex, bodyDef.PartLookup, MirroredPart?.Id);
            ValidatedSet.SetValue(ref def.altPartIndex, bodyDef.PartLookup, AltPart?.Id);

            ValidatedSet.SetValue(ref def.sprite, SpriteInfo, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            SpriteInfo?.RequestInternals(assetProvider);
            MirroredPart?.RequestInternals(assetProvider);
            AltPart?.RequestInternals(assetProvider);
        }

        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return MirroredPart;
            yield return AltPart;
        }
    }
}
