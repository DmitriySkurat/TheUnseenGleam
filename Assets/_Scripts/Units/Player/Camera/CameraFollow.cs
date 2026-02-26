using UnityEngine;
using HSM;

public class CameraFollow : MonoBehaviour, IPlayerComponent
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    [SerializeField] private float cameraMoveRange = 3f; // How far the camera can offset
    [SerializeField] private float smoothSpeed = 5f;     // Smoothing speed

    private Vector3 mouseOffset; // Current camera offset
    private bool isLookingAround; // Is "look around" active
    private Vector2 screenCenter; // Cache screen center for performance
    
    PlayerContext _ctx;
    PlayerStateDriver _driver;


    public void Initialize(PlayerContext context, PlayerStateDriver driver) {
        _ctx = context;
        _driver = driver;
        
        if (target == null)
            target = _driver.transform;
    }

    private void Start()
    {
        UpdateScreenCenter();
    }

    private void Update()
    {
        // if (PauseMenu.isPaused || !player.isCurrentlyPlayable)
        //     return;

        if (target == null || _ctx == null) 
            return;

        // Update screen center if resolution changes
        if (screenCenter.x != Screen.width * 0.5f || screenCenter.y != Screen.height * 0.5f)
        {
            UpdateScreenCenter();
        }

        // Check if look-around is possible
        bool canLookAround = Mathf.Approximately(_ctx.input.Move.x, 0f) && _ctx.grounded;

        // Handle look-around state
        if (isLookingAround)
        {
            if (!canLookAround || !Input.GetMouseButton(1))
            {
                isLookingAround = false;
            }
        }
        else
        {
            if (canLookAround && Input.GetMouseButtonDown(1))
            {
                isLookingAround = true;
            }
        }

        // Handle offset
        if (isLookingAround)
        {
            // Get cursor offset from screen center
            Vector2 mousePos = Input.mousePosition;
            float deltaX = (mousePos.x - screenCenter.x) / screenCenter.x;
            float deltaY = (mousePos.y - screenCenter.y) / screenCenter.y;
            Vector2 delta = new Vector2(deltaX, deltaY);

            // Clamp and scale the offset
            delta = Vector2.ClampMagnitude(delta, 1f);
            mouseOffset = new Vector3(delta.x, delta.y, 0f) * cameraMoveRange;
        }
        else
        {
            // Smoothly return to center when not looking around
            mouseOffset = Vector3.Lerp(mouseOffset, Vector3.zero, Time.deltaTime * smoothSpeed);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        Vector3 desiredPos = targetPos + mouseOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
    }

    private void UpdateScreenCenter()
    {
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    public void SetTarget(Transform targetToSet)
    {
        target = targetToSet;
    }

    public bool IsLookingAround() => isLookingAround;
}