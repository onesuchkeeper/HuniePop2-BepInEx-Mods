using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

namespace SingleDate;

[Serializable]
public class SingleSaveGirl
{
    public int RelationshipLevel;
    public HashSet<RelativeId> UnlockedPhotos;

    public void Clean()
    {
        UnlockedPhotos ??= new();
        UnlockedPhotos = new HashSet<RelativeId>(UnlockedPhotos.Where(x => ModInterface.Data.IsRegistered(GameDataType.Photo, x)));
    }
}
