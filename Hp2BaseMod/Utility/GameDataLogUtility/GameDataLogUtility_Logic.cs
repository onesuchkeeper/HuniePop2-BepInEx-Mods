namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void LogLogicCondition(LogicCondition logicCondition)
    {
        if (logicCondition == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {logicCondition.type}"))
        {
            switch (logicCondition.type)
            {
                case LogicConditionType.LOCATION:
                    ModInterface.Log.Message($"Must be {ModInterface.Data.GetDataId(GameDataType.Location, logicCondition.locationDefinition.id)} - {logicCondition.locationDefinition.locationName}");
                    break;
                case LogicConditionType.DAYTIME:
                    ModInterface.Log.Message($"Must be {logicCondition.daytimeType}");
                    break;
                case LogicConditionType.GIRL_PAIR:
                    ModInterface.Log.Message($"Must be {ModInterface.Data.GetDataId(GameDataType.GirlPair, logicCondition.girlPairDefinition.id)} - {logicCondition.girlPairDefinition.name}");
                    break;
                case LogicConditionType.GIRL_ORIENTATION:
                    ModInterface.Log.Message($"Doll on {logicCondition.dollOrientation} must be {ModInterface.Data.GetDataId(GameDataType.Girl, logicCondition.girlDefinition.id)} - {logicCondition.girlDefinition.name}");
                    break;
                case LogicConditionType.WING_COUNT:
                    ModInterface.Log.Message($"Must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FRUIT_COUNT:
                    ModInterface.Log.Message($"{logicCondition.affectionType} fruit must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.PUZZLE_RESOURCE:
                    ModInterface.Log.Message($"{logicCondition.resourceType} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.STORY_PROGRESS:
                    ModInterface.Log.Message($"must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FLAG_VALUE:
                    ModInterface.Log.Message($"{logicCondition.stringValue} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.IS_TUTORIAL:
                    ModInterface.Log.Message($"Must be {logicCondition.boolValue}");
                    break;
                case LogicConditionType.DIFFICULTY:
                    ModInterface.Log.Message($"Must be {logicCondition.settingDifficulty}");
                    break;
                case LogicConditionType.DATE_TYPE:
                    ModInterface.Log.Message($"Must be {logicCondition.dateType}");
                    break;
            }
        }
    }

    public static void LogLogicAction(LogicAction logicAction)
    {
        if (logicAction == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {logicAction.type}"))
        {
            switch (logicAction.type)
            {
                case LogicActionType.TRAVEL_TO:
                    ModInterface.Log.Message($"Time passing amount: {logicAction.intValue}");
                    ModInterface.Log.Message($"Destination location definition: {ModInterface.Data.GetDataId(GameDataType.Location, logicAction.locationDefinition.id)} - {logicAction.locationDefinition.locationName}");
                    var pairString = logicAction.girlPairDefinition == null
                        ? "null"
                        : $"{ModInterface.Data.GetDataId(GameDataType.GirlPair, logicAction.girlPairDefinition.id)} - {logicAction.girlPairDefinition.name}";
                    ModInterface.Log.Message($"Pair at loc {pairString}");
                    ModInterface.Log.Message($"Pair sides flipped: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_WINGS:
                    ModInterface.Log.Message($"This action just doesn't do anything! Odd...");
                    break;
                case LogicActionType.ADD_FRUIT:
                    ModInterface.Log.Message($"Fruit type: {logicAction.affectionType}");
                    ModInterface.Log.Message($"Fruit count: {logicAction.intValue}");
                    break;
                case LogicActionType.ADD_PUZZLE_RESOURCE:
                    ModInterface.Log.Message($"Resource type: {logicAction.resourceType}");
                    ModInterface.Log.Message($"Resource amount: {logicAction.intValue}");
                    ModInterface.Log.Message($"Give to alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.SET_STORY_PROGRESS:
                    ModInterface.Log.Message($"Story Progress value: {logicAction.intValue}");
                    break;
                case LogicActionType.SET_FLAG:
                    ModInterface.Log.Message($"Flag name: {logicAction.stringValue}");
                    ModInterface.Log.Message($"Flag value: {logicAction.intValue}");
                    break;
                case LogicActionType.START_CUTSCENE:
                    ModInterface.Log.Message($"Cutscene id: {ModInterface.Data.GetDataId(GameDataType.Cutscene, logicAction.cutsceneDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_FOCUS:
                    ModInterface.Log.Message($"Focus alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.RESET_DOLLS:
                    ModInterface.Log.Message($"Unload: {logicAction.boolValue}");
                    break;
                case LogicActionType.DIALOG_OPTION_FLAG:
                    ModInterface.Log.Message($"Flag gets set the the current 'Game.Session.Dialog.selectedDialogOptionIndex'");
                    ModInterface.Log.Message($"Flag name: {logicAction.stringValue}");
                    break;
                case LogicActionType.CHANGE_PUZZLE_STATE:
                    ModInterface.Log.Message($"Puzzle state: {logicAction.puzzleState}");
                    break;
                case LogicActionType.POP_DATE_GIFT:
                    ModInterface.Log.Message($"Target alt girl: {logicAction.boolValue}");
                    ModInterface.Log.Message($"Item to populate Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_MET:
                    ModInterface.Log.Message($"Girl id: {ModInterface.Data.GetDataId(GameDataType.Girl, logicAction.girlDefinition.id)}");
                    ModInterface.Log.Message($"Set value to: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_INVENTORY_ITEM:
                    ModInterface.Log.Message($"Item Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.BACKGROUND_MUSIC:
                    ModInterface.Log.Message($"Background Music: {logicAction.backgroundMusic?.clip?.name ?? "null"}");
                    break;
                case LogicActionType.UI_TAG_EVENT:
                    ModInterface.Log.Message($"Name: {logicAction.stringValue}");
                    break;
            }
        }
    }

    public static void LogLogicBundle(LogicBundle logicBundle)
    {
        using (ModInterface.Log.MakeIndent("Conditions"))
        {
            foreach (var condition in logicBundle.conditions)
            {
                LogLogicCondition(condition);
            }
        }

        using (ModInterface.Log.MakeIndent("Actions"))
        {
            foreach (var condition in logicBundle.actions)
            {
                LogLogicAction(condition);
            }
        }
    }
}