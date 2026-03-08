using UnityEngine;

namespace Entity.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Movement")]
        public float awareSpeed = 2f;
        public float maxJumpVelocity = 10f;
        public float maxAcceleration = 10f;
        public float gravityMult = 1f;

        [Header("Dimensions")]
        public Vector2Int entitySize = Vector2Int.one;
        public Vector2 colliderSize = Vector2.one;
    }
}
