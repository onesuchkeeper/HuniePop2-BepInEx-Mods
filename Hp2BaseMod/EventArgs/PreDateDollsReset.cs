using System;

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