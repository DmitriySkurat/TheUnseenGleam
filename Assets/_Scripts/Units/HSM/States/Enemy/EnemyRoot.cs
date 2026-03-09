using Entity.Enemy;

namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyIdle Idle;
        public readonly EnemyChase Chase;

        private readonly EnemyContext ctx;

        public EnemyRoot(StateMachine m, EnemyContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Idle = new EnemyIdle(m, this, ctx);
            Chase = new EnemyChase(m, this, ctx);
        }

        protected override State GetInitialState() => Idle;

        protected override void OnUpdate(float deltaTime)
        {
            ctx?.RefreshTarget();
            base.OnUpdate(deltaTime);
        }
    }
}
