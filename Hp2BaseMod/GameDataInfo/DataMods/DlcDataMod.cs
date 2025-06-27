// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="DlcDefinition"/>.
    /// </summary>
    public class DlcDataMod : DataMod, IGameDataMod<DlcDefinition>
    {
        public string DlcName;

        /// <inheritdoc/>
        public DlcDataMod() { }

        public DlcDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal DlcDataMod(DlcDefinition def)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            DlcName = def.dlcName;
        }

        /// <inheritdoc/>
        public void SetData(DlcDefinition def, GameDefinitionProvider _, AssetProvider __)
        {
            ValidatedSet.SetValue(ref def.dlcName, DlcName, InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
