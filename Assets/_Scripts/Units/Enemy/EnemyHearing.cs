using UnityEngine;
using Entity.Enemy;
using HSM;
using UnityEngine.PlayerLoop;
using System;

[RequireComponent(typeof(EnemyStateDriver))]
public class EnemyHearing : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;
    
    [Header("Hearing")]
    [SerializeField, Min(0f)] private float hearingMultiplier = 1f;
    [SerializeField] private bool ignoreOwnNoise = true;

    [Header("Investigate Settings")]
    [SerializeField, Min(0f)] private float investigateStopDistance = 0.35f;
    [SerializeField, Min(0f)] private float noiseMemoryDuration = 4f;

    private EnemyStateDriver _driver;
    private Transform _noiseTarget;
    
    private NoiseSystem _noiseSystem;
    private bool _subscribed;
    private static Transform _targetsRoot;
    
    public void Initialize() 
    {
        _driver = GetComponent<EnemyStateDriver>();
        TrySubscribeNoise();
        
        EnsureNoiseTarget();
        BindContext(_driver != null ? _driver.Context : null);
    }
    
    public void Dispose()
    {
        if (_noiseSystem != null && _subscribed)
        {
            _noiseSystem.NoiseEmitted -= HandleNoise;
            _subscribed = false;
        }
    }

    private void Update()
    {
        if (_driver != null && _driver.Context != null)
        {
            var ctx = _driver.Context;
            if (ctx.hasNoiseTarget && ctx.noiseMemoryDuration > 0f && Time.time - ctx.lastNoiseTime > ctx.noiseMemoryDuration)
            {
                ctx.hasNoiseTarget = false;
            }
        }

        if (_subscribed) return;
        TrySubscribeNoise();
    }

    private void TrySubscribeNoise()
    {
        if (_subscribed) return;
        try
        {
            _noiseSystem = Services.Get<NoiseSystem>();
        }
        catch (Exception)
        {
            _noiseSystem = null;
        }

        if (_noiseSystem == null) return;
        _noiseSystem.NoiseEmitted += HandleNoise;
        _subscribed = true;
    }

    private static Transform GetTargetsRoot()
    {
        if (_targetsRoot != null) return _targetsRoot;
        var existing = GameObject.Find("EnemyTargetsRoot");
        if (existing != null)
        {
            _targetsRoot = existing.transform;
            return _targetsRoot;
        }
        var root = new GameObject("EnemyTargetsRoot");
        _targetsRoot = root.transform;
        return _targetsRoot;
    }


    private void EnsureNoiseTarget()
    {
        if (_noiseTarget != null) return;

        Transform root = GetTargetsRoot();
        var go = new GameObject("NoiseTarget");
        go.transform.SetParent(root, true);
        go.transform.localPosition = Vector3.zero;
        _noiseTarget = go.transform;
    }

    private void EnsureContext()
    {
        if (_driver == null) _driver = GetComponent<EnemyStateDriver>();
        if (_driver == null) return;

        EnemyContext ctx = _driver.Context;
        if (ctx == null) return;

        BindContext(ctx);
    }

    public void BindContext(EnemyContext ctx)
    {
        if (ctx == null) return;
        if (_noiseTarget == null) EnsureNoiseTarget();
        ctx.noiseTarget = _noiseTarget;
        ctx.investigateStopDistance = investigateStopDistance;
        ctx.noiseMemoryDuration = noiseMemoryDuration;
        ctx.hearing = this;
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

        ctx.lastKnownPlayerPosition = noiseEvent.Position;
        ctx.hasLastKnownPlayerPosition = true;
        ctx.lastKnownPlayerTime = Time.time;

        if (ctx.debugConditions)
        {
            ctx.Log($"Noise heard: type={noiseEvent.Type}, pos={noiseEvent.Position}, radius={noiseEvent.Radius:F2}, dist={Mathf.Sqrt(sqrDistance):F2}");
        }

        ctx.movement?.FaceTowards(noiseEvent.Position);
    }
}
