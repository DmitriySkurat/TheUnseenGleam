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
}
