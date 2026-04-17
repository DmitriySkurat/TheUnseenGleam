public interface ILifecycle
{
    InitializationOrder Order { get; }
    void Initialize();
    void Dispose();
}