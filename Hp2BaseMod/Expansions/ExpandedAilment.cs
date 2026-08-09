namespace Hp2BaseMod;

/// <summary>
/// Holds additional runtime state for an <see cref="Ailment"/> instance.
/// Populated at construction time by <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/>
/// when the definition has one set.
/// </summary>
[Expansion(typeof(Ailment))]
[Deprecates(nameof(Ailment.Enable), $"Use {nameof(ExpandedAilmentManager.Enable)} instead")]
[Deprecates(nameof(Ailment.Disable), $"Use {nameof(ExpandedAilmentManager.Disable)} instead")]
public partial class ExpandedAilment
{
    /// <summary>
    /// The scripted behaviour attached to this ailment instance.
    /// Null if this is a purely data-driven ailment.
    /// </summary>
    private IScriptedAilment _scriptedAilment;

    private void OnInit()
    {
        var scriptedFactory = _core.definition.GetExpansion().ScriptedAilmentFactory;
        if (scriptedFactory == null) return;
        _scriptedAilment = scriptedFactory(_core);
    }

    internal void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        if (_isEnabled) return;
        _isEnabled = true;
        _disableCount = 0;
        foreach(var trigger in _triggers)
        {
            if (!trigger.subDefinition.defaultDisabled)
            {
                trigger.Enable();
            }
        }

        _core.Enable();
        _scriptedAilment?.Enable(ailmentManager, owner);
    }

    internal void Disable()
    {
        if (!_isEnabled) return;
        _isEnabled = false;

        if (!_definition.persistentFlags)
        {
            _flags.Clear();
        }

        foreach (var trigger in _triggers)
        {
            trigger.Disable();
        }

        _core.Disable();
        _scriptedAilment?.Disable();
    }
}