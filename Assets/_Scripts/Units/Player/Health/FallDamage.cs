using UnityEngine;

public class FallDamage : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 20;
    
    [Header("Settings")]
    [SerializeField] private float minImpactSpeed = 10f;   // безопасная скорость
    [SerializeField] private float damageMultiplier = 2f;  // множитель урона
    [SerializeField] private float lethalSpeed = 25f;      // мгновенная смерть (опционально)

    private PlayerContext _ctx;
    private Rigidbody2D _rb;

    private float _maxFallSpeed;
    private bool _wasGrounded;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _rb = _ctx.rb;
    }

    private void Update()
    {
        bool grounded = _ctx.grounded;
        float yVel = _rb.linearVelocity.y;

        // 📉 В воздухе — накапливаем максимальную скорость падения
        if (!grounded && yVel < -0.1f)
        {
            _maxFallSpeed = Mathf.Max(_maxFallSpeed, Mathf.Abs(yVel));
        }

        // 🟢 МОМЕНТ ПРИЗЕМЛЕНИЯ (было в воздухе → стало на земле)
        if (!_wasGrounded && grounded)
        {
            ApplyFallDamage();
            _maxFallSpeed = 0f;
        }

        _wasGrounded = grounded;
    }

    private void ApplyFallDamage()
    {
        // мгновенная смерть
        if (_maxFallSpeed >= lethalSpeed)
        {
            _ctx.health.TakeDamage(9999f);
            Debug.Log($"💀 Lethal fall! Speed: {_maxFallSpeed:F1}");
            return;
        }

        // обычный урон
        if (_maxFallSpeed < minImpactSpeed) return;

        float damage = (_maxFallSpeed - minImpactSpeed) * damageMultiplier;

        _ctx.health.TakeDamage(damage);

        Debug.Log($"Fall damage: {damage:F1}, impact speed: {_maxFallSpeed:F1}");
    }
}