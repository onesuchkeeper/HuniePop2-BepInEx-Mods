// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="DialogTriggerDefinition"/>.
    /// </summary>
    public class DialogTriggerDataMod : DataMod, IGameDataMod<DialogTriggerDefinition>
    {
        public DialogTriggerForceType? ForceType;

        public RelativeId? ResponseTriggerDefinitionID;

        public int? Priority;

        /// <inheritdoc/>
        public DialogTriggerDataMod() { }

        public DialogTriggerDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal DialogTriggerDataMod(DialogTriggerDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            ForceType = def.forceType;
            ResponseTriggerDefinitionID = new RelativeId(def.responseTriggerDefinition);
            Priority = def.priority;
        }

        /// <inheritdoc/>
        public void SetData(DialogTriggerDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.forceType, ForceType);
            ValidatedSet.SetValue(ref def.priority, Priority);

            ValidatedSet.SetValue(ref def.responseTriggerDefinition,
                                  (DialogTriggerDefinition)gameDataProvider.GetDefinition(GameDataType.DialogTrigger, ResponseTriggerDefinitionID),
                                  InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
