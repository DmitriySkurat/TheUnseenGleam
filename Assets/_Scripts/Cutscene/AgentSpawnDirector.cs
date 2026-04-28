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
    [SerializeField] private GameObject _stumbleTrigger;
    [Tooltip("Смещение по X от текущей позиции игрока при появлении StumbleTrigger")]
    [SerializeField] private float _stumbleTriggerOffsetX = 5f;
    [Tooltip("Задержка после появления агентов перед появлением StumbleTrigger (сек)")]
    [SerializeField] private float _stumbleTriggerDelay = 1f;

    [Header("Activation")]
    [Tooltip("Запустить последовательность сразу при старте сцены")]
    [SerializeField] private bool _activateOnStart = false;
    [Tooltip("Запустить при входе любого объекта с тегом Player в коллайдер этого объекта")]
    [SerializeField] private bool _activateOnPlayerEnter = true;

    private PlayerContext _playerCtx;
    private bool _activated;

    public void Initialize()
    {
        if (Services.IsRegistered<PlayerContext>())
            _playerCtx = Services.Get<PlayerContext>();

        foreach (var a in _agents)
            if (a != null) a.SetActive(false);

        if (_stumbleTrigger != null)
            _stumbleTrigger.SetActive(false);
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

        if (_stumbleTrigger == null) yield break;

        if (_stumbleTriggerDelay > 0f)
            yield return new WaitForSeconds(_stumbleTriggerDelay);

        if (_playerCtx?.transform != null)
        {
            Vector3 pos = _stumbleTrigger.transform.position;
            pos.x = _playerCtx.transform.position.x + _stumbleTriggerOffsetX;
            _stumbleTrigger.transform.position = pos;
        }

        _stumbleTrigger.SetActive(true);
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
