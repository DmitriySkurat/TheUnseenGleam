namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyPatrol Patrol;
        public readonly EnemyChasing Chasing;
        public readonly EnemyInvestigating Investigating;
        public readonly EnemySearching Searching;
        public readonly EnemyReturningToPatrol ReturningToPatrol;

        private readonly EnemyContext ctx;

        public EnemyRoot(StateMachine m, EnemyContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol = new EnemyPatrol(m, this, ctx);
            Chasing = new EnemyChasing(m, this, ctx);
            Investigating = new EnemyInvestigating(m, this, ctx);
            Searching = new EnemySearching(m, this, ctx);
            ReturningToPatrol = new EnemyReturningToPatrol(m, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override void OnEnter()
        {
            if (ctx.hearing != null)
                ctx.hearing.OnHeard += HandleHeard;
        }

        protected override void OnExit()
        {
            if (ctx.hearing != null)
                ctx.hearing.OnHeard -= HandleHeard;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.IsTraversingLink)
            {
                if (ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null)
                    ctx.UpdateKnownPlayerPosition(ctx.playerTransform.position);
                return;
            }

            if (ActiveChild != null)
                ActiveChild.Update(deltaTime);

            if (Machine?.Sequencer == null || Machine.Sequencer.IsTransitioning)
                return;

            bool canSeePlayer = ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null;
            if (!canSeePlayer)
                return;

            ctx.UpdateKnownPlayerPosition(ctx.playerTransform.position);

            var chasing = Machine.GetState<EnemyChasing>();
            if (Machine.Root.Leaf() == chasing)
                return;

            ctx.BeginVisualChase();
            Machine.Sequencer.RequestTransition(Machine.Root.Leaf(), chasing);
        }

        private void HandleHeard(NoiseEvent noiseEvent, float loudness)
        {
            if (noiseEvent.Source == null || noiseEvent.Source == ctx.selfTransform.gameObject)
                return;

            bool isPlayerNoise = ctx.playerTransform != null
                && noiseEvent.Source == ctx.playerTransform.gameObject;

            if (isPlayerNoise && ctx.hasDetectedPlayer)
            {
                ctx.UpdateKnownPlayerPosition(noiseEvent.Position);

                if (ctx.IsTraversingLink)
                    return;

                Machine.Sequencer.RequestTransition(
                    Machine.Root.Leaf(),
                    Machine.GetState<EnemyChasing>());
                return;
            }

            if (Machine.Root.Leaf() is EnemyChasing && !isPlayerNoise)
                return;

            if (ctx.IsTraversingLink)
                return;

            ctx.investigationTarget = noiseEvent.Position;
            Machine.Sequencer.RequestTransition(
                Machine.Root.Leaf(),
                Machine.GetState<EnemyInvestigating>());
        }
    }
}
