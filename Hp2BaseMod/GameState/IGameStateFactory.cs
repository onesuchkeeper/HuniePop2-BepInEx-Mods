namespace Hp2BaseMod;

public interface IGameStateFactory
{
    /// <summary>
    /// Generates the text to show on the main menu's profile selection
    /// </summary>
    /// <returns></returns>
    string GetProfileString(PlayerFile playerFile);

    /// <summary>
    /// Determines if the save allows for the game to be in this state.
    /// </summary>
    /// <returns>True if state is allowed, false otherwise.</returns>
    bool ValidateSave(SaveFile saveFile, out string error);
}

public interface IGameStateFactory<T> : IGameStateFactory
    where T : IGameStatePayload
{
    public IGameState Create(T payload);
}