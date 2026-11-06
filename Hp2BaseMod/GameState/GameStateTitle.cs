using System;

namespace Hp2BaseMod; 

public class GameStateTitle : IGameState 
{ 
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new GameStateTitle();

        public string GetProfileString(PlayerFile playerFile) => string.Empty;

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            error = "Individual save files cannot be loaded into title state";
            return false;
        }
    }

    [Serializable]
    public class Payload : IGameStatePayload
    {
        
    }

    public RelativeId Id => GameStateId.Title; 
    
    public bool IsSavable => false;

    private bool _disposed;
    public GameStateTitle() 
    { 
        ModInterface.Events.GameSessionBegan += OnGameSessionBegan;
    }
    
    public void Dispose() 
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            ModInterface.Events.GameSessionBegan -= OnGameSessionBegan; 
        }
    }
    
    private void OnGameSessionBegan(GameSession gameSession) 
    { 
        var targetStatePayload = ModInterface.Save.GetCurrentFile().GameStatePayload ?? new GameStateHub.Payload(); 
        ModInterface.GameState.ChangeState(targetStatePayload);
    }

    public void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        throw new NotImplementedException();
    }

    public void DepartTransition(Action continueTransitionCallback)
    {
        throw new NotImplementedException();
    }

    public IGameStatePayload GeneratePayload() => new Payload();

    public bool AllowGifts => false;

    // ---- what the player can use here ----
    //
    // The title has no session: there is no phone, no girls and nothing to buy. These are only asked if something
    // reaches for them anyway, and the answer is always "no".

    public bool HidesCellphone(UiCellphone cellphone) => throw new NotImplementedException();

    public FinderStatus GetFinderStatus() => throw new NotImplementedException();

    public bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot) => throw new NotImplementedException();

    public bool AllowsStore => throw new NotImplementedException();
    public bool StoreShowsNotice => throw new NotImplementedException();
    public bool AllowsPairApp => throw new NotImplementedException();
    public bool HasGirlsPresent => throw new NotImplementedException();

    public string GetGiftError(ItemDefinition item) => throw new NotImplementedException();

    public bool DialogIgnoresCellphone => throw new NotImplementedException();
    public bool DollFocusIsInteractive => throw new NotImplementedException();
    public bool HintsSimStaminaCost => throw new NotImplementedException();
    public bool AllowsPause => throw new NotImplementedException();
    public bool AutosavesOnArrival => throw new NotImplementedException();

    public bool ShowsHubMenu => throw new NotImplementedException();
    public bool UseStylesOnDatesFlag => throw new NotImplementedException();
}