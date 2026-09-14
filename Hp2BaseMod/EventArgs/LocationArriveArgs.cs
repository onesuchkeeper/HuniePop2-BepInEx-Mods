namespace Hp2BaseMod;

public class LocationArriveArgs 
{ 
    /// <summary>
    /// The location to arrive at.
    /// </summary>
    public LocationDefinition locationDef;

    /// <summary>
    /// The pair to be at the location we arrive at
    /// </summary>
    public GirlPairDefinition girlPairDef;

    /// <summary>
    /// If the sides of the pair should be flipped
    /// </summary>
    public bool sidesFlipped;

    /// <summary>
    /// If this is the first arrival in the game session
    /// </summary>
    public bool initialArrive;

    /// <summary>
    /// If the cellphone should be placed on the left when arriving
    /// </summary>
    public bool cellphoneOnLeft;

    /// <summary>
    /// Cutscene to play when arriving
    /// </summary>
    public CutsceneDefinition meetingCutscene;

    /// <summary>
    /// The location the player is arriving from.
    /// </summary>
    public LocationDefinition previousLocationDef;

    /// <summary>
    /// If set to true, aborts the arrival transition.
    /// </summary>
    public bool Canceled;
}
