using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 10;
    private PlayerContext _ctx;
    private float _currentHealth;
    
    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        
        _currentHealth = _ctx.stats.MaxPlayerHealth;
        
        _ctx.health = this;
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        
        Debug.Log($"Current Health: {_currentHealth}");
        
        if(_currentHealth < 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        Debug.Log("Player died");
    }
}