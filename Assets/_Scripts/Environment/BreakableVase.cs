using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BreakableVase : MonoBehaviour
{
    [SerializeField] private float breakNoiseRadius = 7f;
    [SerializeField, Range(0f, 0.5f)] private float noiseVariance = 0.1f;
    [SerializeField] private NoiseType noiseType = NoiseType.LoudNoise;
    [SerializeField] public float BreakMinSpeed = 3f;

    [Header("Fragments")]
    [SerializeField] private int   _fragmentCount    = 5;
    [SerializeField] private float _fragmentGravity  = 3f;
    [SerializeField] private float _fragmentLifetime = 0.9f;
    [SerializeField] private string _fragmentLayer   = "Ground";
    [SerializeField] private PhysicsMaterial2D _fragmentMaterial;

    private NoiseSystem         _noiseSystem;
    private AudioManager        _audioManager;
    private NoiseScriptableStats _noiseStats;
    private SpriteRenderer      _sr;
    private Collider2D          _collider;
    private bool                _broken;

    private void Start()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
        if (Services.IsRegistered<AudioManager>())
            _audioManager = Services.Get<AudioManager>();
        if (Services.IsRegistered<PlayerContext>())
            _noiseStats = Services.Get<PlayerContext>().noiseStats;
        _sr       = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void OnHitByProjectile(Vector2 hitPoint, GameObject source)
    {
        if (_broken) return;
        _broken = true;

        _noiseSystem?.EmitNoise(hitPoint, breakNoiseRadius, gameObject, noiseType, noiseVariance);
        if (_noiseStats != null && _noiseStats.VaseBreakClip != null)
            _audioManager?.PlaySfxAtPoint(_noiseStats.VaseBreakClip, hitPoint);

        _collider.enabled = false;
        SpawnCrumble(transform.position, _sr.sprite);
        _sr.enabled = false;
    }

    private void SpawnCrumble(Vector3 origin, Sprite sprite)
    {
        SpawnFragment(origin, sprite,
            scale:      1f,
            velocity:   new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(-0.5f, 0.5f)),
            angularVel: Random.Range(-120f, 120f),
            lifetime:   _fragmentLifetime);

        for (int i = 0; i < _fragmentCount; i++)
        {
            var offset = new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.2f, 0.2f), 0f);
            SpawnFragment(origin + offset, sprite,
                scale:      Random.Range(0.2f, 0.45f),
                velocity:   new Vector2(Random.Range(-3f, 3f), Random.Range(0.5f, 2.5f)),
                angularVel: Random.Range(-360f, 360f),
                lifetime:   Random.Range(_fragmentLifetime * 0.5f, _fragmentLifetime));
        }
    }

    private void SpawnFragment(Vector3 pos, Sprite sprite, float scale, Vector2 velocity, float angularVel, float lifetime)
    {
        var go = new GameObject("_VaseFrag");
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * scale;

        int layer = LayerMask.NameToLayer(_fragmentLayer);
        go.layer  = layer >= 0 ? layer : 0;

        var sr            = go.AddComponent<SpriteRenderer>();
        sr.sprite         = sprite;
        sr.sharedMaterial = _sr.sharedMaterial;
        sr.sortingLayerID = _sr.sortingLayerID;
        sr.sortingOrder   = _sr.sortingOrder + 1;
        sr.color          = _sr.color;

        var col    = go.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;
        if (_fragmentMaterial) col.sharedMaterial = _fragmentMaterial;

        var rb                    = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = _fragmentGravity;
        rb.linearVelocity         = velocity;
        rb.angularVelocity        = angularVel;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        StartCoroutine(FadeAndDestroy(go, sr, lifetime));
    }

    private IEnumerator FadeAndDestroy(GameObject go, SpriteRenderer sr, float duration)
    {
        float elapsed   = 0f;
        Color baseColor = sr.color;
        while (elapsed < duration)
        {
            if (go == null) yield break;
            elapsed    += Time.deltaTime;
            baseColor.a = 1f - Mathf.SmoothStep(0f, 1f, elapsed / duration);
            sr.color    = baseColor;
            yield return null;
        }
        if (go != null) Destroy(go);
    }
}
