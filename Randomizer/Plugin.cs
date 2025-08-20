using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Extension.StringExtension;

namespace Hp2Randomizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance => _instance;
    private static Plugin _instance;

    private static int _modId;

    /// <summary>
    /// Sets the swap handler for a particular Special Girl.
    /// Handler accepts the the special girl, then the girl she swaps with
    /// and swaps their properties
    /// </summary>
    public static void SetSpecialCharacterSwapHandler(RelativeId specialGirlId, Action<GirlDefinition, GirlDefinition> swapHandler)
        => _swapHandlers[specialGirlId] = swapHandler;

    private static Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>> _swapHandlers = new Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>>();

    private DialogTriggerDefinition[] _moanTriggers;
    private bool _keepSwappedWings;

    public static readonly string ConfigGeneralName = "General";

    public static readonly string ConfigSeedName = "Seed";
    public static readonly string ConfigRandomizeNamesName = "RandomizeNames";
    public static readonly string ConfigRandomizeBaggageName = "RandomizeBaggage";
    public static readonly string ConfigRandomizePairsName = "RandomizePairs";
    public static readonly string ConfigRandomizeAffectionName = "RandomizeAffection";
    public static readonly string ConfigIncludeKyuName = "IncludeKyu";
    public static readonly string ConfigIncludeNymphojinnName = "IncludeNymphojinn";
    public static readonly string ConfigForceSpecialNormalSwapName = "ForceSpecialNormalSwap";
    public static readonly string ConfigSwappedSpecialKeepWingsName = "SwappedSpecialKeepWings";
    public static readonly string ConfigDisableName = "Disable";

    private void Awake()
    {
        _instance = this;

        this.Config.Bind(ConfigGeneralName, ConfigSeedName, -1, "Randomizer seed. Set to -1 for a random seed.");
        this.Config.Bind(ConfigGeneralName, ConfigRandomizeNamesName, true, "If character names will be randomized.");
        this.Config.Bind(ConfigGeneralName, ConfigRandomizeBaggageName, true, "If character baggages will be randomized.");
        this.Config.Bind(ConfigGeneralName, ConfigRandomizePairsName, true, "If character pairings will be randomized.");
        this.Config.Bind(ConfigGeneralName, ConfigRandomizeAffectionName, true, "If character favorite and least favorite affection will be randomized.");
        this.Config.Bind(ConfigGeneralName, ConfigIncludeKyuName, true, "If Kyu should be included in the randomized characters.");
        this.Config.Bind(ConfigGeneralName, ConfigIncludeNymphojinnName, true, "If the Nymphojinn should be included in the randomized characters.");
        this.Config.Bind(ConfigGeneralName, ConfigForceSpecialNormalSwapName, true, "If special characters should always be swapped with a normal character.");
        this.Config.Bind(ConfigGeneralName, ConfigSwappedSpecialKeepWingsName, true, "If special characters should keep their wings when swapped.");
        this.Config.Bind(ConfigGeneralName, ConfigDisableName, false, "Disables the randomizer entirely.");

        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        ModInterface.AddCommand(new SetSeedCommand());

        ModInterface.Events.PostDataMods += On_PostDataMods;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        //if randomization has made a girl's saved outfit invalid, set it to the default
        foreach (var fileGirl in file.girls)
        {
            if (fileGirl.outfitIndex >= fileGirl.girlDefinition.outfits.Count
                || !fileGirl.unlockedOutfits.Contains(fileGirl.outfitIndex))
            {
                fileGirl.outfitIndex = fileGirl.girlDefinition.defaultOutfitIndex;
            }

            if (fileGirl.hairstyleIndex >= fileGirl.girlDefinition.hairstyles.Count
                || !fileGirl.unlockedHairstyles.Contains(fileGirl.hairstyleIndex))
            {
                fileGirl.hairstyleIndex = fileGirl.girlDefinition.defaultHairstyleIndex;
            }
        }
    }

    private void SwapNymphojinn(GirlDefinition nymphojinnDef, GirlDefinition otherGirlDef)
    {
        //find the neutral glowing eyes to use for expressions that don't have glowing eyes
        var defaultGlowEyesIndex = -1;
        if (otherGirlDef.expressions.TryGetFirst(x => x.expressionType == GirlExpressionType.NEUTRAL, out var neutralExpression))
        {
            defaultGlowEyesIndex = neutralExpression.partIndexEyesGlow;
        }
        else
        {
            ModInterface.Log.LogWarning($"Unable to find neutral expression for girl {otherGirlDef.name}");
        }

        foreach (var expression in otherGirlDef.expressions)
        {
            expression.partIndexEyes = expression.partIndexEyesGlow == -1
                ? defaultGlowEyesIndex
                : expression.partIndexEyesGlow;
        }

        foreach (var expression in otherGirlDef.expressions)
        {
            if (expression.partIndexEyesGlow != -1)
            {
                expression.partIndexEyes = expression.partIndexEyesGlow;
            }
        }

        foreach (var dt in _moanTriggers)
        {
            var hold = dt.dialogLineSets[(int)nymphojinnDef.dialogTriggerTab];
            dt.dialogLineSets[(int)nymphojinnDef.dialogTriggerTab] = dt.dialogLineSets[(int)otherGirlDef.dialogTriggerTab];
            dt.dialogLineSets[(int)otherGirlDef.dialogTriggerTab] = hold;
        }

        nymphojinnDef.defaultHairstyleIndex = 0;
        nymphojinnDef.defaultOutfitIndex = 0;

        if (_keepSwappedWings && otherGirlDef.specialEffectPrefab == null)
        {
            otherGirlDef.specialEffectPrefab = nymphojinnDef.specialEffectPrefab;
        }
    }

    private void SwapKyu(GirlDefinition kyuDef, GirlDefinition otherGirlDef)
    {
        kyuDef.defaultHairstyleIndex = 1;
        kyuDef.defaultOutfitIndex = 1;

        if (_keepSwappedWings && otherGirlDef.specialEffectPrefab == null)
        {
            otherGirlDef.specialEffectPrefab = kyuDef.specialEffectPrefab;
        }
    }

    private void On_PostDataMods()
    {
        int seed;
        if (this.Config.TryGetEntry<int>(ConfigGeneralName, ConfigSeedName, out var seedConfig)
            && seedConfig.Value != -1)
        {
            seed = seedConfig.Value;
        }
        else
        {
            seed = Environment.TickCount;
            this.Config[ConfigGeneralName, ConfigSeedName].BoxedValue = seed;
        }

        if (ConfigGrab(ConfigDisableName, false))
        {
            ModInterface.Log.LogInfo($"Randomizer disabled");
            return;
        }

        if (ConfigGrab(ConfigIncludeKyuName, true))
        {
            _swapHandlers.Add(Girls.KyuId, SwapKyu);
        }

        if (ConfigGrab(ConfigIncludeNymphojinnName, true))
        {
            _swapHandlers.Add(Girls.MoxieId, SwapNymphojinn);
            _swapHandlers.Add(Girls.JewnId, SwapNymphojinn);
        }

        _keepSwappedWings = ConfigGrab(ConfigSwappedSpecialKeepWingsName, true);

        _moanTriggers = [
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 43)),
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 44)),
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 45)),
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 46)),
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 47))
        ];

        ModInterface.Log.LogInfo($"Randomizing, seed:{seed}");
        var random = new Random(seed);

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false);
        var normalPairs = Game.Data.GirlPairs.GetAll().Where(x =>
            x.girlDefinitionOne != null
            && x.girlDefinitionTwo != null
            && !x.girlDefinitionOne.specialCharacter
            && !x.girlDefinitionTwo.specialCharacter)
            .ToArray();

        //determine pairs
        var degreeTotal = normalPairs.Length * 2;
        var pairAllotment = degreeTotal / normalGirls.Count;
        var remaining = degreeTotal % normalGirls.Count;

        var degrees = new int[normalGirls.Count];
        int i = 0;
        for (i = 0; i < remaining; i++)
        {
            degrees[i] = pairAllotment + 1;
        }

        for (; i < normalGirls.Count; i++)
        {
            degrees[i] = pairAllotment;
        }

        IEnumerable<(int a, int b)> pairings;
        if (!ConfigGrab(ConfigRandomizePairsName, true))
        {
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }
        else if (UndirectedGraph.TryMakeConnectedHHGraph(degrees, out var graph))
        {
            pairings = graph.Randomize(degreeTotal * 3, random).Edges;
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to create simple graph from pairings, they will not be randomized.");
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }

        //gather data pools
        var namePool = new List<(string name, string nickName)>()
        {
            //hp
            ("Misty", null),
            ("Aiko", null),
            ("Beli", null),
            ("Kyanna", null),
            ("Tiffany", null),
            ("Celeste", null),
            ("Venus", null),
            ("Nikki", null),
            ("Audrey", null),
            ("Momo", null),
            //hcs
            ("Marlena", null),
            ("Nadia", null),
            ("Renee", null),
        };

        var assignedNames = new Dictionary<GirlDefinition, (string name, string nickName)>();

        var baggagePool = new List<(GirlDefinition girl, ItemDefinition ailment)>();

        foreach (var girl in normalGirls)
        {
            namePool.Add((girl.girlName, girl.girlNickName));
            baggagePool.AddRange(girl.baggageItemDefs.Select(x => (girl, x)));
        }

        var randomizedNames = ConfigGrab(ConfigRandomizeNamesName, true);

        var canSwapCharms = ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "SwapCharms", out Action<RelativeId, RelativeId> m_swapCharms);

        //special characters don't have all the stuff they need,
        //so instead I'll just swap their visuals and other bits with someone
        //swaps like this aren't the same odds but I don't really care
        foreach (var id_handler in _swapHandlers)
        {
            var specialGirl = ModInterface.GameData.GetGirl(id_handler.Key);
            var targetGirl = normalGirls[random.Next() % normalGirls.Count];

            namePool.Add((specialGirl.girlName, specialGirl.girlNickName));

            assignedNames[specialGirl] = randomizedNames
                ? namePool.PopRandom(random)
                : (specialGirl.girlName, specialGirl.girlNickName);

            var specialId = ModInterface.Data.GetDataId(GameDataType.Girl, specialGirl.id);
            var targetId = ModInterface.Data.GetDataId(GameDataType.Girl, targetGirl.id);

            if (canSwapCharms)
            {
                m_swapCharms(specialId, targetId);
            }

            id_handler.Value.Invoke(specialGirl, targetGirl);

            var specialGirlExpanded = specialGirl.Expansion();
            var targetGirlExpanded = targetGirl.Expansion();

            var holdIdToIndex = specialGirlExpanded.OutfitIdToIndex;
            specialGirlExpanded.OutfitIdToIndex = targetGirlExpanded.OutfitIdToIndex;
            targetGirlExpanded.OutfitIdToIndex = holdIdToIndex;

            holdIdToIndex = specialGirlExpanded.HairstyleIdToIndex;
            specialGirlExpanded.HairstyleIdToIndex = targetGirlExpanded.HairstyleIdToIndex;
            targetGirlExpanded.HairstyleIdToIndex = holdIdToIndex;

            holdIdToIndex = specialGirlExpanded.ExpressionIdToIndex;
            specialGirlExpanded.ExpressionIdToIndex = targetGirlExpanded.ExpressionIdToIndex;
            targetGirlExpanded.ExpressionIdToIndex = holdIdToIndex;

            holdIdToIndex = specialGirlExpanded.PartIdToIndex;
            specialGirlExpanded.PartIdToIndex = targetGirlExpanded.PartIdToIndex;
            targetGirlExpanded.PartIdToIndex = holdIdToIndex;

            var holdIndexToId = specialGirlExpanded.OutfitIndexToId;
            specialGirlExpanded.OutfitIndexToId = targetGirlExpanded.OutfitIndexToId;
            targetGirlExpanded.OutfitIndexToId = holdIndexToId;

            holdIndexToId = specialGirlExpanded.HairstyleIndexToId;
            specialGirlExpanded.HairstyleIndexToId = targetGirlExpanded.HairstyleIndexToId;
            targetGirlExpanded.HairstyleIndexToId = holdIndexToId;

            holdIndexToId = specialGirlExpanded.ExpressionIndexToId;
            specialGirlExpanded.ExpressionIndexToId = targetGirlExpanded.ExpressionIndexToId;
            targetGirlExpanded.ExpressionIndexToId = holdIndexToId;

            holdIndexToId = specialGirlExpanded.PartIndexToId;
            specialGirlExpanded.PartIndexToId = targetGirlExpanded.PartIndexToId;
            targetGirlExpanded.PartIndexToId = holdIndexToId;

            var holdVec2 = specialGirlExpanded.HeadPosition;
            specialGirlExpanded.HeadPosition = targetGirlExpanded.HeadPosition;
            targetGirlExpanded.HeadPosition = holdVec2;

            holdVec2 = specialGirlExpanded.BackPosition;
            specialGirlExpanded.BackPosition = targetGirlExpanded.BackPosition;
            targetGirlExpanded.BackPosition = holdVec2;

            var holdParts = specialGirl.parts;
            specialGirl.parts = targetGirl.parts;
            targetGirl.parts = holdParts;

            var holdSpecialParts = specialGirl.specialParts;
            specialGirl.specialParts = targetGirl.specialParts;
            targetGirl.specialParts = holdSpecialParts;

            var holdInt = specialGirl.partIndexBlink;
            specialGirl.partIndexBlink = targetGirl.partIndexBlink;
            targetGirl.partIndexBlink = holdInt;

            holdInt = specialGirl.partIndexBlushHeavy;
            specialGirl.partIndexBlushHeavy = targetGirl.partIndexBlushHeavy;
            targetGirl.partIndexBlushHeavy = holdInt;

            holdInt = specialGirl.partIndexBlushLight;
            specialGirl.partIndexBlushLight = targetGirl.partIndexBlushLight;
            targetGirl.partIndexBlushLight = holdInt;

            holdInt = specialGirl.partIndexBody;
            specialGirl.partIndexBody = targetGirl.partIndexBody;
            targetGirl.partIndexBody = holdInt;

            var holdIndexList = specialGirl.partIndexesPhonemes;
            specialGirl.partIndexesPhonemes = targetGirl.partIndexesPhonemes;
            targetGirl.partIndexesPhonemes = holdIndexList;

            holdIndexList = specialGirl.partIndexesPhonemesTeeth;
            specialGirl.partIndexesPhonemesTeeth = targetGirl.partIndexesPhonemesTeeth;
            targetGirl.partIndexesPhonemesTeeth = holdIndexList;

            holdInt = specialGirl.partIndexMouthNeutral;
            specialGirl.partIndexMouthNeutral = targetGirl.partIndexMouthNeutral;
            targetGirl.partIndexMouthNeutral = holdInt;

            holdInt = specialGirl.partIndexNipples;
            specialGirl.partIndexNipples = targetGirl.partIndexNipples;
            targetGirl.partIndexNipples = holdInt;

            holdInt = specialGirl.defaultOutfitIndex;
            specialGirl.defaultOutfitIndex = targetGirl.defaultOutfitIndex;
            targetGirl.defaultOutfitIndex = holdInt;

            holdInt = specialGirl.defaultHairstyleIndex;
            specialGirl.defaultHairstyleIndex = targetGirl.defaultHairstyleIndex;
            targetGirl.defaultHairstyleIndex = holdInt;

            var holdOutfits = specialGirl.outfits;
            specialGirl.outfits = targetGirl.outfits;
            targetGirl.outfits = holdOutfits;

            var holdHair = specialGirl.hairstyles;
            specialGirl.hairstyles = targetGirl.hairstyles;
            targetGirl.hairstyles = holdHair;

            holdInt = specialGirl.defaultExpressionIndex;
            specialGirl.defaultExpressionIndex = targetGirl.defaultExpressionIndex;
            targetGirl.defaultExpressionIndex = holdInt;

            var holdExpressions = specialGirl.expressions;
            specialGirl.expressions = targetGirl.expressions;
            targetGirl.expressions = holdExpressions;

            holdVec2 = specialGirl.specialEffectOffset;
            specialGirl.specialEffectOffset = targetGirl.specialEffectOffset;
            targetGirl.specialEffectOffset = holdVec2;

            var holdSpecialEffectPrefab = specialGirl.specialEffectPrefab;
            specialGirl.specialEffectPrefab = targetGirl.specialEffectPrefab;
            targetGirl.specialEffectPrefab = holdSpecialEffectPrefab;

            var holdSprite = specialGirl.cellphoneHead;
            specialGirl.cellphoneHead = targetGirl.cellphoneHead;
            targetGirl.cellphoneHead = holdSprite;

            holdSprite = specialGirl.cellphoneHeadAlt;
            specialGirl.cellphoneHeadAlt = targetGirl.cellphoneHeadAlt;
            targetGirl.cellphoneHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHead;
            specialGirl.cellphoneMiniHead = targetGirl.cellphoneMiniHead;
            targetGirl.cellphoneMiniHead = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHeadAlt;
            specialGirl.cellphoneMiniHeadAlt = targetGirl.cellphoneMiniHeadAlt;
            targetGirl.cellphoneMiniHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphonePortrait;
            specialGirl.cellphonePortrait = targetGirl.cellphonePortrait;
            targetGirl.cellphonePortrait = holdSprite;

            holdSprite = specialGirl.cellphonePortraitAlt;
            specialGirl.cellphonePortraitAlt = targetGirl.cellphonePortraitAlt;
            targetGirl.cellphonePortraitAlt = holdSprite;

            holdVec2 = specialGirl.upsetEmitterPos;
            specialGirl.upsetEmitterPos = targetGirl.upsetEmitterPos;
            targetGirl.upsetEmitterPos = holdVec2;

            holdVec2 = specialGirl.breathEmitterPos;
            specialGirl.breathEmitterPos = targetGirl.breathEmitterPos;
            targetGirl.breathEmitterPos = holdVec2;

            var holdBool = specialGirl.hasAltStyles;
            specialGirl.hasAltStyles = targetGirl.hasAltStyles;
            targetGirl.hasAltStyles = holdBool;

            var holdString = specialGirl.altStylesFlagName;
            specialGirl.altStylesFlagName = targetGirl.altStylesFlagName;
            targetGirl.altStylesFlagName = holdString;

            var holdCode = specialGirl.altStylesCodeDefinition;
            specialGirl.altStylesCodeDefinition = targetGirl.altStylesCodeDefinition;
            targetGirl.altStylesCodeDefinition = holdCode;

            holdCode = specialGirl.unlockStyleCodeDefinition;
            specialGirl.unlockStyleCodeDefinition = targetGirl.unlockStyleCodeDefinition;
            targetGirl.unlockStyleCodeDefinition = holdCode;

            holdInt = specialGirl.failureExpressionIndex;
            specialGirl.failureExpressionIndex = targetGirl.failureExpressionIndex;
            targetGirl.failureExpressionIndex = holdInt;
        }

        var handledCutscenes = new HashSet<CutsceneDefinition>();

        var randomizedBaggage = ConfigGrab(ConfigRandomizeBaggageName, true);
        var randomizedAffection = ConfigGrab(ConfigRandomizeAffectionName, true);

        //randomize girls
        foreach (var girl in normalGirls)
        {
            var name = randomizedNames ? namePool.PopRandom(random) : (girl.name, girl.girlNickName);
            assignedNames[girl] = name;

            if (randomizedBaggage)
            {
                var baggageA = baggagePool.PopRandom(random);
                var baggageB = baggagePool.PopRandom(random);
                var baggageC = baggagePool.PopRandom(random);

                girl.baggageItemDefs = new List<ItemDefinition>(){
                baggageA.ailment,
                baggageB.ailment,
                baggageC.ailment
            };
                ReplaceCutsceneGirls(baggageA.ailment.cutsceneDefinition.steps, [(baggageA.girl, girl, assignedNames[girl])]);
                ReplaceCutsceneGirls(baggageB.ailment.cutsceneDefinition.steps, [(baggageB.girl, girl, assignedNames[girl])]);
                ReplaceCutsceneGirls(baggageC.ailment.cutsceneDefinition.steps, [(baggageC.girl, girl, assignedNames[girl])]);

                handledCutscenes.Add(baggageA.ailment.cutsceneDefinition);
                handledCutscenes.Add(baggageB.ailment.cutsceneDefinition);
                handledCutscenes.Add(baggageC.ailment.cutsceneDefinition);
            }

            if (randomizedAffection)
            {
                var favoriteAffection = random.Next() % 4;
                girl.favoriteAffectionType = (PuzzleAffectionType)favoriteAffection;
                girl.leastFavoriteAffectionType = (PuzzleAffectionType)((favoriteAffection + 1 + (random.Next() % 3)) % 4);
            }
        }

        //apply pair randomization
        var finalPairsEnumerator = pairings.GetEnumerator();
        finalPairsEnumerator.MoveNext();

        foreach (var pair in normalPairs)
        {
            var flip = random.Next() % 2 == 0;

            var newGirlOne = normalGirls[flip ? finalPairsEnumerator.Current.a : finalPairsEnumerator.Current.b];
            var newGirlTwo = normalGirls[flip ? finalPairsEnumerator.Current.b : finalPairsEnumerator.Current.a];

            foreach (var cutscene in pair.relationshipCutsceneDefinitions)
            {
                ReplaceCutsceneGirls(cutscene?.steps, [
                    (pair.girlDefinitionOne, newGirlOne, assignedNames[newGirlOne]),
                    (pair.girlDefinitionTwo, newGirlTwo, assignedNames[newGirlTwo])
                ]);

                handledCutscenes.Add(cutscene);
            }

            pair.girlDefinitionOne = newGirlOne;
            pair.girlDefinitionTwo = newGirlTwo;
            finalPairsEnumerator.MoveNext();
        }

        //swap lola in tutorial pair 
        var swapPool = normalGirls.ToList();
        var lolaDef = ModInterface.GameData.GetGirl(Girls.LolaId);
        normalGirls.Remove(lolaDef);
        var lolaSwap = swapPool.PopRandom(random);

        var tutorialPair = Game.Data.GirlPairs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.GirlPair, new RelativeId(-1, 26)));

        tutorialPair.girlDefinitionTwo = lolaSwap;

        //for all other cutscenes lets just swap out each normal girl with another random one
        var replaceGroups = normalGirls.Select(x =>
        {
            var newGirl = swapPool.PopRandom(random);
            return (x, newGirl, assignedNames[newGirl]);
        }).Append((lolaDef, lolaSwap, assignedNames[lolaSwap])).ToArray();

        foreach (var cutscene in Game.Data.Cutscenes.GetAll().Where(x => !handledCutscenes.Contains(x)))
        {
            ReplaceCutsceneGirls(cutscene.steps, replaceGroups);
        }

        //names get changed after all cutscenes so that the name data is still matches
        foreach (var girl_names in assignedNames)
        {
            girl_names.Key.girlName = girl_names.Value.name;
            girl_names.Key.girlNickName = girl_names.Value.nickName;
        }
    }

    private void ReplaceCutsceneGirls(IEnumerable<CutsceneStepSubDefinition> steps,
        params IEnumerable<(GirlDefinition oldGirl, GirlDefinition newGirl, (string name, string nickName) newName)> replaceGroups)
    {
        if (steps == null)
        {
            return;
        }

        var strReplaces = replaceGroups.Select(x => (x.oldGirl.girlName, x.newName.name))
            .Concat(
                replaceGroups.Where(x => !x.oldGirl.girlNickName.IsNullOrWhiteSpace())
                    .Select(x => (x.oldGirl.girlNickName, x.newName.nickName.IsNullOrWhiteSpace()
                        ? x.newName.name
                        : x.newName.nickName))
            );

        foreach (var step in steps)
        {
            step.stringValue = step.stringValue.Replace(strReplaces);

            if (replaceGroups.TryGetFirst(x => step.girlDefinition == x.oldGirl, out var girlDef))
            {
                step.girlDefinition = girlDef.newGirl;
            }

            if (replaceGroups.TryGetFirst(x => step.targetGirlDefinition == x.oldGirl, out var targetGirlDef))
            {
                step.targetGirlDefinition = targetGirlDef.newGirl;
            }

            switch (step.stepType)
            {
                case CutsceneStepType.BRANCH:
                    foreach (var branch in step.branches)
                    {
                        ReplaceCutsceneGirls(branch?.cutsceneDefinition?.steps,
                            replaceGroups);
                        ReplaceCutsceneGirls(branch?.steps,
                            replaceGroups);
                    }
                    break;
                case CutsceneStepType.SUB_CUTSCENE:
                    CutsceneDefinition cutsceneDefinition = null;
                    switch (step.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            cutsceneDefinition = step.subCutsceneDefinition;
                            break;
                        //These two cases are ignored because every pair-cutscene is already being cleaned
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND:
                            break;
                        //This case depends on an inner cutscene being passed in, which is not relevant here.
                        case CutsceneStepSubCutsceneType.INNER:
                            break;
                    }

                    if (cutsceneDefinition != null && cutsceneDefinition.steps.Count > 0)
                    {
                        ReplaceCutsceneGirls(cutsceneDefinition.steps, replaceGroups);
                    }
                    break;
                case CutsceneStepType.DIALOG_OPTIONS:
                    foreach (var option in step.dialogOptions)
                    {
                        option.dialogOptionText = option.dialogOptionText.Replace(strReplaces);
                        option.yuriDialogOptionText = option.yuriDialogOptionText.Replace(strReplaces);
                    }
                    ReplaceCutsceneGirls(step.dialogOptions.SelectMany(x => x.steps), replaceGroups);
                    break;
                case CutsceneStepType.SHOW_NOTIFICATION:
                    //nothing to do here, the string value is already corrected above
                    break;
            }
        }
    }

    private T ConfigGrab<T>(string name, T defaultValue)
        => this.Config.TryGetEntry<T>(ConfigGeneralName, name, out var configEntry)
            ? configEntry.Value
            : defaultValue;
}
