using System;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public class RequestStyleChangeEventArgs : EventArgs
{
    /// <summary>
    /// Percentage of style change to be applied between 0 and 1
    /// </summary>
    public float ApplyChance;

    /// <summary>
    /// The style to change the girl to
    /// </summary>
    public GirlStyleInfo Style;

    /// <summary>
    /// The definition of the girl
    /// </summary>
    public GirlDefinition Def => _def;
    private GirlDefinition _def;

    /// <summary>
    /// The location traveled to
    /// </summary>
    public LocationDefinition Loc => _loc;
    private LocationDefinition _loc;

    public RequestStyleChangeEventArgs(GirlDefinition def, LocationDefinition loc, float percentage, GirlStyleInfo style)
    {
        _def = def;
        _loc = loc;
        ApplyChance = percentage;
        Style = style;
    }
}
