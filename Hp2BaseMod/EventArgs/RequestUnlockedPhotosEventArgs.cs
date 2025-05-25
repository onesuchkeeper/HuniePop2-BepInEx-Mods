using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

public class RequestUnlockedPhotosEventArgs : EventArgs
{
    /// <summary>
    /// Photos unlocked and accessible by the player. Will be parsed after event completes so don't worry about duplicates or order.
    /// </summary>
    public List<PhotoDefinition> UnlockedPhotos;
}
