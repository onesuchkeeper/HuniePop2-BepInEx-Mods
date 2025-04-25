using System.Collections.Generic;
using Hp2BaseMod;

namespace SingleDate;

public class SaveData
{
    public List<SaveFile> SaveFiles;

    public bool ShowSingleUpsetHint;

    public void Clean()
    {
        SaveFiles ??= new List<SaveFile>();
    }
}