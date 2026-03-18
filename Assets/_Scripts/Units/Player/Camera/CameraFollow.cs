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

    public bool IsLookingAround() => _isLookingAround;

    public void Initialize() {
        _ctx = Services.Get<PlayerContext>();
        
        target = _ctx.transform;
    }

    private void Update()
    {
        if (target == null || _ctx == null) 
            return;
        
        bool canLookAround = Mathf.Approximately(_ctx.input.Move.x, 0f) && _ctx.grounded;
        
        _isLookingAround = canLookAround && _ctx.input.LookAroundHeld; 

        if (_isLookingAround)
        {
            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 mouse = _ctx.input.MousePosition;
                Vector2 viewport = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);
                Vector2 centered = (viewport - new Vector2(0.5f, 0.5f)) * 2f; // -1..1
                Vector3 desiredOffset = new Vector3(centered.x, centered.y, 0f) * lookRange;
                float lookSpeed = lookSensivity < 1f ? lookSensivity * 100f : lookSensivity;
                _currentOffset = Vector3.Lerp(_currentOffset, desiredOffset, Time.deltaTime * lookSpeed);
            }
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
