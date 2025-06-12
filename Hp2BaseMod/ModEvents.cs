using System;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod
{
    public class ModEvents
    {
        /// <summary>
        /// Notifies before the game is saved
        /// </summary>
        public event Action PreGameSave;
        internal void NotifyPreSave() => PreGameSave?.Invoke();

        /// <summary>
        /// Notifies after the game is saved
        /// </summary>
        public event Action PostGameSave;
        internal void NotifyPostSave() => PostGameSave?.Invoke();

        /// <summary>
        /// Notifies before the save file is loaded
        /// </summary>
        public event Action<PlayerFile> PreLoadPlayerFile;
        internal void NotifyPreLoadSaveFile(PlayerFile file) => PreLoadPlayerFile?.Invoke(file);

        /// <summary>
        /// Notifies before data mods are implemented
        /// </summary>
        public event Action PreDataMods;
        internal void NotifyPreDataMods() => PreDataMods?.Invoke();

        /// <summary>
        /// Notifies after data mods have been implemented
        /// </summary>
        public event Action PostDataMods;
        internal void NotifyPostDataMods() => PostDataMods?.Invoke();

        /// <summary>
        /// Notifies when a girl's style will potentially change<br/>
        /// </summary>
        public event Action<RequestStyleChangeEventArgs> RequestStyleChange;
        internal RequestStyleChangeEventArgs NotifyRequestStyleChange(GirlDefinition girl, LocationDefinition loc, float percentage, GirlStyleInfo style)
        {
            var args = new RequestStyleChangeEventArgs(girl, loc, percentage, style);
            RequestStyleChange?.Invoke(args);
            return args;
        }

        /// <summary>
        /// Triggers before SaveData is applied to the persistence<br/>
        /// This can be used to add additional data or unlock things
        /// </summary>
        public event Action<SaveData> PrePersistenceReset;
        internal void NotifyPrePersistenceReset(SaveData playerData) => PrePersistenceReset?.Invoke(playerData);

        /// <summary>
        /// Notifies after SaveData is applied to the persistence
        /// </summary>
        public event Action<SaveData> PostPersistenceReset;
        internal void NotifyPostPersistenceReset(SaveData playerData) => PostPersistenceReset?.Invoke(playerData);

        /// <summary>
        /// Notifies after a code is submitted
        /// </summary>
        public event Action<CodeDefinition> PostCodeSubmitted;
        internal void NotifyPostCodeSubmitted(CodeDefinition codeDefinition) => PostCodeSubmitted?.Invoke(codeDefinition);

        public event Action PreRoundOverCutscene;
        internal void NotifyPreRoundOverCutscene() => PreRoundOverCutscene?.Invoke();

        public event Action<FinderSlotPopulateEventArgs> FinderSlotsPopulate;
        internal void NotifyPreFinderSlotPopulatePairs(FinderSlotPopulateEventArgs args) => FinderSlotsPopulate?.Invoke(args);

        public event Action<RequestUnlockedPhotosEventArgs> RequestUnlockedPhotos;
        internal void NotifyRequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args) => RequestUnlockedPhotos?.Invoke(args);

        public event Action<LocationArriveSequenceArgs> LocationArriveSequence;
        internal void NotifyLocationArriveSequence(LocationArriveSequenceArgs sequence) => LocationArriveSequence?.Invoke(sequence);

        public event Action<LocationDepartSequenceArgs> LocationDepartSequence;
        internal void NotifyLocationDepartSequence(LocationDepartSequenceArgs sequence) => LocationDepartSequence?.Invoke(sequence);

        public event Action<RandomDollSelectedArgs> RandomDollSelected;
        internal void NotifyRandomDollSelected(RandomDollSelectedArgs args) => RandomDollSelected?.Invoke(args);

        public event Action<DateLocationSelectedArgs> DateLocationSelected;
        internal void NotifyDateLocationSelected(DateLocationSelectedArgs args) => DateLocationSelected?.Invoke(args);
    }
}