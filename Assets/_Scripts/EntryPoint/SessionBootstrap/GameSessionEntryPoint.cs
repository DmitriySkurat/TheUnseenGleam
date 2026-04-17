using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionEntryPoint : MonoBehaviour, IService
{
    private SessionServiceRegistry _registry;
    private bool _isEnding;

    private void Awake()
    {
        _registry = GetComponent<SessionServiceRegistry>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isEnding = false;
    }

    public async Task InitializeAsync()
    {
        if (_registry != null)
            await _registry.InitializeAsync();
    }

    // Завершение сессии при выходе в меню
    public void End()
    {
        _registry?.Dispose();
        Services.Unregister<GameSessionEntryPoint>();
        Destroy(gameObject);
    }
}
