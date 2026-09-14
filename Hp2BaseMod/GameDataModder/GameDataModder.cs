// Hp2BaseModdedLoader 2021, by OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod
{
    internal static class GameDataModder
    {
        public static void Mod(GameData gameData, GameDefinitionProvider gameDataProvider)
        {
            ModInterface.Log.Message($"Loaded data sources: [{string.Join(", ", ModInterface.Save.SourceGUID_Id.Select(x => $"{x.Key} - {x.Value}"))}]");

            try
            {
                var assetProvider = ModInterface.Assets;
                GameDataContext context = null;

                using (ModInterface.Log.MakeIndent("Gathering base GameData"))
                {
                    context = DefaultGameDataHandler.CollectDefaultData(gameData, gameDataProvider);

                    using (ModInterface.Log.MakeIndent("loading internal assets"))
                    {
                        foreach (var entry in context.abilityDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.cutsceneDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.dialogTriggerDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.energyDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.girlDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.itemDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.locationDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.photoDataDict) assetProvider.Load(entry.Value);
                        foreach (var entry in context.tokenDataDict) assetProvider.Load(entry.Value);
                    }

                    // these are only referenced in the cutscenes
                    assetProvider.PhotosWindow = context.cutsceneDataDict[107].steps[4].windowPrefab;
                    assetProvider.KyuButtWindow = context.cutsceneDataDict[167].steps[25].windowPrefab;
                    assetProvider.ItemNotifierWindow = context.cutsceneDataDict[171].steps[4].windowPrefab;
                }

                using (ModInterface.Log.MakeIndent("Modifying GameData"))
                {
                    // grab data mods
                    ModInterface.Log.Message("grabbing data mods from the mod interface");
                    ModInterface.Log.IncreaseIndent();

                    context.abilityDataMods = ModInterface.DataMod.AbilityDataMods;
                    context.ailmentDataMods = ModInterface.DataMod.AilmentDataMods;
                    context.codeDataMods = ModInterface.DataMod.CodeDataMods;
                    context.cutsceneDataMods = ModInterface.DataMod.CutsceneDataMods;
                    context.dialogTriggerDataMods = ModInterface.DataMod.DialogTriggerDataMods;
                    context.dlcDataMods = ModInterface.DataMod.DlcDataMods;
                    context.energyDataMods = ModInterface.DataMod.EnergyDataMods;
                    context.girlDataMods = ModInterface.DataMod.GirlDataMods;
                    context.girlPairDataMods = ModInterface.DataMod.GirlPairDataMods;
                    context.itemDataMods = ModInterface.DataMod.ItemDataMods;
                    context.locationDataMods = ModInterface.DataMod.LocationDataMods;
                    context.photoDataMods = ModInterface.DataMod.PhotoDataMods;
                    context.questionDataMods = ModInterface.DataMod.QuestionDataMods;
                    context.tokenDataMods = ModInterface.DataMod.TokenDataMods;

                    GirlSubDataModder.GatherSubMods(context.girlDataMods, out var GirlToBodyToMods, out var dialogLineModsByIdByDialogTriggerByGirlId);
                    ModInterface.Log.DecreaseIndent();

                    var nextQuestionIndex = context.questionDataDict.Count();

                    // create and register missing empty mods for used ids, all need to exist before any are setup because they reference one another
                    using (ModInterface.Log.MakeIndent("creating data for new ids"))
                    {
                        PreProcess(context.abilityDataDict, context.abilityDataMods, GameDataType.Ability, assetProvider);
                        PreProcess(context.ailmentDataDict, context.ailmentDataMods, GameDataType.Ailment, assetProvider);
                        PreProcess(context.codeDataDict, context.codeDataMods, GameDataType.Code, assetProvider);
                        PreProcess(context.cutsceneDataDict, context.cutsceneDataMods, GameDataType.Cutscene, assetProvider);
                        PreProcess(context.dialogTriggerDataDict, context.dialogTriggerDataMods, GameDataType.DialogTrigger, assetProvider);
                        PreProcess(context.dlcDataDict, context.dlcDataMods, GameDataType.Dlc, assetProvider);
                        PreProcess(context.energyDataDict, context.energyDataMods, GameDataType.Energy, assetProvider);
                        PreProcess(context.girlDataDict, context.girlDataMods, GameDataType.Girl, assetProvider);
                        PreProcess(context.girlPairDataDict, context.girlPairDataMods, GameDataType.GirlPair, assetProvider);
                        PreProcess(context.itemDataDict, context.itemDataMods, GameDataType.Item, assetProvider);
                        PreProcess(context.locationDataDict, context.locationDataMods, GameDataType.Location, assetProvider);
                        PreProcess(context.photoDataDict, context.photoDataMods, GameDataType.Photo, assetProvider);
                        PreProcess(context.questionDataDict, context.questionDataMods, GameDataType.Question, assetProvider);
                        PreProcess(context.tokenDataDict, context.tokenDataMods, GameDataType.Token, assetProvider);
                    }

                    using (ModInterface.Log.MakeIndent("cataloging internal assets"))
                    {
                        assetProvider.FulfilInternalAssetRequests();
                    }

                    GameDataModApplicator.ApplyDataMods(gameData,
                        gameDataProvider,
                        assetProvider,
                        context,
                        GirlToBodyToMods,
                        dialogLineModsByIdByDialogTriggerByGirlId);
                }

                using (ModInterface.Log.MakeIndent("verifying gamedata integrity"))
                {
                    var lailani = gameDataProvider.GetGirl(Girls.Lailani);

                    using (ModInterface.Log.MakeIndent("girls"))
                    {

                        var favorites = gameDataProvider.GetDialogTrigger(new RelativeId(-1, 5));

                        foreach (var girl in Game.Data.Girls.GetAll())
                        {
                            using (ModInterface.Log.MakeIndent(girl.girlName))
                            {
                                var girlId = girl.ModId();
                                var girlExp = girl.GetExpansion();
                                girl.badFoodTypes ??= new();
                                if (!girl.badFoodTypes.Any())
                                {
                                    girl.badFoodTypes.Add((ItemFoodType)(-1));
                                }

                                if (girl.herQuestions.IsNullOrEmpty())
                                {
                                    girl.herQuestions = lailani.herQuestions;
                                }
                                girl.uniqueItemDefs ??= new();
                                girl.shoesItemDefs ??= new();
                                girl.baggageItemDefs ??= new();
                                girlExp.TalkHandler ??= new GirlTalkHandler();

                                //Safeguard until custom affection is fully implemented
                                if (girlExp.FavAffection != null)
                                {
                                    if (girlExp.FavAffection.Id == PuzzleAffectionId.Talent) girl.favoriteAffectionType = PuzzleAffectionType.TALENT;
                                    if (girlExp.FavAffection.Id == PuzzleAffectionId.Flirtation) girl.favoriteAffectionType = PuzzleAffectionType.FLIRTATION;
                                    if (girlExp.FavAffection.Id == PuzzleAffectionId.Romance) girl.favoriteAffectionType = PuzzleAffectionType.ROMANCE;
                                    if (girlExp.FavAffection.Id == PuzzleAffectionId.Sexuality) girl.favoriteAffectionType = PuzzleAffectionType.SEXUALITY;
                                }

                                if (girlExp.LeastFavAffection != null)
                                {
                                    if (girlExp.LeastFavAffection.Id == PuzzleAffectionId.Talent) girl.leastFavoriteAffectionType = PuzzleAffectionType.TALENT;
                                    if (girlExp.LeastFavAffection.Id == PuzzleAffectionId.Flirtation) girl.leastFavoriteAffectionType = PuzzleAffectionType.FLIRTATION;
                                    if (girlExp.LeastFavAffection.Id == PuzzleAffectionId.Romance) girl.leastFavoriteAffectionType = PuzzleAffectionType.ROMANCE;
                                    if (girlExp.LeastFavAffection.Id == PuzzleAffectionId.Sexuality) girl.leastFavoriteAffectionType = PuzzleAffectionType.SEXUALITY;
                                }
                            }
                        }
                    }

                    using (ModInterface.Log.MakeIndent("items"))
                    {
                        foreach (var item in Game.Data.Items.GetAll())
                        {
                            var expansion = item.GetExpansion();
                            expansion.GiftHandler ??= new NonGiftItemHandler();

                            //Safeguard until custom affection is fully implemented
                            if (expansion.Affection != null)
                            {
#pragma warning disable HP001 // Deprecated member usage
                                if (expansion.Affection.Id == PuzzleAffectionId.Talent) item.affectionType = PuzzleAffectionType.TALENT;
                                if (expansion.Affection.Id == PuzzleAffectionId.Flirtation) item.affectionType = PuzzleAffectionType.FLIRTATION;
                                if (expansion.Affection.Id == PuzzleAffectionId.Romance) item.affectionType = PuzzleAffectionType.ROMANCE;
                                if (expansion.Affection.Id == PuzzleAffectionId.Sexuality) item.affectionType = PuzzleAffectionType.SEXUALITY;
#pragma warning restore HP001 // Deprecated member usage
                            }
                        }
                    }

                    using (ModInterface.Log.MakeIndent("tokens"))
                    {
                        var defaultTokenHandler = gameDataProvider._tokenHandlers[TokenHandlerId.Default];
                        
                        foreach (var token in Game.Data.Tokens.GetAll())
                        {
                            var expansion = token.GetExpansion();
                            expansion.TokenHandler ??= defaultTokenHandler;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.Error($"{e}");
            }
        }

        private static void PreProcess<D>(Dictionary<int, D> dict,
            IEnumerable<IGameDataMod<D>> mods,
            GameDataType type,
            AssetProvider assetProvider)
            where D : Definition, new()
        {
            foreach (var mod in mods)
            {
                var runtimeId = ModInterface.Data.GetRuntimeDataId(type, mod.Id);

                if (!dict.ContainsKey(runtimeId))
                {
                    var newDef = ScriptableObject.CreateInstance<D>();
                    newDef.id = runtimeId;
                    newDef.name = runtimeId.ToString();

                    dict.Add(runtimeId, newDef);
                }

                mod.RequestInternals(assetProvider);
            }
        }
    }
}
