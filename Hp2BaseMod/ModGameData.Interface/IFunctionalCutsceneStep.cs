namespace Hp2BaseMod.ModGameData.Interface;

public delegate void CutsceneStepComplete(IFunctionalCutsceneStep source);

/// <summary>
/// A cutscene step handled functionally rather than data driven.
/// </summary>
public interface IFunctionalCutsceneStep
{
    /// <summary>
    /// Notifies when the step has completed
    /// </summary>
    public event CutsceneStepComplete Complete;

    /// <summary>
    /// Starts preforming the step
    /// </summary>
    public void Act();
}