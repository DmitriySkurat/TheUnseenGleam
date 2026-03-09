using UnityEngine;
using Pathfinding;

namespace Entity.Enemy
{
    [System.Serializable]
    public class EnemyContext
    {
        public EnemyHandler enemy;
        public PathfinderHandler pathfinder;
        public Transform self;
        public Transform target;

        public float detectRange = 6f;
        public float loseRange = 8f;

        public bool autoFindTargetByTag = true;
        public string targetTag = "Player";

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
