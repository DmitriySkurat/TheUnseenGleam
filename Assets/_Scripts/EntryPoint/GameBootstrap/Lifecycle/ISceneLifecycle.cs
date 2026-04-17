public interface ISceneLifecycle
{
    InitializationOrder Order { get; }
    void Initialize();
    void Dispose();
}