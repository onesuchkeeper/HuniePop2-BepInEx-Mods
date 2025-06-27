// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using DG.Tweening;

namespace Hp2BaseMod;

/// <summary>
/// Args used in <see cref="ModEvents.LocationArriveSequence"/>.
/// </summary>
public class LocationArriveSequenceArgs : EventArgs
{
    /// <summary>
    /// Sequence played upon arriving.
    /// </summary>
    public Sequence Sequence;

    /// <summary>
    /// Position left doll should move to on arrival.
    /// </summary>
    public DollPositionType LeftDollPosition;

    /// <summary>
    /// Position left doll should move to on arrival.
    /// </summary>
    public DollPositionType RightDollPosition;
}
