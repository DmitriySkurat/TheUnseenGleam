using System;
using UnityEngine;

public class AgentHearing : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Hearing")]
    [SerializeField, Min(0f)] private float maxHearingRange = 15f;

    [Header("Occlusion")]
    [SerializeField] private LayerMask occlusionMask;
    [SerializeField, Range(0f, 1f)] private float occlusionMultiplier = 0.4f;
    [SerializeField, Min(1)] private int occlusionSamples = 5;
    [SerializeField, Range(0f, 1f)] private float occlusionSampleRadius = 0.5f;
    [SerializeField] private bool includeCenterSample = true;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color hearingRangeColor = new Color(0.2f, 0.8f, 1f, 0.15f);
    [SerializeField] private Color lastHeardColor = new Color(1f, 0.4f, 0.1f, 0.9f);
    [SerializeField, Min(0f)] private float lastHeardMarkerRadius = 1.5f;
    [SerializeField] private bool drawOcclusionSamples = true;
    [SerializeField] private Color sampleClearColor = new Color(0.3f, 1f, 0.3f, 0.9f);
    [SerializeField] private Color sampleBlockedColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField, Min(0f)] private float sampleMarkerRadius = 0.08f;

    public event Action<NoiseEvent, float> OnHeard;

    private NoiseSystem _noiseSystem;
    private Vector2? _lastHeardPosition;
    private Vector2? _lastNoisePosition;
    private Vector2? _lastListenerPosition;
    private float _lastSampleRadius;

    public void Initialize()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
        _noiseSystem.NoiseEmitted += HandleNoiseEmitted;
    }

    public void Dispose()
    {
        _noiseSystem.NoiseEmitted -= HandleNoiseEmitted;
    }

    private void HandleNoiseEmitted(NoiseEvent noiseEvent)
    {
        Vector2 listenerPos = transform.position;
        Vector2 noisePos = noiseEvent.Position;

        float distance = Vector2.Distance(listenerPos, noisePos);
        if (distance > maxHearingRange)
            return;

        float effectiveRadius = noiseEvent.Radius;

        if (occlusionMask.value != 0)
        {
            float sampleRadius = noiseEvent.Radius * occlusionSampleRadius;
            _lastNoisePosition = noisePos;
            _lastListenerPosition = listenerPos;
            _lastSampleRadius = sampleRadius;

            if (IsOccluded(listenerPos, noisePos, sampleRadius))
                effectiveRadius *= occlusionMultiplier;
        }

        if (distance > effectiveRadius)
            return;

        float loudness = Mathf.Clamp01(1f - (distance / noiseEvent.Radius));

        _lastHeardPosition = noisePos;
        OnHeard?.Invoke(noiseEvent, loudness);
    }

    private bool IsOccluded(Vector2 listenerPos, Vector2 noisePos, float sampleRadius)
    {
        if (includeCenterSample)
        {
            if (Physics2D.Linecast(listenerPos, noisePos, occlusionMask).collider == null)
                return false;
        }

        if (occlusionSamples <= 1 || sampleRadius <= 0f)
            return Physics2D.Linecast(listenerPos, noisePos, occlusionMask).collider != null;

        Vector2 toNoise = noisePos - listenerPos;
        if (toNoise.sqrMagnitude < 0.0001f)
            return false;

        Vector2 perp = new Vector2(-toNoise.normalized.y, toNoise.normalized.x);
        float denom = occlusionSamples > 1 ? occlusionSamples - 1 : 1f;

        for (int i = 0; i < occlusionSamples; i++)
        {
            float t = occlusionSamples > 1 ? i / denom : 0f;
            Vector2 samplePos = noisePos + perp * Mathf.Lerp(-sampleRadius, sampleRadius, t);

            if (Physics2D.Linecast(listenerPos, samplePos, occlusionMask).collider == null)
                return false;
        }

        return true;
    }


#if UNITY_EDITOR
    void OnDrawGizmos()      => DrawHearing();
    void OnDrawGizmosSelected() => DrawHearing();

    private void DrawHearing()
    {
        if (!drawGizmos || !isActiveAndEnabled) return;

        Gizmos.color = hearingRangeColor;
        DrawCircle(transform.position, maxHearingRange);

        if (_lastHeardPosition.HasValue)
        {
            Gizmos.color = lastHeardColor;
            Gizmos.DrawSphere(_lastHeardPosition.Value, lastHeardMarkerRadius);
        }

        DrawOcclusionSamples();
    }

    private void DrawOcclusionSamples()
    {
        if (!drawOcclusionSamples) return;
        if (!_lastNoisePosition.HasValue || !_lastListenerPosition.HasValue) return;
        if (occlusionMask.value == 0) return;

        Vector2 listenerPos = _lastListenerPosition.Value;
        Vector2 noisePos = _lastNoisePosition.Value;

        if (includeCenterSample)
            DrawSample(listenerPos, noisePos);

        if (occlusionSamples <= 1 || _lastSampleRadius <= 0f)
        {
            if (!includeCenterSample)
                DrawSample(listenerPos, noisePos);
            return;
        }

        Vector2 toNoise = noisePos - listenerPos;
        if (toNoise.sqrMagnitude < 0.0001f) return;

        Vector2 perp = new Vector2(-toNoise.normalized.y, toNoise.normalized.x);
        float denom = occlusionSamples > 1 ? occlusionSamples - 1 : 1f;

        for (int i = 0; i < occlusionSamples; i++)
        {
            float t = occlusionSamples > 1 ? i / denom : 0f;
            DrawSample(listenerPos, noisePos + perp * Mathf.Lerp(-_lastSampleRadius, _lastSampleRadius, t));
        }
    }

    private void DrawSample(Vector2 from, Vector2 to)
    {
        bool blocked = Physics2D.Linecast(from, to, occlusionMask).collider != null;
        Gizmos.color = blocked ? sampleBlockedColor : sampleClearColor;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(to, sampleMarkerRadius);
    }

    private static void DrawCircle(Vector2 center, float radius, int segments = 64)
    {
        float step = 2f * Mathf.PI / segments;
        Vector2 prev = center + new Vector2(radius, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
