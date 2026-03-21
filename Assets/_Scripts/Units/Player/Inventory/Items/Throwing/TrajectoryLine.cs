using UnityEngine;

public class TrajectoryLine : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.PostProcessing;

    [Header("References")]
    [SerializeField] private PlayerAimAndThrow _playerAimAndThrow;
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private LayerMask groundMask;

    [Header("Trajectory Line Smoothness/Length")]
    [SerializeField] private int _segmentCount = 50;
    [SerializeField] private float _curveLenght = 3.5f;
    
    private Vector2[] _segments;
    private LineRenderer _lineRenderer;
    
    private PebbleBehavior _pebbleBehavior;
    private PlayerContext _ctx;
    
    private float _projectileSpeed;
    private float _projectileGravity;
    
    private const float TIME_CURVE_ADDITION = 0.5f;
    
    public void Initialize()
    {
        _segments = new Vector2[_segmentCount];
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _segmentCount;
        
        _ctx = Services.Get<PlayerContext>();
        
        _pebbleBehavior = _playerAimAndThrow.pebble.GetComponent<PebbleBehavior>();
        _projectileSpeed = _pebbleBehavior.pebbleSpeed;
        _projectileGravity = _pebbleBehavior.pebbleGravity;
    }
    
    private void Update()
    {
        if (_lineRenderer == null) return;
        
        if (_ctx == null || !_ctx.input.AttackHeld)
        {
            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 0;
            return;
        }

        if (_playerAimAndThrow != null &&
            _playerAimAndThrow.CurrentAimDistance < _playerAimAndThrow.MinThrowRadius)
        {
            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 0;
            return;
        }
        
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = _segmentCount;

        Vector2 startPos = _bulletSpawnPoint.position;

        _segments[0] = startPos;
        _lineRenderer.SetPosition(0, startPos);

        var speed = _playerAimAndThrow != null ? _playerAimAndThrow.CurrentProjectileSpeed : _projectileSpeed;
        Vector2 startVelocity = _bulletSpawnPoint.up * speed;

        for (int i = 1; i < _segmentCount; i++)
        {
            float timeOffset = i * Time.fixedDeltaTime * _curveLenght;

            Vector2 gravityOffset =
                TIME_CURVE_ADDITION * Physics2D.gravity * _projectileGravity * Mathf.Pow(timeOffset, 2);

            Vector2 nextPoint =
                _segments[0] + startVelocity * timeOffset + gravityOffset;

            Vector2 prevPoint = _segments[i - 1];

            RaycastHit2D hit = Physics2D.Linecast(prevPoint, nextPoint, groundMask);

            if (hit)
            {
                _lineRenderer.positionCount = i + 1;
                _lineRenderer.SetPosition(i, hit.point);
                return;
            }

            _segments[i] = nextPoint;
            _lineRenderer.SetPosition(i, _segments[i]);
        }
    }
} 
