using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GravityManager : MonoBehaviour
{
    [System.Serializable]
    private struct TileGravity
    {
        public TileBase tile;
        public Vector2 direction;
    }

    [SerializeField] private List<TileGravity> mappings = new List<TileGravity>();

    public static Dictionary<TileBase, Vector2> tileGravity = new Dictionary<TileBase, Vector2>();

    private void OnEnable()
    {
        BuildMap();
    }

    private void OnValidate()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        tileGravity = new Dictionary<TileBase, Vector2>();
        foreach (var mapping in mappings)
        {
            if (mapping.tile == null)
            {
                continue;
            }
            if (mapping.direction == Vector2.zero)
            {
                continue;
            }
            tileGravity[mapping.tile] = mapping.direction.normalized;
        }
    }
}
