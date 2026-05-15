using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BreakableVase : MonoBehaviour
{
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private float breakNoiseRadius = 7f;
    [SerializeField, Range(0f, 0.5f)] private float noiseVariance = 0.1f;
    [SerializeField] private NoiseType noiseType = NoiseType.LoudNoise;

    private NoiseSystem _noiseSystem;
    private SpriteRenderer _sr;
    private Collider2D _collider;
    private bool _broken;

    private void Start()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
        _sr = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void OnHitByProjectile(Vector2 hitPoint, GameObject source)
    {
        if (_broken) return;
        _broken = true;

        _noiseSystem?.EmitNoise(hitPoint, breakNoiseRadius, gameObject, noiseType, noiseVariance);
        Debug.Log($"[BreakableVase] {gameObject.name} broke at {hitPoint}, noise radius: {breakNoiseRadius}");

        if (brokenSprite != null)
            _sr.sprite = brokenSprite;

        _collider.enabled = false;
    }
}
