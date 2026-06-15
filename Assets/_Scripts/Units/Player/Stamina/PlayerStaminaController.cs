using UnityEngine;

public class PlayerStaminaController : MonoBehaviour, ISessionLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 5;

    private PlayerContext _ctx;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        
        _ctx = Services.Get<PlayerContext>();

        if (_ctx == null || _ctx.stats == null)
            return;

        if (_ctx.stamina <= 0f)
        {
            var pending = SaveVariables.PendingSave;
            _ctx.stamina = pending != null && pending.playerHealth > 0f
                ? pending.stamina
                : _ctx.stats.MaxStamina;
        }

        _ctx.currentStaminaDrainMultiplier = 0f;
    }
    
    public void Dispose()
    {
        
    }

    private void FixedUpdate()
    {
        //Debug.Log($"Current stamina: {_ctx.stamina:0F}");
        //Debug.Log($"currentStaminaDrainMultiplier: {_ctx.currentStaminaDrainMultiplier}");
        //Debug.Log($"currentStaminaBreathDrainMultiplier: {_ctx.currentStaminaBreathDrainMultiplier}");
        
        UpdateStamina(Time.fixedDeltaTime);
    }
    
    private void UpdateStamina(float deltaTime)
    {
        if (_ctx.isHoldingBreath) return;
        
        float consumption = _ctx.stats.StaminaDrainPerSecond * _ctx.currentStaminaDrainMultiplier * deltaTime;

        if (consumption > 0f)
        {
            _ctx.stamina = Mathf.Max(0f, _ctx.stamina - consumption);
        }
        
            RegenStamina(deltaTime);
        
    }
    
    private void RegenStamina(float deltaTime)
    {
        bool isMoving = Mathf.Abs(_ctx.velocity.x) > 0.1f;
        float multiplier = isMoving ? _ctx.stats.StaminaRegenMovingMultiplier : 1f;
        if (_ctx.IsAdrenalineActive)
            multiplier *= _ctx.stats.AdrenalineRegenMultiplier;
        float regeneration = _ctx.stats.StaminaRegenPerSecond * multiplier * deltaTime;

        _ctx.stamina = Mathf.Min(_ctx.stats.MaxStamina, _ctx.stamina + regeneration);
    }
}
