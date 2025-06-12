using System;
using System.Collections.Generic;

namespace SingleDate;

[Serializable]
public class SingleSaveData
{
    public List<SingleSaveFile> SaveFiles;

    public void Clean()
    {
        SaveFiles ??= new List<SingleSaveFile>();
        SaveFiles.ForEach(x => x.Clean());
    }
}
