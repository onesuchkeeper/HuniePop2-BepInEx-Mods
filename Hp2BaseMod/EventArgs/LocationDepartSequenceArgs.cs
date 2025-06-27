// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using DG.Tweening;

namespace Hp2BaseMod;

/// <summary>
/// Args used in <see cref="ModEvents.LocationDepartSequence"/>.
/// </summary>
public class LocationDepartSequenceArgs : EventArgs
{
    /// <summary>
    /// Sequence played when departing location.
    /// </summary>
    public Sequence Sequence;
}
