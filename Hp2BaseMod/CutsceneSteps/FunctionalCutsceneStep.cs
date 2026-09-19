using System;
using Hp2BaseMod.ModGameData.Interface;

namespace Hp2BaseMod.ModGameData;

public class FunctionalCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    public event CutsceneStepComplete Complete;

    private readonly Action<Action> _action;

    public FunctionalCutsceneStep(Action<Action> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Act() => _action(() => Complete?.Invoke(this));
}