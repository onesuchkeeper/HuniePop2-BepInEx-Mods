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
    public class DialogLineDataMod : DataMod, IGirlSubDataMod<DialogLine>
    {
        public string DialogText;

        public string YuriDialogText;

        public bool? Yuri;

        public IGameDefinitionInfo<AudioClip> YuriAudioClipInfo;

        public IGameDefinitionInfo<AudioClip> AudioClipInfo;

        public DialogLineExpression StartExpression;

        public DialogLineExpression EndExpression;

        public List<DialogLineExpression> Expressions;

        public DialogLineDataMod() { }
        public DialogLineDataMod(RelativeId id, InsertStyle insertStyle = InsertStyle.append, int priority = 0)
        : base(id, InsertStyle.replace, priority)
        {

        }

        internal DialogLineDataMod(DialogLine def, AssetProvider assetProvider, RelativeId id)
        : base(id, InsertStyle.replace, 0)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (assetProvider == null) { throw new ArgumentNullException(nameof(assetProvider)); }

            DialogText = def.dialogText;
            Yuri = def.yuri;
            YuriDialogText = def.yuriDialogText;
            StartExpression = def.startExpression;
            Expressions = def.expressions;
            EndExpression = def.endExpression;

            if (def.yuriAudioClip != null) { YuriAudioClipInfo = new AudioClipInfo(def.yuriAudioClip, assetProvider); }
            if (def.audioClip != null) { AudioClipInfo = new AudioClipInfo(def.audioClip, assetProvider); }
        }

        /// <inheritdoc/>
        public void SetData(ref DialogLine def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            InsertStyle insertStyle,
            RelativeId girlId,
            GirlDefinition girlDef)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<DialogLine>();
            }

            ValidatedSet.SetValue(ref def.dialogText, DialogText, insertStyle);
            ValidatedSet.SetValue(ref def.yuri, Yuri);
            ValidatedSet.SetValue(ref def.yuriDialogText, YuriDialogText, insertStyle);
            ValidatedSet.SetValue(ref def.startExpression, StartExpression, insertStyle);
            ValidatedSet.SetValue(ref def.expressions, Expressions, insertStyle);
            ValidatedSet.SetValue(ref def.endExpression, EndExpression, insertStyle);

            ValidatedSet.SetValue(ref def.yuriAudioClip, YuriAudioClipInfo, insertStyle, gameData, assetProvider);
            ValidatedSet.SetValue(ref def.audioClip, AudioClipInfo, insertStyle, gameData, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            YuriAudioClipInfo?.RequestInternals(assetProvider);
            AudioClipInfo?.RequestInternals(assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods() => null;
    }
}
