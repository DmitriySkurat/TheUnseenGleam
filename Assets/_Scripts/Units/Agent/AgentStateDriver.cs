using System.Linq;
using UnityEngine;
using PlatNav;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlatNavHandler))]
    public class AgentStateDriver : MonoBehaviour, IInitializable
    {
        public InitializationOrder Order => InitializationOrder.Enemy;

        [SerializeField] private AgentScriptableStats _stats;
        [SerializeField] private Transform[] _patrolPoints;

        [Header("Debugging")]
        [SerializeField] private Utility.Logger _logger;

        private AgentContext _ctx;
        private StateMachine _machine;
        private AgentRoot _root;

        public void Initialize()
        {
            _ctx = new AgentContext();
            _ctx.stats = _stats;
            _ctx.transform = transform;
            _ctx.rb = GetComponent<Rigidbody2D>();
            _ctx.nav = GetComponent<PlatNavHandler>();
            _ctx.vision = GetComponent<AgentVision>();
            _ctx.hearing = GetComponent<AgentHearing>();
            _ctx.interactor = GetComponent<AgentInteractor>();
            if (_ctx.interactor != null)
                _ctx.interactor.Initialize(_ctx);
            _ctx.spawnPosition = transform.position;
            _ctx.playerTransform = Services.Get<PlayerContext>()?.transform;

            if (_patrolPoints != null && _patrolPoints.Length > 0)
                _ctx.patrolPointTransforms = _patrolPoints;

            // Ensure no tracked target is left over from inspector setup
            _ctx.nav.SetTarget(null);

            // Forward hearing events into context so states can react via GetTransition
            if (_ctx.hearing != null)
                _ctx.hearing.OnHeard += OnHeard;

            _root = new AgentRoot(null, _ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();
        }

        void FixedUpdate()
        {
            _machine.Tick(Time.fixedDeltaTime);
            PrintStatePath();
        }

        void OnDestroy()
        {
            if (_ctx?.hearing != null)
                _ctx.hearing.OnHeard -= OnHeard;
        }

        void OnHeard(NoiseEvent noise, float loudness)
        {
            _ctx.pendingNoiseAlert = true;
            _ctx.pendingNoiseLoudness = loudness;
            _ctx.pendingNoisePosition = noise.Position;
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

        #endregion
    }
}
