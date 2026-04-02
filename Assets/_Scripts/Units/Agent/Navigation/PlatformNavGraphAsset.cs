using UnityEngine;

namespace PlatNav
{
    [CreateAssetMenu(fileName = "PlatNavGraph", menuName = "Data/Platformer Nav Graph")]
    public class PlatformNavGraphAsset : ScriptableObject
    {
        public Vector2Int tilemapSize;
        public Vector2Int tileOrigin;
        public Vector2 cellWorldOrigin; // world center of tile (0,0), for runtime tile→world conversion

        public WalkSegment[] segments;
        public NavLink[] links;
        public AxisBins horizontalBins;
        public AxisBins verticalBins;
    }
}
