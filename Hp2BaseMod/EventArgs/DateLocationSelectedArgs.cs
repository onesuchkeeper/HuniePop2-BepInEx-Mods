using System;

namespace Hp2BaseMod;

public class DateLocationSelectedArgs : EventArgs
{
    public LocationDefinition Location;

    public int PlayerPoints;
    public int LeftPoints;
    public int RightPoints;

    public int LeftStaminaGain;
    public int RightStaminaGain;
}