namespace Hp2BaseMod;

public interface IGameState
{
    /// <summary>
    /// Unique relative ID identifying this game state.
    /// </summary>
    RelativeId Id { get; }

    /// <summary>
    /// Controls whether the game can be paused in this state.
    /// </summary>
    bool AllowPause { get; }

    /// <summary>
    /// Controls whether save data persistence is permitted in this state.
    /// </summary>
    bool AllowSave { get; }

    /// <summary>
    /// Controls whether the cellphone is repositioned to the left side of the screen.
    /// </summary>
    bool UseLeftCellphone { get; }

    /// <summary>
    /// Called when transitioning into this state.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when transitioning out of this state.
    /// </summary>
    void Exit();

    /// <summary>
    /// Handles location arrival preparation (e.g., setting up doll positions, backgrounds).
    /// </summary>
    void OnLocationArrive(LocationArriveArgs args);

    /// <summary>
    /// Defines what happens when a location transition settles (e.g., launching puzzle, opening action bubbles, starting hub).
    /// </summary>
    void OnLocationSettled(LocationSettledArgs args);

    /// <summary>
    /// Resets character dolls according to state rules.
    /// </summary>
    void ResetDolls(bool unload = false);
}