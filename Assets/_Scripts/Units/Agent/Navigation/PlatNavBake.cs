using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlatNav
{
    public class PlatNavBake : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap wallTM;
        [SerializeField] private Tilemap spikesTM;
        [SerializeField] private PlatformNavGraphAsset graph;

        [Header("Entity")]
        [SerializeField] private Vector2 entitySize;

        [Header("Bounds (world tile coordinates)")]
        [SerializeField] private Vector2Int minPos;
        [SerializeField] private Vector2Int maxPos;

        [Header("Bake – Physics")]
        [SerializeField] private float maxJumpVelocity = 10f;
        [SerializeField] private float gravityStrength = 10f;
        [SerializeField] private float walkSpeed       = 3f;

        [Header("Bake – Search")]
        [SerializeField] private int   jumpSearchRadius       = 8;
        [SerializeField] private int   numTrajectoriesToTest   = 5;
        [SerializeField] private float trajectoryStep          = 0.02f;
        [SerializeField] private float maxFallTime             = 5f;
        [SerializeField] private int   allowJumpDownTiles      = 1;

        [Header("Bake – Cost")]
        [SerializeField] private float jumpCostMultiplier = 20f;
        [SerializeField] private float fallCostMultiplier = 4f;

        [Header("Gizmos – Toggles")]
        [SerializeField] internal bool showBakeBounds            = true;
        [SerializeField] internal bool showSegments             = true;
        [SerializeField] internal bool showJumpLinks            = true;
        [SerializeField] internal bool showFallLinks            = true;
        [SerializeField] internal bool showRejectedTrajectories = false;

        [Header("Gizmos – Colors")]
        [SerializeField] internal Color bakeBoundsColor     = new Color(1f,   0.92f, 0.016f, 0.8f);
        [SerializeField] internal Color segmentColor        = new Color(0.2f, 0.9f, 0.3f, 0.9f);
        [SerializeField] internal Color jumpLinkColor       = new Color(0.3f, 0.8f, 1f,   0.8f);
        [SerializeField] internal Color fallLinkColor       = new Color(1f,   0.85f, 0.2f, 0.8f);
        [SerializeField] internal Color rejectedArcColor    = new Color(1f,   0.25f, 0.25f, 0.3f);
        [NonSerialized] public List<Vector3[]> rejectedArcs = new();

        private int                _gridW, _gridH;
        private bool[,]            _solid;   // true = impassable (wall or spike)
        private bool[,]            _spike;   // true = spike tile (subset of solid)

        private int _tileW, _tileH;          // entity footprint in tiles

        private int  GX(int wx) => wx - minPos.x;
        private int  GY(int wy) => wy - minPos.y;
        private bool InBounds(int wx, int wy) => wx >= minPos.x && wx < maxPos.x && wy >= minPos.y && wy < maxPos.y;

        private bool IsSolid(int wx, int wy) => !InBounds(wx, wy) || _solid[GX(wx), GY(wy)];

        private bool IsSpike(int wx, int wy) => InBounds(wx, wy) && _spike[GX(wx), GY(wy)];

        private Vector2 TileCenter(int wx, int wy) => (Vector2)wallTM.GetCellCenterWorld(new Vector3Int(wx, wy, 0));

        private Vector2 SegCoord(in WalkSegment s, int c) => s.axis == SurfaceAxis.Horizontal ? TileCenter(c, s.line) : TileCenter(s.line, c);

        private static Vector2Int GravVec(GravityDirection g) => new Vector2Int(0, -1);

        private static Vector2Int PerpVec(GravityDirection g) => new Vector2Int(1, 0);

        private static SurfaceAxis GravAxis(GravityDirection g) => SurfaceAxis.Horizontal;

        private Vector2 AccelFor(GravityDirection g) => Vector2.down * gravityStrength;

        private static short FpCost(float cost) => (short)Mathf.Clamp(Mathf.RoundToInt(cost * 100f), short.MinValue, short.MaxValue);

        private void BuildGrids()
        {
            _gridW = maxPos.x - minPos.x;
            _gridH = maxPos.y - minPos.y;
            _solid = new bool[_gridW, _gridH];
            _spike = new bool[_gridW, _gridH];
            _tileW = Mathf.CeilToInt(entitySize.x);
            _tileH = Mathf.CeilToInt(entitySize.y);

            for (int wx = minPos.x; wx < maxPos.x; wx++)
            {
                for (int wy = minPos.y; wy < maxPos.y; wy++)
                {
                    int gx = GX(wx), gy = GY(wy);
                    var cell = new Vector3Int(wx, wy, 0);

                    bool wall  = wallTM.HasTile(cell);
                    bool spike = spikesTM != null && spikesTM.HasTile(cell);

                    _solid[gx, gy] = wall || spike;
                    _spike[gx, gy] = spike;
                }
            }

            Debug.Log($"[PlatNav] Grid {_gridW}x{_gridH} built ({_gridW * _gridH} tiles)");
        }

        private bool EntityFitsTiles(int wx, int wy, GravityDirection g)
        {
            Vector2Int up   = -GravVec(g);
            Vector2Int perp = PerpVec(g);
            int startW = -(_tileW / 2);

            for (int h = 0; h < _tileH; h++)
            for (int w = startW; w < startW + _tileW; w++)
            {
                int cx = wx + up.x * h + perp.x * w;
                int cy = wy + up.y * h + perp.y * w;
                if (IsSolid(cx, cy)) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the entity fits at a continuous world position by testing
        /// every tile its AABB overlaps.  Unlike EntityFitsTiles (which snaps to
        /// one tile), this handles sub-tile offsets that cause the entity to span
        /// additional tiles.
        /// </summary>
        private bool EntityFitsAtWorld(Vector2 worldPos)
        {
            float halfW = entitySize.x * 0.5f;
            // Entity stands at worldPos (foot-center). Body extends upward.
            // Foot tile bottom = worldPos.y - 0.5  (tile center → bottom edge)
            float bottom = worldPos.y - 0.5f;
            float top    = bottom + entitySize.y;
            float left   = worldPos.x - halfW;
            float right  = worldPos.x + halfW;

            int txMin, txMax, tyMin, tyMax;
            if (wallTM != null)
            {
                // Use tilemap world->cell conversion to respect grid origin/scale
                var minCell = wallTM.WorldToCell(new Vector3(left, bottom, 0f));
                var maxCell = wallTM.WorldToCell(new Vector3(right - 0.001f, top - 0.001f, 0f));
                txMin = minCell.x; txMax = maxCell.x;
                tyMin = minCell.y; tyMax = maxCell.y;
            }
            else
            {
                // Fallback: assume 1x1 tiles aligned to world origin
                txMin = Mathf.FloorToInt(left);
                txMax = Mathf.FloorToInt(right  - 0.001f);
                tyMin = Mathf.FloorToInt(bottom);
                tyMax = Mathf.FloorToInt(top    - 0.001f);
            }

            for (int tx = txMin; tx <= txMax; tx++)
            for (int ty = tyMin; ty <= tyMax; ty++)
            {
                if (IsSolid(tx, ty)) return false;
            }
            return true;
        }

        // private bool HasGround(int wx, int wy, GravityDirection g)
        // {
        //     Vector2Int down = GravVec(g);
        //     Vector2Int perp = PerpVec(g);
        //     int startW = -(_tileW / 2);
        //     bool any = false;

        //     for (int w = startW; w < startW + _tileW; w++)
        //     {
        //         int gx = wx + down.x + perp.x * w;
        //         int gy = wy + down.y + perp.y * w;
        //         if (IsSpike(gx, gy)) return false;  // spike under foot = not walkable
        //         if (IsSolid(gx, gy)) any = true;
        //     }
        //     return any;
        // }
        
        private bool HasGround(int wx, int wy, GravityDirection g)
        {
            Vector2Int down = GravVec(g);
            Vector2Int perp = PerpVec(g);
            int startW = -(_tileW / 2);
            bool any = false;
            
            for (int w = startW; w < startW + _tileW; w++)
            {
                int gx = wx + down.x + perp.x * w;
                int gy = wy + down.y + perp.y * w;
                
                if (IsSpike(gx, gy)) return false;
                
                if (InBounds(gx, gy) && _solid[GX(gx), GY(gy)]) 
                {
                    any = true;
                }
            }
            return any;
        }

        private List<(Vector2Int pos, GravityDirection g)> FindSurfaceTiles()
        {
            var result = new List<(Vector2Int, GravityDirection)>();
            const GravityDirection g = GravityDirection.Down;

            for (int wx = minPos.x; wx < maxPos.x; wx++)
            for (int wy = minPos.y; wy < maxPos.y; wy++)
            {
                if (IsSolid(wx, wy)) continue;
                if (!HasGround(wx, wy, g)) continue;
                if (!EntityFitsTiles(wx, wy, g)) continue;

                result.Add((new Vector2Int(wx, wy), g));
            }

            Debug.Log($"[PlatNav] {result.Count} walkable surface tiles");
            return result;
        }

        private List<WalkSegment> MergeSegments(List<(Vector2Int pos, GravityDirection g)> tiles)
        {
            var groups = new Dictionary<(GravityDirection, int), List<int>>();

            foreach (var (pos, g) in tiles)
            {
                var axis  = GravAxis(g);
                int line  = axis == SurfaceAxis.Horizontal ? pos.y : pos.x;
                int coord = axis == SurfaceAxis.Horizontal ? pos.x : pos.y;

                var key = (g, line);
                if (!groups.TryGetValue(key, out var list))
                {
                    list = new List<int>();
                    groups[key] = list;
                }
                list.Add(coord);
            }

            var segments = new List<WalkSegment>();
            foreach (var ((g, line), coords) in groups)
            {
                coords.Sort();

                int runStart = coords[0], runEnd = coords[0];
                for (int i = 1; i < coords.Count; i++)
                {
                    if (coords[i] == runEnd + 1)
                    {
                        runEnd = coords[i];
                    }
                    else
                    {
                        segments.Add(NewSeg(g, line, runStart, runEnd));
                        runStart = runEnd = coords[i];
                    }
                }
                segments.Add(NewSeg(g, line, runStart, runEnd));
            }

            Debug.Log($"[PlatNav] {segments.Count} walk segments");
            return segments;
        }

        private static WalkSegment NewSeg(GravityDirection g, int line, int min, int max) => new()
        {
            gravity  = g,
            axis     = GravAxis(g),
            line     = line,
            min      = min,
            max      = max,
            firstOut = 0,
            outCount = 0
        };

        private static long TileKey(int wx, int wy) => ((long)wx << 32) | (uint)wy;

        private Dictionary<long, int> BuildTileLookup(List<WalkSegment> segs)
        {
            var map = new Dictionary<long, int>();
            for (int si = 0; si < segs.Count; si++)
            {
                var s = segs[si];
                for (int c = s.min; c <= s.max; c++)
                {
                    int wx = s.axis == SurfaceAxis.Horizontal ? c : s.line;
                    int wy = s.axis == SurfaceAxis.Horizontal ? s.line : c;
                    map[TileKey(wx, wy)] = si;
                }
            }
            return map;
        }

        private static int SegAt(Dictionary<long, int> lut, int wx, int wy) => lut.TryGetValue(TileKey(wx, wy), out int idx) ? idx : -1;

        private List<NavLink> BuildLinks(List<WalkSegment> segs, Dictionary<long, int> lut)
        {
            var links = new List<NavLink>();
            BuildFallLinks(segs, lut, links);
            BuildJumpLinks(segs, lut, links);
            Debug.Log($"[PlatNav] {links.Count} links total (fall + jump)");
            return links;
        }

        private void BuildFallLinks(List<WalkSegment> segs,
            Dictionary<long, int> lut, List<NavLink> links)
        {
            for (int si = 0; si < segs.Count; si++)
            {
                var seg  = segs[si];
                var perp = PerpVec(seg.gravity);

                for (int sign = -1; sign <= 1; sign += 2)
                {
                    int edgeC = sign < 0 ? seg.min - 1 : seg.max + 1;
                    int srcC  = sign < 0 ? seg.min     : seg.max;

                    int ox, oy;
                    if (seg.axis == SurfaceAxis.Horizontal) { ox = edgeC; oy = seg.line; }
                    else                                    { ox = seg.line; oy = edgeC; }

                    if (IsSolid(ox, oy)) continue;

                    Vector2 startPos = TileCenter(ox, oy);
                    
                    // Симулируем несколько скоростей падения
                    float[] speeds = { walkSpeed, walkSpeed * 0.8f, walkSpeed * 0.4f, 0f };
                    HashSet<int> foundSegs = new HashSet<int>();

                    foreach (float speed in speeds)
                    {
                        Vector2 startVel = new Vector2(perp.x, perp.y) * (sign * speed);
                        SimulateFall(startPos, startVel, seg.gravity, si, srcC, segs, lut, links, foundSegs);
                    }
                }
            }
        }

        private void SimulateFall(Vector2 pos, Vector2 vel,
            GravityDirection startGrav, int srcSeg, int srcCoord,
            List<WalkSegment> segs, Dictionary<long, int> lut,
            List<NavLink> links, HashSet<int> foundSegs)
        {
            var acc  = AccelFor(startGrav);
            float dt = trajectoryStep;

            Vector2 origPos = pos;
            Vector2 origVel = vel;
            Vector2 prevPos = pos;

            for (float t = dt; t < maxFallTime; t += dt)
            {
                vel += acc * dt;
                pos += vel * dt;

                if (!EntityFitsAtWorld(pos))
                {
                    var prevCell = wallTM.WorldToCell(new Vector3(prevPos.x, prevPos.y, 0));
                    int landSeg = SegAt(lut, prevCell.x, prevCell.y);
                    
                    // Если нашли новый сегмент для приземления — записываем
                    if (landSeg >= 0 && landSeg != srcSeg && foundSegs.Add(landSeg))
                    {
                        EmitFallLink(srcSeg, srcCoord, landSeg, segs[landSeg],
                                     prevCell.x, prevCell.y, t, origPos, origVel, startGrav, links);
                    }
                    return; // Врезались в стену или пол — конец этой дуги
                }

                var cell = wallTM.WorldToCell(new Vector3(pos.x, pos.y, 0));
                int segHere = SegAt(lut, cell.x, cell.y);
                
                if (segHere >= 0 && segHere != srcSeg)
                {
                    if (foundSegs.Add(segHere))
                    {
                        EmitFallLink(srcSeg, srcCoord, segHere, segs[segHere],
                                     cell.x, cell.y, t, origPos, origVel, startGrav, links);
                    }
                    return; // Коснулись поверхности
                }

                prevPos = pos;
            }
        }

        private void EmitFallLink(int fromSeg, int fromCoord,
            int toSeg, in WalkSegment toS,
            int landWX, int landWY, float time,
            Vector2 startPos, Vector2 startVel,
            GravityDirection startGrav,
            List<NavLink> links)
        {
            int toCoord = toS.axis == SurfaceAxis.Horizontal ? landWX : landWY;
            toCoord = Mathf.Clamp(toCoord, toS.min, toS.max);

            links.Add(new NavLink
            {
                fromSeg           = fromSeg,
                toSeg             = toSeg,
                fromMin           = fromCoord,
                fromMax           = fromCoord,
                toMin             = toCoord,
                toMax             = toCoord,
                moveType          = LinkMoveType.Fall,
                requiredAbilities = AbilityMask.None,
                costFp            = FpCost(time * fallCostMultiplier),
                launchPos         = startPos,
                launchVelocity    = startVel,
                landPos           = TileCenter(landWX, landWY),
                flightTime        = time,
                linkGravity       = startGrav,
                gravityStrength   = gravityStrength
            });
        }

        private void BuildJumpLinks(List<WalkSegment> segs,
            Dictionary<long, int> lut, List<NavLink> links)
        {
            var emitted = new HashSet<(int fromSeg, int fromCoord, int toSeg, int toCoord)>();

            for (int si = 0; si < segs.Count; si++)
            {
                var seg    = segs[si];
                var nearby = GatherNearbySegments(seg, si, lut);

                foreach (int tj in nearby)
                {
                    if (TryJumpLinkFromSide(si, seg, tj, segs[tj], -1, out NavLink linkNeg))
                        TryAddJumpLink(linkNeg, links, emitted);

                    if (TryJumpLinkFromSide(si, seg, tj, segs[tj],  1, out NavLink linkPos))
                        TryAddJumpLink(linkPos, links, emitted);
                }
            }
        }

        private void TryAddJumpLink(NavLink link, List<NavLink> links,
            HashSet<(int fromSeg, int fromCoord, int toSeg, int toCoord)> emitted)
        {
            if (!TryOrientJumpLinkUpward(link, out NavLink upwardLink))
                return;

            var key = (upwardLink.fromSeg, upwardLink.fromMin, upwardLink.toSeg, upwardLink.toMin);
            if (emitted.Add(key))
                links.Add(upwardLink);
        }

        private HashSet<int> GatherNearbySegments(
            in WalkSegment seg, int selfIdx, Dictionary<long, int> lut)
        {
            var set = new HashSet<int>();
            int r = jumpSearchRadius;

            if (seg.axis == SurfaceAxis.Horizontal)
            {
                for (int x = seg.min - r; x <= seg.max + r; x++)
                for (int y = seg.line - r; y <= seg.line + r; y++)
                {
                    int s = SegAt(lut, x, y);
                    if (s >= 0 && s != selfIdx) set.Add(s);
                }
            }
            else
            {
                for (int y = seg.min - r; y <= seg.max + r; y++)
                for (int x = seg.line - r; x <= seg.line + r; x++)
                {
                    int s = SegAt(lut, x, y);
                    if (s >= 0 && s != selfIdx) set.Add(s);
                }
            }
            return set;
        }

        private bool TryJumpLinkFromSide(int fi, in WalkSegment from, int ti, in WalkSegment to,
            int sideSign, out NavLink result)
        {
            Vector2 acc = AccelFor(from.gravity);
            float maxJumpDistSq = jumpSearchRadius * jumpSearchRadius;
            float bestCost = float.MaxValue;
            bool any = false;

            int bestFromCoord = 0;
            int bestToCoord   = 0;
            Vector2 bestLaunchPos  = Vector2.zero;
            Vector2 bestLaunchVel  = Vector2.zero;
            Vector2 bestLandPos    = Vector2.zero;
            float   bestFlightTime = 0f;

            foreach (int fc in GetJumpLaunchCoords(from, sideSign))
            {
                Vector2 sp = SegCoord(from, fc);

                foreach (int tc in GetJumpLandingCoords(to, fc))
                {
                    Vector2 tp = SegCoord(to, tc);

                    if ((tp - sp).sqrMagnitude > maxJumpDistSq)
                        continue;

                    var (tMin, tLow, tMax) = JumpTimes(sp, tp, acc);
                    if (tMin < 0f) continue;

                    var times = SampleJumpTimes(tMin, tLow, tMax);
                    if (Mathf.Abs(sp.y - tp.y) < 0.5f)
                        times.Sort((a, b) => Mathf.Abs(a - tLow).CompareTo(Mathf.Abs(b - tLow)));

                    foreach (float T in times)
                    {
                        Vector2 v0 = (tp - sp) / T - 0.5f * acc * T;

                        if (v0.sqrMagnitude > maxJumpVelocity * maxJumpVelocity)
                            continue;
                        if (!ValidateArc(sp, v0, acc, T, from.gravity, out var arcPts))
                        {
                            if (showRejectedTrajectories && arcPts != null)
                                rejectedArcs.Add(arcPts);
                            continue;
                        }

                        float c = T * jumpCostMultiplier;
                        if (c < bestCost)
                        {
                            bestCost       = c;
                            bestFromCoord  = fc;
                            bestToCoord    = tc;
                            bestLaunchPos  = sp;
                            bestLaunchVel  = v0;
                            bestLandPos    = tp;
                            bestFlightTime = T;
                        }

                        any = true;
                        break; // shortest sampled valid arc wins for this pair
                    }
                }
            }

            if (!any) { result = default; return false; }

            result = new NavLink
            {
                fromSeg           = fi,
                toSeg             = ti,
                fromMin           = bestFromCoord,
                fromMax           = bestFromCoord,
                toMin             = bestToCoord,
                toMax             = bestToCoord,
                moveType          = LinkMoveType.Jump,
                requiredAbilities = AbilityMask.Jump,
                costFp            = FpCost(bestCost),
                launchPos         = bestLaunchPos,
                launchVelocity    = bestLaunchVel,
                landPos           = bestLandPos,
                flightTime        = bestFlightTime,
                linkGravity       = from.gravity,
                gravityStrength   = gravityStrength
            };
            return true;
        }

        private bool TryOrientJumpLinkUpward(NavLink link, out NavLink upwardLink)
        {
            const float heightEpsilon = 0.001f;
            float allowDownHeight = allowJumpDownTiles * wallTM.cellSize.y;

            if (link.moveType != LinkMoveType.Jump || link.launchPos.y <= link.landPos.y + allowDownHeight + heightEpsilon)
            {
                upwardLink = link;
                return true;
            }

            Vector2 start = link.landPos;
            Vector2 target = link.launchPos;
            Vector2 acc = Vector2.down * link.gravityStrength;

            float bestCost = float.MaxValue;
            float bestFlightTime = 0f;
            Vector2 bestLaunchVel = Vector2.zero;
            bool any = false;

            var (tMin, tLow, tMax) = JumpTimes(start, target, acc);
            if (tMin < 0f)
            {
                upwardLink = default;
                return false;
            }

            var times = SampleJumpTimes(tMin, tLow, tMax);
            AddJumpTimeSample(times, link.flightTime);

            for (int i = 0; i < times.Count; i++)
            {
                float time = times[i];
                Vector2 v0 = (target - start) / time - 0.5f * acc * time;

                if (v0.sqrMagnitude > maxJumpVelocity * maxJumpVelocity)
                    continue;

                if (!ValidateArc(start, v0, acc, time, link.linkGravity, out var arcPts))
                {
                    if (showRejectedTrajectories && arcPts != null)
                        rejectedArcs.Add(arcPts);
                    continue;
                }

                float cost = time * jumpCostMultiplier;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestFlightTime = time;
                    bestLaunchVel = v0;
                }

                any = true;
            }

            if (!any)
            {
                upwardLink = default;
                return false;
            }

            upwardLink = link;
            upwardLink.fromSeg = link.toSeg;
            upwardLink.toSeg = link.fromSeg;
            upwardLink.fromMin = link.toMin;
            upwardLink.fromMax = link.toMax;
            upwardLink.toMin = link.fromMin;
            upwardLink.toMax = link.fromMax;
            upwardLink.launchPos = start;
            upwardLink.launchVelocity = bestLaunchVel;
            upwardLink.landPos = target;
            upwardLink.flightTime = bestFlightTime;
            upwardLink.costFp = FpCost(bestCost);
            return true;
        }

        private IEnumerable<int> GetJumpLaunchCoords(WalkSegment seg, int sideSign)
        {
            int primary = sideSign < 0 ? seg.min : seg.max;
            int secondary = sideSign < 0 ? seg.min + 1 : seg.max - 1;

            yield return primary;

            if (secondary >= seg.min && secondary <= seg.max && secondary != primary)
                yield return secondary;
        }

        private IEnumerable<int> GetJumpLandingCoords(WalkSegment seg, int aroundCoord)
        {
            int min = Mathf.Max(seg.min, aroundCoord - jumpSearchRadius);
            int max = Mathf.Min(seg.max, aroundCoord + jumpSearchRadius);

            for (int c = min; c <= max; c++)
                yield return c;
        }

        private List<float> SampleJumpTimes(float tMin, float tLow, float tMax)
        {
            var samples = new List<float>(numTrajectoriesToTest + 3);

            AddJumpTimeSample(samples, tMin);
            AddJumpTimeSample(samples, tLow);
            AddJumpTimeSample(samples, tMax);

            if (numTrajectoriesToTest > 1)
            {
                for (int k = 0; k < numTrajectoriesToTest; k++)
                {
                    float frac = (float)k / (numTrajectoriesToTest - 1);
                    AddJumpTimeSample(samples, Mathf.Lerp(tMin, tMax, frac));
                }
            }

            samples.Sort();
            return samples;
        }

        private static void AddJumpTimeSample(List<float> samples, float value)
        {
            const float epsilon = 0.0001f;

            for (int i = 0; i < samples.Count; i++)
            {
                if (Mathf.Abs(samples[i] - value) <= epsilon)
                    return;
            }

            samples.Add(value);
        }

        private (float tMin, float tLow, float tMax) JumpTimes(Vector2 start, Vector2 target, Vector2 acc)
        {
            Vector2 dp = target - start;
            float aa   = Vector2.Dot(acc, acc);
            if (aa < 1e-4f) return (-1, -1, -1);

            float vmSq = maxJumpVelocity * maxJumpVelocity;
            float b1   = Vector2.Dot(dp, acc) + vmSq;
            float disc = b1 * b1 - aa * dp.sqrMagnitude;
            if (disc < 0) return (-1, -1, -1);

            float sqD   = Mathf.Sqrt(disc);
            float t2Min = 2f * (b1 - sqD) / aa;
            float t2Max = 2f * (b1 + sqD) / aa;
            if (t2Min < 0 || t2Max < 0) return (-1, -1, -1);

            float tMin = Mathf.Sqrt(Mathf.Max(0f, t2Min));
            float tMax = Mathf.Sqrt(Mathf.Max(0f, t2Max));
            float tLow = Mathf.Pow(4f * dp.sqrMagnitude / aa, 0.25f);
            tLow = Mathf.Clamp(tLow, tMin, tMax);

            return (tMin, tLow, tMax);
        }

        private bool ValidateArc(Vector2 start, Vector2 v0, Vector2 acc, float T, GravityDirection grav, out Vector3[] points)
        {
            bool collectPts = showRejectedTrajectories;
            var pts = collectPts ? new List<Vector3>(Mathf.CeilToInt(T / trajectoryStep) + 1) : null;
            if (collectPts) pts.Add(start);

            for (float t = trajectoryStep; t < T; t += trajectoryStep)
            {
                Vector2 p = start + v0 * t + 0.5f * acc * (t * t);
                if (collectPts) pts.Add(p);

                if (!EntityFitsAtWorld(p))
                {
                    points = collectPts ? pts.ToArray() : null;
                    return false;
                }
                
                Vector2 vel = v0 + acc * t;
                if (vel.sqrMagnitude > (maxJumpVelocity + 0.01f) * (maxJumpVelocity + 0.01f))
                {
                    points = collectPts ? pts.ToArray() : null;
                    return false;
                }
            }

            points = null;
            return true;
        }

        private static AxisBins BuildAxisBins(WalkSegment[] segs, SurfaceAxis axis, int binSize)
        {
            var matching = new List<int>();
            for (int i = 0; i < segs.Length; i++)
                if (segs[i].axis == axis) matching.Add(i);

            if (matching.Count == 0)
                return new AxisBins
                {
                    axis = axis, binSize = binSize, binCount = 0,
                    first = Array.Empty<int>(),
                    count = Array.Empty<ushort>(),
                    refs  = Array.Empty<int>()
                };

            int lo = int.MaxValue, hi = int.MinValue;
            foreach (int i in matching)
            {
                lo = Mathf.Min(lo, segs[i].min);
                hi = Mathf.Max(hi, segs[i].max);
            }

            int origin = lo;
            int nBins  = (hi - lo + binSize) / binSize;
            var bins   = new List<int>[nBins];
            for (int b = 0; b < nBins; b++) bins[b] = new List<int>();

            foreach (int i in matching)
            {
                int bMin = Mathf.Clamp((segs[i].min - origin) / binSize, 0, nBins - 1);
                int bMax = Mathf.Clamp((segs[i].max - origin) / binSize, 0, nBins - 1);
                for (int b = bMin; b <= bMax; b++) bins[b].Add(i);
            }

            var firstArr = new int[nBins];
            var countArr = new ushort[nBins];
            var refsList = new List<int>();
            for (int b = 0; b < nBins; b++)
            {
                firstArr[b] = refsList.Count;
                countArr[b] = (ushort)bins[b].Count;
                refsList.AddRange(bins[b]);
            }

            return new AxisBins
            {
                axis     = axis,
                binSize  = binSize,
                origin   = origin,
                binCount = nBins,
                first    = firstArr,
                count    = countArr,
                refs     = refsList.ToArray()
            };
        }

        private static void AssignOutLinks(List<WalkSegment> segs, List<NavLink> links)
        {
            links.Sort((a, b) => a.fromSeg.CompareTo(b.fromSeg));

            for (int i = 0; i < segs.Count; i++)
            {
                var s = segs[i];
                s.firstOut = 0;
                s.outCount = 0;
                segs[i] = s;
            }

            for (int li = 0; li < links.Count; li++)
            {
                int seg = links[li].fromSeg;
                var s   = segs[seg];
                if (s.outCount == 0) s.firstOut = (ushort)li;
                s.outCount++;
                segs[seg] = s;
            }
        }

        [ContextMenu("Bake Navigation Graph")]
        public void Bake()
        {
            if (!wallTM) { Debug.LogError("[PlatNav] Wall tilemap not assigned.");  return; }
            if (!graph)  { Debug.LogError("[PlatNav] Graph asset not assigned.");   return; }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Clear rejected arcs from previous bake
            rejectedArcs.Clear();

            // 1. Classify every tile: solid / spike / gravity direction
            BuildGrids();

            // 2. Find all tiles where the entity can stand
            var surface = FindSurfaceTiles();

            // 3. Merge contiguous same-gravity surface tiles into segments
            var segments = MergeSegments(surface);

            // 4. Tile → segment spatial lookup
            var lut = BuildTileLookup(segments);

            // 5. Compute all edges: falls, jumps, gravity transitions
            var links = BuildLinks(segments, lut);

            // 6. Sort links and assign outgoing-link indices
            AssignOutLinks(segments, links);

            // 7. Build spatial index for fast queries
            var segArr = segments.ToArray();
            var hBins  = BuildAxisBins(segArr, SurfaceAxis.Horizontal, 4);
            var vBins  = BuildAxisBins(segArr, SurfaceAxis.Vertical,   4);

            // 8. Write into the ScriptableObject asset
            graph.tilemapSize      = maxPos - minPos;
            graph.tileOrigin       = minPos;
            graph.cellWorldOrigin  = (Vector2)wallTM.GetCellCenterWorld(Vector3Int.zero);
            graph.segments         = segArr;
            graph.links          = links.ToArray();
            graph.horizontalBins = hBins;
            graph.verticalBins   = vBins;

#if UNITY_EDITOR
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
#endif

            sw.Stop();
            Debug.Log($"[PlatNav] Bake finished in {sw.ElapsedMilliseconds} ms — " +
                      $"{segArr.Length} segments, {links.Count} links");

            // free bake-time memory
            _solid = null;
            _spike = null;

            if (rejectedArcs.Count > 0)
                Debug.Log($"[PlatNav] {rejectedArcs.Count} rejected trajectories recorded for gizmo display");
        }

        private Vector3 SegWorldPos(in WalkSegment s, int coord)
        {
            int wx = s.axis == SurfaceAxis.Horizontal ? coord : s.line;
            int wy = s.axis == SurfaceAxis.Horizontal ? s.line : coord;
            return TileCenterSafe(wx, wy);
        }

        private Vector3 TileCenterSafe(int wx, int wy)
        {
            if (wallTM != null)
                return wallTM.GetCellCenterWorld(new Vector3Int(wx, wy, 0));
            // fallback: assume 1-unit tiles
            return new Vector3(wx + 0.5f, wy + 0.5f, 0f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!ShouldDrawGraphGizmos())
                return;
            // ── Bake bounds ──
            if (showBakeBounds)
                DrawBakeBounds();

            if (graph == null || graph.segments == null || graph.links == null) return;

            var segs  = graph.segments;
            var links = graph.links;

            if (showSegments && segs.Length > 0) DrawSegments(segs);

            if ((showJumpLinks || showFallLinks) && links.Length > 0) DrawLinks(segs, links);

            if (showRejectedTrajectories && rejectedArcs != null && rejectedArcs.Count > 0) DrawRejectedArcs();
        }

        private void DrawBakeBounds()
        {
            Vector3 bl, tr;
            if (wallTM != null)
            {
                bl = wallTM.CellToWorld(new Vector3Int(minPos.x, minPos.y, 0));
                tr = wallTM.CellToWorld(new Vector3Int(maxPos.x, maxPos.y, 0));
            }
            else
            {
                bl = new Vector3(minPos.x, minPos.y, 0);
                tr = new Vector3(maxPos.x, maxPos.y, 0);
            }

            Vector3 tl = new Vector3(bl.x, tr.y, 0);
            Vector3 br = new Vector3(tr.x, bl.y, 0);

            Color fill = bakeBoundsColor;
            fill.a = 0.035f;
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { bl, tl, tr, br }, fill, bakeBoundsColor);
        }

        private void DrawSegments(WalkSegment[] segs)
        {
            Gizmos.color = segmentColor;
            for (int i = 0; i < segs.Length; i++)
            {
                var s = segs[i];
                Vector3 a = SegWorldPos(s, s.min);
                Vector3 b = SegWorldPos(s, s.max);

                UnityEditor.Handles.color = segmentColor;
                UnityEditor.Handles.DrawAAPolyLine(3f, a, b);

                // small diamonds at endpoints
                Gizmos.color = segmentColor;
                float sz = 0.12f;
                Gizmos.DrawWireSphere(a, sz);
                Gizmos.DrawWireSphere(b, sz);
            }
        }

        private void DrawLinks(WalkSegment[] segs, NavLink[] links)
        {
            for (int i = 0; i < links.Length; i++)
            {
                var lk = links[i];

                Color col;
                if (lk.moveType == LinkMoveType.Jump)
                {
                    if (!showJumpLinks) continue;
                    col = jumpLinkColor;
                }
                else
                {
                    if (!showFallLinks) continue;
                    col = fallLinkColor;
                }

                if (lk.flightTime > 0f)
                    DrawExactTrajectory(lk, col);
            }
        }

        private void DrawExactTrajectory(in NavLink lk, Color col)
        {
            const int steps = 32;
            float dt = lk.flightTime / steps;

            UnityEditor.Handles.color = col;
            Vector3 prev = lk.launchPos;

            if (lk.moveType == LinkMoveType.Jump)
            {
                // Constant-gravity analytical
                Vector2 acc = lk.Acceleration;
                for (int s = 1; s <= steps; s++)
                {
                    float t = s * dt;
                    Vector3 p = (Vector3)(lk.launchPos + lk.launchVelocity * t + 0.5f * acc * (t * t));
                    UnityEditor.Handles.DrawAAPolyLine(2.5f, prev, p);
                    prev = p;
                }
            }
            else // Fall – Euler re-simulation
            {
                Vector2 pos = lk.launchPos;
                Vector2 vel = lk.launchVelocity;
                Vector2 acc = lk.Acceleration;

                for (int s = 1; s <= steps; s++)
                {
                    vel += acc * dt;
                    pos += vel * dt;
                    Vector3 p = pos;
                    UnityEditor.Handles.DrawAAPolyLine(2.5f, prev, p);
                    prev = p;
                }
            }

            // Arrowhead at landing
            Vector3 dir = ((Vector3)lk.landPos - prev).normalized;
            DrawArrowHead(lk.landPos, dir, col, 0.25f);

            // Launch marker – circle
            Gizmos.color = col;
            Gizmos.DrawWireSphere(lk.launchPos, 0.15f);

            // Land marker – square
            Gizmos.color = col;
            Gizmos.DrawWireCube(lk.landPos, Vector3.one * 0.35f);
        }

        private static void DrawArcGizmo(Vector3 from, Vector3 to, Color col)
        {
            const int steps = 16;
            Vector3 mid = (from + to) * 0.5f;
            float dist = Vector3.Distance(from, to);
            mid.y += dist * 0.35f; // arc height

            UnityEditor.Handles.color = col;
            Vector3 prev = from;
            for (int s = 1; s <= steps; s++)
            {
                float t = s / (float)steps;
                // quadratic bezier
                Vector3 p = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * mid + t * t * to;
                UnityEditor.Handles.DrawAAPolyLine(2f, prev, p);
                prev = p;
            }

            // arrowhead at destination
            DrawArrowHead(prev, (to - mid).normalized, col, 0.25f);
        }

        private static void DrawArrowGizmo(Vector3 from, Vector3 to, Color col)
        {
            UnityEditor.Handles.color = col;
            UnityEditor.Handles.DrawAAPolyLine(2f, from, to);
            Vector3 dir = (to - from).normalized;
            DrawArrowHead(to, dir, col, 0.25f);
        }

        private static void DrawArrowHead(Vector3 tip, Vector3 dir, Color col, float size)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            Gizmos.color = col;
            Vector3 right = Vector3.Cross(dir, Vector3.forward).normalized;
            Vector3 a = tip - dir * size + right * size * 0.5f;
            Vector3 b = tip - dir * size - right * size * 0.5f;
            Gizmos.DrawLine(tip, a);
            Gizmos.DrawLine(tip, b);
        }

        private void DrawRejectedArcs()
        {
            UnityEditor.Handles.color = rejectedArcColor;
            foreach (var arc in rejectedArcs)
            {
                if (arc == null || arc.Length < 2) continue;
                // draw dashed
                for (int i = 0; i < arc.Length - 1; i += 2)
                {
                    int j = Mathf.Min(i + 1, arc.Length - 1);
                    UnityEditor.Handles.DrawDottedLine(arc[i], arc[j], 3f);
                }
            }
        }

        private bool ShouldDrawGraphGizmos()
        {
            if (graph == null)
                return true;

            var selectedBake = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<PlatNavBake>()
                : null;

            if (selectedBake != null && selectedBake.graph == graph)
                return selectedBake == this;

            var allBakers = FindObjectsOfType<PlatNavBake>();
            PlatNavBake primaryDrawer = this;

            for (int i = 0; i < allBakers.Length; i++)
            {
                var baker = allBakers[i];
                if (baker == null || baker.graph != graph)
                    continue;

                if (baker.GetInstanceID() < primaryDrawer.GetInstanceID())
                    primaryDrawer = baker;
            }

            return primaryDrawer == this;
        }
#endif
    }
}
