using UnityEngine;

public class PlayerHealth : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 10;
    private PlayerContext _ctx;
    private float _currentHealth;
    private float _regenDelayTimer;

    public float CurrentHealth => _currentHealth;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        _currentHealth = _ctx.stats.MaxPlayerHealth;
        _regenDelayTimer = 0f;
        _ctx.isAlive = true;

        _ctx.health = this;
    }

    private void Update()
    {
        HandleRegen();
    }

    private void HandleRegen()
    {
        if (!_ctx.isAlive || _currentHealth >= _ctx.stats.MaxPlayerHealth)
            return;

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        _currentHealth = Mathf.Min(_ctx.stats.MaxPlayerHealth, _currentHealth + _ctx.stats.HealthRegenPerSecond * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (!_ctx.isAlive || damage <= 0f)
            return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        _regenDelayTimer = _ctx.stats.HealthRegenDelay;

        Debug.Log($"Current Health: {_currentHealth}");

        if (_currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (!_ctx.isAlive)
            return;

        _ctx.isAlive = false;
        Debug.Log("Player died");
    }
}
