// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make an AudioKlip
    /// </summary>
    public class CutsceneStepInfo : IGameDefinitionInfo<CutsceneStepSubDefinition>
    {
        public CutsceneStepType? StepType;

        public CutsceneStepProceedType? ProceedType;

        public CutsceneStepDollTargetType? DollTargetType;

        public DollOrientationType? TargetDollOrientation;

        public GirlExpressionType? ExpressionType;

        public DollPositionType? DollPositionType;

        public CutsceneStepAnimationType? AnimationType;

        public CutsceneStepSubCutsceneType? SubCutsceneType;

        public CutsceneStepNotificationType? NotificationType;

        public GirlPairRelationshipType? GirlPairRelationshipType;

        public string SpecialStepPrefabName;

        public string BannerTextPrefabName;

        public string WindowPrefabName;

        public string EmitterBehaviorName;

        public string StringValue;

        public float? FloatValue;

        public float? ProceedFloat;

        public RelativeId? TargetGirlDefinitionId;

        public int? IntValue;

        public int? EaseType;

        public RelativeId? DialogTriggerDefinitionId;

        public RelativeId? GirlDefinitionId;

        public int? ExpressionIndex;

        public RelativeId? HairstyleId;

        public RelativeId? OutfitId;

        public RelativeId? SubCutsceneDefinitionId;

        public bool? SkipStep;

        public bool? TargetAlt;

        public bool? BoolValue;

        public bool? SetMood;

        public bool? ProceedBool;

        public IGameDefinitionInfo<Vector2> PositionInfo;

        public IGameDefinitionInfo<AudioKlip> AudioKlipInfo;

        public IGameDefinitionInfo<LogicAction> LogicActionInfo;

        public IDialogLineDataMod DialogLine;

        public List<IGameDefinitionInfo<CutsceneDialogOptionSubDefinition>> DialogOptionInfos;

        public List<IGameDefinitionInfo<CutsceneBranchSubDefinition>> BranchInfos;

        /// <summary>
        /// Writes to the game data definition this represents
        /// </summary>
        /// <param name="def">The target game data definition to write to.</param>
        /// <param name="gameDataProvider">The game data.</param>
        /// <param name="assetProvider">The asset provider.</param>
        /// <param name="insertStyle">The insert style.</param>
        public void SetData(ref CutsceneStepSubDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<CutsceneStepSubDefinition>();
            }

            ValidatedSet.SetValue(ref def.skipStep, SkipStep);
            ValidatedSet.SetValue(ref def.stepType, StepType);
            ValidatedSet.SetValue(ref def.proceedType, ProceedType);
            ValidatedSet.SetValue(ref def.dollTargetType, DollTargetType);
            ValidatedSet.SetValue(ref def.targetDollOrientation, TargetDollOrientation);
            ValidatedSet.SetValue(ref def.targetAlt, TargetAlt);
            ValidatedSet.SetValue(ref def.boolValue, BoolValue);
            ValidatedSet.SetValue(ref def.intValue, IntValue);
            ValidatedSet.SetValue(ref def.floatValue, FloatValue);
            ValidatedSet.SetValue(ref def.stringValue, StringValue, insertStyle);
            ValidatedSet.SetValue(ref def.easeType, (Ease?)EaseType);
            ValidatedSet.SetValue(ref def.expressionType, ExpressionType);
            ValidatedSet.SetValue(ref def.setMood, SetMood);
            ValidatedSet.SetValue(ref def.dollPositionType, DollPositionType);
            ValidatedSet.SetValue(ref def.expressionIndex, ExpressionIndex);

            if (GirlDefinitionId.HasValue)
            {
                var girlExpansion = ExpandedGirlDefinition.Get(GirlDefinitionId.Value);

                if (HairstyleId.HasValue)
                {
                    ValidatedSet.SetValue(ref def.hairstyleIndex, girlExpansion.HairstyleLookup[HairstyleId.Value]);
                }

                if (OutfitId.HasValue)
                {
                    ValidatedSet.SetValue(ref def.outfitIndex, girlExpansion.OutfitLookup[OutfitId.Value]);
                }
            }

            ValidatedSet.SetValue(ref def.animationType, AnimationType);
            ValidatedSet.SetValue(ref def.subCutsceneType, SubCutsceneType);
            ValidatedSet.SetValue(ref def.girlPairRelationshipType, GirlPairRelationshipType);
            ValidatedSet.SetValue(ref def.notificationType, NotificationType);
            ValidatedSet.SetValue(ref def.proceedBool, ProceedBool);
            ValidatedSet.SetValue(ref def.proceedFloat, ProceedFloat);

            ValidatedSet.SetValue(ref def.targetGirlDefinition, gameDataProvider.GetGirl(TargetGirlDefinitionId), insertStyle);
            ValidatedSet.SetValue(ref def.girlDefinition, gameDataProvider.GetGirl(GirlDefinitionId), insertStyle);
            ValidatedSet.SetValue(ref def.dialogTriggerDefinition, gameDataProvider.GetDialogTrigger(DialogTriggerDefinitionId), insertStyle);
            ValidatedSet.SetValue(ref def.subCutsceneDefinition, gameDataProvider.GetCutscene(SubCutsceneDefinitionId), insertStyle);

            ValidatedSet.SetValue(ref def.windowPrefab, assetProvider.GetInternalAsset<UiWindow>(WindowPrefabName), insertStyle);
            ValidatedSet.SetValue(ref def.emitterBehavior, assetProvider.GetInternalAsset<EmitterBehavior>(EmitterBehaviorName), insertStyle);
            ValidatedSet.SetValue(ref def.specialStepPrefab, assetProvider.GetInternalAsset<CutsceneStepSpecial>(SpecialStepPrefabName), insertStyle);
            ValidatedSet.SetValue(ref def.bannerTextPrefab, assetProvider.GetInternalAsset<BannerTextBehavior>(BannerTextPrefabName), insertStyle);

            ValidatedSet.SetValue(ref def.logicAction, LogicActionInfo, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.audioKlip, AudioKlipInfo, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.position, PositionInfo, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.branches, BranchInfos, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.dialogOptions, DialogOptionInfos, insertStyle, gameDataProvider, assetProvider);

            if (DialogLine != null)
            {
                def.dialogLine ??= new();
                DialogLine.SetData(def.dialogLine, gameDataProvider, assetProvider);
            }
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            DialogOptionInfos?.ForEach(x => x?.RequestInternals(assetProvider));
            BranchInfos?.ForEach(x => x?.RequestInternals(assetProvider));

            AudioKlipInfo?.RequestInternals(assetProvider);
            LogicActionInfo?.RequestInternals(assetProvider);
            PositionInfo?.RequestInternals(assetProvider);

            if (!string.IsNullOrWhiteSpace(WindowPrefabName))
            {
                assetProvider.RequestInternal(typeof(UiWindow), WindowPrefabName);
            }

            if (!string.IsNullOrWhiteSpace(EmitterBehaviorName))
            {
                assetProvider.RequestInternal(typeof(EmitterBehavior), EmitterBehaviorName);
            }

            if (!string.IsNullOrWhiteSpace(SpecialStepPrefabName))
            {
                assetProvider.RequestInternal(typeof(CutsceneStepSpecial), SpecialStepPrefabName);
            }

            if (!string.IsNullOrWhiteSpace(BannerTextPrefabName))
            {
                assetProvider.RequestInternal(typeof(BannerTextBehavior), BannerTextPrefabName);
            }
        }
    }
}
