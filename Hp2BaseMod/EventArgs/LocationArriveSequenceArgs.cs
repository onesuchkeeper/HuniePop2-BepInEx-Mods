using System;
using DG.Tweening;

namespace Hp2BaseMod;

public class LocationArriveSequenceArgs : EventArgs
{
    public Sequence Sequence;

    public DollPositionType LeftDollPosition;

    public DollPositionType RightDollPosition;
}
