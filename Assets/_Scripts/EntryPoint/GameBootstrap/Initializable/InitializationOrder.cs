public enum InitializationOrder
{
    SceneServices = 1,
    GameplayCore = 100,
    Player = 200,
    Enemy = 250,
    Camera = 300,
    PostProcessing = 400,
    UI = 500,
    Audio = 600
}