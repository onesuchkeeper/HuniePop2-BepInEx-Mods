using System.Collections.Generic;

internal class TweaksSaveData
{
    public List<TweaksSaveFile> Files;

    public TweaksSaveFile GetCurrentFile()
    {
        while ((Files.Count - 1) < Game.Persistence.loadedFileIndex)
        {
            var file = new TweaksSaveFile();
            file.Clean();
            Files.Add(file);
        }

        return Files[Game.Persistence.loadedFileIndex];
    }

    public void Clean()
    {
        Files ??= new List<TweaksSaveFile>();

        foreach (var file in Files)
        {
            file.Clean();
        }
    }
}