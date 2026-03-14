using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlatNav
{
    public enum GravityDirection : byte
    {
        None = 0,
        Down = 1,
    }

    public enum SurfaceAxis : byte
    {
        Horizontal = 0,
        Vertical   = 1
    }

    public enum LinkMoveType : byte
    {
        Jump = 0,
        Fall = 1,
    }

    [Flags]
    public enum AbilityMask : ushort
    {
        None       = 0,
        Jump       = 1 << 0,
        WallJump = 1 << 1,
    }

    [Serializable]
    public struct WalkSegment
    {
        public GravityDirection gravity;
        public SurfaceAxis axis;
        public int line; // y (horizontal) or x (vertical)
        public int min; // xMin (horizontal) or yMin (vertical)
        public int max; // xMax (horizontal) or yMax (vertical)
        public ushort firstOut; // index into Link array
        public ushort outCount; // number of outgoing links

        public bool ContainsPoint(Vector2Int p)
        {
            if (axis == SurfaceAxis.Horizontal)
                return p.y == line && p.x >= min && p.x <= max;
            else
                return p.x == line && p.y >= min && p.y <= max;
        }

        public (Vector2Int a, Vector2Int b) Endpoints()
        {
            return axis == SurfaceAxis.Horizontal
                ? (new Vector2Int(min, line), new Vector2Int(max, line))
                : (new Vector2Int(line, min), new Vector2Int(line, max));
        }
    }

    [Serializable]
    public struct NavLink
    {
        public int fromSeg;
        public int toSeg;

        // Takeoff window along "from" segment: coordinate along its varying axis
        public int fromMin;
        public int fromMax;

        // Landing/arrival window along "to" segment
        public int toMin;
        public int toMax;

        public LinkMoveType moveType;
        public AbilityMask requiredAbilities;

        public short costFp;

        public Vector2 launchPos;
        public Vector2 launchVelocity;
        public Vector2 landPos;
        public float flightTime;
        public GravityDirection linkGravity;
        public float gravityStrength;

        public float Cost => costFp * 0.01f;

        public bool IsUsable(AbilityMask abilities) => (abilities & requiredAbilities) == requiredAbilities;

        public Vector2 Acceleration => Vector2.down * gravityStrength;

        public Vector2 EvaluatePosition(float t) => launchPos + launchVelocity * t + 0.5f * Acceleration * (t * t);

        public Vector2 EvaluateVelocity(float t) => launchVelocity + Acceleration * t;
    }

    [Serializable]
    public struct AxisBins
    {
        public SurfaceAxis axis; // which orientation these bins index
        public int binSize; // in tiles
        public int origin; // coordinate origin for bins (min X or min Y)
        public int binCount;

        // Compressed adjacency: for each bin i => refs[first[i]..first[i]+count[i]-1]
        public int[] first;
        public ushort[] count;
        public int[] refs; // segment indices
    }

}
