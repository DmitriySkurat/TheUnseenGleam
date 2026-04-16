/// <summary>
/// Хранит глобальное состояние текущей сессии сохранения.
/// ActiveSlot: индекс активного слота (1–3), -1 если слот не выбран.
/// </summary>
public static class SaveVariables
{
    public static int ActiveSlot { get; set; } = -1;
}
