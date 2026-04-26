using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DarknessZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float _damagePerSecond = 20f;

    [Header("Light Detection")]
    [Tooltip("Маска слоёв для стен, перекрывающих свет (как в AgentLightSensor)")]
    [SerializeField] private LayerMask _occlusionMask;

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

    // Скорость — save/restore через currentSpeedMultiplier
    private bool _speedModified;
    private float _savedSpeedMultiplier;

    // Прочие эффекты
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

        UpdateSpeedMultiplier(darknessActive);

        if (_ambientLight == null) return;
        float targetAmbient = darknessActive ? 0f : _ambientLightBaseIntensity;
        float speed = 1f / Mathf.Max(0.001f, _fadeDuration);
        _currentAmbientIntensity = Mathf.MoveTowards(_currentAmbientIntensity, targetAmbient, speed * Time.deltaTime);
        _ambientLight.intensity = _currentAmbientIntensity;
    }

    private void UpdateSpeedMultiplier(bool darknessActive)
    {
        if (_ctx == null) return;

        if (darknessActive && !_speedModified)
        {
            _savedSpeedMultiplier = _ctx.currentSpeedMultiplier;
            _ctx.currentSpeedMultiplier = _ctx.stats.DarknessSpeedMultiplier;
            _speedModified = true;
        }
        else if (!darknessActive && _speedModified)
        {
            _ctx.currentSpeedMultiplier = _savedSpeedMultiplier;
            _speedModified = false;
        }
    }

    private void ApplyDamage(bool darknessActive)
    {
        if (!darknessActive || _ctx == null) return;
        _ctx.health?.TakeDamage(_damagePerSecond * Time.deltaTime);
    }

    private const float FullAngle = 359.9f;

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

            if (IsLightHittingZone(light))
                return true;
        }
        return false;
    }

    private bool IsLightHittingZone(Light2D light)
    {
        Vector2 lightPos = light.transform.position;

        // Проверяем ближайшую точку зоны к источнику света и центр bounds
        if (IsPointIlluminatedByLight(light, lightPos, _col.ClosestPoint(lightPos)))
            return true;
        if (IsPointIlluminatedByLight(light, lightPos, _col.bounds.center))
            return true;

        return false;
    }

    private bool IsPointIlluminatedByLight(Light2D light, Vector2 lightPos, Vector2 targetPoint)
    {
        float outerRadius = light.pointLightOuterRadius;
        if (outerRadius <= 0f) return false;

        Vector2 toTarget = targetPoint - lightPos;
        float distance = toTarget.magnitude;
        if (distance > outerRadius) return false;

        // Проверка углового конуса (если не 360°)
        float outerAngle = light.pointLightOuterAngle;
        if (outerAngle < FullAngle)
        {
            Vector2 forward = light.transform.up;
            if (forward.sqrMagnitude > 0f && distance > 0f)
            {
                float cosHalfAngle = Mathf.Cos(outerAngle * 0.5f * Mathf.Deg2Rad);
                float dot = Vector2.Dot(forward.normalized, toTarget / distance);
                if (dot < cosHalfAngle) return false;
            }
        }

        // Raycast: проверяем, нет ли стены между светом и точкой
        if (_occlusionMask.value != 0 && distance > 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(lightPos, toTarget / distance, distance, _occlusionMask);
            if (hit.collider != null) return false;
        }

        return true;
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

        if (_ctx != null && _speedModified)
        {
            _ctx.currentSpeedMultiplier = _savedSpeedMultiplier;
            _speedModified = false;
        }

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
        _vignetteController?.SetDarknessActive(false);

        if (_ctx != null && _speedModified)
            _ctx.currentSpeedMultiplier = _savedSpeedMultiplier;

        if (_ambientLight != null)
            _ambientLight.intensity = _ambientLightBaseIntensity;
    }
}
