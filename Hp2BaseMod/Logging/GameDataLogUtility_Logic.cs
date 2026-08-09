namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void Log(this LogicCondition logicCondition) => LogLogicCondition(logicCondition);

    public static void LogLogicCondition(LogicCondition logicCondition, ILogger logger = null)
    {
        logger ??= ModInterface.Log;

        if (logicCondition == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Type: {logicCondition.type}"))
        {
            switch (logicCondition.type)
            {
                case LogicConditionType.LOCATION:
                    logger.Message($"Must be {ModInterface.Data.GetDataId(GameDataType.Location, logicCondition.locationDefinition.id)} - {logicCondition.locationDefinition.locationName}");
                    break;
                case LogicConditionType.DAYTIME:
                    logger.Message($"Must be {logicCondition.daytimeType}");
                    break;
                case LogicConditionType.GIRL_PAIR:
                    logger.Message($"Must be {ModInterface.Data.GetDataId(GameDataType.GirlPair, logicCondition.girlPairDefinition.id)} - {logicCondition.girlPairDefinition.name}");
                    break;
                case LogicConditionType.GIRL_ORIENTATION:
                    logger.Message($"Doll on {logicCondition.dollOrientation} must be {ModInterface.Data.GetDataId(GameDataType.Girl, logicCondition.girlDefinition.id)} - {logicCondition.girlDefinition.name}");
                    break;
                case LogicConditionType.WING_COUNT:
                    logger.Message($"Must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FRUIT_COUNT:
                    logger.Message($"{logicCondition.affectionType} fruit must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.PUZZLE_RESOURCE:
                    logger.Message($"{logicCondition.resourceType} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.STORY_PROGRESS:
                    logger.Message($"must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FLAG_VALUE:
                    logger.Message($"{logicCondition.stringValue} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.IS_TUTORIAL:
                    logger.Message($"Must be {logicCondition.boolValue}");
                    break;
                case LogicConditionType.DIFFICULTY:
                    logger.Message($"Must be {logicCondition.settingDifficulty}");
                    break;
                case LogicConditionType.DATE_TYPE:
                    logger.Message($"Must be {logicCondition.dateType}");
                    break;
            }
        }
    }

    public static void LogLogicAction(LogicAction logicAction, ILogger logger = null)
    {
        logger ??= ModInterface.Log;

        if (logicAction == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Type: {logicAction.type}"))
        {
            switch (logicAction.type)
            {
                case LogicActionType.TRAVEL_TO:
                    logger.Message($"Time passing amount: {logicAction.intValue}");
                    logger.Message($"Destination location definition: {ModInterface.Data.GetDataId(GameDataType.Location, logicAction.locationDefinition.id)} - {logicAction.locationDefinition.locationName}");
                    var pairString = logicAction.girlPairDefinition == null
                        ? "null"
                        : $"{ModInterface.Data.GetDataId(GameDataType.GirlPair, logicAction.girlPairDefinition.id)} - {logicAction.girlPairDefinition.name}";
                    logger.Message($"Pair at loc {pairString}");
                    logger.Message($"Pair sides flipped: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_WINGS:
                    logger.Message($"This action just doesn't do anything! Odd...");
                    break;
                case LogicActionType.ADD_FRUIT:
                    logger.Message($"Fruit type: {logicAction.affectionType}");
                    logger.Message($"Fruit count: {logicAction.intValue}");
                    break;
                case LogicActionType.ADD_PUZZLE_RESOURCE:
                    logger.Message($"Resource type: {logicAction.resourceType}");
                    logger.Message($"Resource amount: {logicAction.intValue}");
                    logger.Message($"Give to alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.SET_STORY_PROGRESS:
                    logger.Message($"Story Progress value: {logicAction.intValue}");
                    break;
                case LogicActionType.SET_FLAG:
                    logger.Message($"Flag name: {logicAction.stringValue}");
                    logger.Message($"Flag value: {logicAction.intValue}");
                    break;
                case LogicActionType.START_CUTSCENE:
                    logger.Message($"Cutscene id: {ModInterface.Data.GetDataId(GameDataType.Cutscene, logicAction.cutsceneDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_FOCUS:
                    logger.Message($"Focus alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.RESET_DOLLS:
                    logger.Message($"Unload: {logicAction.boolValue}");
                    break;
                case LogicActionType.DIALOG_OPTION_FLAG:
                    logger.Message($"Flag gets set the the current 'Game.Session.Dialog.selectedDialogOptionIndex'");
                    logger.Message($"Flag name: {logicAction.stringValue}");
                    break;
                case LogicActionType.CHANGE_PUZZLE_STATE:
                    logger.Message($"Puzzle state: {logicAction.puzzleState}");
                    break;
                case LogicActionType.POP_DATE_GIFT:
                    logger.Message($"Target alt girl: {logicAction.boolValue}");
                    logger.Message($"Item to populate Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_MET:
                    logger.Message($"Girl id: {ModInterface.Data.GetDataId(GameDataType.Girl, logicAction.girlDefinition.id)}");
                    logger.Message($"Set value to: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_INVENTORY_ITEM:
                    logger.Message($"Item Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.BACKGROUND_MUSIC:
                    logger.Message($"Background Music: {logicAction.backgroundMusic?.clip?.name ?? "null"}");
                    break;
                case LogicActionType.UI_TAG_EVENT:
                    logger.Message($"Name: {logicAction.stringValue}");
                    break;
            }
        }
    }

    public static void LogLogicBundle(LogicBundle logicBundle, ILogger logger = null)
    {
        logger ??= ModInterface.Log;

        using (logger.MakeIndent("Conditions"))
        {
            foreach (var condition in logicBundle.conditions)
            {
                LogLogicCondition(condition);
            }
        }

        using (logger.MakeIndent("Actions"))
        {
            foreach (var condition in logicBundle.actions)
            {
                LogLogicAction(condition, logger);
            }
        }
    }
}