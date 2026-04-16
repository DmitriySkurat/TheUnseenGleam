using UnityEngine;

public class DeathMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private GameObject deathGameMenu;

    private PlayerHealth _playerHealth;

    public void Initialize()
    {
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
        Time.timeScale = 0f;
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        deathGameMenu.SetActive(false);
        Utility.SceneLoader.Load(Utility.SceneNames.Demo);
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        Utility.SceneLoader.Load(Utility.SceneNames.Menu);
    }
}
