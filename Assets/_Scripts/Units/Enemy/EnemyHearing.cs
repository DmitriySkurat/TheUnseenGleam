using UnityEngine;
using Entity.Enemy;
using HSM;

[RequireComponent(typeof(EnemyStateDriver))]
public class EnemyHearing : MonoBehaviour
{
    [Header("Hearing")]
    [SerializeField, Min(0f)] private float hearingMultiplier = 1f;
    [SerializeField] private bool ignoreOwnNoise = true;

    [Header("Investigate Settings")]
    [SerializeField, Min(0f)] private float investigateStopDistance = 0.35f;
    [SerializeField, Min(0f)] private float noiseMemoryDuration = 4f;

    private EnemyStateDriver _driver;
    private Transform _noiseTarget;

    private void Awake()
    {
        _driver = GetComponent<EnemyStateDriver>();
        EnsureNoiseTarget();
    }

    private void OnEnable()
    {
        NoiseSystem.NoiseEmitted += HandleNoise;
        EnsureContext();
    }

    private void OnDisable()
    {
        NoiseSystem.NoiseEmitted -= HandleNoise;
    }

    private void EnsureNoiseTarget()
    {
        if (_noiseTarget != null) return;

        var go = new GameObject("NoiseTarget");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        _noiseTarget = go.transform;
    }

    private void EnsureContext()
    {
        if (_driver == null) _driver = GetComponent<EnemyStateDriver>();
        if (_driver == null) return;

        EnemyContext ctx = _driver.Context;
        if (ctx == null) return;

        ctx.noiseTarget = _noiseTarget;
        ctx.investigateStopDistance = investigateStopDistance;
        ctx.noiseMemoryDuration = noiseMemoryDuration;
    }

    private void HandleNoise(NoiseEvent noiseEvent)
    {
        if (ignoreOwnNoise && noiseEvent.Source == gameObject) return;

        if (_driver == null || _driver.Context == null) EnsureContext();
        EnemyContext ctx = _driver != null ? _driver.Context : null;
        if (ctx == null) return;

        // Fast distance check using squared magnitude.
        Vector2 selfPos = ctx.self != null ? (Vector2)ctx.self.position : (Vector2)transform.position;
        float effectiveRadius = noiseEvent.Radius * hearingMultiplier;
        float sqrRadius = effectiveRadius * effectiveRadius;
        float sqrDistance = (selfPos - noiseEvent.Position).sqrMagnitude;

        if (sqrDistance > sqrRadius) return;

        if (_noiseTarget == null) EnsureNoiseTarget();
        if (_noiseTarget != null)
        {
            _noiseTarget.position = new Vector3(noiseEvent.Position.x, noiseEvent.Position.y, _noiseTarget.position.z);
        }

        // Cache the noise for the state machine.
        ctx.lastNoisePosition = noiseEvent.Position;
        ctx.lastNoiseType = noiseEvent.Type;
        ctx.lastNoiseTime = Time.time;
        ctx.hasNoiseTarget = true;
    }
}
