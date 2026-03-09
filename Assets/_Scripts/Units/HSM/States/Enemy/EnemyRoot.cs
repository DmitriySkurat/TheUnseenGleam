using Entity.Enemy;

namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyPatrolState Patrol;
        public readonly EnemyInvestigateState Investigate;
        public readonly EnemyChaseState Chase;
        public readonly EnemySearchState Search;

        public EnemyRoot(StateMachine m, EnemyContext ctx) : base(m, null)
        {
            Patrol = new EnemyPatrolState(m, this, ctx);
            Investigate = new EnemyInvestigateState(m, this, ctx);
            Chase = new EnemyChaseState(m, this, ctx);
            Search = new EnemySearchState(m, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }
    }
}
