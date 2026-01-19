public interface IModSave<T>
{
    T Convert(int runtimeId);
    void SetData(T data);
}