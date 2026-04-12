using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Agent", fileName = "AgentScriptableStats")]
public class AgentScriptableStats : ScriptableObject
{
    [Header("LAYERS")]
    [Tooltip("Layer considered as ground")]
    public LayerMask GroundLayer;

    [Header("PATROL")]
    [Tooltip("Movement speed while patrolling")]
    [Min(0f)]
    public float PatrolSpeed = 3f;

    [Tooltip("Time to wait at each patrol point before moving to the next")]
    [Min(0f)]
    public float PatrolWaitTime = 2f;

    [Tooltip("Distance from spawn position used when auto-generating patrol points")]
    [Min(0.5f)]
    public float DefaultPatrolDistance = 4f;

    [Header("SUSPICION")]
    [Tooltip("Time the agent stands still after spotting the player before deciding to chase")]
    [Min(0f)]
    public float SuspicionTimeOnSight = 1.5f;

    [Tooltip("Time the agent stands still after hearing a noise before deciding what to do next")]
    [Min(0f)]
    public float SuspicionTimeOnNoise = 2.5f;

    [Tooltip("Noise radius at or above which a heard noise triggers Search instead of returning to Patrol")]
    [Min(0f)]
    public float SearchNoiseRadius = 3f;

    [Header("CHASE")]
    [Tooltip("Movement speed while chasing the player")]
    [Min(0f)]
    public float ChaseSpeed = 5f;

    [Tooltip("How long the agent keeps chasing after losing direct sight of the player before switching to PredictionChase")]
    [Min(0f)]
    public float ChaseVisionGraceTime = 1.5f;

    [Tooltip("How long (seconds) the agent remembers and navigates to the player's predicted position after losing sight")]
    [Min(0f)]
    public float ChaseMemoryTime = 2f;

    [Header("PREDICTION CHASE")]
    [Tooltip("Maximum time (seconds) the agent spends in PredictionChase before giving up and switching to Search")]
    [Min(0f)]
    public float PredictionSearchTime = 6f;

    [Header("SEARCH")]
    [Tooltip("Movement speed while heading to the last known position")]
    [Min(0f)]
    public float SearchSpeed = 4f;

    [Tooltip("How long the agent waits at each wander waypoint before moving to the next")]
    [Min(0f)]
    public float SearchWaitTime = 1f;

    [Tooltip("Total time the agent wanders around the search point before returning to Patrol")]
    [Min(0f)]
    public float SearchWanderDuration = 8f;

    [Tooltip("Distance of the left/right wander points from the search center")]
    [Min(0.5f)]
    public float SearchWanderDistance = 3f;

    [Header("BLINDING")]
    [Tooltip("Минимальное время стояния на месте перед тем как отреагировать (погнаться или получить стан)")]
    [Min(0f)]
    public float BlindedByPlayerReactionTime = 0.3f;

    [Tooltip("Если игрок слепит агента дольше этого времени — агент уходит в AgentStunned вместо Chase")]
    [Min(0f)]
    public float BlindDurationToStun = 2f;

    [Tooltip("Сколько секунд агент стоит оглушённым после длительного ослепления")]
    [Min(0f)]
    public float StunDuration = 3f;

    [Tooltip("Скорость движения когда агент ослеплён статическим (не игроцким) источником света")]
    [Min(0f)]
    public float BlindedByEnvironmentSpeed = 4f;

    [Tooltip("Окно (секунды) после потери видимости игрока, в течение которого захват всё ещё срабатывает по дистанции")]
    [Min(0f)]
    public float GrabProximityGraceWindow = 0.4f;

    [Header("GRAB")]
    [Tooltip("Distance from the player at which the agent stops chasing and initiates a grab")]
    [Min(0f)]
    public float AttackRange = 1.5f;

    [Tooltip("Delay (seconds) before the grab locks the player on first contact in a chase")]
    [Min(0f)]
    public float AttackFirstHitDelay = 0.5f;

    [Tooltip("Number of alternating A/D presses the player needs to escape the grab")]
    [Min(1)]
    public int GrabEscapeCount = 10;

    [Tooltip("Cooldown (seconds) before the agent can grab again after the player escapes")]
    [Min(0f)]
    public float GrabCooldown = 3f;

    [Tooltip("How far in front of the agent the player is placed when grabbed")]
    [Min(0f)]
    public float GrabPlayerOffset = 0.5f;

    [Header("ALERT")]
    [Tooltip("Длительность состояния Alert — агент стоит на месте и сигнализирует союзникам")]
    [Min(0f)]
    public float AlertDuration = 3f;

    [Tooltip("Радиус (м), в котором другие агенты слышат тревогу и переходят в Search")]
    [Min(0f)]
    public float AlertRadius = 15f;

    [Header("INTERACTION")]
    [Tooltip("Layer mask for interactable objects (used by Linecast)")]
    public LayerMask InteractableLayer;

    [Tooltip("How far ahead the agent checks for interactable objects")]
    [Min(0.1f)]
    public float InteractRange = 1.5f;

    [Tooltip("Minimum time between consecutive interactions")]
    [Min(0f)]
    public float InteractCooldown = 1f;
}
