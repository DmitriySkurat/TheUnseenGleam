using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;
    
    public Image staminaBarFill;
    
    private PlayerContext _playerCtx;
    
    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
            
        _playerCtx = Services.Get<PlayerContext>();
    }
    
    public void Dispose()
    {
        
    }

    private void Update()
    {
        if (_playerCtx != null)
        {
            staminaBarFill.fillAmount = _playerCtx.stamina / _playerCtx.stats.MaxStamina;
        }
    }
}
