using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MagicalMirror : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 10;
    
    [Header("References")]
    [SerializeField] private Transform hand;
    [SerializeField] private Transform lightSpawnPoint;
    [SerializeField] private GameObject mirrorLightPrefab;

    [Header("Capacity")]
    [SerializeField, Min(0f)] private float maxCharge = 100f;
    [SerializeField, Range(0f, 1f)] private float initialCharge = 0f;

    [Header("Charging")]
    [SerializeField, Min(0f)] private float chargePerSecond = 10f;
    [SerializeField, Min(0f)] private float minLightStrengthToCharge = 1f;
    [SerializeField] private bool chargeOnlyWhenGrounded = true;

    [Header("Usage")]
    [SerializeField, Min(0f)] private float chargeCostPerSecond = 10f;
    [SerializeField] private float flickerCharge = 20f;
    [SerializeField, Min(0f)] private float minChargeToReactivate = 5f;

    
    [Header("Light Power")]
    [SerializeField] private float minLightIntensity = 7f;
    [SerializeField] private float maxLightIntensity = 20f;
    
    [Header("Crouch Settings")]
    [SerializeField] private Vector3 crouchHandOffset = new Vector3(0f, -0.5f, 0f);
    
    private Vector3 _originalLocalHandPos;
    private bool _isLocalPosSaved;
    
    private GameObject _lightInstance;
    private Light2D _light2D;
    private LightSystem _lightSystem;
    private PlayerContext _ctx;
    private float _currentCharge;
    private bool _depleted;

    private bool HasEnoughChargeForUse => !_depleted && _currentCharge > 0;
    
    public float CurrentCharge => maxCharge <= 0f ? 0f : Mathf.Clamp01(_currentCharge / maxCharge);
    public bool IsFull => maxCharge > 0f && _currentCharge >= maxCharge;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();
        _lightSystem = Services.Get<LightSystem>();

        _currentCharge = maxCharge <= 0f ? 0f : Mathf.Clamp01(initialCharge) * maxCharge;
        
        CreateLight();
        SetLightActive(false);
    }
    
    public void Dispose()
    {
        
    }
    
    private void CreateLight()
    {
        if (lightSpawnPoint == null || mirrorLightPrefab == null)
            return;

        _lightInstance = Instantiate(mirrorLightPrefab, lightSpawnPoint.position, lightSpawnPoint.rotation, lightSpawnPoint);
        _light2D = _lightInstance.GetComponent<Light2D>();
        if (_lightInstance != null && _lightInstance.GetComponent<MirrorLightSource>() == null)
            _lightInstance.AddComponent<MirrorLightSource>();

        if (_light2D != null)
            _lightSystem?.RegisterSpotLight(_light2D);
    }
    
    private void SetLightActive(bool active)
    {
        if (_lightInstance != null && _lightInstance.activeSelf != active)
            _lightInstance.SetActive(active);
    }

    private void Update()
    {
        if (_ctx == null || _ctx.lightSensor == null || maxCharge <= 0f)
        {
            StopUsingMirror();
            return;
        }

        if (!CanUseSelectedMirror())
        {
            StopUsingMirror();
            return;
        }

        bool isUsing = _ctx.input.AttackHeld && HasEnoughChargeForUse;
        if (isUsing)
        {
            SetLightActive(true);
            UpdateLight();
            UpdateLightPower();
            Use();
        }
        else
        {
            StopUsingMirror();
        }

        if (isUsing)
            return;

        if (chargeOnlyWhenGrounded && !_ctx.grounded)
            return;

        if (_currentCharge >= maxCharge)
        {
            _currentCharge = maxCharge;
            return;
        }

        float strength = _ctx.lightSensor.CurrentStrength;
        if (strength < minLightStrengthToCharge)
            return;

        _currentCharge = Mathf.Min(maxCharge, _currentCharge + chargePerSecond * Time.deltaTime);

        if (_depleted && _currentCharge >= minChargeToReactivate)
            _depleted = false;
    }

    private void OnDisable()
    {
        StopUsingMirror();
    }

    private void OnDestroy()
    {
        if (_light2D != null)
            _lightSystem?.UnregisterSpotLight(_light2D);
    }
    
    private void SaveOriginalHandPosition()
    {
        if (hand != null && !_isLocalPosSaved)
        {
            _originalLocalHandPos = hand.localPosition;
            _isLocalPosSaved = true;
        }
    }
    
    private void UpdateLight()
    {
        if (hand == null) return;
        SaveOriginalHandPosition();

        if (_ctx != null && _ctx.isCrouching)
        {
            hand.localPosition = _originalLocalHandPos + crouchHandOffset;
        }
        else
        {
            hand.localPosition = _originalLocalHandPos;
        }

        var camera = Camera.main;
        if (camera == null) return;

        var screenPos = _ctx.input.MousePosition;

        var camZ = -camera.transform.position.z;
        var worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));

        var handPos = (Vector2)hand.position;
        var direction = ((Vector2)worldPosition - (Vector2)handPos).normalized;

        if (direction.sqrMagnitude > 0.0001f)
            hand.up = direction;
    }
    
    private void UpdateLightPower()
    {
        if (_light2D == null || _ctx.lightSensor == null)
            return;

        float strength = _ctx.lightSensor.CurrentStrength;
        strength = Mathf.Clamp01(strength);

        float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, strength);

        if (_currentCharge < flickerCharge)
        {
            intensity *= Random.Range(0.8f, 1f);
        }

        _light2D.intensity = intensity;
    }

    public void Use()
    {
        float cost = chargeCostPerSecond * Time.deltaTime;
        _currentCharge = Mathf.Max(0f, _currentCharge - cost);

        if (_currentCharge <= 0f)
            _depleted = true;

        Debug.Log($"Use magical mirror. Current charge: {_currentCharge}/{maxCharge}");
    }

    private void StopUsingMirror()
    {
        SetLightActive(false);
    }

    private bool CanUseSelectedMirror()
    {
        return _ctx != null
            && !_ctx.isLedgeGrabbing
            && !_ctx.isStumbleFalling
            && _ctx.SelectedHotbarItem != null
            && _ctx.SelectedHotbarItem.itemName == ItemName.Mirror
            && _ctx.inventory != null
            && _ctx.inventory.Has(_ctx.SelectedHotbarItem);
    }
}
