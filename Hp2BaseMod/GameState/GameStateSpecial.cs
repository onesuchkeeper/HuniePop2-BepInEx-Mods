using System;

namespace Hp2BaseMod;

/// <summary>
/// Active at special locations: scripted events where the game is running a story and the player isn't free to use the phone.
/// </summary>
public sealed class GameStateSpecial : GameStateLocation
{
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new GameStateSpecial(payload.IsSavable);

        public string GetProfileString(PlayerFile playerFile)
        {
            var location = Game.Persistence.playerFile.locationDefinition;

            return location != null
                ? location.locationName
                : "Special Event";
        }

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public class Payload(bool isSavable = true) : IGameStatePayload
    {
        public bool IsSavable = isSavable;
    }

    public override RelativeId Id => GameStateId.Special;

    public override bool IsSavable => _isSavable;
    private bool _isSavable;

    // ---- what the player can use here: nothing, the event is in charge ----

    public override bool HidesCellphone(UiCellphone cellphone) => true;

    // the finder shows its message 3 (not available)
    public override FinderStatus GetFinderStatus() => FinderStatus.Unavailable(3);

    public override bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot) => false;

    public override bool AllowsStore => false;
    public override bool StoreShowsNotice => false;
    public override bool AllowsPairApp => false;
    public override bool HasGirlsPresent => false;

    public override string GetGiftError(ItemDefinition item) => NoGirlsNearbyGiftError;

    public override bool DialogIgnoresCellphone => false;
    public override bool DollFocusIsInteractive => false;
    public override bool HintsSimStaminaCost => false;
    public override bool AllowsPause => true;

    private bool _disposed;
    public GameStateSpecial(bool isSavable) 
        : base()
    {
        _isSavable = isSavable;

        var locationManager = Game.Session.Location.GetExpansion(); 
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.LocationArrived += On_LocationArrived;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            var locationManager = Game.Session.Location.GetExpansion(); 
            locationManager.ResolveDollStyles -= OnResolveDollStyles;
            locationManager.LocationArrived -= On_LocationArrived;
        }

        base.Dispose(disposing);
    }

    private void OnResolveDollStyles(ResolveDollStylesArgs args) => ApplyDefaultDollStyles(args);

    public override void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        if (payload != null)
        {
            _pendingState = payload;
        }
        
        continueTransitionCallback();
    }

    private void On_LocationArrived(LocationArriveArgs args)
    {
        Game.Session.Puzzle.puzzleStatus.Clear();
        FinishArrival(args);
    }

    public override IGameStatePayload GeneratePayload() => new Payload();
    public override bool AutosavesOnArrival => true;
    public override bool ShowsHubMenu => false;
    public override bool UseStylesOnDatesFlag => false;
}