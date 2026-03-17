using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public readonly struct NoiseEvent
{
    // Immutable payload for observers.
    public readonly Vector2 Position;
    public readonly float Radius;
    public readonly GameObject Source;
    public readonly NoiseType Type;

    public NoiseEvent(Vector2 position, float radius, GameObject source, NoiseType type)
    {
        Position = position;
        Radius = radius;
        Source = source;
        Type = type;
    }
}

public class NoiseSystem : MonoBehaviour, ISceneService
{
    public event Action<NoiseEvent> NoiseEmitted;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField, Min(0f)] private float gizmoLifetime = 1.5f;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0.1f, 0.8f);
    [SerializeField] private Color sourceColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField, Min(0.01f)] private float sourcePointRadius = 0.06f;

    private readonly List<NoiseGizmo> _gizmos = new List<NoiseGizmo>(32);

    public async Task InitializeAsync()
    {
        
        await Task.CompletedTask;
    }

    public void EmitNoise(Vector3 position, float radius, GameObject source, NoiseType type)
    {
        if (radius <= 0f) return;

        var noiseEvent = new NoiseEvent((Vector2)position, radius, source, type);
        NoiseEmitted?.Invoke(noiseEvent);

        // Store for Gizmos rendering.
        RegisterDebugNoise(noiseEvent);
        
    }

    private void RegisterDebugNoise(NoiseEvent noiseEvent)
    {
        if (!drawGizmos) return;
        _gizmos.Add(new NoiseGizmo(noiseEvent.Position, noiseEvent.Radius, Time.time));
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (_gizmos.Count == 0) return;

        float now = Application.isPlaying ? Time.time : 0f;

        for (int i = _gizmos.Count - 1; i >= 0; i--)
        {
            var g = _gizmos[i];
            if (Application.isPlaying && gizmoLifetime > 0f && now - g.time > gizmoLifetime)
            {
                _gizmos.RemoveAt(i);
                continue;
            }

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(g.position, g.radius);

            Gizmos.color = sourceColor;
            Gizmos.DrawSphere(g.position, sourcePointRadius);
        }
    }

    private readonly struct NoiseGizmo
    {
        public readonly Vector3 position;
        public readonly float radius;
        public readonly float time;

        public NoiseGizmo(Vector2 position, float radius, float time)
        {
            this.position = position;
            this.radius = radius;
            this.time = time;
        }
    }
}
