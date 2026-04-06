using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerInteractor))]
    public class PlayerStateDriver : MonoBehaviour, IInitializable 
    {
        public InitializationOrder Order => InitializationOrder.Player;
    
        [Header("Debugging")]
        [SerializeField] private Utility.Logger _logger;
    
        private PlayerContext _ctx;
        
        private StateMachine _machine;
        private PlayerRoot _root;
        
        
        public void Initialize()
        {
            _ctx = Services.Get<PlayerContext>();
            _ctx.transform = transform;
            _ctx.rb = GetComponentInChildren<Rigidbody2D>();
            _ctx.anim = GetComponentInChildren<Animator>();
            _ctx.renderer = GetComponentInChildren<Renderer>();
            _ctx.lightSensor = GetComponentInChildren<PlayerLightSensor>(); 


            _root = new PlayerRoot(null, _ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();
        }        


        void FixedUpdate() {
            _machine.Tick(Time.fixedDeltaTime);
            
            //Debug
            PrintStatePath();
            //_logger.Log($"ctx.isClimbing: {_ctx.isClimbing}, ctx.velocity: {_ctx.velocity}", this);
            //_logger.Log($"Stamina: {ctx.stamina}", this);
        }

        
        #region Debugging

        private string _lastPath;
        
        void PrintStatePath() 
        {
            var path = StatePath(_machine.Root.Leaf());
            if (path != _lastPath) {
                _logger.Log("State: " + path, this);
                _lastPath = path;
            }
        }

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
        
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            var col = _ctx.coll as CapsuleCollider2D;
            
            // Коллайдер
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);

            Gizmos.matrix = Matrix4x4.identity;
            
            // CeilingAbove
            Gizmos.color = _ctx.ceilingAbove ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _ctx.stats.CeilingCheckDistance);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * _ctx.stats.CeilingCheckDistance, 0.05f);

            // Проверка вниз (groundHit)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.down * _ctx.stats.GrounderDistance);
            Gizmos.DrawWireSphere(col.bounds.center + Vector3.down * _ctx.stats.GrounderDistance, 0.05f);

            // Проверка вверх (ceilingHit)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.up * _ctx.stats.GrounderDistance);
            Gizmos.DrawWireSphere(col.bounds.center + Vector3.up * _ctx.stats.GrounderDistance, 0.05f);
        
        }
        
        #endregion
    }   
}
