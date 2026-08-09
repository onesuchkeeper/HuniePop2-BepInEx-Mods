namespace Hp2BaseMod;

// Implemented by a base class that an [Expansion] partial class inherits from, when that base
// class already stores the core reference itself (e.g. UiPatchController<T>'s protected _core).
//
// This is purely a signal to the generator, not a runtime abstraction the generator relies on
// dynamically - ExpansionGenerator detects it at compile time via classSymbol.AllInterfaces and
// branches codegen accordingly:
//   - Implemented (and its T matches the [Expansion] attribute's base type): the generator skips
//     emitting its own "_core" field and "Core" property entirely, and every generated accessor
//     that would otherwise target "_core" targets this interface's "Core" property instead. There
//     is deliberately no separate, second copy of the reference.
//   - Not implemented: behavior is unchanged from today - the generator emits its own private
//     "_core" field and public "Core" property.
//
// This is independent of whether the base class also happens to expose a constructor shaped like
// "SomeBase(T core)" - the generator detects that separately and chains to it with ": base(core)"
// when present, regardless of whether this interface is implemented. A base class can do either,
// both, or neither.
public interface IExpansionCore<out T>
{
    T Core { get; }
}