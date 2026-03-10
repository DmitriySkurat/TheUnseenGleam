public enum InitializationOrder
{
    SceneServices = 1,
    GameplayCore = 100,
    Player = 200,
    Enemy = 300,
    Interactable = 400,
    Camera = 500,
    PostProcessing = 600,
    UI = 700,
    Audio = 800
}