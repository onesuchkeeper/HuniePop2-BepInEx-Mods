// Hp2BaseMod 2025, By OneSuchKeeper

using System;

namespace Hp2BaseMod;

/// <summary>
/// Args used in <see cref="ModEvents.DateLocationSelected"/>.
/// </summary>
public class DateLocationSelectedArgs : EventArgs
{
    public LocationDefinition Location;

    /// <summary>
    /// Points received by the player for initiating the date.
    /// </summary>
    public int PlayerPoints;

    /// <summary>
    /// Points received for the left girl for initiating the date.
    /// </summary>
    public int LeftPoints;

    /// <summary>
    /// Points received for the right girl for initiating the date.
    /// </summary>
    public int RightPoints;

    /// <summary>
    /// Stamina received for the left girl for initiating the date.
    /// </summary>
    public int LeftStaminaGain;

    /// <summary>
    /// Stamina received for the right girl for initiating the date.
    /// </summary>
    public int RightStaminaGain;

    /// <summary>
    /// If the date should not be allowed to occur.
    /// </summary>
    public bool DenyDate;
}