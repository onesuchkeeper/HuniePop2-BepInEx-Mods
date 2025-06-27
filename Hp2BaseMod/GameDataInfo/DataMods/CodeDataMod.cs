// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="CodeDefinition"/>.
    /// </summary>
    public class CodeDataMod : DataMod, IGameDataMod<CodeDefinition>
    {
        public CodeType? CodeType;

        public string OnMessage;

        public string OffMessage;

        public string CodeHash;

        /// <inheritdoc/>
        public CodeDataMod() { }

        public CodeDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal CodeDataMod(CodeDefinition def)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            CodeType = def.codeType;
            OffMessage = def.offMessage;
            OnMessage = def.onMessage;
            CodeHash = def.codeHash;
        }

        /// <inheritdoc/>
        public void SetData(CodeDefinition def, GameDefinitionProvider _, AssetProvider __)
        {
            ValidatedSet.SetValue(ref def.codeType, CodeType);

            ValidatedSet.SetValue(ref def.offMessage, OffMessage, InsertStyle);
            ValidatedSet.SetValue(ref def.onMessage, OnMessage, InsertStyle);
            ValidatedSet.SetValue(ref def.codeHash, CodeHash, InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
