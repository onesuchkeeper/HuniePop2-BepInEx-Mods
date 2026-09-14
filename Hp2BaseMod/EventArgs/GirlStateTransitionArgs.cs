using System;

namespace Hp2BaseMod;

public class GirlStateTransitionArgs : EventArgs
{
    public PuzzleStatusGirl StatusGirl { get; }
    public RelativeId CurrentStateId { get; }
    public RelativeId TargetStateId { get; set; }

    public GirlStateTransitionArgs(PuzzleStatusGirl statusGirl, RelativeId currentStateId, RelativeId targetStateId)
    {
        StatusGirl = statusGirl;
        CurrentStateId = currentStateId;
        TargetStateId = targetStateId;
    }
}
