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
