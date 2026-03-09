/// <summary>
///* This class links the entity to the pathfinding system.
///* it has to be set depending on the game structure.
/// </summary>

using UnityEngine;
using Pathfinding;

namespace Entity.Enemy
{
    public class EnemyHandler : MonoBehaviour, IInitializable
    {
        public InitializationOrder Order => InitializationOrder.Enemy;
    
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

        public void Initialize()
        {
            RB = GetComponent<Rigidbody2D>();
            _pathfinder = GetComponent<PathfinderHandler>();
            
            if (_pathfinder != null)
            {
                _pathfinder.InitPathfinder(this); 
            }
            
            Debug.Log("<color=green>Enemy Initialized!</color>");
            
            // Если у врага есть таргет (например, игрок), можно задать его здесь
            // var player = Services.Get<PlayerContext>();
            // if (player != null) _pathfinder.SetTarget(player.renderer.transform);
        }
    }
}
