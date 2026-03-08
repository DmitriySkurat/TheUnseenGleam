/// <summary>
///* This class links the entity to the pathfinding system.
///* it has to be set depending on the game structure.
/// </summary>

using UnityEngine;

namespace Entity.Enemy
{
    public class EnemyHandler : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody2D RB;
        public EnemyData enemyData;

        [Header("State")]
        public bool IsGrounded;

        public float HorizontalVelocity
        {
            get
            {
                if (RB == null)
                {
                    return 0f;
                }
                return Vector2.Dot(RB.linearVelocity, transform.right);
            }
            set
            {
                if (RB == null)
                {
                    return;
                }

                Vector2 right = transform.right;
                Vector2 up = transform.up;
                Vector2 current = RB.linearVelocity;
                float vertical = Vector2.Dot(current, up);
                RB.linearVelocity = right * value + up * vertical;
            }
        }
    }
}
