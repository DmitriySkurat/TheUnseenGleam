using UnityEngine;

public class PlayerStaminaController : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player + 5;

    private PlayerContext _ctx;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        if (_ctx == null || _ctx.stats == null)
            return;

        _ctx.stamina = _ctx.stats.MaxStamina;
        _ctx.currentStaminaDrainMultiplier = 0;
    }

    private void FixedUpdate()
    {
        Debug.Log($"currentStaminaDrainMultiplier: {_ctx.stamina:0F}");
        Debug.Log($"currentStaminaDrainMultiplier: {_ctx.currentStaminaDrainMultiplier}");
        Debug.Log($"currentStaminaBreathDrainMultiplier: {_ctx.currentStaminaBreathDrainMultiplier}");
        UpdateStamina();
    }
    
    private void UpdateStamina()
    {
        float consumption = _ctx.stats.StaminaDrainPerSecond * _ctx.currentStaminaDrainMultiplier * Time.deltaTime;

        if (consumption > 0f)
        {
            _ctx.stamina = Mathf.Max(0f, _ctx.stamina - consumption);
        }
        else
        {
            float regeneration = _ctx.stats.StaminaRegenPerSecond * Time.deltaTime;
            
            _ctx.stamina = Mathf.Min(_ctx.stats.MaxStamina, _ctx.stamina + regeneration);
        }
    }
}
