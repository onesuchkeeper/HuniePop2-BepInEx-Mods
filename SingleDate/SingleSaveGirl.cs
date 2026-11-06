using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

namespace SingleDate;

[Serializable]
/// <summary>One girl's saved single date progress.</summary>
public class SingleSaveGirl
{
    /// <summary>
    /// Her single date level, from 0 to <see cref="Plugin.MaxSingleGirlRelationshipLevel"/>. Tracks progress toward
    /// completing the pair and is separate from the base game's relationship type so extra stages can be configured.
    /// One below max is when the sex date is available, max is lovers.
    /// </summary>
    public int RelationshipLevel;
    /// <summary>The date and sex photos she has shown, used to prefer new photos and to fill the photo gallery.</summary>
    public HashSet<RelativeId> UnlockedPhotos;

    /// <summary>Creates missing data and removes photos that are no longer registered.</summary>
    public void Clean()
    {
        UnlockedPhotos ??= new();
        UnlockedPhotos = [.. UnlockedPhotos.Where(x => ModInterface.Data.IsRegistered(GameDataType.Photo, x))];
    }
}
