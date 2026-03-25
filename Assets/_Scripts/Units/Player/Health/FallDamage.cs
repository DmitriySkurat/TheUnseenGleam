using UnityEngine;

public class FallDamage : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 20;

    private PlayerContext _ctx;
    private Rigidbody2D _rb;

    private float _maxHeightY;      // максимальная высота достигнутая во время падения
    private float _airborneStartY;  // высота в начале полета
    private bool _wasGrounded;
    private bool _isTracking;       // след ли мы активно за падением

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        _rb = _ctx.rb;
        
        if (_ctx != null && _ctx.transform != null)
        {
            _maxHeightY = _ctx.transform.position.y;
        }
    }

    private void Update()
    {
        if (_ctx == null || _ctx.transform == null) return;

        bool grounded = _ctx.grounded;
        float currentY = _ctx.transform.position.y;

        if (_wasGrounded && !grounded)
        {
            _airborneStartY = currentY;
            _maxHeightY = currentY;
            _isTracking = true;
        }

        if (_isTracking && !grounded && currentY > _maxHeightY)
        {
            _maxHeightY = currentY;
        }

        // 🟢 МОМЕНТ ПРИЗЕМЛЕНИЯ (было в воздухе → стало на земле)
        if (!_wasGrounded && grounded && _isTracking)
        {
            ApplyFallDamage(currentY);
            _isTracking = false;
        }

        _wasGrounded = grounded;
    }

    private void ApplyFallDamage(float landingY)
    {
        if (_ctx == null || _ctx.stats == null)
            return;

        float fallHeight = Mathf.Max(0f, _maxHeightY - landingY);

        // Игнорируем малые падения
        if (fallHeight < _ctx.stats.FallDamageMinHeight)
        {
            Debug.Log($"Fall too short: {fallHeight:F2}м (требуется {_ctx.stats.FallDamageMinHeight:F2}м)");
            return;
        }

        // Проверяем, выполнил ли игрок кувырок при приземлении
        bool didPerformRoll = _ctx.IsLandingRollActive;

        // Мгновенная смерть при очень высоком падении
        if (fallHeight >= _ctx.stats.FallDamageLethalHeight)
        {
            _ctx.health.TakeDamage(9999f);
            Debug.Log($"💀 Летальное падение! Высота: {fallHeight:F2}м");
            return;
        }

        // Вычисляем базовый урон
        float damage = (fallHeight - _ctx.stats.FallDamageMinHeight) * _ctx.stats.FallDamagePerUnit;

        // Если игрок выполнил кувырок, урон значительно меньше
        if (didPerformRoll)
        {
            damage *= _ctx.stats.FallDamageRollMultiplier;
            Debug.Log($"✅ Кувырок выполнен! Урон падения: {damage:F1} (высота {fallHeight:F2}м, урон без кувырка был бы {damage / _ctx.stats.FallDamageRollMultiplier:F1})");
        }
        else
        {
            Debug.Log($"❌ Кувырок НЕ выполнен! Урон падения: {damage:F1} (высота {fallHeight:F2}м)");
        }

        _ctx.health.TakeDamage(damage);
    }
}