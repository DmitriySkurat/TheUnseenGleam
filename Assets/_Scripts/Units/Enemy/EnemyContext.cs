using UnityEngine;
using Pathfinding;

namespace Entity.Enemy
{
    [System.Serializable]
    public class EnemyContext
    {
        public EnemyHandler enemy;
        public PathfinderHandler pathfinder;
        public EnemyMotor movement;
        public EnemyVision vision;
        public EnemyHearing hearing;
        public Transform self;
        public Transform player;

        public float detectRange = 6f;
        public float loseRange = 8f;

        public bool autoFindTargetByTag = true;
        public string targetTag = "Player";

        // ===== MEMORY =====
        public Vector2 lastKnownPlayerPosition;
        public bool hasLastKnownPlayerPosition;
        public float lastKnownPlayerTime;

        // ===== PATROL / SEARCH =====
        public Transform[] patrolPoints;
        public int patrolIndex;
        public float patrolWaitTime = 1.25f;
        public float patrolPointReachedDistance = 0.35f;
        public float searchRadius = 3f;
        public float searchDuration = 4f;
        public float searchPointReachedDistance = 0.35f;

        // ===== NOISE / HEARING =====
        public Transform noiseTarget;
        public Vector2 lastNoisePosition;
        public NoiseType lastNoiseType;
        public float lastNoiseTime;
        public float noiseMemoryDuration = 4f;
        public float investigateStopDistance = 0.35f;
        public bool hasNoiseTarget;
    }
}
