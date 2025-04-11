using System;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public struct ModSaveStoreProduct
    {
        public RelativeId? ItemId;
        public int ItemCost;

        public void Strip(SaveFileStoreProduct save)
        {
            if (save == null) { return; }

            ItemCost = save.itemCost;

            ItemId = ModInterface.Data.GetDataId(GameDataType.Item, save.itemId);

            if (ItemId.Value.SourceId != -1)
            {
                save.itemId = -1;
            }
        }

        public void SetData(SaveFileStoreProduct save)
        {
            if (save == null) { return; }

            ValidatedSet.SetFromRelativeId(ref save.itemId, GameDataType.Item, ItemId);

            if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Item, ItemId, out var id))
            {
                save.itemId = id;
            }
        }

        public SaveFileStoreProduct Convert(int index)
        {
            var save = new SaveFileStoreProduct(index)
            {
                itemCost = ItemCost
            };
            ValidatedSet.SetFromRelativeId(ref save.itemId, GameDataType.Item, ItemId);
            return save;
        }
    }
}