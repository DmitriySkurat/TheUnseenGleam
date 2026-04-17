using UnityEngine;

/// <summary>
/// Размещается в игровых сценах.
/// При инициализации восстанавливает состояние игрока из сохранения,
/// если PlayerPersistentState ещё не заполнен (например, при прямом запуске сцены).
/// </summary>
public class SaveLoader : MonoBehaviour, ISceneLifecycle
{
    // Запускается до Player (PlayerHealth и PlayerStaminaController)
    public InitializationOrder Order => InitializationOrder.Player - 1;

    public void Initialize()
    {
        if (SaveVariables.ActiveSlot < 0)
            return;

        var playerState = Services.Get<PlayerPersistentState>();
        if (playerState.HasData)
            return;

        var saveData = SaveManager.Load(SaveVariables.ActiveSlot);

        // health > 0 означает реальное сохранение, а не заглушку от NewGame
        if (saveData != null && saveData.health > 0f)
        {
            playerState.Save(saveData.health, saveData.stamina);
            Debug.Log($"[SaveLoader] Loaded slot {SaveVariables.ActiveSlot}: HP={saveData.health}, Stamina={saveData.stamina}");
        }
    }
    
    public void Dispose()
    {
        
    }
}
