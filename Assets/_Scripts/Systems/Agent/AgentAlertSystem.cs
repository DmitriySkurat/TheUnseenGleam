using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Сервис кооперации агентов. Регистрируется в Services через GameplaySceneServiceRegistry.
/// AgentAlert вызывает BroadcastAlert; AgentStateDriver каждого агента подписывается
/// в Initialize() и выставляет ctx.alertPending = true при получении сигнала.
/// </summary>
public class AgentAlertSystem : MonoBehaviour, ISceneService
{
    public event Action<Vector2> OnAlertBroadcast;

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public void BroadcastAlert(Vector2 position)
        => OnAlertBroadcast?.Invoke(position);
}
