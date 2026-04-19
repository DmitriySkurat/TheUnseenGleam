using UnityEngine;
using EntryPoint;

public class DeathMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private GameObject deathGameMenu;

    private PlayerHealth _playerHealth;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _playerHealth = Services.Get<PlayerContext>().health;
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
        deathGameMenu.SetActive(true);
        //Time.timeScale = 0f;
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        deathGameMenu.SetActive(false);
        if (Services.IsRegistered<SessionBootstrap>())
            Services.Get<SessionBootstrap>().EndSession();
        Utility.SceneLoader.Load(SceneNames.Demo);
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        Utility.SceneLoader.Load(SceneNames.Menu);
    }
}
