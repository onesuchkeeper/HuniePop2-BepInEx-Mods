using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod;

internal class GameDataModApplicator
{
    public static void ApplyDataMods(GameData gameData,
        GameDefinitionProvider gameDataProvider,
        AssetProvider assetProvider,
        GameDataContext context,
        Dictionary<RelativeId, Dictionary<RelativeId, BodyData>> GirlToBodyToMods,
        Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IDialogLineDataMod>>>> dialogLineModsByIdByDialogTriggerByGirlId)
    {
        using (ModInterface.Log.MakeIndent("applying mod data to game data"))
        {
            using (ModInterface.Log.MakeIndent("Abilities"))
            {
                SetData(context.abilityDataDict, context.abilityDataMods, gameDataProvider, assetProvider, GameDataType.Ability);
            }

            using (ModInterface.Log.MakeIndent("Ailments"))
            {
                SetData(context.ailmentDataDict, context.ailmentDataMods, gameDataProvider, assetProvider, GameDataType.Ailment);
            }

            using (ModInterface.Log.MakeIndent("Codes"))
            {
                SetData(context.codeDataDict, context.codeDataMods, gameDataProvider, assetProvider, GameDataType.Code);
            }

            using (ModInterface.Log.MakeIndent("Cutscenes"))
            {
                SetData(context.cutsceneDataDict, context.cutsceneDataMods, gameDataProvider, assetProvider, GameDataType.Cutscene);
            }

            using (ModInterface.Log.MakeIndent("Dialog triggers"))
            {
                SetData(context.dialogTriggerDataDict, context.dialogTriggerDataMods, gameDataProvider, assetProvider, GameDataType.DialogTrigger);
            }

            using (ModInterface.Log.MakeIndent("Dlcs"))
            {
                SetData(context.dlcDataDict, context.dlcDataMods, gameDataProvider, assetProvider, GameDataType.Dlc);
            }

            using (ModInterface.Log.MakeIndent("Energy"))
            {
                SetData(context.energyDataDict, context.energyDataMods, gameDataProvider, assetProvider, GameDataType.Energy);
            }

            using (ModInterface.Log.MakeIndent("Girls"))
            {
                SetData(context.girlDataDict, context.girlDataMods, gameDataProvider, assetProvider, GameDataType.Girl);

                foreach (var girlId_BodyToMods in GirlToBodyToMods)
                {
                    var girl = context.girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_BodyToMods.Key)];
                    var expansion = ExpandedGirlDefinition.Get(girlId_BodyToMods.Key);

                    using (ModInterface.Log.MakeIndent($"Girl {girlId_BodyToMods.Key} - {girl.girlName}"))
                    {
                        using (ModInterface.Log.MakeIndent($"Bodies"))
                        {
                            foreach (var bodyId_Mods in girlId_BodyToMods.Value)
                            {
                                var body = expansion.Bodies.GetOrNew(bodyId_Mods.Key);
                                using (ModInterface.Log.MakeIndent($"Body {bodyId_Mods.Key} - {girl.girlName}"))
                                {
                                    SetSubDefMods(bodyId_Mods.Value.partMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, body.GetOrNewPart);
                                    SetSubDefMods(bodyId_Mods.Value.specialPartMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, body.GetOrNewSpecialPart);
                                    SetSubDefMods(bodyId_Mods.Value.outfitMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetOrNewOutfit(body, x));
                                    SetSubDefMods(bodyId_Mods.Value.hairstyleMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetOrNewHairstyle(body, x));
                                    SetSubDefMods(bodyId_Mods.Value.expressionMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetOrNewExpression(body, x));

                                    foreach (var mod in bodyId_Mods.Value.bodyMods)
                                    {
                                        mod.SetData(body, gameDataProvider, assetProvider, girlId_BodyToMods.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("girl lines"))
            {
                foreach (var girlId_ModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                {
                    foreach (var dialogTriggerId_ModsById in girlId_ModsByIdByDialogTrigger.Value)
                    {
                        var dialogTrigger = context.dialogTriggerDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, dialogTriggerId_ModsById.Key)];
                        var dtExpansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerId_ModsById.Key);

                        foreach (var lineId_Mods in dialogTriggerId_ModsById.Value)
                        {
                            var line = dtExpansion.GetLineOrNew(dialogTrigger, girlId_ModsByIdByDialogTrigger.Key, lineId_Mods.Key);

                            foreach (var mod in lineId_Mods.Value.OrderBy(x => x.LoadPriority))
                            {
                                mod.SetData(line, gameDataProvider, assetProvider);
                            }
                        }
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("girl pairs"))
            {
                SetData(context.girlPairDataDict, context.girlPairDataMods, gameDataProvider, assetProvider, GameDataType.GirlPair);

                using (ModInterface.Log.MakeIndent("styles"))
                {
                    foreach (var pairMod in context.girlPairDataMods)
                    {
                        var expansion = ExpandedGirlPairDefinition.Get(pairMod.Id);
                        expansion.PairStyle = pairMod.GetStyles();
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("items"))
            {
                SetData(context.itemDataDict, context.itemDataMods, gameDataProvider, assetProvider, GameDataType.Item);
            }

            using (ModInterface.Log.MakeIndent("locations"))
            {
                SetData(context.locationDataDict, context.locationDataMods, gameDataProvider, assetProvider, GameDataType.Location);
            }

            using (ModInterface.Log.MakeIndent("photos"))
            {
                SetData(context.photoDataDict, context.photoDataMods, gameDataProvider, assetProvider, GameDataType.Photo);
            }

            using (ModInterface.Log.MakeIndent("questions"))
            {
                SetData(context.questionDataDict, context.questionDataMods, gameDataProvider, assetProvider, GameDataType.Question);
            }

            using (ModInterface.Log.MakeIndent("tokens"))
            {
                SetData(context.tokenDataDict, context.tokenDataMods, gameDataProvider, assetProvider, GameDataType.Token);
            }
        }

        // using (ModInterface.Log.MakeIndent("registering functional mods"))
        // {
        //     ModInterface.Log.LogInfo("ailments");
        //     ModInterface.Data.RegisterFunctionalAilments(ailmentDataMods.OfType<IFunctionalAilmentDataMod>());
        // }
    }

    private static void SetData<T>(Dictionary<int, T> data,
                                       IEnumerable<IGameDataMod<T>> mods,
                                       GameDefinitionProvider gameDataProvider,
                                       AssetProvider prefabProvider,
                                       GameDataType type)
    {
        //first split into groups of the same id
        var modsByRuntimeId = new Dictionary<int, List<IGameDataMod<T>>>();
        foreach (var mod in mods)
        {
            var runtimeId = ModInterface.Data.GetRuntimeDataId(type, mod.Id);
            if (!modsByRuntimeId.ContainsKey(runtimeId))
            {
                modsByRuntimeId.Add(runtimeId, new List<IGameDataMod<T>>() { mod });
            }
            else
            {
                modsByRuntimeId[runtimeId].Add(mod);
            }
        }

        // then set the data
        foreach (var entry in modsByRuntimeId)
        {
            // order by the load priority
            foreach (var mod in entry.Value.OrderBy(x => x.LoadPriority))
            {
                mod.SetData(data[entry.Key], gameDataProvider, prefabProvider);
            }
        }
    }

    private static void SetSubDefMods<T>(Dictionary<RelativeId, List<IBodySubDataMod<T>>> idToMods,
        GameDefinitionProvider gameData,
        AssetProvider assetProvider,
        RelativeId girlId,
        GirlBodySubDefinition bodyDef,
        Func<RelativeId, T> getTarget)
    {
        using (ModInterface.Log.MakeIndent(typeof(T).Name))
        {
            foreach (var id_mods in idToMods)
            {
                using (ModInterface.Log.MakeIndent())
                {
                    var target = getTarget.Invoke(id_mods.Key);

                    foreach (var mod in id_mods.Value.OrderBy(x => x.LoadPriority))
                    {
                        mod.SetData(target, gameData, assetProvider, girlId, bodyDef);
                    }
                }
            }
        }
    }
}
