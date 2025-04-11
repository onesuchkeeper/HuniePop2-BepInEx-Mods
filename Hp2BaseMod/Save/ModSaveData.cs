// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Save
{
    [Serializable]
    class ModSaveData
    {
        private static int _defaultFileCount = 4;

        public List<RelativeId> UnlockedCodes;
        public List<ModSaveFile> DefaultFiles;
        public List<ModSaveFile> AddedFiles;
        public Dictionary<string, int> SourceGUID_Id = new Dictionary<string, int>();
        public Dictionary<int, string> SourceSaves = new Dictionary<int, string>();

        /// <inheritdoc/>
        public void Strip(SaveData saveData)
        {
            UnlockedCodes = null;
            DefaultFiles = null;
            AddedFiles = null;

            if (saveData == null) { return; }

            if (saveData.unlockedCodes != null)
            {
                ModInterface.Log.LogInfo("Stripping Modded Codes");
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
                ModInterface.Log.LogInfo("Stripping Modded data from save files");
                DefaultFiles = new List<ModSaveFile>();
                AddedFiles = new List<ModSaveFile>();

                foreach (var file in saveData.files.Take(_defaultFileCount))
                {
                    var newModSaveFile = new ModSaveFile();
                    newModSaveFile.Strip(file);
                    DefaultFiles.Add(newModSaveFile);
                }

                foreach (var file in saveData.files.Skip(_defaultFileCount))
                {
                    var modFile = new ModSaveFile();
                    modFile.Strip(file);
                    AddedFiles.Add(modFile);
                }

                saveData.files = saveData.files.Take(_defaultFileCount).ToList();
            }
        }

        /// <inheritdoc/>
        public void SetData(SaveData saveData)
        {
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

            if (DefaultFiles != null && saveData.files != null)
            {
                ModInterface.Log.LogInfo("Injecting Modded Files");
                while (saveData.files.Count < DefaultFiles.Count)
                {
                    saveData.files.Add(new SaveFile());
                }

                var saveDataIt = saveData.files.GetEnumerator();

                if (saveDataIt.MoveNext())
                {
                    foreach (var modSaveFile in DefaultFiles)
                    {
                        modSaveFile.SetData(saveDataIt.Current);
                        saveDataIt.MoveNext();
                    }
                }

                if (AddedFiles != null)
                {
                    foreach (var file in AddedFiles)
                    {
                        saveData.files.Add(file.Convert());
                    }
                }
            }
        }
    }
}