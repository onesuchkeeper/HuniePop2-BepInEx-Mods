using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Hp2BaseMod;

namespace Cheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public partial class Plugin : Hp2BaseModPlugin
{
    private const string GENERAL_CONFIG_CAT = "general";

    public static ConfigEntry<bool> UnlockAllStyles => _unlockAllStyles;
    private static ConfigEntry<bool> _unlockAllStyles;

    public static ConfigEntry<bool> MeetAllGirls => _meetAllGirls;
    private static ConfigEntry<bool> _meetAllGirls;

    public static ConfigEntry<bool> LearnAllFavorites => _learnAllFavorites;
    private static ConfigEntry<bool> _learnAllFavorites;

    public static ConfigEntry<bool> LearnAllBaggage => _learnAllBaggage;
    private static ConfigEntry<bool> _learnAllBaggage;

    public static ConfigEntry<int> FruitCount => _fruitCount;
    private static ConfigEntry<int> _fruitCount;

    public static ConfigEntry<int> DateAffection => _dateAffection;
    private static ConfigEntry<int> _dateAffection;

    public static ConfigEntry<int> DateMoves => _dateMoves;
    private static ConfigEntry<int> _dateMoves;

    public static ConfigEntry<int> DateStamina => _dateStamina;
    private static ConfigEntry<int> _dateStamina;

    public static ConfigEntry<int> DatePassion => _datePassion;
    private static ConfigEntry<int> _datePassion;

    public static ConfigEntry<int> DateSentiment => _dateSentiment;
    private static ConfigEntry<int> _dateSentiment;

    public static ConfigEntry<int> BonusRoundAffection => _bonusRoundAffection;
    private static ConfigEntry<int> _bonusRoundAffection;

    private static readonly FieldInfo f_testMode = AccessTools.Field(typeof(GameManager), "_testMode");
    private static readonly FieldInfo f_learnedBaggage = AccessTools.Field(typeof(PlayerFileGirl), "_learnedBaggage");

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        base.Awake();

        _unlockAllStyles = Config.Bind(GENERAL_CONFIG_CAT, nameof(UnlockAllStyles), false, "If true unlocks every style for every girl on startup.");
        _meetAllGirls = Config.Bind(GENERAL_CONFIG_CAT, nameof(MeetAllGirls), false, "If true sets all girls to have been met on startup.");
        _learnAllFavorites = Config.Bind(GENERAL_CONFIG_CAT, nameof(LearnAllFavorites), false, "If true learns the favorites of all girls on startup.");
        _learnAllBaggage = Config.Bind(GENERAL_CONFIG_CAT, nameof(LearnAllBaggage), false, "If true learns the baggages of all girls on startup.");

        _fruitCount = Config.Bind(GENERAL_CONFIG_CAT, nameof(FruitCount), -1, "If value is greater than -1, sets all fruit counts to that value on startup.");

        _dateAffection = Config.Bind(GENERAL_CONFIG_CAT, nameof(DateAffection), 0, "Affection added to every token match during dates.");
        _dateMoves = Config.Bind(GENERAL_CONFIG_CAT, nameof(DateMoves), 0, "Moves added to every token match during dates.");
        _dateStamina = Config.Bind(GENERAL_CONFIG_CAT, nameof(DateStamina), 0, "Stamina added to every token match during dates.");
        _dateSentiment = Config.Bind(GENERAL_CONFIG_CAT, nameof(DateSentiment), 0, "Sentiment added to every token match during dates.");
        _datePassion = Config.Bind(GENERAL_CONFIG_CAT, nameof(DatePassion), 0, "Passion added to every token match during dates.");

        _bonusRoundAffection = Config.Bind(GENERAL_CONFIG_CAT, nameof(BonusRoundAffection), 0, "If true during bonus rounds every token match earns 50 additional affection.");

        ModInterface.Log.ShowDebug = true;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        using (ModInterface.Log.MakeIndent("Activating Cheats"))
        {
            if (MeetAllGirls.Value)
            {
                ModInterface.Log.Message(nameof(MeetAllGirls));
                foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
                {
                    file.GetPlayerFileGirl(girl).playerMet = true;
                }
            }

            if (UnlockAllStyles.Value)
            {
                ModInterface.Log.Message(nameof(UnlockAllStyles));
                foreach (var fileGirl in file.girls)
                {
                    var expansion = fileGirl.girlDefinition.Expansion();

                    foreach (var body in expansion.Bodies.Values)
                    {
                        foreach (var outfit in expansion.OutfitIndexToId.Keys)
                        {
                            fileGirl.UnlockOutfit(outfit);
                        }

                        foreach (var hairstyleId_index in expansion.HairstyleIdToIndex)
                        {
                            fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                        }
                    }
                }
            }

            if (LearnAllFavorites.Value)
            {
                ModInterface.Log.Message(nameof(LearnAllFavorites));
                foreach (var fileGirl in file.girls)
                {
                    var expansion = fileGirl.girlDefinition.Expansion();

                    foreach (var question in expansion.FavQuestionIdToAnswerId.Keys.Select(ModInterface.GameData.GetQuestion))
                    {
                        fileGirl.LearnFavAnswer(question);
                    }
                }
            }

            if (LearnAllBaggage.Value)
            {
                ModInterface.Log.Message(nameof(LearnAllBaggage));
                foreach (var fileGirl in file.girls)
                {
                    f_learnedBaggage.SetValue(fileGirl, Enumerable.Range(0, fileGirl.girlDefinition.baggageItemDefs.Count()));
                }
            }

            if (FruitCount.Value > -1)
            {
                ModInterface.Log.Message(nameof(FruitCount));

                // we're setting them all so we don't care which ones they actually are
                var fruitTypeCount = ModInterface.Data.GetIds(GameDataType.Fruit).Count();
                for (int i = 0; i < fruitTypeCount; i++)
                {
                    file.fruitCounts[i] = FruitCount.Value;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
public static class PuzzleSetGetMatchRewards_Patch
{
    public static void Prefix(PuzzleStatus __instance)
    {
        if (__instance.bonusRound)
        {
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, Plugin.BonusRoundAffection.Value, false);
        }
        else
        {
            __instance.AddResourceValue(PuzzleResourceType.MOVES, Plugin.DateMoves.Value, false);
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, Plugin.DateAffection.Value, false);
            __instance.AddResourceValue(PuzzleResourceType.STAMINA, Plugin.DateStamina.Value, false);
            __instance.AddResourceValue(PuzzleResourceType.PASSION, Plugin.DatePassion.Value, false);
            __instance.AddResourceValue(PuzzleResourceType.SENTIMENT, Plugin.DateSentiment.Value, false);
        }
    }
}
