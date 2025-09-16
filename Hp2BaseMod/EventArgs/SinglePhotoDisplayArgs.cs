// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

public class SinglePhotoDisplayArgs : EventArgs
{
    // The photo to be shown
    public RelativeId BigPhotoId;

    // Pool of photos to be shown after the current photo closes
    public List<RelativeId> NextPhotos;
}