// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="DialogLine"/>.
    /// </summary>
    public class DialogLineDataMod : DataMod, IDialogLineDataMod
    {
        /// <summary>
        /// Default or male player text
        /// </summary>
        public string DialogText;

        /// <summary>
        /// female player text
        /// </summary>
        public string YuriDialogText;

        /// <summary>
        /// If line has female player specific text. See <see cref="YuriDialogText"/>.
        /// </summary>
        public bool? Yuri;

        /// <summary>
        /// Audio specific for female player
        /// </summary>
        public IGameDefinitionInfo<AudioClip> YuriAudioClipInfo;

        /// <summary>
        /// default of male audio
        /// </summary>
        public IGameDefinitionInfo<AudioClip> AudioClipInfo;

        /// <summary>
        /// expression used at the start of the line
        /// </summary>
        public DialogLineExpression StartExpression;

        /// <summary>
        /// expression used at the end of the line
        /// </summary>
        public DialogLineExpression EndExpression;

        /// <summary>
        /// expressions used throught the line
        /// </summary>
        public List<DialogLineExpression> Expressions;

        public DialogLineDataMod() { }

        public DialogLineDataMod(RelativeId id, InsertStyle insertStyle = InsertStyle.append, int priority = 0)
        : base(id, insertStyle, priority)
        {

        }

        /// <inheritdoc/>
        public void SetData(DialogLine def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider)
        {
            if (def == null) return;

            ValidatedSet.SetValue(ref def.dialogText, DialogText, InsertStyle);
            ValidatedSet.SetValue(ref def.yuri, Yuri);
            ValidatedSet.SetValue(ref def.yuriDialogText, YuriDialogText, InsertStyle);
            ValidatedSet.SetValue(ref def.startExpression, StartExpression, InsertStyle);
            ValidatedSet.SetValue(ref def.expressions, Expressions, InsertStyle);
            ValidatedSet.SetValue(ref def.endExpression, EndExpression, InsertStyle);

            ValidatedSet.SetValue(ref def.yuriAudioClip, YuriAudioClipInfo, InsertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.audioClip, AudioClipInfo, InsertStyle, gameData, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            YuriAudioClipInfo?.RequestInternals(assetProvider);
            AudioClipInfo?.RequestInternals(assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods() => null;
    }
}
