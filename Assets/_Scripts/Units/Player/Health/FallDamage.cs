using UnityEngine;

public class FallDamage : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 20;
    
    [Header("Settings")]
    [SerializeField] private float minFallHeight = 3f;
    [SerializeField] private float damageMultiplier = 10f;
    
    private PlayerContext _ctx;
    private Rigidbody2D _rb;
    
    private float _fallStartY;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _rb = _ctx.rb;
    }

    private void Update()
    {
        // Начало падения
        if (_rb.linearVelocity.y < -0.1f && !_ctx.isFalling)
        {
            _ctx.isFalling = true;
            _fallStartY = transform.position.y;
        }

        // Приземление
        if (_ctx.isFalling && Mathf.Abs(_rb.linearVelocity.y) < 0.01f)
        {
            float fallDistance = _fallStartY - transform.position.y;

            if (fallDistance > minFallHeight)
            {
                float damage = (fallDistance - minFallHeight) * damageMultiplier;
                _ctx.health.TakeDamage(damage);
            }

            _ctx.isFalling = false;
        }
    }
}