using System.Linq;
using UnityEngine;
using PlatNav;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlatNavHandler))]
    public class AgentStateDriver : MonoBehaviour, ISceneLifecycle
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
            _ctx.lightSensor = GetComponent<AgentLightSensor>();
            if (_ctx.interactor != null)
                _ctx.interactor.Initialize(_ctx);
            _ctx.spawnPosition = transform.position;
            var playerCtx = Services.IsRegistered<PlayerContext>() ? Services.Get<PlayerContext>() : null;
            _ctx.playerTransform = playerCtx?.transform;
            _ctx.playerRb     = _ctx.playerTransform != null ? _ctx.playerTransform.GetComponent<Rigidbody2D>() : null;
            _ctx.playerHealth = _ctx.playerTransform != null ? _ctx.playerTransform.GetComponent<PlayerHealth>() : null;
            _ctx.playerCtx    = playerCtx;

            if (_patrolPoints != null && _patrolPoints.Length > 0)
                _ctx.patrolPointTransforms = _patrolPoints;

            // Ensure no tracked target is left over from inspector setup
            _ctx.nav.SetTarget(null);

            // Forward hearing events into context so states can react via GetTransition
            if (_ctx.hearing != null)
                _ctx.hearing.OnHeard += OnHeard;

            // Forward alert broadcasts from other agents into context
            _ctx.alertSystem = Services.Get<AgentAlertSystem>();
            _ctx.alertSystem.OnAlertBroadcast += OnAlertReceived;

            _ctx.searchCoordinator = Services.Get<AgentSearchCoordinator>();

            _root = new AgentRoot(null, _ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();
        }
        
        public void Dispose()
        {
            _ctx.hearing.OnHeard -= OnHeard;
            _ctx.alertSystem.OnAlertBroadcast -= OnAlertReceived;
        }

        void FixedUpdate()
        {
            UpdateBlindingState();
            UpdateGrabReactionTimer();
            _machine.Tick(Time.fixedDeltaTime);
            PrintStatePath();
        }

        void UpdateBlindingState()
        {
            if (_ctx.lightSensor == null)
            {
                _ctx.isBlindedByPlayer      = false;
                _ctx.isBlindedByEnvironment = false;
                _ctx.blindedByPlayerTimer   = 0f;
                return;
            }

            if (_ctx.lightSensor.TryGetBlindingInfo(out bool isPlayerOwned, out Vector2 lightPos))
            {
                _ctx.isBlindedByPlayer      = isPlayerOwned;
                _ctx.isBlindedByEnvironment = !isPlayerOwned;
                _ctx.blindingSourcePosition = lightPos;
            }
            else
            {
                _ctx.isBlindedByPlayer      = false;
                _ctx.isBlindedByEnvironment = false;
            }

            // Таймер непрерывного ослепления игроком — растёт во всех состояниях
            if (_ctx.isBlindedByPlayer)
                _ctx.blindedByPlayerTimer += Time.fixedDeltaTime;
            else
                _ctx.blindedByPlayerTimer = 0f;
        }

        bool _wasPlayerGrabbed;

        void UpdateGrabReactionTimer()
        {
            if (_ctx.playerCtx == null || _ctx.isGrabbingPlayer)
            {
                _ctx.grabReactionTimer = 0f;
                _wasPlayerGrabbed = false;
                return;
            }

            bool isGrabbed = _ctx.playerCtx.isGrabbed;

            // Сброс таймера при начале нового захвата
            if (isGrabbed && !_wasPlayerGrabbed)
                _ctx.grabReactionTimer = _ctx.stats.GrabReactionDelay;

            if (isGrabbed && _ctx.grabReactionTimer > 0f)
                _ctx.grabReactionTimer -= Time.fixedDeltaTime;

            if (!isGrabbed)
                _ctx.grabReactionTimer = 0f;

            _wasPlayerGrabbed = isGrabbed;
        }

        void OnDestroy()
        {
            if (_ctx?.hearing != null)
                _ctx.hearing.OnHeard -= OnHeard;

            if (_ctx?.alertSystem != null)
                _ctx.alertSystem.OnAlertBroadcast -= OnAlertReceived;
        }

        void OnHeard(NoiseEvent noise)
        {
            if (_ctx.playerCtx != null && _ctx.playerCtx.isGrabbed)
                return;

            _ctx.pendingNoiseAlert = true;
            _ctx.pendingNoiseRadius = noise.Radius;
            _ctx.pendingNoisePosition = noise.Position;
        }

        void OnAlertReceived(Vector2 position)
        {
            float dist = Vector2.Distance(_ctx.transform.position, position);
            if (dist <= _ctx.stats.AlertRadius)
            {
                _ctx.alertPending  = true;
                _ctx.alertPosition = position;
            }
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

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_stats == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _stats.AttackRange);
            Gizmos.color = new Color(1f, 0.5f, 0f); // orange
            Gizmos.DrawWireSphere(transform.position, _stats.AlertRadius);
        }
#endif
    }
}
