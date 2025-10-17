// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Save
{
    /// <summary>
    /// This splits mod data from default data. The goal is to keep as much valid data in the default save file as
    /// possible so the player keeps their progress even when uninstalling mods
    /// </summary>
    [Serializable]
    class ModSaveData
    {
        private static readonly int DEFAULT_FILE_COUNT = 4;

        public List<RelativeId> UnlockedCodes;
        public List<SaveFile> AdditionalFiles;
        public List<ModSaveFile> ModFiles = new();

        public ModSaveFile GetCurrentFile()
        {
            while (ModFiles.Count < Game.Persistence.loadedFileIndex + 1)
            {
                ModFiles.Add(new());
            }

            return ModFiles[Game.Persistence.loadedFileIndex];
        }

        public Dictionary<string, int> SourceGUID_Id = new Dictionary<string, int>();
        public Dictionary<int, string> SourceSaves = new Dictionary<int, string>();

        /// <summary>
        /// Removes modded data from the saveData and updates this with it
        /// </summary>
        /// <param name="saveData"></param>
        public void Strip(SaveData saveData)
        {
            UnlockedCodes = null;
            AdditionalFiles = null;

            if (saveData == null) { return; }

            //codes
            if (saveData.unlockedCodes != null)
            {
                UnlockedCodes = new List<RelativeId>();
                var defaultCodes = new List<int>();

                foreach (var code in saveData.unlockedCodes)
                {
                    if (DefaultData.IsDefaultCode(code))
                    {
                        defaultCodes.Add(code);
                    }
                    else
                    {
                        UnlockedCodes.Add(ModInterface.Data.GetDataId(GameDataType.Code, code));
                    }
                }

                saveData.unlockedCodes = defaultCodes;
            }

            //save files
            if (saveData.files != null)
            {
                SaveUtility.MatchListLength(saveData.files, ModFiles);

                foreach (var (file, mod) in saveData.files.Zip(ModFiles, (x, y) => (x, y)))
                {
                    mod.Strip(file);
                }

                AdditionalFiles = saveData.files.Skip(DEFAULT_FILE_COUNT).ToList();
                saveData.files = saveData.files.Take(DEFAULT_FILE_COUNT).ToList();
            }
        }

        /// <inheritdoc/>
        public void SetData(SaveData saveData)
        {
            if (saveData == null) { throw new ArgumentNullException(nameof(saveData)); }

            if (UnlockedCodes != null)
            {
                saveData.unlockedCodes ??= new();

                foreach (var code in UnlockedCodes)
                {
                    if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Code, code, out var runtimeId))
                    {
                        saveData.unlockedCodes.Add(runtimeId);
                    }
                }
            }

            if (AdditionalFiles != null)
            {
                saveData.files ??= new();

                SaveUtility.MatchListLength(DEFAULT_FILE_COUNT, saveData.files);

                saveData.files.AddRange(AdditionalFiles);
            }

            if (ModFiles != null && saveData.files != null)
            {
                foreach (var (mod, file) in ModFiles.Zip(saveData.files, (mod, file) => (mod, file)))
                {
                    mod.SetData(file);
                }
            }
        }
    }
}