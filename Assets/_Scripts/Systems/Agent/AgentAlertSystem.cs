using System;
using UnityEngine;

/// <summary>
/// Сервис кооперации агентов. Регистрируется в Services через GameplaySceneServiceRegistry.
/// AgentAlert вызывает BroadcastAlert; AgentStateDriver каждого агента подписывается
/// в Initialize() и выставляет ctx.alertPending = true при получении сигнала.
/// </summary>
public class AgentAlertSystem : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.GameplayCore;

    public event Action<Vector2> OnAlertBroadcast;

    public void Initialize() { }

    public void Dispose() { }

    public void BroadcastAlert(Vector2 position)
        => OnAlertBroadcast?.Invoke(position);
}
