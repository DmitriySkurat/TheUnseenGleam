using UnityEngine;

/// <summary>
/// Запускается при старте сессии.
/// Восстанавливает состояние игрока из сохранения в PlayerContext.
/// </summary>
public class SaveLoader : MonoBehaviour, ISceneLifecycle
{
    // Запускается до Player (PlayerHealth и PlayerStaminaController)
    public InitializationOrder Order => InitializationOrder.Player - 1;

    public void Initialize()
    {
        if (SaveVariables.ActiveSlot < 0)
            return;

        var saveData = SaveManager.Load(SaveVariables.ActiveSlot);

        // health > 0 означает реальное сохранение, а не заглушку от NewGame
        if (saveData == null || saveData.health <= 0f)
            return;

        var ctx = Services.Get<PlayerContext>();
        ctx.currentHealth = saveData.health;
        ctx.stamina = saveData.stamina;

        Debug.Log($"[SaveLoader] Loaded slot {SaveVariables.ActiveSlot}: HP={saveData.health}, Stamina={saveData.stamina}");
    }

    public void Dispose() { }
}
