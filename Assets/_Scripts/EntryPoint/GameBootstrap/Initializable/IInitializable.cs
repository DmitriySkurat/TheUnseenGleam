public interface IInitializable
{
    InitializationOrder Order { get; }
    void Initialize();
}