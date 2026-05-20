using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CollapsingTilemap : MonoBehaviour
{
    [SerializeField] private float _collapseDelay    = 0.6f;
    [SerializeField] private float _shakeAmplitude   = 0.06f;
    [SerializeField] private float _waveStepDelay    = 0.08f; // задержка между соседними тайлами
    [SerializeField] private float _respawnDelay     = 0f;

    [Header("Crumble")]
    [SerializeField] private int   _fragmentCount    = 4;
    [SerializeField] private float _fragmentGravity  = 3f;
    [SerializeField] private float _fragmentLifetime = 0.9f;
    [SerializeField] private string _fragmentLayer   = "Ground";
    [SerializeField] private PhysicsMaterial2D _fragmentMaterial;

    public bool IsCollapsing => _pending.Count > 0;

    private Tilemap         _tilemap;
    private TilemapRenderer _tilemapRenderer;
    private PlayerContext   _playerCtx;
    private readonly HashSet<Vector3Int> _pending = new();

    static readonly Vector3Int[] Dirs = { Vector3Int.right, Vector3Int.left };

    private void Awake()
    {
        _tilemap         = GetComponent<Tilemap>();
        _tilemapRenderer = GetComponent<TilemapRenderer>();
    }

    private void FixedUpdate()
    {
        if (_playerCtx == null)
        {
            if (!Services.IsRegistered<PlayerContext>()) return;
            _playerCtx = Services.Get<PlayerContext>();
        }

        if (!_playerCtx.grounded) return;

        var feet = new Vector3(
            _playerCtx.coll.bounds.center.x,
            _playerCtx.coll.bounds.min.y - 0.05f,
            0f);
        var cell = _tilemap.WorldToCell(feet);

        if (_tilemap.GetTile(cell) == null) return;
        if (_pending.Contains(cell)) return;

        // BFS с расстояниями от точки наступания
        var distances = FloodFill(cell);
        foreach (var c in distances.Keys)
            _pending.Add(c);

        foreach (var (c, dist) in distances)
            StartCoroutine(CollapseTile(c, dist * _waveStepDelay));
    }

    // Возвращает cell → расстояние от start в тайлах
    Dictionary<Vector3Int, int> FloodFill(Vector3Int start)
    {
        var dist  = new Dictionary<Vector3Int, int> { [start] = 0 };
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var dir in Dirs)
            {
                var next = current + dir;
                if (dist.ContainsKey(next)) continue;
                if (_tilemap.GetTile(next) == null) continue;
                dist[next] = dist[current] + 1;
                queue.Enqueue(next);
            }
        }

        return dist;
    }

    IEnumerator CollapseTile(Vector3Int cell, float startDelay)
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        if (_tilemap.GetTile(cell) == null) yield break;

        _tilemap.SetTileFlags(cell, TileFlags.None);
        var origMatrix = _tilemap.GetTransformMatrix(cell);
        var tile       = _tilemap.GetTile(cell);
        var sprite     = _tilemap.GetSprite(cell);
        var origin     = _tilemap.GetCellCenterWorld(cell);

        // Тряска — тайл остаётся в тайлмапе, освещение не меняется
        float elapsed = 0f;
        while (elapsed < _collapseDelay)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / _collapseDelay;
            float shake = Mathf.Sin(elapsed * 45f) * _shakeAmplitude * (1f - t);
            _tilemap.SetTransformMatrix(cell, Matrix4x4.Translate(new Vector3(shake, 0f, 0f)) * origMatrix);
            yield return null;
        }

        _tilemap.SetTransformMatrix(cell, origMatrix);
        _tilemap.SetTile(cell, null);
        _pending.Remove(cell);
        SpawnCrumble(origin, sprite);

        if (_respawnDelay > 0f)
        {
            yield return new WaitForSeconds(_respawnDelay);
            _tilemap.SetTile(cell, tile);
            _tilemap.SetTileFlags(cell, TileFlags.None);
            _tilemap.SetTransformMatrix(cell, origMatrix);
        }
    }

    void SpawnCrumble(Vector3 origin, Sprite sprite)
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

    void SpawnFragment(Vector3 pos, Sprite sprite, float scale, Vector2 velocity, float angularVel, float lifetime)
    {
        var go = new GameObject("_CrumbleFrag");
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * scale;

        int layer = LayerMask.NameToLayer(_fragmentLayer);
        go.layer  = layer >= 0 ? layer : 0;

        var sr            = go.AddComponent<SpriteRenderer>();
        sr.sprite         = sprite;
        sr.sharedMaterial = _tilemapRenderer.sharedMaterial;
        sr.sortingLayerID = _tilemapRenderer.sortingLayerID;
        sr.sortingOrder   = _tilemapRenderer.sortingOrder + 1;
        sr.color          = _tilemap.color;

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

    IEnumerator FadeAndDestroy(GameObject go, SpriteRenderer sr, float duration)
    {
        float elapsed   = 0f;
        Color baseColor = sr.color;
        while (elapsed < duration)
        {
            if (go == null) yield break;
            elapsed     += Time.deltaTime;
            baseColor.a  = 1f - Mathf.SmoothStep(0f, 1f, elapsed / duration);
            sr.color     = baseColor;
            yield return null;
        }
        if (go != null) Destroy(go);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var tm = GetComponent<Tilemap>();
        if (tm == null) return;
        Gizmos.color = new Color(0.8f, 0.35f, 0f, 0.25f);
        Gizmos.DrawCube(tm.localBounds.center + transform.position, tm.localBounds.size);
    }
#endif
}
