// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using Hp2BaseMod.ModGameData.Interface;

namespace Hp2BaseMod.ModGameData;

/// <summary>
/// Basic implementation of <see cref="IFunctionalCutsceneStep"/> using a provided action.
/// </summary>
public class FunctionalCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    public event CutsceneStepComplete Complete;

    private readonly Action<Action> _action;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="action">The action to preform. Must accept a callback delegate to notify when the step has been completed.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FunctionalCutsceneStep(Action<Action> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Act() => _action(() => Complete?.Invoke(this));
}