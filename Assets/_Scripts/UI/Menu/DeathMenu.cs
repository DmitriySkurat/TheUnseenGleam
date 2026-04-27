using UnityEngine;
using UnityEngine.SceneManagement;
using EntryPoint;

public class DeathMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private GameObject deathGameMenu;

    private PlayerHealth _playerHealth;
    private PlayerContext _playerCtx;

    private InputManager _inputManager;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _inputManager = Services.Get<InputManager>();

        _playerCtx = Services.Get<PlayerContext>();
        _playerHealth = _playerCtx.health;
        _playerHealth.OnDied += Show;

        deathGameMenu.SetActive(false);
    }

    public void Dispose()
    {
        if (_playerHealth != null)
            _playerHealth.OnDied -= Show;
    }

    private void Show()
    {
        if (_playerCtx != null && _playerCtx.diedWhileGrabbed)
            return;

        ShowNow();
    }

    public void ShowNow()
    {
        deathGameMenu.SetActive(true);
        _inputManager.EnableUI();
        //Time.timeScale = 0f;
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        deathGameMenu.SetActive(false);
        if (Services.IsRegistered<SessionBootstrap>())
            Services.Get<SessionBootstrap>().EndSession();
        var sceneName = SaveVariables.PendingSave?.sceneName ?? SceneManager.GetActiveScene().name;
        Utility.SceneLoader.Load(sceneName);
        
        _inputManager.EnableGameplay();
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        Utility.SceneLoader.Load(SceneNames.Menu);
    }
}
