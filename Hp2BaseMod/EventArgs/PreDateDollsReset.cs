using System;

namespace Hp2BaseMod;

public class PreDateDollResetArgs : EventArgs
{
    public enum StyleType
    {
        File,
        Location,
        Sex
    }

    public StyleType Style;
}