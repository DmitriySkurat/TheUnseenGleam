using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAimAndThrow _playerAimAndThrow;
    [SerializeField] private Transform _bulletSpawnPoint;

    [Header("Trajectory Line Smoothness/Length")]
    [SerializeField] private int _segmentCount = 50;
    [SerializeField] private float _curveLenght = 3.5f;
    
    private Vector2[] _segments;
    private LineRenderer _lineRenderer;
    
    private PebbleBehavior _pebbleBehavior;
    
    private float _projectileSpeed;
    private float _projectileGravity;
    
    private const float TIME_CURVE_ADDITION = 0.5f;
    
    private void Start()
    {
        _segments = new Vector2[_segmentCount];
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _segmentCount;
        
        _pebbleBehavior = _playerAimAndThrow.GetComponentInChildren<PebbleBehavior>();
        _projectileSpeed = _pebbleBehavior.pebbleSpeed;
        _projectileGravity = _pebbleBehavior.pebbleGravity;
    }
    
    private void Update()
    {
        Vector2 startPos = _bulletSpawnPoint.position;
        _segments[0] = startPos;
        _lineRenderer.SetPosition(0, startPos);
        
        Vector2 startVelocity = transform.up * _projectileSpeed;
        
        for (int i = 1; i < _segmentCount; i++)
        {
            float timeOffset = i * Time.fixedDeltaTime * _curveLenght;
            
            Vector2 gravityOffset = TIME_CURVE_ADDITION * Physics2D.gravity * _projectileGravity * Mathf.Pow(timeOffset, 2);
            
            _segments[i] = _segments[0] + startVelocity * timeOffset + gravityOffset;
            _lineRenderer.SetPosition(i, _segments[i]);
        }
        
    }
} 