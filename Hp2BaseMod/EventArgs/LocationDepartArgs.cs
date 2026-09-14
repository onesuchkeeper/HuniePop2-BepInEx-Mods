namespace Hp2BaseMod;

public class LocationDepartArgs
{
    public LocationDefinition to;
    public LocationDefinition from;
    public GirlPairDefinition girlPairDef;
    public bool sidesFlipped;

    /// <summary>
    /// Set to true to cancel standard LocationManager.Depart execution.
    /// </summary>
    public bool Canceled;
}