using UnityEngine;

public class MagicalMirror : MonoBehaviour, IInitializable
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
    [SerializeField, Min(0f)] private float minLightStrengthToCharge = 0.1f;
    [SerializeField] private bool chargeOnlyWhenGrounded = true;

    [Header("Usage")]
    [SerializeField, Min(0f)] private float chargeCostPerSecond = 10f;
    
    
    private GameObject _lightInstance;

    private PlayerContext _ctx;

    private float _currentCharge;
    
    private bool _isUsing;
    
    private bool HasEnoughChargeForUse => _currentCharge > 0;
    
    public float CurrentCharge => maxCharge <= 0f ? 0f : Mathf.Clamp01(_currentCharge / maxCharge);
    public bool IsFull => maxCharge > 0f && _currentCharge >= maxCharge;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        _currentCharge = maxCharge <= 0f ? 0f : Mathf.Clamp01(initialCharge) * maxCharge;
        
        CreateLight();
        SetLightActive(false);
    }
    
    private void CreateLight()
    {
        if (lightSpawnPoint == null || mirrorLightPrefab == null)
            return;

        _lightInstance = Instantiate(mirrorLightPrefab, lightSpawnPoint.position, lightSpawnPoint.rotation, lightSpawnPoint);
    }
    
    private void SetLightActive(bool active)
    {
        if (_lightInstance != null && _lightInstance.activeSelf != active)
            _lightInstance.SetActive(active);
    }

    private void Update()
    {
        if (_ctx == null || _ctx.lightSensor == null || maxCharge <= 0f)
            return;

        if (!CanUseSelectedMirror())
            return;

        if (_ctx.input.AttackHeld && HasEnoughChargeForUse)
        {
            _isUsing = true;
            SetLightActive(true);
            UpdateLight();
            Use();
        }
        else
        {
            _isUsing = false;
            
            SetLightActive(false);
        }    

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
    }
    
    private void UpdateLight()
{
    if (hand == null) return;

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

    public void Use()
    {
        float cost = chargeCostPerSecond * Time.deltaTime;
        _currentCharge = Mathf.Max(0f, _currentCharge - cost);
    
        Debug.Log($"Use magical mirror. Current charge: {_currentCharge}/{maxCharge}");
    }

    private bool CanUseSelectedMirror()
    {
        return _ctx != null
            && _ctx.SelectedHotbarItem != null
            && _ctx.SelectedHotbarItem.itemName == ItemName.Mirror
            && _ctx.inventory != null
            && _ctx.inventory.Has(_ctx.SelectedHotbarItem);
    }
}
