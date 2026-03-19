using UnityEngine;

public class MagicalMirror : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 10;

    [Header("Capacity")]
    [SerializeField, Min(0f)] private float maxCharge = 100f;
    [SerializeField, Range(0f, 1f)] private float initialCharge = 0f;

    [Header("Charging")]
    [SerializeField, Min(0f)] private float chargePerSecond = 10f;
    [SerializeField, Min(0f)] private float minLightStrengthToCharge = 0.1f;
    [SerializeField] private bool chargeOnlyWhenGrounded = true;

    private PlayerContext _ctx;
    private LightExposureSensor _lightExposureSensor;

    private float _currentCharge;

    public float CurrentCharge => maxCharge <= 0f ? 0f : Mathf.Clamp01(_currentCharge / maxCharge);
    public bool IsFull => maxCharge > 0f && _currentCharge >= maxCharge;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _lightExposureSensor = _ctx != null ? _ctx.lightSensor : null;

        _currentCharge = maxCharge <= 0f ? 0f : Mathf.Clamp01(initialCharge) * maxCharge;
    }

    private void Update()
    {
        if (_ctx == null || _lightExposureSensor == null || maxCharge <= 0f)
            return;
            
        Debug.Log($"current mirror charge: {_currentCharge}");

        if (chargeOnlyWhenGrounded && !_ctx.grounded)
            return;

        if (_currentCharge >= maxCharge)
        {
            _currentCharge = maxCharge;
            return;
        }

        float strength = _lightExposureSensor.CurrentStrength;
        if (strength < minLightStrengthToCharge)
            return;

        _currentCharge = Mathf.Min(maxCharge, _currentCharge + chargePerSecond * Time.deltaTime);
    }
}
