using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimAndThrow : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 1;
    
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;
    
    private GameObject bulletInst;
    
    private Vector2 worldPosition;
    private Vector2 direction;
    
    private PlayerContext _ctx;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
    }
    
    
    private void LateUpdate()
    {
        HandleGunRotation();
        HandleGunShooting();
    }
    
    // private void HandleGunRotation()
    // {
    //     worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    //     direction = (worldPosition - (Vector2)gun.transform.position).normalized;
    //     gun.transform.right = direction;
    // }
    
    private void HandleGunRotation()
    {
        if (_ctx == null || gun == null) return;
        
        if (!_ctx.input.AttackHeld) return;

        var camera = Camera.main;
        if (camera == null) return;

        var screenPos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : _ctx.input.MousePosition;

        var camZ = -camera.transform.position.z;
        worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));
        direction = (worldPosition - (Vector2)gun.transform.position).normalized;

        if (direction.sqrMagnitude > 0.0001f)
            gun.transform.right = direction;
    }
    
    private void HandleGunShooting()
    {
        if (_ctx == null) return;
        
        if (_ctx.input.AttackHeld)
        {
            bulletInst = Instantiate(bullet, bulletSpawnPoint.position, gun.transform.rotation);
        }
    }
}
