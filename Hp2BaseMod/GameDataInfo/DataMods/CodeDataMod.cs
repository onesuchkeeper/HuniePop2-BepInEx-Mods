﻿// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a CodeDefinition
    /// </summary>
    public class CodeDataMod : DataMod, IGameDataMod<CodeDefinition>
    {
        public CodeType? CodeType;

        public string OnMessage;

        public string OffMessage;

        public string CodeHash;

        /// <inheritdoc/>
        public CodeDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public CodeDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
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
