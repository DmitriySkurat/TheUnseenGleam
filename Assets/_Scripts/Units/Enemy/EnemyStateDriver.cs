using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using Entity.Enemy;

namespace HSM
{
    [RequireComponent(typeof(EnemyHandler), typeof(PathfinderHandler))]
    public class EnemyStateDriver : MonoBehaviour, IInitializable
    {
        public InitializationOrder Order => InitializationOrder.Enemy;

        [Header("References")]
        [SerializeField] private EnemyHandler enemy;
        [SerializeField] private PathfinderHandler pathfinder;
        [SerializeField] private PathfindingGraph graph;
        [SerializeField] private Tilemap navigationTilemap;
        [SerializeField] private Transform agent;
        [SerializeField] private Transform target;

        [Header("Targeting")]
        [SerializeField] private bool autoFindTargetByTag = true;
        [SerializeField] private string targetTag = "Player";
        [SerializeField, Min(0f)] private float detectRange = 6f;
        [SerializeField, Min(0f)] private float loseRange = 8f;

        [Header("Behavior")]
        [SerializeField] private bool requireGroundedInPlay = false;

        [Header("Debugging")]
        [SerializeField] private Utility.Logger _logger;

        private EnemyContext _ctx;
        private StateMachine _machine;
        private EnemyRoot _root;

        public void Initialize()
        {
            if (enemy == null) enemy = GetComponent<EnemyHandler>();
            if (pathfinder == null) pathfinder = GetComponent<PathfinderHandler>();
            if (agent == null) agent = transform;

            if (enemy == null || pathfinder == null)
            {
                Debug.LogError("EnemyStateDriver: EnemyHandler or PathfinderHandler is missing", this);
                return;
            }

            pathfinder.Configure(graph, navigationTilemap, agent);
            pathfinder.InitPathfinder(enemy);
            pathfinder.SetRequireGroundedInPlay(requireGroundedInPlay);

            if (target != null) pathfinder.SetTarget(target);

            _ctx = new EnemyContext
            {
                enemy = enemy,
                pathfinder = pathfinder,
                self = agent != null ? agent : transform,
                target = target,
                detectRange = detectRange,
                loseRange = loseRange,
                autoFindTargetByTag = autoFindTargetByTag,
                targetTag = targetTag,
            };
            _ctx.ClampRanges();

            _root = new EnemyRoot(null, _ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();

            _logger?.Log("Enemy HSM initialized", this);
        }

        private void FixedUpdate()
        {
            if (_machine == null) return;
            _machine.Tick(Time.fixedDeltaTime);
        }
    }
}
