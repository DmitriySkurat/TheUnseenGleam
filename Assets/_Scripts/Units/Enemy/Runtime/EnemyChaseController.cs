using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using Entity.Enemy;

public class EnemyChaseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathfinderHandler pathfinder;
    [SerializeField] private EnemyHandler enemy;
    [SerializeField] private PathfindingGraph graph;
    [SerializeField] private Tilemap navigationTilemap;
    [SerializeField] private Transform agent;
    [SerializeField] private Transform target;

    [Header("Targeting")]
    [SerializeField] private bool autoFindTargetByTag = true;
    [SerializeField] private string targetTag = "Player";

    [Header("Behavior")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool requireGroundedInPlay = false;

    private void Awake()
    {
        if (pathfinder == null)
        {
            pathfinder = GetComponent<PathfinderHandler>();
        }
        if (enemy == null)
        {
            enemy = GetComponent<EnemyHandler>();
        }
        if (agent == null)
        {
            agent = transform;
        }
    }

    private void Start()
    {
        if (autoStart)
        {
            StartChase();
        }
    }

    public void StartChase()
    {
        if (pathfinder == null)
        {
            Debug.LogError("EnemyChaseController: PathfinderHandler is missing", this);
            return;
        }
        if (enemy == null)
        {
            Debug.LogError("EnemyChaseController: EnemyHandler is missing", this);
            return;
        }
        if (enemy.RB == null)
        {
            Debug.LogError("EnemyChaseController: EnemyHandler.RB is null. Add Rigidbody2D or assign it.", this);
            return;
        }
        if (target == null && autoFindTargetByTag && !string.IsNullOrEmpty(targetTag))
        {
            GameObject go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null)
            {
                target = go.transform;
            }
        }

        pathfinder.Configure(graph, navigationTilemap, agent);
        pathfinder.InitPathfinder(enemy);
        if (target != null)
        {
            pathfinder.SetTarget(target);
        }
        pathfinder.SetRequireGroundedInPlay(requireGroundedInPlay);
        pathfinder.tryFindPath = true;
    }

    public void StopChase()
    {
        if (pathfinder != null)
        {
            pathfinder.tryFindPath = false;
        }
    }
}
