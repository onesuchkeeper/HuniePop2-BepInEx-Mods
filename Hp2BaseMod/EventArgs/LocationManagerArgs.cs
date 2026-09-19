using System;
using DG.Tweening;

namespace Hp2BaseMod;

public class ArrivalCompletedArgs
{
    public CutsceneDefinition ArrivalCutscene;
}

public class ResolveDollStylesArgs
{
    /// <summary>The location dolls are being styled for.</summary>
    public LocationDefinition Location;
 
    /// <summary>The currently loaded pair, or null if none is loaded (e.g. Hub).</summary>
    public GirlPairDefinition GirlPair;
 
    /// <summary>Whether the pair's left/right sides are currently flipped.</summary>
    public bool SidesFlipped;
}

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
    public CutsceneDefinition arrivalCutscene;

    /// <summary>
    /// The location the player is arriving from.
    /// </summary>
    public LocationDefinition previousLocationDef;

    /// <summary>
    /// If set to true, aborts the arrival transition.
    /// </summary>
    public bool Canceled;
}

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
    /// Position right doll should move to on arrival.
    /// </summary>
    public DollPositionType RightDollPosition;
}

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

    /// <summary>
    /// Transition instance to use for this departure/arrival.
    /// </summary>
    public LocationTransition transition;
}

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

public class LocationSettledArgs
{
    public UiWindow actionBubblesWindow;

    public bool hasArrivalCutscene;
}