using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    // Why would you make them all in random orders?
    private static readonly Dictionary<RelativeId, List<RelativeId>> FavoriteQuestionOrder = new()
    {
        {Girls.Aiko,
            new()
            {
                Questions.LastName,
                Questions.Height,
                Questions.Birthday,
                Questions.FavColour,
                Questions.Age,
                Questions.Weight,
                Questions.FavSeason,
                Questions.Education,
                Questions.Occupation,
                Questions.CupSize,
                Questions.Hobby,
                Questions.FavHangout,
            }
        },
        {Girls.Audrey,
            new()
            {
                Questions.LastName,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.FavColour,
                Questions.FavHangout,
                Questions.Age,
                Questions.Height,
                Questions.Occupation,
                Questions.Birthday,
                Questions.Hobby,
                Questions.FavSeason,
            }
        },
        {Girls.Beli,
            new()
            {
                Questions.FavSeason,
                Questions.Weight,
                Questions.Age,
                Questions.CupSize,
                Questions.FavHangout,
                Questions.FavColour,
                Questions.Height,
                Questions.Hobby,
                Questions.Occupation,
                Questions.Education,
                Questions.LastName,
                Questions.Birthday,
            }
        },
        {Girls.Celeste,
            new()
            {
                Questions.Hobby,
                Questions.FavHangout,
                Questions.Birthday,
                Questions.FavSeason,
                Questions.CupSize,
                Questions.Height,
                Questions.Occupation,
                Questions.Weight,
                Questions.FavColour,
                Questions.HomeWorld,
                Questions.LastName,
                Questions.Birthday,
            }
        },
        {Hp2BaseMod.Girls.JessieId,
            new()
            {
                Questions.Weight,
                Questions.FavColour,
                Questions.Education,
                Questions.Birthday,
                Questions.Occupation,
                Questions.CupSize,
                Questions.FavHangout,
                Questions.Age,
                Questions.Height,
                Questions.FavSeason,
                Questions.Hobby,
                Questions.LastName,
            }
        },
        {Girls.Kyanna,
            new()
            {
                Questions.LastName,
                Questions.Age,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.Occupation,
                Questions.Hobby,
                Questions.FavColour,
                Questions.FavSeason,
                Questions.FavHangout,
                Questions.Height,
                Questions.Birthday,
            }
        },
        {Hp2BaseMod.Girls.KyuId,
            new()
            {
                Questions.FavHangout,
                Questions.Height,
                Questions.FavColour,
                Questions.LastName,
                Questions.HomeWorld,
                Questions.FavSeason,
                Questions.Weight,
                Questions.Birthday,
                Questions.Occupation,
                Questions.Age,
                Questions.CupSize,
                Questions.Hobby,
            }
        },
        {Hp2BaseMod.Girls.LolaId,
            new()
            {
                Questions.LastName,
                Questions.Birthday,
                Questions.Age,
                Questions.Education,
                Questions.FavColour,
                Questions.Hobby,
                Questions.FavHangout,
                Questions.CupSize,
                Questions.FavSeason,
                Questions.Weight,
                Questions.Height,
                Questions.Occupation,
            }
        },
        {Girls.Momo,
            new()
            {
                Questions.HomeWorld,
                Questions.CupSize,
                Questions.FavColour,
                Questions.Occupation,
                Questions.Hobby,
                Questions.Weight,
                Questions.Height,
                Questions.Birthday,
                Questions.FavHangout,
                Questions.LastName,
                Questions.FavSeason,
                Questions.Age,
            }
        },
        {Girls.Nikki,
            new()
            {
                Questions.Age,
                Questions.FavSeason,
                Questions.Birthday,
                Questions.Height,
                Questions.Education,
                Questions.Weight,
                Questions.Hobby,
                Questions.FavHangout,
                Questions.Occupation,
                Questions.FavColour,
                Questions.CupSize,
                Questions.LastName,
            }
        },
        {Girls.Tiffany,
            new()
            {
                Questions.LastName,
                Questions.Education,
                Questions.Birthday,
                Questions.FavHangout,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.FavSeason,
                Questions.Age,
                Questions.Height,
                Questions.Hobby,
                Questions.FavColour,
            }
        },
        {Girls.Venus,
            new()
            {
                Questions.LastName,
                Questions.Height,
                Questions.FavSeason,
                Questions.Occupation,
                Questions.FavHangout,
                Questions.Weight,
                Questions.HomeWorld,
                Questions.Hobby,
                Questions.CupSize,
                Questions.Age,
                Questions.Birthday,
                Questions.FavColour,
            }
        },
    };

    private void ExtractQueries(RelativeId girlId, OrderedDictionary girlDef, SerializedFile file, Dictionary<RelativeId, IDialogLineDataMod> favoriteDialogLines)
    {
        if (!FavoriteQuestionOrder.TryGetValue(girlId, out var questionOrder))
        {
            ModInterface.Log.Warning($"Failed to find query order for girl {girlId}");
            return;
        }

        if (girlDef.TryGetValue("talkQueries", out List<object> talkQueries))
        {
            var questionOrderEnum = questionOrder.GetEnumerator();
            foreach (var talkQ in talkQueries.OfType<OrderedDictionary>())
            {
                if (UnityAssetPath.TryExtract(talkQ, out var talkQPath)
                    && _extractor.TryExtractMonoBehavior(file, talkQPath, out var talkQuery)
                    && talkQuery.TryGetValue<List<object>>("steps", out var steps))
                {
                    foreach (var step in steps.OfType<OrderedDictionary>().Skip(1))//skip the initial dt, move to the options
                    {
                        foreach (var line in ExtractQueryLines(step, file))
                        {
                            if (!questionOrderEnum.MoveNext()) break;
                            favoriteDialogLines[questionOrderEnum.Current] = line;
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<DialogLineDataMod> ExtractQueryLines(OrderedDictionary dialogSceneStep, SerializedFile file)
    {
        if (dialogSceneStep.TryGetValue("responseOptions", out List<object> responseOptions))
        {
            foreach (var branch in responseOptions.OfType<OrderedDictionary>())
            {
                if (branch.TryGetValue("steps", out List<object> steps))
                {
                    foreach (var step in steps.OfType<OrderedDictionary>())
                    {
                        if (step.TryGetValue("conditionalBranchs", out List<object> conditionalBranches))
                        {
                            foreach (var conditionalBranch in conditionalBranches.OfType<OrderedDictionary>())
                            {
                                if (conditionalBranch.TryGetValue("steps", out List<object> conditionalBranchSteps))
                                {
                                    foreach (var conditionalBranchStep in conditionalBranchSteps.OfType<OrderedDictionary>())
                                    {
                                        if (conditionalBranchStep.TryGetValue("type", out int type)
                                            && type == 0
                                            && conditionalBranchStep.TryGetValue("sceneLine", out OrderedDictionary sceneLine)
                                            && sceneLine.TryGetValue("dialogLine", out OrderedDictionary dialogLine))
                                        {
                                            yield return ExtractDialogLine(dialogLine, file);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}