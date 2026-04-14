using UnityEngine;

public class PlayerHealth : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 10;
    private PlayerContext _ctx;
    private float _currentHealth;
    
    public float CurrentHealth => _currentHealth;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        
        _currentHealth = _ctx.stats.MaxPlayerHealth;
        _ctx.isAlive = true;
        
        _ctx.health = this;
    }
    
    public void TakeDamage(float damage)
    {
        if (!_ctx.isAlive || damage <= 0f)
            return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        
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
