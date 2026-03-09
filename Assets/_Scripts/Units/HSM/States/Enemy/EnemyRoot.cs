using Entity.Enemy;

namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyIdle Idle;
        public readonly EnemyInvestigate Investigate;
        public readonly EnemyChase Chase;

        public EnemyRoot(StateMachine m, EnemyContext ctx) : base(m, null)
        {
            Idle = new EnemyIdle(m, this, ctx);
            Investigate = new EnemyInvestigate(m, this, ctx);
            Chase = new EnemyChase(m, this, ctx);
        }

        protected override State GetInitialState() => Idle;

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }
    }
}
