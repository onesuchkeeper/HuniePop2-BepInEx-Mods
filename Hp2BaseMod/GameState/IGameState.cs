using System;

namespace Hp2BaseMod;

public interface IGameState
{
    /// <summary>
    /// Unique relative ID identifying this game state.
    /// </summary>
    RelativeId Id { get; }
    bool IsSavable { get; }

    void DepartTransition(RelativeId? nextStateId, Action continueTransitionCallback);
    void DepartTransition(Action continueTransitionCallback);

    /// <summary>
    /// Called when transitioning into this state.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when transitioning out of this state.
    /// </summary>
    void Exit();

    /// <summary>
    /// Generates the text to show on the main menu's profile selection
    /// </summary>
    /// <returns></returns>
    string GetProfileString(PlayerFile playerFile);
}
