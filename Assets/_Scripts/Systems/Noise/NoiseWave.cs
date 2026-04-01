using UnityEngine;

public class NoiseWave : MonoBehaviour
{
    private Material _mat;

    private float _startTime;
    private float _duration;
    private float _maxRadius;

    private static readonly int RadiusID = Shader.PropertyToID("_WaveDistanceFromCenter");
    private static readonly int CenterID = Shader.PropertyToID("_RingSpawnPosition");

    public void Init(Vector2 worldPos, float maxRadius, float duration)
    {
        _mat = GetComponent<SpriteRenderer>().material;

        _startTime = Time.time;
        _duration = duration;
        _maxRadius = maxRadius;

        Vector2 uv = Camera.main.WorldToViewportPoint(worldPos);

        _mat.SetVector(CenterID, uv);
    }

    private void Update()
    {
        float t = (Time.time - _startTime) / _duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float radius = Mathf.Lerp(0f, _maxRadius, t);

        _mat.SetFloat(RadiusID, radius);
    }
}