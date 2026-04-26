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
    [Tooltip("Время fade-in/out оверлея и эффектов (секунды)")]
    [SerializeField] private float _fadeDuration = 0.8f;

    [Header("Visual")]
    [Tooltip("Чёрный SpriteRenderer-оверлей на дочернем объекте (сортировка: Darkness)")]
    [SerializeField] private SpriteRenderer _overlay;

    private Collider2D _col;

    // Состояние игрока
    private bool _playerInside;
    private PlayerContext _ctx;

    // Тьма
    private float _dissipateTimer;
    private float _currentAlpha = 1f;

    // Эффекты
    private VignetteController _vignetteController;
    private Light2D _ambientLight;
    private float _ambientLightBaseIntensity;
    private float _currentAmbientIntensity;

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

    private void Start()
    {
        _vignetteController = FindObjectOfType<VignetteController>();
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        UpdateDissipation();

        bool darknessActive = _playerInside && _dissipateTimer <= 0f;

        UpdateOverlay();
        UpdateEffects(darknessActive);
        ApplyDamage(darknessActive);
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

    private void UpdateEffects(bool darknessActive)
    {
        _vignetteController?.SetDarknessActive(darknessActive);

        if (_ctx != null)
            _ctx.darknessSpeedMultiplier = darknessActive ? _ctx.stats.DarknessSpeedMultiplier : 1f;

        if (_ambientLight == null) return;

        float targetAmbient = darknessActive ? 0f : _ambientLightBaseIntensity;
        float speed = 1f / Mathf.Max(0.001f, _fadeDuration);
        _currentAmbientIntensity = Mathf.MoveTowards(_currentAmbientIntensity, targetAmbient, speed * Time.deltaTime);
        _ambientLight.intensity = _currentAmbientIntensity;
    }

    private void ApplyDamage(bool darknessActive)
    {
        if (!darknessActive || _ctx == null) return;
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
        CacheAmbientLight();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_ctx != null)
            _ctx.darknessSpeedMultiplier = 1f;
        _playerInside = false;
        _ctx = null;
    }

    private void CacheAmbientLight()
    {
        if (_ctx?.transform == null || _ambientLight != null) return;

        var ambientGO = _ctx.transform.Find("PlayerAmbientLight");
        if (ambientGO == null) return;

        _ambientLight = ambientGO.GetComponent<Light2D>();
        if (_ambientLight != null)
        {
            _ambientLightBaseIntensity = _ambientLight.intensity;
            _currentAmbientIntensity = _ambientLightBaseIntensity;
        }
    }

    private void OnDestroy()
    {
        // Сброс эффектов при удалении зоны из сцены.
        _vignetteController?.SetDarknessActive(false);
        if (_ctx != null)
            _ctx.darknessSpeedMultiplier = 1f;
        if (_ambientLight != null)
            _ambientLight.intensity = _ambientLightBaseIntensity;
    }
}
