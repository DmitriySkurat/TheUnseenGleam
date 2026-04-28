using System.Collections;
using UnityEngine;

/// <summary>
/// Управляет появлением агентов и последующим размещением StumbleTrigger рядом с игроком.
/// Запуск: автоматически через Start, либо вручную через Activate() / вход в триггер-зону.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AgentSpawnDirector : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Agents")]
    [Tooltip("Объекты агентов — должны быть отключены в сцене до срабатывания")]
    [SerializeField] private GameObject[] _agents;
    [Tooltip("Задержка перед появлением агентов (сек)")]
    [SerializeField] private float _agentSpawnDelay = 0f;

    [Header("Stumble Trigger")]
    [SerializeField] private StumbleTriggerSpawner _stumbleTriggerSpawner;

    [Header("Activation")]
    [Tooltip("Запустить последовательность сразу при старте сцены")]
    [SerializeField] private bool _activateOnStart = false;
    [Tooltip("Запустить при входе любого объекта с тегом Player в коллайдер этого объекта")]
    [SerializeField] private bool _activateOnPlayerEnter = true;

    private bool _activated;

    public void Initialize()
    {
        foreach (var a in _agents)
            if (a != null) a.SetActive(false);
    }

    public void Dispose() { }

    void Start()
    {
        if (_activateOnStart)
            Activate();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_activateOnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;
        Activate();
    }

    public void Activate()
    {
        if (_activated) return;
        _activated = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (_agentSpawnDelay > 0f)
            yield return new WaitForSeconds(_agentSpawnDelay);

        foreach (var a in _agents)
            if (a != null) a.SetActive(true);

        _stumbleTriggerSpawner?.StartWatching();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
