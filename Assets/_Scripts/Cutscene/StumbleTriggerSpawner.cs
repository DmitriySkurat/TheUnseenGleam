using UnityEngine;

/// <summary>
/// Активирует StumbleTrigger рядом с игроком, когда любой ScriptedAgent
/// входит в заданный радиус около игрока.
/// </summary>
public class StumbleTriggerSpawner : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Tooltip("Радиус вокруг игрока — при входе агента в него появляется StumbleTrigger")]
    [SerializeField] private float _agentProximityRadius = 8f;

    [Tooltip("StumbleTrigger, который нужно активировать (изначально выключен)")]
    [SerializeField] private GameObject _stumbleTrigger;

    [Tooltip("Смещение по X от игрока при размещении StumbleTrigger")]
    [SerializeField] private float _offsetX = 4f;

    private PlayerContext _playerCtx;
    private bool _spawned;
    private bool _watching;

    public void StartWatching() => _watching = true;

    public void Initialize()
    {
        if (Services.IsRegistered<PlayerContext>())
            _playerCtx = Services.Get<PlayerContext>();

        if (_stumbleTrigger != null)
            _stumbleTrigger.SetActive(false);
    }

    public void Dispose() { }

    void Update()
    {
        if (_spawned || !_watching || _playerCtx == null) return;

        var agents = Object.FindObjectsByType<ScriptedAgent>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            float dist = Vector2.Distance(agent.transform.position, _playerCtx.transform.position);
            if (dist <= _agentProximityRadius)
            {
                Spawn();
                return;
            }
        }
    }

    void Spawn()
    {
        _spawned = true;

        if (_stumbleTrigger == null) return;

        Vector3 pos = _stumbleTrigger.transform.position;
        pos.x = _playerCtx.transform.position.x + _offsetX;
        pos.y = _playerCtx.transform.position.y;
        _stumbleTrigger.transform.position = pos;
        _stumbleTrigger.SetActive(true);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, _agentProximityRadius);
    }
#endif
}
