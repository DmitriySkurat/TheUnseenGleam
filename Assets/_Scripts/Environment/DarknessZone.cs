using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DarknessZone : MonoBehaviour
{
    private const float FullAngle = 359.9f;

    [Header("Damage")]
    [SerializeField] private float _damagePerSecond = 20f;

    [Header("Light Detection")]
    [Tooltip("Маска слоёв для стен, перекрывающих свет")]
    [SerializeField] private LayerMask _occlusionMask;
    [Tooltip("Радиус стирания тьмы вокруг точки попадания света (world units)")]
    [SerializeField, Min(0.1f)] private float _eraseRadius = 1.5f;
    [Tooltip("Если alpha пикселя под игроком выше этого — тьма активна")]
    [SerializeField, Range(0f, 1f)] private float _darknessThreshold = 0.4f;

    [Header("Dissipation")]
    [Tooltip("Сколько секунд тьма восстанавливается от 0 до полной (после ухода света)")]
    [SerializeField] private float _fadeDuration = 2f;

    [Header("Visual")]
    [Tooltip("SpriteRenderer-оверлей (спрайт будет заменён динамической текстурой)")]
    [SerializeField] private SpriteRenderer _overlay;
    [Tooltip("Разрешение текстуры тьмы (пикселей на world unit)")]
    [SerializeField] private float _pixelsPerUnit = 16f;

    private Collider2D _col;
    private Bounds _zoneBounds;

    // Texture
    private Texture2D _darknessTexture;
    private Color32[] _pixels;
    private int _texWidth, _texHeight;
    private bool _textureDirty;
    private bool _hasAnyCleared;

    // Player state
    private bool _playerInside;
    private PlayerContext _ctx;
    private bool _speedModified;
    private float _savedSpeedMultiplier;

    // Effects
    private VignetteController _vignetteController;
    private Light2D _ambientLight;
    private float _ambientLightBaseIntensity;
    private float _currentAmbientIntensity;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
    }

    private void Start()
    {
        _vignetteController = FindObjectOfType<VignetteController>();
        InitDarknessTexture();
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        EraseWithActiveLights();
        RestoreDarkness();

        if (_textureDirty)
        {
            _darknessTexture.SetPixels32(_pixels);
            _darknessTexture.Apply();
            _textureDirty = false;
        }

        bool darknessActive = _playerInside && IsPlayerInDarkness();
        UpdateEffects(darknessActive);
        ApplyDamage(darknessActive);
    }

    // ── Texture Init ─────────────────────────────────────────────────────────

    private void InitDarknessTexture()
    {
        if (_overlay == null) return;

        _zoneBounds = _col.bounds;
        _texWidth = Mathf.Max(1, Mathf.RoundToInt(_zoneBounds.size.x * _pixelsPerUnit));
        _texHeight = Mathf.Max(1, Mathf.RoundToInt(_zoneBounds.size.y * _pixelsPerUnit));

        _darknessTexture = new Texture2D(_texWidth, _texHeight, TextureFormat.RGBA32, false);
        _darknessTexture.filterMode = FilterMode.Bilinear;
        _darknessTexture.wrapMode = TextureWrapMode.Clamp;

        _pixels = new Color32[_texWidth * _texHeight];
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = new Color32(0, 0, 0, 255);

        _darknessTexture.SetPixels32(_pixels);
        _darknessTexture.Apply();

        Sprite sprite = Sprite.Create(
            _darknessTexture,
            new Rect(0, 0, _texWidth, _texHeight),
            new Vector2(0.5f, 0.5f),
            _pixelsPerUnit
        );

        _overlay.sprite = sprite;
        _overlay.color = Color.white;
        _overlay.transform.position = new Vector3(
            _zoneBounds.center.x,
            _zoneBounds.center.y,
            _overlay.transform.position.z
        );
        _overlay.transform.localScale = Vector3.one;
    }

    // ── Per-pixel darkness ────────────────────────────────────────────────────

    private void RestoreDarkness()
    {
        if (!_hasAnyCleared || _fadeDuration <= 0f) return;

        float restorePerFrame = (255f / _fadeDuration) * Time.deltaTime;
        bool stillAnyCleared = false;

        for (int i = 0; i < _pixels.Length; i++)
        {
            if (_pixels[i].a >= 255) continue;

            int newAlpha = _pixels[i].a + Mathf.RoundToInt(restorePerFrame);
            _pixels[i].a = (byte)Mathf.Min(255, newAlpha);
            _textureDirty = true;

            if (_pixels[i].a < 255)
                stillAnyCleared = true;
        }

        _hasAnyCleared = stillAnyCleared;
    }

    private void EraseWithActiveLights()
    {
        if (!Services.IsRegistered<LightSystem>()) return;

        var spotLights = Services.Get<LightSystem>().GetSpotLights();
        for (int i = 0; i < spotLights.Count; i++)
        {
            var light = spotLights[i];
            if (light == null || !light.enabled || !light.gameObject.activeInHierarchy) continue;
            if (light.GetComponent<MirrorLightSource>() == null &&
                light.GetComponent<PlayerLightSource>() == null) continue;

            ProcessLightErase(light);
        }
    }

    private void ProcessLightErase(Light2D light)
    {
        Vector2 lightPos = light.transform.position;
        float outerRadius = light.pointLightOuterRadius;
        if (outerRadius <= 0f) return;

        float outerAngle = light.pointLightOuterAngle;

        if (outerAngle >= FullAngle)
        {
            // 360° источник: стираем у ближайшей точки зоны
            Vector2 center = IsInZoneBoundsXY(lightPos)
                ? lightPos
                : (Vector2)_col.ClosestPoint(lightPos);
            TryEraseAtPoint(lightPos, center, outerRadius);
        }
        else
        {
            // Направленный источник (зеркало): сэмплируем лучи внутри конуса
            Vector2 forward = ((Vector2)light.transform.up).normalized;
            float halfAngle = outerAngle * 0.5f;

            int numRays = Mathf.Clamp(Mathf.RoundToInt(halfAngle / 5f) * 2 + 1, 1, 9);
            const int numSamples = 8;

            for (int r = 0; r < numRays; r++)
            {
                float angle = numRays > 1
                    ? Mathf.Lerp(-halfAngle, halfAngle, (float)r / (numRays - 1))
                    : 0f;
                Vector2 dir = Rotate2D(forward, angle);

                for (int s = 0; s < numSamples; s++)
                {
                    float dist = outerRadius * (s + 1f) / numSamples;
                    Vector2 samplePoint = lightPos + dir * dist;

                    if (!IsInZoneBoundsXY(samplePoint)) continue;

                    if (_occlusionMask.value != 0)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(lightPos, dir, dist, _occlusionMask);
                        if (hit.collider != null) break; // стена — дальше этот луч не идёт
                    }

                    EraseAt(samplePoint, _eraseRadius);
                }
            }
        }
    }

    private void TryEraseAtPoint(Vector2 lightPos, Vector2 eraseCenter, float lightRadius)
    {
        Vector2 toCenter = eraseCenter - lightPos;
        float dist = toCenter.magnitude;
        if (dist > lightRadius) return;

        if (_occlusionMask.value != 0 && dist > 0.01f)
        {
            RaycastHit2D hit = Physics2D.Raycast(lightPos, toCenter.normalized, dist, _occlusionMask);
            if (hit.collider != null) return;
        }

        EraseAt(eraseCenter, _eraseRadius);
    }

    private void EraseAt(Vector2 worldPos, float worldRadius)
    {
        float u = (worldPos.x - _zoneBounds.min.x) / _zoneBounds.size.x;
        float v = (worldPos.y - _zoneBounds.min.y) / _zoneBounds.size.y;

        int cx = Mathf.RoundToInt(u * (_texWidth - 1));
        int cy = Mathf.RoundToInt(v * (_texHeight - 1));
        int rPx = Mathf.CeilToInt(worldRadius * _pixelsPerUnit);
        float innerFraction = 0.6f; // 60% радиуса — полностью прозрачная зона

        for (int py = cy - rPx; py <= cy + rPx; py++)
        {
            if (py < 0 || py >= _texHeight) continue;
            for (int px = cx - rPx; px <= cx + rPx; px++)
            {
                if (px < 0 || px >= _texWidth) continue;

                float dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                if (dist > rPx) continue;

                // Мягкий край: от innerFraction*rPx до rPx идёт градиент
                float t = 1f - Mathf.Clamp01((dist - rPx * innerFraction) / Mathf.Max(1f, rPx * (1f - innerFraction)));
                byte targetAlpha = (byte)(255 * (1f - t));

                int idx = py * _texWidth + px;
                if (_pixels[idx].a > targetAlpha)
                {
                    _pixels[idx].a = targetAlpha;
                    _textureDirty = true;
                    _hasAnyCleared = true;
                }
            }
        }
    }

    private bool IsPlayerInDarkness()
    {
        if (_ctx == null) return false;
        return GetAlphaAtWorldPos(_ctx.transform.position) > _darknessThreshold;
    }

    private float GetAlphaAtWorldPos(Vector2 worldPos)
    {
        float u = (worldPos.x - _zoneBounds.min.x) / _zoneBounds.size.x;
        float v = (worldPos.y - _zoneBounds.min.y) / _zoneBounds.size.y;

        int px = Mathf.Clamp(Mathf.RoundToInt(u * (_texWidth - 1)), 0, _texWidth - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (_texHeight - 1)), 0, _texHeight - 1);
        return _pixels[py * _texWidth + px].a / 255f;
    }

    private bool IsInZoneBoundsXY(Vector2 point)
    {
        return point.x >= _zoneBounds.min.x && point.x <= _zoneBounds.max.x &&
               point.y >= _zoneBounds.min.y && point.y <= _zoneBounds.max.y;
    }

    // ── Effects & Damage ──────────────────────────────────────────────────────

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

    // ── Triggers ──────────────────────────────────────────────────────────────

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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void OnDestroy()
    {
        _vignetteController?.SetDarknessActive(false);

        if (_ctx != null && _speedModified)
            _ctx.currentSpeedMultiplier = _savedSpeedMultiplier;

        if (_ambientLight != null)
            _ambientLight.intensity = _ambientLightBaseIntensity;

        if (_darknessTexture != null)
            Destroy(_darknessTexture);
    }
}
