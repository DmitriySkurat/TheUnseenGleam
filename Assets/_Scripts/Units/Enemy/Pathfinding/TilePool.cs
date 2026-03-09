using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class TilePool
    {
        public List<Tile> targets;
        public List<Tile> origins;
        public List<TilePool> children;

        public TilePool()
        {
            origins = new List<Tile>();
            targets = new List<Tile>();
            children = new List<TilePool>();
        }
    }
}