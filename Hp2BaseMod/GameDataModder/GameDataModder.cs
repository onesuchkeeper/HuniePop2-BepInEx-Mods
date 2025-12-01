// Hp2BaseModdedLoader 2021, by OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using UnityEngine;

namespace Hp2BaseMod
{
    internal static class GameDataModder
    {
        public static void Mod(GameData gameData)
        {
            ModInterface.Log.LogInfo($"Loaded data sources: [{string.Join(", ", ModInterface.Save.SourceGUID_Id.Select(x => $"{x.Key} - {x.Value}"))}]");

            try
            {
                var assetProvider = ModInterface.Assets;
                var gameDataProvider = ModInterface.GameData;
                var context = new GameDataContext();

                using (ModInterface.Log.MakeIndent("Modifying GameData"))
                {
                    DefaultGameDataHandler.CollectDefaultData(gameData,
                        out context.abilityDataDict,
                        out context.ailmentDataDict,
                        out context.codeDataDict,
                        out context.cutsceneDataDict,
                        out context.dialogTriggerDataDict,
                        out context.dlcDataDict,
                        out context.energyDataDict,
                        out context.girlDataDict,
                        out context.girlPairDataDict,
                        out context.itemDataDict,
                        out context.locationDataDict,
                        out context.photoDataDict,
                        out context.questionDataDict,
                        out context.tokenDataDict);

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

                    // grab data mods
                    ModInterface.Log.LogInfo("grabbing data mods from the mod interface");
                    ModInterface.Log.IncreaseIndent();

                    context.abilityDataMods = ModInterface.AbilityDataMods;
                    context.ailmentDataMods = ModInterface.AilmentDataMods;
                    context.codeDataMods = ModInterface.CodeDataMods;
                    context.cutsceneDataMods = ModInterface.CutsceneDataMods;
                    context.dialogTriggerDataMods = ModInterface.DialogTriggerDataMods;
                    context.dlcDataMods = ModInterface.DlcDataMods;
                    context.energyDataMods = ModInterface.EnergyDataMods;
                    context.girlDataMods = ModInterface.GirlDataMods;
                    context.girlPairDataMods = ModInterface.GirlPairDataMods;
                    context.itemDataMods = ModInterface.ItemDataMods;
                    context.locationDataMods = ModInterface.LocationDataMods;
                    context.photoDataMods = ModInterface.PhotoDataMods;
                    context.questionDataMods = ModInterface.QuestionDataMods;
                    context.tokenDataMods = ModInterface.TokenDataMods;

                    GirlSubDataModder.GatherSubMods(context.girlDataMods, out var GirlToBodyToMods, out var dialogLineModsByIdByDialogTriggerByGirlId);
                    ModInterface.Log.DecreaseIndent();

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

                    GirlSubDataModder.HandleSubMods(context.girlDataDict,
                        context.girlDataMods,
                        GirlToBodyToMods,
                        context.questionDataDict,
                        context.questionDataMods,
                        context.dialogTriggerDataDict,
                        dialogLineModsByIdByDialogTriggerByGirlId);

                    //Requesting internal data
                    using (ModInterface.Log.MakeIndent("cataloging internal assets"))
                    {
                        assetProvider.FulfilInternalAssetRequests();
                    }

                    //setup defs
                    GameDataModApplicator.ApplyDataMods(gameData,
                        gameDataProvider,
                        assetProvider,
                        context,
                        GirlToBodyToMods,
                        dialogLineModsByIdByDialogTriggerByGirlId);
                }

                using (ModInterface.Log.MakeIndent("verifying gamedata integrity"))
                {
                    var lailani = ModInterface.GameData.GetGirl(Girls.LailaniId);

                    using (ModInterface.Log.MakeIndent("girls"))
                    {
                        foreach (var girl in Game.Data.Girls.GetAll())
                        {
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
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError($"{e}");
            }
        }

        private static void PreProcess<D>(Dictionary<int, D> dict, IEnumerable<IGameDataMod<D>> mods, GameDataType type, AssetProvider assetProvider)
            where D : Definition, new()
        {
            foreach (var mod in mods)
            {
                var runtimeId = ModInterface.Data.GetRuntimeDataId(type, mod.Id);

                if (!dict.ContainsKey(runtimeId))
                {
                    var newDef = ScriptableObject.CreateInstance<D>();
                    newDef.id = runtimeId;

                    dict.Add(runtimeId, newDef);
                }

                mod.RequestInternals(assetProvider);
            }
        }
    }
}
