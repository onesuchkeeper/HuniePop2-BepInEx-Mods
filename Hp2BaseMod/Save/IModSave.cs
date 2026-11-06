// Hp2BaseMod 2026, By OneSuchKeeper

public interface IModSave<T>
{
    T Convert(int runtimeId);
    void SetData(T data);
}