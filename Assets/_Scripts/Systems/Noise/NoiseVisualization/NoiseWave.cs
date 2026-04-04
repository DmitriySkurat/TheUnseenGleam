using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NoiseWave : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private float _startTime;
    private float _duration;
    private float _maxRadius;

    private const int Segments = 64;

    // До какого % от исходного радиуса расширяется кольцо (0..1)
    [SerializeField, Range(0f, 1f)] private float radiusFraction = 1f;
    // Кольцо начинает рассеиваться после прохождения этого % от визуального радиуса (0..1)
    [SerializeField, Range(0f, 1f)] private float fadeStartFraction = 0.6f;

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
        _maxRadius = maxRadius * radiusFraction;

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
        float alpha = t < fadeStartFraction
            ? 1f
            : 1f - Mathf.InverseLerp(fadeStartFraction, 1f, t);
        SetAlpha(alpha);
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
