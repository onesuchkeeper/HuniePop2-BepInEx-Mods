namespace Hp2BaseMod;

public static class ItemDefinition_Ext
{
    public static RelativeId ModId(this ItemDefinition def)
        => ModInterface.Data.GetDataId(GameDataType.Item, def.id);
}