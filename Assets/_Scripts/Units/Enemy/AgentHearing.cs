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

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 1f, 0.8f);
    [SerializeField] private Color lastHeardColor = new Color(1f, 0.4f, 0.1f, 0.9f);
    [SerializeField, Min(0f)] private float lastHeardMarkerRadius = 1.5f;

    public event Action<NoiseEvent, float> OnHeard;

    private NoiseSystem _noiseSystem;
    private Vector2? _lastHeardPosition;

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
            var hit = Physics2D.Linecast(listenerPos, noisePos, occlusionMask);
            if (hit.collider != null)
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
    }
#endif
}
