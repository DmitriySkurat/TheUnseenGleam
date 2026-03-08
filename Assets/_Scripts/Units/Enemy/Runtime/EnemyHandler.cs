/// <summary>
///* This class links the entity to the pathfinding system.
///* it has to be set depending on the game structure.
/// </summary>

using UnityEngine;
using Pathfinding;

namespace Entity.Enemy
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PathfinderHandler))]
    public class EnemyHandler : MonoBehaviour//, IInitializable
    {
        //public InitializationOrder Order => InitializationOrder.GameplayCore; // Или добавить Enemy = 250 в enum

        [Header("References")]
        public EnemyData enemyData;
        public Rigidbody2D RB { get; private set; }
        private PathfinderHandler _pathfinder;

        [Header("Collision")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.1f;

        public bool IsGrounded { get; private set; }
        
        public float HorizontalVelocity 
        { 
            set => RB.linearVelocity = new Vector2(value, RB.linearVelocity.y); 
        }
        
        public void Awake()
        {
            RB = GetComponent<Rigidbody2D>();
            _pathfinder = GetComponent<PathfinderHandler>();
        }

        // public void Initialize()
        // {
        //     RB = GetComponent<Rigidbody2D>();
        //     _pathfinder = GetComponent<PathfinderHandler>();
            
        //     // Если у врага есть таргет (например, игрок), можно задать его здесь
        //     // var player = Services.Get<PlayerContext>();
        //     // if (player != null) _pathfinder.SetTarget(player.renderer.transform);
        // }

        private void FixedUpdate()
        {
            if (RB == null) return;
            CheckGrounded();
        }

        private void CheckGrounded()
        {
            // Простая проверка земли (адаптируй под свои нужды)
            Vector2 position = transform.position;
            Vector2 direction = Vector2.down;
            
            RaycastHit2D hit = Physics2D.Raycast(position, direction, groundCheckDistance, groundLayer);
            IsGrounded = hit.collider != null;
        }
    }
}