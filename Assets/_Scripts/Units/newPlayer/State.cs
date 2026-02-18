using UnityEngine;

namespace newFSM 
{
    public abstract class State : MonoBehaviour
    {
        public bool isComplete { get; protected set; }
        
        protected float startTime;
        
        public float time => Time.time - startTime;

        protected Rigidbody2D body;
        protected Animator animator;
        protected PlayerMovement input;
        
        public virtual void Enter() {}
        public virtual void Do() {}
        public virtual void FixedDo() {}
        public virtual void Exit() {}
        
        public void Setup(Rigidbody2D _body, Animator _animator, PlayerMovement _playerMovement)
        {
            this.body = _body;
            this.animator = _animator;
            this.input = _playerMovement;
        }
        
        public void Initialize()
        {
            isComplete = false;
            startTime = Time.time;
        }
    }
    
}