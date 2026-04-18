/// <summary>
/// Хранит глобальное состояние текущей сессии сохранения.
/// ActiveSlot: индекс активного слота (1–3), -1 если слот не выбран.
/// PendingSave: данные последнего загруженного сохранения, которые PlayerInventory
///              применяет при Initialize() (сбрасывается после применения).
/// </summary>
public static class SaveVariables
{
    public static int ActiveSlot { get; set; } = -1;
    public static SaveData PendingSave { get; set; }
}
