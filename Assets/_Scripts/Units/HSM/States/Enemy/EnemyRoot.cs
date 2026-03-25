using UnityEngine;
using PlatNav;

namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyPatrol Patrol;
        public readonly EnemyChasing Chasing;
        public readonly EnemyInvestigating Investigating;
        public readonly EnemySearching Searching;
        public readonly EnemyReturningToPatrol ReturningToPatrol;

        private readonly EnemyContext _ctx;

        public EnemyRoot(StateMachine machine, EnemyContext ctx) : base(machine, null)
        {
            _ctx = ctx;
            
            Patrol = new EnemyPatrol(machine, this, ctx);
            Chasing = new EnemyChasing(machine, this, ctx);
            Investigating = new EnemyInvestigating(machine, this, ctx);
            Searching = new EnemySearching(machine, this, ctx);
            ReturningToPatrol = new EnemyReturningToPatrol(machine, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override State GetTransition()
        {
            // Check for vision
            if (_ctx.vision != null && _ctx.vision.CanSeePlayer && _ctx.playerTransform != null)
            {
                UpdateKnownPlayerPosition(_ctx.playerTransform.position);
                return Chasing;
            }

            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Early exit if traversing platform link
            if (IsTraversingLink())
            {
                base.OnUpdate(deltaTime);
                return;
            }

            // Update player reference
            ResolvePlayerTransform();

            // Check vision
            bool canSeePlayer = _ctx.vision != null && _ctx.vision.CanSeePlayer && _ctx.playerTransform != null;
            if (canSeePlayer)
            {
                UpdateKnownPlayerPosition(_ctx.playerTransform.position);
                if (IsTraversingLink())
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                // Transition to chasing if not already there
                if (!(Machine.Root.Leaf() is EnemyChasing))
                {
                    Machine.Sequencer.RequestTransition(this, Chasing);
                }
            }

            _ctx.wasSeeingPlayerLastFrame = canSeePlayer;

            base.OnUpdate(deltaTime);
        }

        private bool IsTraversingLink()
        {
            return _ctx.navHandler != null && _ctx.navHandler.State == PlatNavState.TraversingLink;
        }

        private void ResolvePlayerTransform()
        {
            if (_ctx.playerTransform != null)
                return;

            if (_ctx.playerContext == null)
            {
                _ctx.playerContext = Services.Get<PlayerContext>();
            }

            if (_ctx.playerContext != null)
            {
                _ctx.playerTransform = _ctx.playerContext.transform;
            }
        }

        private void UpdateKnownPlayerPosition(Vector2 position)
        {
            _ctx.lastKnownPlayerPosition = position;
            _ctx.hasKnownPlayerPosition = true;
            _ctx.hasDetectedPlayer = true;
        }
    }
}
