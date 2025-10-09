// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Save
{
    [Serializable]
    class ModSaveData
    {
        private static readonly int _defaultFileCount = 4;

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

            if (saveData.files != null)
            {
                AdditionalFiles = new List<SaveFile>();

                var defaultFiles = saveData.files.Take(_defaultFileCount).ToList();
                foreach (var file in defaultFiles)
                {
                    var newModSaveFile = new ModSaveFile();
                    newModSaveFile.Strip(file);
                    ModFiles.Add(newModSaveFile);
                }

                var fileTotal = AdditionalFiles.Count + _defaultFileCount;
                while (fileTotal > ModFiles.Count)
                {
                    ModFiles.Add(new());
                }

                foreach (var file in saveData.files.Skip(_defaultFileCount))
                {
                    AdditionalFiles.Add(file);
                    var modFile = new ModSaveFile();
                    modFile.Strip(file);
                    ModFiles.Add(modFile);
                }

                saveData.files = defaultFiles;
            }
        }

        /// <inheritdoc/>
        public void SetData(SaveData saveData)
        {
            if (saveData == null) { throw new ArgumentNullException(nameof(saveData)); }

            if (UnlockedCodes != null)
            {
                if (saveData.unlockedCodes == null)
                {
                    saveData.unlockedCodes = new List<int>();
                }

                // codes
                ModInterface.Log.LogInfo("Injecting Modded Codes");
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

                while (saveData.files.Count < _defaultFileCount)
                {
                    saveData.files.Add(new());
                }

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