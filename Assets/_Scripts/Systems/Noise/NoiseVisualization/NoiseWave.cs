using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NoiseWave : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private float _startTime;
    private float _duration;
    private float _maxRadius;

    private const int Segments = 64;

    public void Init(float maxRadius, float duration)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.positionCount = Segments + 1;
        _lineRenderer.loop = false;
        _lineRenderer.startWidth = 0.06f;
        _lineRenderer.endWidth = 0.06f;

        _startTime = Time.time;
        _duration = duration;
        _maxRadius = maxRadius;

        DrawCircle(0f);
    }

    private void Update()
    {
        float t = (Time.time - _startTime) / _duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        DrawCircle(Mathf.Lerp(0f, _maxRadius, t));
        SetAlpha(1f - t);
    }

    private void DrawCircle(float radius)
    {
        for (int i = 0; i <= Segments; i++)
        {
            float angle = (float)i / Segments * Mathf.PI * 2f;
            _lineRenderer.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            ));
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = _lineRenderer.startColor;
        Color faded = new Color(c.r, c.g, c.b, alpha);
        _lineRenderer.startColor = faded;
        _lineRenderer.endColor = faded;
    }
}
