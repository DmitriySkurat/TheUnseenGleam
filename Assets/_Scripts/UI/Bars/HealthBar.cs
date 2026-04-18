using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;
    
    public Image healthBarFill;
    
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
            healthBarFill.fillAmount = _playerCtx.health.CurrentHealth / _playerCtx.stats.MaxPlayerHealth;
        }
    }
}
