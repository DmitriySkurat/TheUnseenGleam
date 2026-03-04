using UnityEngine;
using HSM;
using Unity.VisualScripting;

public class CameraFollow : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Camera;

    [Header("References")] 
    [SerializeField] private Transform target;
    
    [Header("Movement")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    
    [Header("Look Around")]
    [SerializeField] private float lookRange = 3f; 
    [SerializeField] private float returnSpeed = 5f; 
    [SerializeField] private float lookSensivity = 0.01f;

    private Vector3 _currentOffset;
    private bool _isLookingAround; 
    
    
    
    PlayerContext _ctx;
    //PlayerStateDriver _driver;

    public bool IsLookingAround() => _isLookingAround;

    public void Initialize() {
        _ctx = Services.Get<PlayerContext>();
        // _driver = driver;
        
        // if (target == null)
        //     target = _driver.transform;
    }

    private void Update()
    {
        // if (PauseMenu.isPaused || !player.isCurrentlyPlayable)
        //     return;

        if (target == null || _ctx == null) 
            return;
        
        bool canLookAround = Mathf.Approximately(_ctx.input.Move.x, 0f) && _ctx.grounded;
        
        _isLookingAround = canLookAround && _ctx.input.LookAroundHeld; 

        if (_isLookingAround)
        {
            Vector2 lookDelta = _ctx.input.LookPosition; 
            _currentOffset += new Vector3(lookDelta.x, lookDelta.y, 0f) * lookSensivity;
            _currentOffset = Vector3.ClampMagnitude(_currentOffset, lookRange);
        }
        else
        {
            _currentOffset = Vector3.Lerp(_currentOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        Vector3 desiredPos = targetPos + _currentOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * returnSpeed);
    }
    
}