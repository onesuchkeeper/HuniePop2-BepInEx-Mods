// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a CutsceneDialogOptionSubDefinition
    /// </summary>
    public class CutsceneDialogOptionInfo : IGameDefinitionInfo<CutsceneDialogOptionSubDefinition>
    {
        public string DialogOptionText;

        public string YuriDialogOptionText;

        public bool? Yuri;

        public List<IGameDefinitionInfo<CutsceneStepSubDefinition>> Steps;

        /// <summary>
        /// Constructor
        /// </summary>
        public CutsceneDialogOptionInfo() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        public CutsceneDialogOptionInfo(CutsceneDialogOptionSubDefinition def, AssetProvider assetProvider)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (assetProvider == null) { throw new ArgumentNullException(nameof(assetProvider)); }

            DialogOptionText = def.dialogOptionText;
            Yuri = def.yuri;
            YuriDialogOptionText = def.yuriDialogOptionText;

            if (def.steps != null) { Steps = def.steps.Select(x => (IGameDefinitionInfo<CutsceneStepSubDefinition>)new CutsceneStepInfo(x, assetProvider)).ToList(); }
        }

        /// <inheritdoc/>
        public void SetData(ref CutsceneDialogOptionSubDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<CutsceneDialogOptionSubDefinition>();
            }

            ValidatedSet.SetValue(ref def.dialogOptionText, DialogOptionText, insertStyle);
            ValidatedSet.SetValue(ref def.yuri, Yuri);
            ValidatedSet.SetValue(ref def.yuriDialogOptionText, YuriDialogOptionText, insertStyle);
            ValidatedSet.SetListValue(ref def.steps, Steps, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Steps?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
