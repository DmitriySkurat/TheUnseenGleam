using UnityEngine;

/// <summary>
/// Восстанавливает состояние игрока из сохранения в PlayerContext.
/// Должен быть ISessionLifecycle с Order < Player, чтобы выполниться ДО
/// PlayerHealth (Player+10) и PlayerInventory (Player+10).
/// Инвентарь передаётся через SaveVariables.PendingSave и применяется
/// в PlayerInventory.Initialize().
/// </summary>
public class SaveLoader : MonoBehaviour, ISessionLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player - 1;

    public void Initialize()
    {
        SaveVariables.PendingSave = null;

        if (SaveVariables.ActiveSlot < 0)
            return;

        var saveData = SaveManager.Load(SaveVariables.ActiveSlot);

        // health > 0 означает реальное сохранение, а не заглушку от NewGame
        if (saveData == null || saveData.health <= 0f)
            return;

        if (!Services.IsRegistered<PlayerContext>())
            return;

        var ctx = Services.Get<PlayerContext>();
        ctx.currentHealth = saveData.health;
        ctx.stamina = saveData.stamina;

        // Инвентарь применяется в PlayerInventory.Initialize() (Order Player+10).
        // PendingSave сбрасывается там же после восстановления.
        SaveVariables.PendingSave = saveData;

        Debug.Log($"[SaveLoader] Slot {SaveVariables.ActiveSlot}: HP={saveData.health}, Stamina={saveData.stamina}, Items={saveData.inventory.Count}");
    }

    public void Dispose() { }
}
