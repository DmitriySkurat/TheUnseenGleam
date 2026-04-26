using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DarknessZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float _damagePerSecond = 20f;

    [Header("Light Detection")]
    [Tooltip("Как близко должен быть источник света игрока к границе зоны, чтобы рассеять тьму")]
    [SerializeField] private float _lightDetectionRadius = 4f;

    [Header("Dissipation")]
    [Tooltip("Сколько секунд тьма остаётся рассеянной после того, как свет пропал")]
    [SerializeField] private float _dissipateDelay = 2f;
    [Tooltip("Время fade-in/out оверлея в секундах")]
    [SerializeField] private float _fadeDuration = 0.8f;

    [Header("Visual")]
    [Tooltip("Чёрный SpriteRenderer-оверлей на дочернем объекте (сортировка: Darkness)")]
    [SerializeField] private SpriteRenderer _overlay;

    private Collider2D _col;
    private bool _playerInside;
    private PlayerContext _ctx;
    private float _dissipateTimer;
    private float _currentAlpha = 1f;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        if (_overlay != null)
        {
            var c = _overlay.color;
            c.a = 1f;
            _overlay.color = c;
        }
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        UpdateDissipation();
        UpdateOverlay();
        ApplyDamage();
    }

    private void UpdateDissipation()
    {
        if (IsPlayerLightPresent())
            _dissipateTimer = _dissipateDelay;
        else
            _dissipateTimer = Mathf.Max(0f, _dissipateTimer - Time.deltaTime);
    }

    private void UpdateOverlay()
    {
        if (_overlay == null) return;

        float target = _dissipateTimer > 0f ? 0f : 1f;
        float speed = 1f / Mathf.Max(0.001f, _fadeDuration);
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, target, speed * Time.deltaTime);

        var c = _overlay.color;
        c.a = _currentAlpha;
        _overlay.color = c;
    }

    private void ApplyDamage()
    {
        if (!_playerInside || _ctx == null || _dissipateTimer > 0f) return;
        _ctx.health?.TakeDamage(_damagePerSecond * Time.deltaTime);
    }

    // Проверяем MirrorLightSource (свет зеркала) и PlayerLightSource (любой свет игрока).
    private bool IsPlayerLightPresent()
    {
        if (!Services.IsRegistered<LightSystem>()) return false;

        var spotLights = Services.Get<LightSystem>().GetSpotLights();
        for (int i = 0; i < spotLights.Count; i++)
        {
            var light = spotLights[i];
            if (light == null || !light.enabled || !light.gameObject.activeInHierarchy) continue;
            if (light.GetComponent<MirrorLightSource>() == null &&
                light.GetComponent<PlayerLightSource>() == null) continue;

            // Расстояние от ближайшей точки зоны до источника света.
            Vector2 closest = _col.ClosestPoint(light.transform.position);
            float dist = Vector2.Distance(closest, (Vector2)light.transform.position);
            if (dist <= _lightDetectionRadius)
                return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Services.IsRegistered<PlayerContext>())
            _ctx = Services.Get<PlayerContext>();
        _playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        _ctx = null;
    }
}
