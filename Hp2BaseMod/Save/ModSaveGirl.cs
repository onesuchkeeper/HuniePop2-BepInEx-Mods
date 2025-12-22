using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public class ModSaveGirl : IModSave<SaveFileGirl>
    {
        private static readonly int _defaultDateGiftSlotCount = 4;

        public bool PlayerMet;
        public int RelationshipPoints;
        public int RelationshipUpCount;
        public int StaminaFreeze;
        public bool StyleOnDates;

        public RelativeId? ActiveBaggage;
        public RelativeId HairstyleId;
        public RelativeId OutfitId;

        public List<RelativeId> LearnedBaggage;
        public List<RelativeId> ReceivedUniques;
        public List<RelativeId> ReceivedShoes;
        public List<RelativeId> LearnedFavs;
        public List<RelativeId> RecentHerQuestions;
        public List<RelativeId> UnlockedOutfits;
        public List<RelativeId> UnlockedHairstyles;
        public List<ModSaveInventorySlot> DateGiftSlots;
        public RelativeId BodyId;

        public void Strip(SaveFileGirl saveFileGirl)
        {
            var girlExpanded = ExpandedGirlDefinition.Get(saveFileGirl.girlId);

            PlayerMet = saveFileGirl.playerMet;
            RelationshipPoints = saveFileGirl.relationshipPoints;
            RelationshipUpCount = saveFileGirl.relationshipUpCount;
            StaminaFreeze = saveFileGirl.staminaFreeze;
            StyleOnDates = saveFileGirl.stylesOnDates;

            //the active baggage index refers to the position in the learned baggage, which is kinda weird but whatever
            //this needs to come before we strip the learnedBaggage
            if (saveFileGirl.activeBaggageIndex >= 0 && saveFileGirl.learnedBaggage.Count > saveFileGirl.activeBaggageIndex)
            {
                ActiveBaggage = ModInterface.Data.GetDataId(GameDataType.Ailment, saveFileGirl.learnedBaggage[saveFileGirl.activeBaggageIndex]);
            }

            LearnedBaggage = new List<RelativeId>();
            foreach (var baggage in saveFileGirl.learnedBaggage)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Ailment, baggage);
                LearnedBaggage.Add(id);
            }
            saveFileGirl.learnedBaggage = LearnedBaggage.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            //this needs to come after we strip the learned baggage so we don't default to any non-default baggage
            if (ActiveBaggage.HasValue && ActiveBaggage.Value.SourceId != -1)
            {
                saveFileGirl.activeBaggageIndex = saveFileGirl.learnedBaggage.First();
            }

            if (!girlExpanded.HairstyleLookup.TryGetId(saveFileGirl.hairstyleIndex, out HairstyleId))
            {
                HairstyleId = RelativeId.Default;
                saveFileGirl.hairstyleIndex = -1;
            }
            else if (HairstyleId.SourceId != -1)
            {
                saveFileGirl.hairstyleIndex = -1;
            }

            if (!girlExpanded.OutfitLookup.TryGetId(saveFileGirl.outfitIndex, out OutfitId))
            {
                OutfitId = RelativeId.Default;
                saveFileGirl.outfitIndex = -1;
            }
            else if (OutfitId.SourceId != -1)
            {
                saveFileGirl.outfitIndex = -1;
            }

            ReceivedUniques = new List<RelativeId>();
            foreach (var item in saveFileGirl.receivedUniques)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Item, item);
                ReceivedUniques.Add(id);
            }
            saveFileGirl.receivedUniques = ReceivedUniques.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            ReceivedShoes = new List<RelativeId>();
            foreach (var item in saveFileGirl.receivedShoes)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Item, item);
                ReceivedShoes.Add(id);
            }
            saveFileGirl.receivedShoes = ReceivedShoes.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            LearnedFavs = new List<RelativeId>();
            foreach (var fav in saveFileGirl.learnedFavs)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Question, fav);
                LearnedFavs.Add(id);
            }
            saveFileGirl.learnedFavs = LearnedFavs.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            RecentHerQuestions = new List<RelativeId>();
            foreach (var question in saveFileGirl.recentHerQuestions)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Question, question);
                RecentHerQuestions.Add(id);
            }
            saveFileGirl.recentHerQuestions = RecentHerQuestions.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            // Outfits
            UnlockedOutfits = new List<RelativeId>();
            foreach (var outfitIndex in saveFileGirl.unlockedOutfits)
            {
                if (girlExpanded.OutfitLookup.TryGetId(outfitIndex, out var id))
                {
                    UnlockedOutfits.Add(id);
                }
            }
            saveFileGirl.unlockedOutfits = UnlockedOutfits.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            // Hairstyle
            UnlockedHairstyles = new List<RelativeId>();
            foreach (var hairstyle in saveFileGirl.unlockedHairstyles)
            {
                if (girlExpanded.HairstyleLookup.TryGetId(hairstyle, out var id))
                {
                    UnlockedHairstyles.Add(id);
                }
            }
            saveFileGirl.unlockedHairstyles = UnlockedHairstyles.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            // DateGiftSlots
            DateGiftSlots = new();
            foreach (var slot in saveFileGirl.dateGiftSlots)
            {
                var saveMod = new ModSaveInventorySlot();
                saveMod.Strip(slot);
                DateGiftSlots.Add(saveMod);
            }
            saveFileGirl.dateGiftSlots = saveFileGirl.dateGiftSlots.Take(_defaultDateGiftSlotCount).ToList();
        }

        public void SetData(SaveFileGirl saveFileGirl)
        {
            //Inventory Slots
            if (DateGiftSlots != null)
            {
                var saveEnum = saveFileGirl.dateGiftSlots.GetEnumerator();
                var dataGiftSlotsIt = DateGiftSlots.GetEnumerator();

                while (saveEnum.MoveNext() && dataGiftSlotsIt.MoveNext())
                {
                    dataGiftSlotsIt.Current.SetData(saveEnum.Current);
                }

                var i = saveFileGirl.dateGiftSlots.Count;
                while (dataGiftSlotsIt.MoveNext())
                {
                    saveFileGirl.dateGiftSlots.Add(dataGiftSlotsIt.Current.Convert(i++));
                }
            }

            Inject(saveFileGirl);
        }

        public SaveFileGirl Convert(int runtimeId)
        {
            var save = new SaveFileGirl(runtimeId)
            {
                playerMet = PlayerMet,
                relationshipPoints = RelationshipPoints,
                relationshipUpCount = RelationshipUpCount,
                staminaFreeze = StaminaFreeze,
                stylesOnDates = StyleOnDates,
                learnedBaggage = new List<int>(),
                receivedUniques = new List<int>(),
                receivedShoes = new List<int>(),
                learnedFavs = new List<int>(),
                unlockedHairstyles = new List<int>(),
                unlockedOutfits = new List<int>(),
                recentHerQuestions = new List<int>(),
                dateGiftSlots = new List<SaveFileInventorySlot>()
            };

            if (DateGiftSlots != null)
            {
                var i = 0;
                foreach (var slot in DateGiftSlots)
                {
                    save.dateGiftSlots.Add(slot.Convert(i++));
                }
            }

            Inject(save);

            return save;
        }

        private void Inject(SaveFileGirl save)
        {
            var girlExpanded = ExpandedGirlDefinition.Get(save.girlId);

            if (girlExpanded.HairstyleLookup.TryGetIndex(HairstyleId, out var hairstyleIndex))
            {
                save.hairstyleIndex = hairstyleIndex;
            }

            if (girlExpanded.OutfitLookup.TryGetIndex(OutfitId, out var outfitIndex))
            {
                save.outfitIndex = outfitIndex;
            }

            ValidatedSet.SetModIds(ref save.learnedBaggage, LearnedBaggage, GameDataType.Ailment);
            ValidatedSet.SetModIds(ref save.receivedUniques, ReceivedUniques, GameDataType.Item);
            ValidatedSet.SetModIds(ref save.receivedShoes, ReceivedShoes, GameDataType.Item);
            ValidatedSet.SetModIds(ref save.learnedFavs, LearnedFavs, GameDataType.Question);
            ValidatedSet.SetModIds(ref save.recentHerQuestions, RecentHerQuestions, GameDataType.Question);

            foreach (var unlockedOutfit in UnlockedOutfits.OrEmptyIfNull())
            {
                if (girlExpanded.OutfitLookup.TryGetIndex(unlockedOutfit, out var index))
                {
                    save.unlockedOutfits.Add(index);
                }
            }

            foreach (var unlockedHairstyle in UnlockedHairstyles.OrEmptyIfNull())
            {
                if (girlExpanded.HairstyleLookup.TryGetIndex(unlockedHairstyle, out var index))
                {
                    save.unlockedHairstyles.Add(index);
                }
            }
        }
    }
}
