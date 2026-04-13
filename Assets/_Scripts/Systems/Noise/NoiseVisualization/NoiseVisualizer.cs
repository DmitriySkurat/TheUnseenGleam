using System.Collections.Generic;
using UnityEngine;

public class NoiseVisualizer : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Header("Setup")]
    [SerializeField] private NoiseWave wavePrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Wave Settings")]
    [SerializeField, Min(0.01f)] private float duration = 1f;

    [Header("Cooldowns")]
    [Tooltip("Минимальный интервал между кольцами шагов (сек)")]
    [SerializeField, Min(0f)] private float footstepCooldown = 0.4f;
    [Tooltip("Минимальный интервал между кольцами дыхания (сек)")]
    [SerializeField, Min(0f)] private float breathingCooldown = 0.75f;
    [Tooltip("Минимальный интервал между кольцами приземления (сек)")]
    [SerializeField, Min(0f)] private float landingCooldown = 0.5f;

    [Header("Debug")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.4f, 0.1f, 0.6f);

    private NoiseSystem _noiseSystem;
    private NoiseScriptableStats _noiseStats;
    private AgentHearing[] _agents;
    private float _lastFootstepSpawnTime;
    private float _lastBreathingSpawnTime;
    private float _lastLandingSpawnTime;
    private readonly Queue<NoiseWave> _activeWaves = new();

    public void Initialize()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
        _noiseStats = Services.Get<PlayerContext>().noiseStats;
        _noiseSystem.NoiseEmitted += OnNoiseEmitted;
        _agents = FindObjectsByType<AgentHearing>(FindObjectsSortMode.None);
    }

    private void OnDisable()
    {
        if (_noiseSystem != null)
            _noiseSystem.NoiseEmitted -= OnNoiseEmitted;
    }

    private void OnNoiseEmitted(NoiseEvent noise)
    {
        if (!ShouldVisualize(noise)) return;
        if (RequiresEnemyCheck(noise) && !AnyEnemyInRange(noise)) return;
        if (IsOnCooldown(noise)) return;

        UpdateCooldown(noise);
        SpawnWave(noise);
    }

    private bool ShouldVisualize(NoiseEvent noise) => noise.Type switch
    {
        NoiseType.Breathing    => true,
        NoiseType.Footstep     => noise.Radius >= _noiseStats.MinFootstepVisualizationRadius,
        NoiseType.Jump         => true,
        NoiseType.Landing      => true,
        NoiseType.ObjectImpact => true,
        _                      => false
    };

    private bool RequiresEnemyCheck(NoiseEvent noise) => noise.Type switch
    {
        NoiseType.Breathing => true,
        _                   => false
    };

    private bool AnyEnemyInRange(NoiseEvent noise)
    {
        foreach (var agent in _agents)
        {
            if (agent == null) continue;
            if (Vector2.Distance(agent.transform.position, noise.Position) <= _noiseStats.VisualizationEnemyDetectionRadius)
                return true;
        }
        return false;
    }

    private bool IsOnCooldown(NoiseEvent noise) => noise.Type switch
    {
        NoiseType.Footstep     => Time.time - _lastFootstepSpawnTime  < footstepCooldown,
        NoiseType.Breathing    => Time.time - _lastBreathingSpawnTime < breathingCooldown,
        NoiseType.Landing      => Time.time - _lastLandingSpawnTime   < landingCooldown,
        NoiseType.ObjectImpact => false,
        _                      => false
    };

    private void UpdateCooldown(NoiseEvent noise)
    {
        switch (noise.Type)
        {
            case NoiseType.Footstep:  _lastFootstepSpawnTime  = Time.time; break;
            case NoiseType.Breathing: _lastBreathingSpawnTime = Time.time; break;
            case NoiseType.Landing:   _lastLandingSpawnTime   = Time.time; break;
        }
    }

    // Непрерывный шум (шаги бега, дыхание) — кольцо следует за игроком
    // Точечный шум (приземление, камень)   — кольцо остаётся в точке события
    private bool FollowsSource(NoiseEvent noise) =>
        noise.Type == NoiseType.Footstep || noise.Type == NoiseType.Breathing;

    private void SpawnWave(NoiseEvent noise)
    {
        while (_activeWaves.Count > 0 && _activeWaves.Peek() == null)
            _activeWaves.Dequeue();

        if (_activeWaves.Count >= _noiseStats.MaxSimultaneousWaves)
        {
            var old = _activeWaves.Dequeue();
            if (old != null) Destroy(old.gameObject);
        }

        NoiseWave wave;

        if (FollowsSource(noise))
        {
            Transform parent = noise.Source != null ? noise.Source.transform : transform;
            wave = Instantiate(wavePrefab, parent.position, Quaternion.identity, parent);
            wave.transform.localPosition = Vector3.zero;
        }
        else
        {
            wave = Instantiate(wavePrefab, noise.Position, Quaternion.identity, null);
        }

        wave.Init(noise.Radius, duration);
        _activeWaves.Enqueue(wave);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_noiseStats == null) return;

        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        UnityEditor.Handles.color = gizmoColor;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, _noiseStats.VisualizationEnemyDetectionRadius);

        UnityEditor.Handles.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.08f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, _noiseStats.VisualizationEnemyDetectionRadius);
    }
#endif
}
