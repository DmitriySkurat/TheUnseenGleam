using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, ISessionLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 10;
    private PlayerContext _ctx;
    private float _currentHealth;
    private float _regenDelayTimer;

    public float CurrentHealth => _currentHealth;
    public event Action OnDied;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();

        if (_ctx.currentHealth > 0f)
            _currentHealth = _ctx.currentHealth;
        else
        {
            var pending = SaveVariables.PendingSave;
            _currentHealth = pending != null && pending.playerHealth > 0f
                ? pending.playerHealth
                : _ctx.stats.MaxPlayerHealth;
        }
        _regenDelayTimer = 0f;
        _ctx.isAlive = true;
        _ctx.health = this;
    }

    public void Dispose()
    {
        _ctx.currentHealth = _currentHealth;
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

        //Debug.Log($"Current Health: {_currentHealth}");

        if (_currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (!_ctx.isAlive)
            return;

        _ctx.isAlive = false;
        _ctx.diedWhileGrabbed = _ctx.isGrabbed;
        Debug.Log("Player died");
        OnDied?.Invoke();
    }
}
