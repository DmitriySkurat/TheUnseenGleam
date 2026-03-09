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

        public bool HasTarget => target != null;

        public float DistanceToTarget
        {
            get
            {
                if (target == null || self == null) return float.PositiveInfinity;
                return Vector3.Distance(self.position, target.position);
            }
        }

        public bool CanChase => HasTarget && DistanceToTarget <= detectRange;
        public bool ShouldStopChase => !HasTarget || DistanceToTarget >= loseRange;

        public void ClampRanges()
        {
            if (loseRange < detectRange) loseRange = detectRange;
        }

        public void RefreshTarget()
        {
            if (target != null) return;
            if (!autoFindTargetByTag || string.IsNullOrEmpty(targetTag)) return;

            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) SetTarget(go.transform);
        }

        public void SetTarget(Transform newTarget)
        {
            if (ReferenceEquals(target, newTarget)) return;
            target = newTarget;
            if (pathfinder != null) pathfinder.SetTarget(target);
        }
    }
}
