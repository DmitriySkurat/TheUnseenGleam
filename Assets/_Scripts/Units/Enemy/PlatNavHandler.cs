/// <summary>
/// Runtime navigation handler for the PlatNav baked graph.
/// Uses A* pathfinding on WalkSegments/NavLinks and drives the entity
/// along segments (gravity-physics walk) and through links (position-override trajectory).
/// </summary>

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entity.PlatNav
{
    public enum PlatNavBehaviour
    {
        FollowTarget,
        RunAwayFromTarget,
    }

    public enum PlatNavState
    {
        Idle,
        WalkingSegment,
        TraversingLink,
    }

    internal struct PathNode
    {
        public int   segIndex;     // current segment
        public int   parentSeg;    // segment we came from (-1 for start)
        public int   linkUsed;     // index of the NavLink used to reach this node (-1 for start)
        public float gCost;        // cost from start
        public float hCost;        // heuristic to goal
        public float fCost => gCost + hCost;
    }

    public struct PathStep
    {
        public int segIndex;       // segment to be on
        public int linkIndex;      // NavLink index used to arrive here (-1 for start segment)
    }

    public class PlatNavHandler : MonoBehaviour
    {
        [Header("PlatNav")]
        [SerializeField] private PlatformNavGraphAsset graph;
        [SerializeField] private Transform target;

        [Header("Runtime")]
        [SerializeField] private PlatNavBehaviour behaviour = PlatNavBehaviour.FollowTarget;
        [SerializeField] private AbilityMask abilities = AbilityMask.Jump;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float repathInterval = 0.3f;

        [Header("Run Away")]
        [SerializeField] private int   runAwayDistance          = 10;
        // [SerializeField] private int   runAwayThresholdDistance = 15;

        [Header("Thresholds")]
        [SerializeField] private float nearTileDist       = 0.2f;
        [SerializeField] private LayerMask solidMask;

        [Header("Debug")]
        [SerializeField] private bool  debugLog  = false;
        [SerializeField] private bool  drawPath  = false;
        [SerializeField] private bool  drawState = false;
        private Rigidbody2D   _rb;
        private Collider2D    _col;
        private PlatNavState _state = PlatNavState.Idle;
        private List<PathStep> _path = new();
        private int _pathIndex;

        // Segment walk
        private int _curSegIndex = -1;
        private Vector2 _walkTarget;

        // Link traversal
        private NavLink _curLink;
        private float   _linkTimer;
        private bool    _linkIsEuler;        // true = fall (euler re-sim), false = jump (analytical)
        private Vector2 _eulerPos, _eulerVel; // running Euler state for falls
        private Vector2 _eulerAcc;

        // Repath timer
        private float _repathTimer;
        private int   _lastTargetSeg = -1;

        public PlatNavState State => _state;
        public bool HasPath => _path.Count > 0;

        public void SetTarget(Transform t) => target = t;
        public void SetBehaviour(PlatNavBehaviour b) => behaviour = b;

        public void Abort()
        {
            _path.Clear();
            _pathIndex = 0;
            if (_state == PlatNavState.TraversingLink)
                EndTraversal();
            _state = PlatNavState.Idle;
        }

        public bool MoveTo(Vector2 targetPosition, float speed)
        {
            walkSpeed = speed;
            if (target == null)
                _walkTarget = targetPosition;

            int goalSeg = FindNearestSegment(targetPosition);
            if (goalSeg >= 0 && RequestPath(goalSeg)) return true;

            // Nearest goal failed A*, try alternatives
            var alts = FindNearestSegments(targetPosition, 4);
            for (int i = 0; i < alts.Count; i++)
            {
                if (alts[i] == goalSeg) continue;
                if (RequestPath(alts[i])) return true;
            }
            return false;
        }

        public bool WanderAround(Vector2 position, float radius, float speed)
        {
            Vector2 rnd = position + Random.insideUnitCircle * radius;
            return MoveTo(rnd, speed);
        }

        private void Awake()
        {
            _rb  = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (graph == null || graph.segments == null) return;

            // Auto-repath towards target
            if (target != null)
            {
                _repathTimer -= Time.deltaTime;
                if (_repathTimer <= 0f)
                {
                    _repathTimer = repathInterval;
                    TryRepath();
                }
            }

            switch (_state)
            {
                case PlatNavState.Idle:
                    break;

                case PlatNavState.WalkingSegment:
                    UpdateSegmentWalk();
                    break;

                case PlatNavState.TraversingLink:
                    UpdateLinkTraversal();
                    break;
            }
        }

        private void TryRepath()
        {
            //if (!IsGrounded() && _state != PlatNavState.TraversingLink) return;
            
            if (_state == PlatNavState.TraversingLink) return;
            if (!IsGrounded()) return;

            switch (behaviour)
            {
                case PlatNavBehaviour.FollowTarget:
                {
                    int goalSeg = FindNearestSegment(target.position);
                    if (goalSeg < 0) return;
                    if (goalSeg == _lastTargetSeg && _path.Count > 0) return;

                    if (RequestPath(goalSeg)) return;

                    // Nearest goal unreachable, try alternatives
                    var alts = FindNearestSegments(target.position, 4);
                    for (int i = 0; i < alts.Count; i++)
                    {
                        if (alts[i] == goalSeg) continue;
                        if (RequestPath(alts[i])) return;
                    }
                    break;
                }
                case PlatNavBehaviour.RunAwayFromTarget:
                {
                    int goalSeg = FindFarthestSegment(target.position);
                    if (goalSeg < 0) return;
                    if (goalSeg == _lastTargetSeg && _path.Count > 0) return;
                    RequestPath(goalSeg);
                    break;
                }
            }
        }

        private bool RequestPath(int goalSeg)
        {
            int startSeg = FindNearestSegment(transform.position);
            if (startSeg < 0) return false;

            if (TryCommitPath(startSeg, goalSeg)) return true;

            // Nearest start unreachable, try alternatives
            var alts = FindNearestSegments(transform.position, 4);
            for (int i = 0; i < alts.Count; i++)
            {
                if (alts[i] == startSeg) continue;
                if (TryCommitPath(alts[i], goalSeg)) return true;
            }

            if (debugLog) Debug.Log("[PlatNav] No path found");
            return false;
        }

        private bool TryCommitPath(int startSeg, int goalSeg)
        {
            List<PathStep> newPath;
            if (behaviour == PlatNavBehaviour.RunAwayFromTarget)
                newPath = ComputePathAway(startSeg, goalSeg, target != null ? (Vector2)target.position : (Vector2)transform.position);
            else
                newPath = ComputePath(startSeg, goalSeg);

            if (newPath == null || newPath.Count == 0) return false;

            if (_state == PlatNavState.TraversingLink)
                EndTraversal();

            _path = newPath;
            _pathIndex = 0;
            _lastTargetSeg = goalSeg;
            BeginStep();
            return true;
        }

        private void BeginStep()
        {
            if (_pathIndex >= _path.Count)
            {
                _state = PlatNavState.Idle;
                _path.Clear();
                if (debugLog) Debug.Log("[PlatNav] Path complete");
                return;
            }

            var step = _path[_pathIndex];

            if (step.linkIndex >= 0)
            {
                // Traverse link
                BeginLinkTraversal(step.linkIndex);
            }
            else
            {
                // Walk along segment
                BeginSegmentWalk(step.segIndex);
            }
        }

        private void AdvanceStep()
        {
            _pathIndex++;
            BeginStep();
        }

        private void BeginSegmentWalk(int segIdx)
        {
            _curSegIndex = segIdx;
            _state = PlatNavState.WalkingSegment;

            // Determine walk target: if there's a next step with a link,
            // walk to the link's launch point on this segment.
            // Otherwise walk toward the segment midpoint nearest the goal.
            ComputeSegmentWalkTarget();
        }

        private void ComputeSegmentWalkTarget()
        {
            if (_pathIndex + 1 < _path.Count)
            {
                var nextStep = _path[_pathIndex + 1];
                if (nextStep.linkIndex >= 0)
                {
                    var link = graph.links[nextStep.linkIndex];
                    // Walk to the launch position of the link
                    _walkTarget = link.launchPos;
                    return;
                }
            }

            // Fallback: walk toward target position projected onto this segment
            if (target != null)
                _walkTarget = ProjectOntoSegment(_curSegIndex, target.position);
            else
                _walkTarget = SegmentCenter(_curSegIndex);
        }

        private void UpdateSegmentWalk()
        {
            if (!IsGrounded()) return; // wait for grounding
            
            if (_pathIndex == _path.Count - 1 && target != null && behaviour == PlatNavBehaviour.FollowTarget)
            {
                _walkTarget = ProjectOntoSegment(_curSegIndex, target.position);
            }

            Vector2 pos = transform.position;
            Vector2 toTarget = _walkTarget - pos;

            // // Use full 2D distance to prevent advancing when on a different platform
            // if (toTarget.sqrMagnitude < nearTileDist * nearTileDist)
            // {
            //     _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            //     AdvanceStep();
            //     return;
            // }

            if (Mathf.Abs(toTarget.x) <= nearTileDist)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                AdvanceStep();
                return;
            }

            // Walk horizontally
            float dir = Mathf.Sign(toTarget.x);
            _rb.linearVelocity = new Vector2(dir * walkSpeed, _rb.linearVelocity.y);
            Vector3 scale = transform.localScale;
            scale.x = dir >= 0f ? 1f : -1f;
            transform.localScale = scale;
        }

        [SerializeField] private float maxLaunchSnap = 1.0f;

        private void BeginLinkTraversal(int linkIdx)
        {
            _curLink = graph.links[linkIdx];

            // Safety: don't teleport to launch if entity is too far away
            float distToLaunch = Vector2.Distance(transform.position, _curLink.launchPos);
            if (distToLaunch > maxLaunchSnap)
            {
                if (debugLog) Debug.Log($"[PlatNav] Link aborted: {distToLaunch:F1} units from launch");
                _path.Clear();
                _pathIndex = 0;
                _state = PlatNavState.Idle;
                return;
            }

            _linkTimer = 0f;
            _state     = PlatNavState.TraversingLink;

            // Switch to kinematic so physics doesn't fight position overrides
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _rb.position = _curLink.launchPos;

            _linkIsEuler = _curLink.moveType == LinkMoveType.Fall;

            if (_linkIsEuler)
            {
                _eulerPos = _curLink.launchPos;
                _eulerVel = _curLink.launchVelocity;
                _eulerAcc = _curLink.Acceleration;
            }

            if (debugLog) Debug.Log($"[PlatNav] Begin {_curLink.moveType} link: {_curLink.fromSeg}→{_curLink.toSeg}, T={_curLink.flightTime:F2}s");
        }

        private void UpdateLinkTraversal()
        {
            float dt = Time.deltaTime;
            _linkTimer += dt;

            // Clamp to flight time
            if (_linkTimer >= _curLink.flightTime)
            {
                // Land
                LandFromTraversal();
                return;
            }

            Vector2 nextPos;

            if (_linkIsEuler)
            {
                _eulerVel += _eulerAcc * dt;
                _eulerPos += _eulerVel * dt;
                nextPos = _eulerPos;
            }
            else
            {
                // Analytical jump (constant gravity)
                nextPos = _curLink.EvaluatePosition(_linkTimer);
            }

            if (!FitsAt(nextPos))
            {
                if (debugLog) Debug.Log("[PlatNav] Trajectory aborted: wall collision");
                AbortTraversal();
                return;
            }

            // Override position
            _rb.position = nextPos;
            _rb.linearVelocity = Vector2.zero;
        }

        private void LandFromTraversal()
        {
            _rb.position = _curLink.landPos;
            EndTraversal();
            AdvanceStep();
        }

        private void AbortTraversal()
        {
            // Stay at current position, re-enable gravity, clear path
            EndTraversal();
            _path.Clear();
            _pathIndex = 0;
            _state = PlatNavState.Idle;
        }

        private void EndTraversal()
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
        }

        private List<PathStep> ComputePath(int startSeg, int goalSeg)
        {
            if (startSeg == goalSeg)
                return new List<PathStep> { new PathStep { segIndex = startSeg, linkIndex = -1 } };

            var segs  = graph.segments;
            var links = graph.links;
            int n     = segs.Length;

            var open    = new List<PathNode>();
            var closed  = new HashSet<int>();
            var nodeMap = new Dictionary<int, PathNode>();

            var startNode = new PathNode
            {
                segIndex  = startSeg,
                parentSeg = -1,
                linkUsed  = -1,
                gCost     = 0f,
                hCost     = SegmentHeuristic(startSeg, goalSeg)
            };
            open.Add(startNode);
            nodeMap[startSeg] = startNode;

            while (open.Count > 0)
            {
                // Pop lowest fCost
                open.Sort((a, b) => a.fCost.CompareTo(b.fCost));
                var cur = open[0];
                open.RemoveAt(0);

                if (cur.segIndex == goalSeg)
                    return ReconstructPath(nodeMap, cur);

                closed.Add(cur.segIndex);

                // Expand outgoing links
                var s = segs[cur.segIndex];
                for (int li = s.firstOut; li < s.firstOut + s.outCount; li++)
                {
                    if (li >= links.Length) break;
                    var lk = links[li];
                    if (!lk.IsUsable(abilities)) continue;

                    int next = lk.toSeg;
                    if (closed.Contains(next)) continue;

                    float g = cur.gCost + lk.Cost;
                    if (nodeMap.TryGetValue(next, out var existing) && existing.gCost <= g)
                        continue;

                    var nn = new PathNode
                    {
                        segIndex  = next,
                        parentSeg = cur.segIndex,
                        linkUsed  = li,
                        gCost     = g,
                        hCost     = SegmentHeuristic(next, goalSeg)
                    };
                    nodeMap[next] = nn;

                    // Update or insert in open
                    open.RemoveAll(x => x.segIndex == next);
                    open.Add(nn);
                }
            }

            return null; // no path
        }

        private List<PathStep> ComputePathAway(int startSeg, int goalSeg, Vector2 threatPos)
        {
            var segs  = graph.segments;
            var links = graph.links;

            var open    = new List<PathNode>();
            var closed  = new HashSet<int>();
            var nodeMap = new Dictionary<int, PathNode>();

            var startNode = new PathNode
            {
                segIndex  = startSeg,
                parentSeg = -1,
                linkUsed  = -1,
                gCost     = 0f,
                hCost     = -SegmentDistanceTo(startSeg, threatPos) // inverted: prefer far segments
            };
            open.Add(startNode);
            nodeMap[startSeg] = startNode;

            PathNode best = startNode;

            while (open.Count > 0)
            {
                open.Sort((a, b) => a.fCost.CompareTo(b.fCost));
                var cur = open[0];
                open.RemoveAt(0);

                float distToThreat = SegmentDistanceTo(cur.segIndex, threatPos);
                if (distToThreat > runAwayDistance)
                    return ReconstructPath(nodeMap, cur);

                if (distToThreat > SegmentDistanceTo(best.segIndex, threatPos))
                    best = cur;

                closed.Add(cur.segIndex);

                var s = segs[cur.segIndex];
                for (int li = s.firstOut; li < s.firstOut + s.outCount; li++)
                {
                    if (li >= links.Length) break;
                    var lk = links[li];
                    if (!lk.IsUsable(abilities)) continue;

                    int next = lk.toSeg;
                    if (closed.Contains(next)) continue;

                    float g = cur.gCost + lk.Cost;
                    if (nodeMap.TryGetValue(next, out var existing) && existing.gCost <= g)
                        continue;

                    var nn = new PathNode
                    {
                        segIndex  = next,
                        parentSeg = cur.segIndex,
                        linkUsed  = li,
                        gCost     = g,
                        hCost     = -SegmentDistanceTo(next, threatPos)
                    };
                    nodeMap[next] = nn;
                    open.RemoveAll(x => x.segIndex == next);
                    open.Add(nn);
                }
            }

            // Didn't reach distance goal, return best we found
            if (best.segIndex != startSeg)
                return ReconstructPath(nodeMap, best);
            return null;
        }

        private List<PathStep> ReconstructPath(Dictionary<int, PathNode> nodeMap, PathNode goal)
        {
            var steps = new List<PathStep>();
            var cur = goal;

            while (cur.parentSeg >= 0)
            {
                // Push: first the link to arrive, then insert the walk-on-segment before it
                steps.Add(new PathStep { segIndex = cur.segIndex, linkIndex = cur.linkUsed });
                cur = nodeMap[cur.parentSeg];
            }

            // Add start segment walk (no link)
            steps.Add(new PathStep { segIndex = cur.segIndex, linkIndex = -1 });
            steps.Reverse();

            if (debugLog) Debug.Log($"[PlatNav] Path: {steps.Count} steps");
            return steps;
        }

        private float SegmentHeuristic(int segA, int segB)
        {
            return Vector2.Distance(SegmentCenter(segA), SegmentCenter(segB));
        }

        private float SegmentDistanceTo(int seg, Vector2 point)
        {
            return Vector2.Distance(SegmentCenter(seg), point);
        }

        private int FindNearestSegment(Vector2 worldPos)
        {
            var segs = graph.segments;
            if (segs == null || segs.Length == 0) return -1;

            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < segs.Length; i++)
            {
                float d = SegmentSqrDistanceTo(i, worldPos);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        private List<int> FindNearestSegments(Vector2 worldPos, int maxCount)
        {
            var segs = graph.segments;
            if (segs == null || segs.Length == 0) return new List<int>();

            var pairs = new List<(float dist, int idx)>(segs.Length);
            for (int i = 0; i < segs.Length; i++)
                pairs.Add((SegmentSqrDistanceTo(i, worldPos), i));

            pairs.Sort((a, b) => a.dist.CompareTo(b.dist));

            int take = Mathf.Min(maxCount, pairs.Count);
            var result = new List<int>(take);
            for (int i = 0; i < take; i++)
                result.Add(pairs[i].idx);
            return result;
        }

        private int FindFarthestSegment(Vector2 threatPos)
        {
            // For run-away, we just want a distant segment.
            // Use brute force scan of segment centers.
            var segs = graph.segments;
            int best = -1;
            float bestDist = -1f;

            for (int i = 0; i < segs.Length; i++)
            {
                float d = SegmentSqrDistanceTo(i, threatPos);
                if (d > bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        private Vector2 SegmentCenter(int segIdx)
        {
            var s = graph.segments[segIdx];
            int midCoord = (s.min + s.max) / 2;
            int wx = s.axis == SurfaceAxis.Horizontal ? midCoord : s.line;
            int wy = s.axis == SurfaceAxis.Horizontal ? s.line : midCoord;
            return TileWorldCenter(wx, wy);
        }

        private Vector2 SegmentWorldPos(in WalkSegment s, int coord)
        {
            int wx = s.axis == SurfaceAxis.Horizontal ? coord : s.line;
            int wy = s.axis == SurfaceAxis.Horizontal ? s.line : coord;
            return TileWorldCenter(wx, wy);
        }

        private float SegmentSqrDistanceTo(int segIdx, Vector2 pos)
        {
            var s = graph.segments[segIdx];
            // Project onto the segment line and clamp
            Vector2 a = SegmentWorldPos(s, s.min);
            Vector2 b = SegmentWorldPos(s, s.max);
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.001f) return (a - pos).sqrMagnitude;

            float t = Mathf.Clamp01(Vector2.Dot(pos - a, ab) / len2);
            Vector2 closest = a + ab * t;
            return (closest - pos).sqrMagnitude;
        }

        private Vector2 ProjectOntoSegment(int segIdx, Vector2 worldPos)
        {
            var s = graph.segments[segIdx];
            Vector2 a = SegmentWorldPos(s, s.min);
            Vector2 b = SegmentWorldPos(s, s.max);
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.001f) return a;

            float t = Mathf.Clamp01(Vector2.Dot(worldPos - a, ab) / len2);
            return a + ab * t;
        }

        private Vector2 TileWorldCenter(int wx, int wy)
        {
            // cellWorldOrigin is baked from tilemap.GetCellCenterWorld(0,0).
            // For graphs baked before this field existed it defaults to (0,0);
            // fall back to the standard 0.5 cell-center offset in that case.
            var o = graph.cellWorldOrigin;
            if (o == Vector2.zero) o = new Vector2(0.5f, 0.5f);
            return o + new Vector2(wx, wy);
        }

        private bool IsGrounded()
        {
            var bounds = _col.bounds;
            return Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, 0.05f, solidMask);
        }

        private static readonly List<Collider2D> _fitsBuffer = new(8);

        private bool FitsAt(Vector2 position)
        {
            var size = _col.bounds.size;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(solidMask);
            filter.useLayerMask = true;
            int count = Physics2D.OverlapBox(position, size * 0.95f, 0f, filter, _fitsBuffer);
            for (int i = 0; i < count; i++)
            {
                if (_fitsBuffer[i] != _col) return false;
            }
            return true;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawPath || _path == null || _path.Count == 0) return;
            if (graph == null || graph.segments == null || graph.links == null) return;

            var segs  = graph.segments;
            var links = graph.links;

            for (int i = 0; i < _path.Count; i++)
            {
                var step = _path[i];
                bool isCurrent = (i == _pathIndex);

                // Draw segment marker
                Vector3 center = SegmentCenter(step.segIndex);
                Gizmos.color = isCurrent ? Color.yellow : Color.green;
                Gizmos.DrawWireSphere(center, isCurrent ? 0.3f : 0.15f);

                // Draw link trajectory
                if (step.linkIndex >= 0)
                {
                    var lk = links[step.linkIndex];

                    Color col = lk.moveType == LinkMoveType.Jump
                        ? new Color(0.3f, 0.8f, 1f, 0.9f)
                        : new Color(1f, 0.85f, 0.2f, 0.9f);

                    if (lk.flightTime > 0f)
                    {
                        DrawTrajectoryGizmo(lk, col);
                    }
                    else
                    {
                        Gizmos.color = col;
                        var fromS = segs[lk.fromSeg];
                        var toS   = segs[lk.toSeg];
                        Gizmos.DrawLine(SegmentCenter(lk.fromSeg), SegmentCenter(lk.toSeg));
                    }
                }
            }

            // Draw state label
            if (drawState)
            {
                Handles.Label(transform.position + Vector3.up * 1.5f, $"[PlatNav] {_state}");
            }
        }

        private void DrawTrajectoryGizmo(in NavLink lk, Color col)
        {
            const int steps = 24;
            float dt = lk.flightTime / steps;
            Handles.color = col;
            Vector3 prev = lk.launchPos;

            if (lk.moveType == LinkMoveType.Jump)
            {
                Vector2 acc = lk.Acceleration;
                for (int s = 1; s <= steps; s++)
                {
                    float t = s * dt;
                    Vector3 p = (Vector3)(lk.launchPos + lk.launchVelocity * t + 0.5f * acc * (t * t));
                    Handles.DrawAAPolyLine(2f, prev, p);
                    prev = p;
                }
            }
            else
            {
                Vector2 pos = lk.launchPos;
                Vector2 vel = lk.launchVelocity;
                Vector2 acc = lk.Acceleration;

                for (int s = 1; s <= steps; s++)
                {
                    vel += acc * dt;
                    pos += vel * dt;

                    Vector3 p = pos;
                    Handles.DrawAAPolyLine(2f, prev, p);
                    prev = p;
                }
            }
        }
#endif
    }
}
