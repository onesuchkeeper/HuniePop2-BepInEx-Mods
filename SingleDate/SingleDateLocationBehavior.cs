using System;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// What every single date game state (<see cref="SingleDateSim"/> and <see cref="SingleDatePuzzleState"/>) does to the
/// location while it is active: one girl on the right, the empty left doll hidden, the cellphone on the left.
/// </summary>
/// <remarks>
/// A state creates one of these in its constructor and disposes it with itself, so the handlers only exist while a
/// single date is running. That replaces <c>Plugin.On_GameStateChanged</c> and the <c>State.IsSingleDate</c> checks
/// the global handlers needed.
/// </remarks>
internal sealed class SingleDateLocationBehavior : IDisposable
{
    // True while the right doll's drop zone is shifted to the INNER slot. Static because one state is disposed before the
    // next one is created, and the shift is applied by whichever single date arrival comes first.
    private static bool _dropZoneShifted;

    private bool _disposed;

    public SingleDateLocationBehavior()
    {
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.LocationArriveSequence += On_LocationArriveSequence;
        locationManager.PreLocationArrive += On_PreLocationArrive;
        locationManager.PreLocationSettled += On_PreLocationSettled;

        ModInterface.Events.RandomDollSelected += On_RandomDollSelected;
        ModInterface.Events.RequestGirlStateTransition += On_RequestGirlStateTransition;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (Game.Session != null)
        {
            var locationManager = Game.Session.Location.GetExpansion();
            locationManager.LocationArriveSequence -= On_LocationArriveSequence;
            locationManager.PreLocationArrive -= On_PreLocationArrive;
            locationManager.PreLocationSettled -= On_PreLocationSettled;
        }

        ModInterface.Events.RandomDollSelected -= On_RandomDollSelected;
        ModInterface.Events.RequestGirlStateTransition -= On_RequestGirlStateTransition;

        // Leaving single date behavior, put the drop zone back where the base game expects it. The next single date
        // arrival (if there is one) shifts it again.
        RestoreDropZone();
    }

    /// <summary>Distance between the right doll's OUTER and INNER positions.</summary>
    private static Vector2 GetDollShift()
    {
        var dollRight = Game.Session.gameCanvas.dollRight;
        return dollRight.GetPositionByType(DollPositionType.INNER) - dollRight.GetPositionByType(DollPositionType.OUTER);
    }

    private static void RestoreDropZone()
    {
        if (!_dropZoneShifted) return;
        _dropZoneShifted = false;

        if (Game.Session == null || Game.Session.gameCanvas == null) return;

        var shift = GetDollShift();
        Game.Session.gameCanvas.dollRight.dropZone.transform.localPosition -= new Vector3(shift.x, shift.y, 0);
    }

    /// <summary>
    /// The left doll is hidden (it is the empty "Nobody" girl) and the right doll is moved to the INNER slot, so the one
    /// girl stands where the pair's right-hand doll normally stands closest to the center. The doll's drop zone (where
    /// gifts can be dropped) and slide layer are shifted by the same amount so they follow her.
    /// </summary>
    private void On_LocationArriveSequence(LocationArriveSequenceArgs sequence)
    {
        var shift = GetDollShift();
        var dollRight = Game.Session.gameCanvas.dollRight;

        sequence.LeftDollPosition = DollPositionType.HIDDEN;
        sequence.RightDollPosition = DollPositionType.INNER;

        // Only shift the drop zone once, even if multiple single date arrivals happen in a row.
        if (!_dropZoneShifted)
        {
            _dropZoneShifted = true;
            dollRight.dropZone.transform.localPosition += new Vector3(shift.x, shift.y, 0);
        }

        // The base game snaps the slide layer to OUTER during arrival setup, so add the OUTER->INNER delta each arrival.
        dollRight.slideLayer.anchoredPosition += shift;
    }

    /// <summary>Sides are never flipped and the phone is on the left, the empty left doll's side.</summary>
    private void On_PreLocationArrive(LocationArriveArgs args)
    {
        args.sidesFlipped = false;
        args.cellphoneOnLeft = true;
    }

    /// <summary>Swaps in the single date action bubbles (talk/date).</summary>
    private void On_PreLocationSettled(LocationSettledArgs args)
        => args.actionBubblesWindow = UiPrefabs.SingleDateBubbles;

    /// <summary>The right doll is the only doll that can speak.</summary>
    private void On_RandomDollSelected(RandomDollSelectedArgs args)
        => args.SelectedDoll = Game.Session.gameCanvas.dollRight;

    /// <summary>Redirects the base mod's Upset state to <see cref="SingleDateUpsetGirlState"/>.</summary>
    private void On_RequestGirlStateTransition(GirlStateTransitionArgs args)
    {
        if (args.TargetStateId == Hp2BaseMod.PuzzleStatusGirlStateId.Upset)
        {
            args.TargetStateId = PuzzleStatusGirlStateId.SingleDateUpset;
        }
    }
}