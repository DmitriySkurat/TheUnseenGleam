using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;
using Pathfinding;
using Entity.Enemy;

namespace HSM
{
    [RequireComponent(typeof(EnemyHandler), typeof(PathfinderHandler), typeof(EnemyMotor))]
    public class EnemyStateDriver : MonoBehaviour, IInitializable
    {
        public InitializationOrder Order => InitializationOrder.Enemy;

        [Header("References")]
        [SerializeField] private EnemyHandler enemy;
        [SerializeField] private PathfinderHandler pathfinder;
        [SerializeField] private PathfindingGraph graph;
        [SerializeField] private Tilemap navigationTilemap;
        [SerializeField] private Transform agent;
        [FormerlySerializedAs("target")]
        [SerializeField] private Transform player;
        [SerializeField] private EnemyMotor movement;
        [SerializeField] private EnemyVision vision;
        [SerializeField] private EnemyHearing hearing;

        [Header("Targeting")]
        [SerializeField] private bool autoFindTargetByTag = true;
        [SerializeField] private string targetTag = "Player";
        [SerializeField, Min(0f)] private float detectRange = 6f;
        [SerializeField, Min(0f)] private float loseRange = 8f;

        [Header("Behavior")]
        [SerializeField] private bool requireGroundedInPlay = false;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField, Min(0f)] private float patrolWaitTime = 1.25f;
        [SerializeField, Min(0f)] private float patrolPointReachedDistance = 0.35f;

        [Header("Search")]
        [SerializeField, Min(0f)] private float searchRadius = 3f;
        [SerializeField, Min(0f)] private float searchDuration = 4f;
        [SerializeField, Min(0f)] private float searchPointReachedDistance = 0.35f;

        [Header("Debugging")]
        [SerializeField] private Utility.Logger _logger;

        private EnemyContext _ctx;
        private StateMachine _machine;
        private EnemyRoot _root;

        public EnemyContext Context => _ctx;

        public void Initialize()
        {
            if (enemy == null) enemy = GetComponent<EnemyHandler>();
            if (pathfinder == null) pathfinder = GetComponent<PathfinderHandler>();
            if (agent == null) agent = transform;
            if (movement == null) movement = GetComponent<EnemyMotor>();
            if (vision == null) vision = GetComponent<EnemyVision>();
            if (hearing == null) hearing = GetComponent<EnemyHearing>();

            if (enemy == null || pathfinder == null)
            {
                Debug.LogError("EnemyStateDriver: EnemyHandler or PathfinderHandler is missing", this);
                return;
            }

            pathfinder.Configure(graph, navigationTilemap, agent);
            pathfinder.InitPathfinder(enemy);
            pathfinder.SetRequireGroundedInPlay(requireGroundedInPlay);

            if (player != null) pathfinder.SetTarget(player);

            if (loseRange < detectRange) loseRange = detectRange;

            _ctx = new EnemyContext
            {
                enemy = enemy,
                pathfinder = pathfinder,
                movement = movement,
                vision = vision,
                hearing = hearing,
                self = agent != null ? agent : transform,
                player = player,
                detectRange = detectRange,
                loseRange = loseRange,
                autoFindTargetByTag = autoFindTargetByTag,
                targetTag = targetTag,
                patrolPoints = patrolPoints,
                patrolWaitTime = patrolWaitTime,
                patrolPointReachedDistance = patrolPointReachedDistance,
                searchRadius = searchRadius,
                searchDuration = searchDuration,
                searchPointReachedDistance = searchPointReachedDistance,
            };

            if (player != null)
            {
                _ctx.lastKnownPlayerPosition = player.position;
                _ctx.hasLastKnownPlayerPosition = true;
                _ctx.lastKnownPlayerTime = Time.time;
            }

            if (vision != null) vision.BindContext(_ctx);
            if (hearing != null) hearing.BindContext(_ctx);

            _root = new EnemyRoot(null, _ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();

            _logger?.Log("Enemy HSM initialized", this);
        }

        private void FixedUpdate()
        {
            if (_machine == null) return;
            RefreshTarget();
            _machine.Tick(Time.fixedDeltaTime);
        }

        private void RefreshTarget()
        {
            if (_ctx == null) return;
            if (_ctx.player != null) return;
            if (!_ctx.autoFindTargetByTag || string.IsNullOrEmpty(_ctx.targetTag)) return;

            var go = GameObject.FindGameObjectWithTag(_ctx.targetTag);
            if (go == null) return;

            _ctx.player = go.transform;
            if (_ctx.pathfinder != null) _ctx.pathfinder.SetTarget(_ctx.player);
            if (_ctx.vision != null && _ctx.vision.Player == null) _ctx.vision.SetPlayer(_ctx.player);
        }

        private void OnDrawGizmos()
        {
            if (_ctx == null) return;

            Color stateColor = Color.white;
            if (_root != null)
            {
                var leaf = _root.Leaf();
                if (leaf is EnemyPatrolState) stateColor = Color.green;
                else if (leaf is EnemyChaseState) stateColor = Color.red;
                else if (leaf is EnemyInvestigateState) stateColor = Color.yellow;
                else if (leaf is EnemySearchState) stateColor = Color.blue;
            }

            if (_ctx.hasLastKnownPlayerPosition)
            {
                Gizmos.color = stateColor;
                Vector3 pos = new Vector3(_ctx.lastKnownPlayerPosition.x, _ctx.lastKnownPlayerPosition.y, 0f);
                Gizmos.DrawSphere(pos, 0.12f);
            }

            if (_ctx.searchRadius > 0f && _ctx.hasLastKnownPlayerPosition)
            {
                Gizmos.color = stateColor;
                Vector3 center = new Vector3(_ctx.lastKnownPlayerPosition.x, _ctx.lastKnownPlayerPosition.y, 0f);
                Gizmos.DrawWireSphere(center, _ctx.searchRadius);
            }
        }
    }
}
