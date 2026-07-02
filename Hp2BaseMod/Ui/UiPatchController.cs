using System.Collections;
using UnityEngine;

public abstract class UiPatchController<T> where T : MonoBehaviour
{
    private enum State
    {
        None,
        Running,
        Applied,
        Destroyed
    }

    protected bool IsAlive => _state != State.Destroyed;
    protected bool IsApplied => _state == State.Applied;

    protected readonly T _core;

    private State _state = State.None;
    private Coroutine _routine;

    protected UiPatchController(T core)
    {
        _core = core;
    }

    /// <summary>
    /// Starts the ui. Must be called via a harmony prefix on the
    /// base classes Start method.
    /// </summary>
    public void Start()
    {
        if (_state != State.None) return;

        if (_core == null) return;

        _state = State.Running;

        OnAttach();

        _routine = _core.StartCoroutine(Pipeline());
    }

    /// <summary>
    /// Handles the destruction of the ui. Must be called via a harmony prefix on the
    /// base classes OnDestroy method.
    /// </summary>
    public void OnDestroy()
    {
        if (_state == State.Destroyed) return;

        _state = State.Destroyed;

        if (_routine != null)
        {
            _core.StopCoroutine(_routine);
            _routine = null;
        }

        OnCleanup();
    }

    private IEnumerator Pipeline()
    {
        // Let Unity Start() finish
        yield return null;

        // Let vanilla Refresh/Populate complete
        yield return null;

        // Let layout settle
        yield return new WaitForEndOfFrame();

        // extra safety frame (Unity UI edge cases)
        yield return null;

        // HARD GUARANTEE: Apply only ever runs once
        if (_state != State.Running) yield break;

        _state = State.Applied;

        Apply();
    }

    /// <summary>
    /// Called when starting, do not modify ui, use <see cref="Apply"> for that instead
    /// </summary>
    protected virtual void OnAttach() { }

    /// <summary>
    /// Called after base ui has been fully loaded. Use as an entry point to modify game ui.
    /// </summary>
    protected abstract void Apply();

    /// <summary>
    /// Called when ui instance is being destroyed
    /// </summary>
    protected virtual void OnCleanup() { }
}