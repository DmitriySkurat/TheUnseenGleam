using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Сервис координации поиска. Когда несколько агентов одновременно входят в AgentSearch,
/// каждый получает уникальный номер сектора, чтобы покрывать разные зоны вокруг
/// точки подозрения, не дублируя маршруты.
///
/// Схема секторов (offset в единицах SearchWanderDistance):
///   index 0 → 0   (центр)
///   index 1 → +1  (правый)
///   index 2 → −1  (левый)
///   index 3 → +2  (дальний правый)
///   index 4 → −2  (дальний левый)  ...
/// </summary>
public class AgentSearchCoordinator : MonoBehaviour, ISceneService
{
    readonly List<AgentContext> _searching = new();

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Зарегистрировать агента как ищущего. Возвращает назначенный индекс сектора.
    /// Повторный вызов для того же агента возвращает уже выданный индекс.
    /// </summary>
    public int Register(AgentContext ctx)
    {
        int existing = _searching.IndexOf(ctx);
        if (existing >= 0)
            return existing;

        _searching.Add(ctx);
        return _searching.Count - 1;
    }

    /// <summary>
    /// Снять агента с учёта после выхода из AgentSearch.
    /// </summary>
    public void Unregister(AgentContext ctx)
    {
        _searching.Remove(ctx);
    }

    /// <summary>
    /// Смещение по X для сектора с данным индексом (в единицах SearchWanderDistance).
    /// 0 → 0, 1 → +1, 2 → −1, 3 → +2, 4 → −2 ...
    /// </summary>
    public static float SectorOffsetMultiplier(int index)
    {
        if (index == 0) return 0f;
        return index % 2 == 1 ? (index + 1) / 2f : -(index / 2f);
    }
}
