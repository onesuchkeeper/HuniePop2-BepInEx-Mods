using System;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public class ModSaveInventorySlot
    {
        public RelativeId? ItemId;
        public int DayTimeStamp;

        public void Strip(SaveFileInventorySlot save)
        {
            if (save == null) { return; }

            ItemId = ModInterface.Data.GetDataId(GameDataType.Item, save.itemId);

            if (ItemId.Value.SourceId != -1)
            {
                save.itemId = -1;
            }
        }

        public void SetData(SaveFileInventorySlot save)
        {
            if (save == null) { return; }

            ValidatedSet.SetFromRelativeId(ref save.itemId, GameDataType.Item, ItemId);
        }

        public SaveFileInventorySlot Convert(int slotIndex)
        {
            var save = new SaveFileInventorySlot(slotIndex)
            {
                daytimeStamp = DayTimeStamp
            };

            ValidatedSet.SetFromRelativeId(ref save.itemId, GameDataType.Item, ItemId);
            return save;
        }
    }
}