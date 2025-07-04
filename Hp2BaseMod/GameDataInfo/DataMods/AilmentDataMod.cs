﻿// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="AilmentDefinition"/>.
    /// </summary>
    public class AilmentDataMod : DataMod, IGameDataMod<AilmentDefinition>
    {
        public RelativeId? ItemDefinitionID;

        public bool? PersistentFlags;

        public AilmentEnableType? EnableType;

        public RelativeId? EnableAbilityDefID;

        public int? EnableTriggerIndex;

        public RelativeId? EnableTokenDefID;

        public int? EnableIntVal;

        public float? EnableFloatVal;

        public bool? EnableBoolVal;

        public string EnableStringVal;

        public RelativeId? DisableAbilityDefID;

        public List<AilmentHintSubDefinition> Hints;

        public List<IGameDefinitionInfo<AilmentTriggerSubDefinition>> Triggers;

        /// <inheritdoc/>
        public AilmentDataMod() { }

        public AilmentDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal AilmentDataMod(AilmentDefinition def)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            PersistentFlags = def.persistentFlags;
            EnableType = def.enableType;
            EnableTriggerIndex = def.enableTriggerIndex;
            Hints = def.hints;
            EnableIntVal = def.enableIntVal;
            EnableFloatVal = def.enableFloatVal;
            EnableBoolVal = def.enableBoolVal;
            EnableStringVal = def.enableStringVal;

            EnableTokenDefID = new RelativeId(def.enableTokenDef);
            EnableAbilityDefID = new RelativeId(def.enableAbilityDef);
            DisableAbilityDefID = new RelativeId(def.disableAbilityDef);
            ItemDefinitionID = new RelativeId(def.itemDefinition);

            Triggers = def.triggers?.Select(x => (IGameDefinitionInfo<AilmentTriggerSubDefinition>)new AilmentTriggerInfo(x)).ToList();
        }

        /// <inheritdoc/>
        public void SetData(AilmentDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.persistentFlags, PersistentFlags);
            ValidatedSet.SetValue(ref def.enableType, EnableType);
            ValidatedSet.SetValue(ref def.enableTriggerIndex, EnableTriggerIndex);
            ValidatedSet.SetValue(ref def.enableIntVal, EnableIntVal);
            ValidatedSet.SetValue(ref def.enableFloatVal, EnableFloatVal);
            ValidatedSet.SetValue(ref def.enableBoolVal, EnableBoolVal);

            ValidatedSet.SetValue(ref def.itemDefinition,
                                  (ItemDefinition)gameDataProvider.GetDefinition(GameDataType.Item, ItemDefinitionID),
                                  InsertStyle);

            ValidatedSet.SetValue(ref def.enableTokenDef,
                                  (TokenDefinition)gameDataProvider.GetDefinition(GameDataType.Token, EnableTokenDefID),
                                  InsertStyle);

            ValidatedSet.SetValue(ref def.enableAbilityDef,
                                  (AbilityDefinition)gameDataProvider.GetDefinition(GameDataType.Ability, EnableAbilityDefID),
                                  InsertStyle);

            ValidatedSet.SetValue(ref def.disableAbilityDef,
                                  (AbilityDefinition)gameDataProvider.GetDefinition(GameDataType.Ability, DisableAbilityDefID),
                                  InsertStyle);

            ValidatedSet.SetValue(ref def.enableStringVal, EnableStringVal, InsertStyle);
            ValidatedSet.SetListValue(ref def.hints, Hints, InsertStyle);
            ValidatedSet.SetListValue(ref def.triggers, Triggers, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Triggers?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
