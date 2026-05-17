using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    public Image staminaBarFill;

    [SerializeField] private Color _flashColor = Color.red;
    [SerializeField] private float _flashInterval = 0.15f;

    private PlayerContext _playerCtx;
    private Color _normalColor;
    private float _flashTimer;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _playerCtx = Services.Get<PlayerContext>();
        _normalColor = staminaBarFill.color;
    }

    public void Dispose()
    {
        if (staminaBarFill != null)
            staminaBarFill.color = _normalColor;
    }

    private void Update()
    {
        if (_playerCtx == null) return;

        staminaBarFill.fillAmount = _playerCtx.stamina / _playerCtx.stats.MaxStamina;

        bool insufficientForRun    = _playerCtx.input.RunHeld
            && !_playerCtx.CanRun
            && _playerCtx.currentStaminaDrainMultiplier == 0f;
        bool insufficientForBreath = _playerCtx.input.HoldBreathHeld
            && !_playerCtx.isHoldingBreath
            && _playerCtx.CanHoldBreath
            && _playerCtx.stamina < _playerCtx.stats.MinStaminaToHoldBreath;
        bool insufficientForJump   = _playerCtx.input.JumpHeld && !_playerCtx.CanJump && _playerCtx.grounded;

        if (insufficientForRun || insufficientForBreath || insufficientForJump)
        {
            _flashTimer += Time.deltaTime;
            bool showFlash = (int)(_flashTimer / _flashInterval) % 2 == 0;
            staminaBarFill.color = showFlash ? _flashColor : _normalColor;
        }
        else
        {
            _flashTimer = 0f;
            staminaBarFill.color = _normalColor;
        }
    }
}
