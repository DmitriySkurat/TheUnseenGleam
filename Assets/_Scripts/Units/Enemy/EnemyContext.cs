using System;
using UnityEngine;
using PlatNav;

/// <summary>
/// Context object that holds all shared data for enemy AI behavior
/// Follows the same pattern as PlayerContext for easier state management
/// </summary>
[Serializable]
public class EnemyContext
{
    // ===== COMPONENTS =====
    public PlatNavHandler navHandler;
    public AgentVision vision;
    public AgentHearing hearing;
    public AgentLightSensor lightSensor;
    public Transform transform;
    public Animator animator;
    public Rigidbody2D rb;

    // ===== PATROL =====
    public Vector2 patrolOrigin;
    public Vector2[] patrolRoute;
    public int patrolIndex;
    public int patrolDirection = 1;
    public float patrolSpeed = 2f;
    public float patrolHalfWidth = 2f;
    public float patrolWaitTime = 1.25f;

    // ===== CHASE =====
    public Vector2 lastKnownPlayerPosition;
    public bool hasKnownPlayerPosition;
    public bool hasDetectedPlayer;
    public bool usingVisualChase;
    public float chaseSpeed = 4.5f;
    public float retargetDistance = 0.35f;
    public float lastSeenPositionDistance = float.MaxValue;

    // ===== INVESTIGATE =====
    public Vector2 investigationTarget;
    public float investigateSpeed = 3f;
    public float investigateWaitTime = 0.75f;

    // ===== SEARCH =====
    public Vector2[] searchRoute = new Vector2[2];
    public int searchIndex;
    public int searchDirection = 1;
    public float searchTimer;
    public float searchDuration = 4f;
    public float searchSpeed = 2.5f;
    public float searchHalfWidth = 1.75f;
    public float searchWaitTime = 0.6f;

    // ===== RETURN TO PATROL =====
    public Vector2 returnTarget;
    public float returnSpeed = 3f;

    // ===== LIGHT RESPONSE =====
    public float lightEscapeDistance = 4f;
    public float lightEscapeSpeed = 4.5f;

    // ===== WAIT STATE =====
    public bool isWaiting;
    public float waitTimer;

    // ===== MOVEMENT COMMAND =====
    public Vector2 manualDestination;
    public bool manualCommandActive;
    public bool manualCommandFailed;

    // ===== PLAYER REFERENCE =====
    public PlayerContext playerContext;
    public Transform playerTransform;
    public bool wasSeeingPlayerLastFrame;
}
