using System;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public struct ModSaveFinderSlot : IModSave<SaveFileFinderSlot>
    {
        public RelativeId? GirlPairId;
        public bool SidesFlipped;

        public void Strip(SaveFileFinderSlot save)
        {
            if (save == null) { return; }

            SidesFlipped = save.sidesFlipped;

            GirlPairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, save.girlPairId);

            if (GirlPairId.Value.SourceId != -1)
            {
                save.girlPairId = -1;
            }
        }

        public void SetData(SaveFileFinderSlot save)
        {
            if (save == null) { return; }

            ValidatedSet.SetFromRelativeId(ref save.girlPairId, GameDataType.GirlPair, GirlPairId);
        }

        public SaveFileFinderSlot Convert(int locationRuntimeId)
        {
            var save = new SaveFileFinderSlot(locationRuntimeId)
            {
                sidesFlipped = SidesFlipped
            };
            ValidatedSet.SetFromRelativeId(ref save.girlPairId, GameDataType.GirlPair, GirlPairId);
            return save;
        }
    }
}