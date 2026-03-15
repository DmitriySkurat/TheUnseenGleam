using System;
using UnityEngine;

public class AgentHearing : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;
    
    [Header("Hearing Settings")]
    [SerializeField, Min(0f)] private float sensitivity = 1f;
    [SerializeField, Min(0f)] private float maxHearingDistance = 0f;
    [SerializeField, Range(0f, 1f)] private float minLoudness = 0.1f;
    [SerializeField] private LayerMask occlusionMask;
    [SerializeField, Range(0f, 1f)] private float occlusionMultiplier = 0.4f;
    [SerializeField, Min(1)] private int occlusionSamples = 5;
    [SerializeField, Range(0f, 1f)] private float occlusionSampleRadius = 0.5f;
    [SerializeField] private bool includeCenterSample = true;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 1f, 0.8f);
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
        float effectiveRadius = noiseEvent.Radius * Mathf.Max(0f, sensitivity);
        if (maxHearingDistance > 0f)
            effectiveRadius = Mathf.Min(effectiveRadius, maxHearingDistance);

        if (effectiveRadius <= 0f)
            return;

        Vector2 listenerPos = transform.position;
        Vector2 noisePos = noiseEvent.Position;

        if (occlusionMask.value != 0)
        {
            float sampleRadius = noiseEvent.Radius * Mathf.Max(0f, occlusionSampleRadius);
            _lastNoisePosition = noisePos;
            _lastListenerPosition = listenerPos;
            _lastSampleRadius = sampleRadius;
            bool occluded = IsOccluded(listenerPos, noisePos, sampleRadius);
            if (occluded)
                effectiveRadius *= occlusionMultiplier;
        }

        if (effectiveRadius <= 0f)
            return;

        float distance = Vector2.Distance(listenerPos, noisePos);
        if (distance > effectiveRadius)
            return;

        float loudness = 1f - (distance / effectiveRadius);
        loudness = Mathf.Clamp01(loudness);

        if (loudness < minLoudness)
            return;

        _lastHeardPosition = noisePos;
        OnHeard?.Invoke(noiseEvent, loudness);
    }

    private bool IsOccluded(Vector2 listenerPos, Vector2 noisePos, float sampleRadius)
    {
        if (includeCenterSample)
        {
            var hit = Physics2D.Linecast(listenerPos, noisePos, occlusionMask);
            if (hit.collider == null)
                return false;
        }

        if (occlusionSamples <= 1 || sampleRadius <= 0f)
        {
            var hit = Physics2D.Linecast(listenerPos, noisePos, occlusionMask);
            return hit.collider != null;
        }

        Vector2 toNoise = noisePos - listenerPos;
        if (toNoise.sqrMagnitude < 0.0001f)
            return false;

        Vector2 dir = toNoise.normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        int samples = occlusionSamples;
        float denom = samples > 1 ? (samples - 1) : 1f;
        for (int i = 0; i < occlusionSamples; i++)
        {
            float t = samples > 1 ? (i / denom) : 0f;
            float offsetAmount = Mathf.Lerp(-sampleRadius, sampleRadius, t);
            Vector2 samplePos = noisePos + perp * offsetAmount;

            var hit = Physics2D.Linecast(listenerPos, samplePos, occlusionMask);
            if (hit.collider == null)
                return false;
        }

        return true;
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawHearing();
    }

    void OnDrawGizmosSelected()
    {
        DrawHearing();
    }

    private void DrawHearing()
    {
        if (!drawGizmos)
            return;

        if (maxHearingDistance > 0f)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, maxHearingDistance);
        }

        if (_lastHeardPosition.HasValue)
        {
            Gizmos.color = lastHeardColor;
            Gizmos.DrawSphere(_lastHeardPosition.Value, lastHeardMarkerRadius);
        }

        DrawOcclusionSamples();
    }

    private void DrawOcclusionSamples()
    {
        if (!drawOcclusionSamples)
            return;
        if (!_lastNoisePosition.HasValue || !_lastListenerPosition.HasValue)
            return;
        if (occlusionMask.value == 0)
            return;

        Vector2 listenerPos = _lastListenerPosition.Value;
        Vector2 noisePos = _lastNoisePosition.Value;
        float sampleRadius = _lastSampleRadius;

        if (includeCenterSample)
            DrawSample(listenerPos, noisePos);

        if (occlusionSamples <= 1 || sampleRadius <= 0f)
        {
            if (!includeCenterSample)
                DrawSample(listenerPos, noisePos);
            return;
        }

        Vector2 toNoise = noisePos - listenerPos;
        if (toNoise.sqrMagnitude < 0.0001f)
            return;

        Vector2 dir = toNoise.normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        int samples = occlusionSamples;
        float denom = samples > 1 ? (samples - 1) : 1f;
        for (int i = 0; i < samples; i++)
        {
            float t = samples > 1 ? (i / denom) : 0f;
            float offsetAmount = Mathf.Lerp(-sampleRadius, sampleRadius, t);
            Vector2 samplePos = noisePos + perp * offsetAmount;
            DrawSample(listenerPos, samplePos);
        }
    }

    private void DrawSample(Vector2 listenerPos, Vector2 samplePos)
    {
        bool blocked = Physics2D.Linecast(listenerPos, samplePos, occlusionMask).collider != null;
        Gizmos.color = blocked ? sampleBlockedColor : sampleClearColor;
        Gizmos.DrawLine(listenerPos, samplePos);
        Gizmos.DrawSphere(samplePos, sampleMarkerRadius);
    }
#endif
}
