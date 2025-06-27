// Hp2BaseMod 2025, By OneSuchKeeper

using System;

namespace Hp2BaseMod;

/// <summary>
/// Args used in <see cref="ModEvents.RandomDollSelected"/>.
/// </summary>
public class RandomDollSelectedArgs : EventArgs
{
    /// <summary>
    /// The doll being selected.
    /// </summary>
    public UiDoll SelectedDoll;
}