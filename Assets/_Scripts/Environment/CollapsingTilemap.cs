using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CollapsingTilemap : MonoBehaviour
{
    [SerializeField] private float _collapseDelay = 0.6f;
    [SerializeField] private float _shakeAmplitude = 0.06f;
    [SerializeField] private float _respawnDelay = 0f;

    private Tilemap _tilemap;
    private TilemapRenderer _tilemapRenderer;
    private PlayerContext _playerCtx;
    private readonly HashSet<Vector3Int> _pending = new();

    static readonly Vector3Int[] Dirs = { Vector3Int.right, Vector3Int.left };

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
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

        // Тайл строго под центром ног игрока
        var feet = new Vector3(
            _playerCtx.coll.bounds.center.x,
            _playerCtx.coll.bounds.min.y - 0.05f,
            0f);
        var cell = _tilemap.WorldToCell(feet);

        if (_tilemap.GetTile(cell) == null) return;
        if (_pending.Contains(cell)) return;

        var group = FloodFill(cell);
        foreach (var c in group)
            _pending.Add(c);

        StartCoroutine(CollapseGroupRoutine(group));
    }

    HashSet<Vector3Int> FloodFill(Vector3Int start)
    {
        var visited = new HashSet<Vector3Int>();
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var dir in Dirs)
            {
                var next = current + dir;
                if (visited.Contains(next)) continue;
                if (_tilemap.GetTile(next) == null) continue;
                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return visited;
    }

    IEnumerator CollapseGroupRoutine(HashSet<Vector3Int> group)
    {
        var cellData = new List<(Vector3Int cell, TileBase tile, GameObject go, Vector3 origin)>();

        foreach (var cell in group)
        {
            var tile   = _tilemap.GetTile(cell);
            var sprite = _tilemap.GetSprite(cell);

            _tilemap.SetTileFlags(cell, TileFlags.None);
            _tilemap.SetColor(cell, Color.clear);

            var go = new GameObject("_CollapsingTile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite         = sprite;
            sr.sortingLayerID = _tilemapRenderer.sortingLayerID;
            sr.sortingOrder   = _tilemapRenderer.sortingOrder;
            sr.color          = _tilemap.color;

            Vector3 origin = _tilemap.GetCellCenterWorld(cell);
            go.transform.position = origin;

            cellData.Add((cell, tile, go, origin));
        }

        float elapsed = 0f;
        while (elapsed < _collapseDelay)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / _collapseDelay;
            float shake = Mathf.Sin(elapsed * 45f) * _shakeAmplitude * (1f - t);
            foreach (var (_, _, go, origin) in cellData)
                go.transform.position = origin + Vector3.right * shake;
            yield return null;
        }

        foreach (var (cell, _, go, _) in cellData)
        {
            Destroy(go);
            _tilemap.SetTile(cell, null);
            _pending.Remove(cell);
        }

        if (_respawnDelay > 0f)
        {
            yield return new WaitForSeconds(_respawnDelay);
            foreach (var (cell, tile, _, _) in cellData)
                _tilemap.SetTile(cell, tile);
        }
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
