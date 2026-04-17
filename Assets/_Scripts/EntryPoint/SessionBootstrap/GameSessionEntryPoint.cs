using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameSessionEntryPoint : MonoBehaviour, IService
{
    private SessionServiceRegistry _registry;

    private void Awake()
    {
        _registry = GetComponent<SessionServiceRegistry>();
    }

    public async Task InitializeAsync()
    {
        if (_registry != null)
            await _registry.InitializeAsync();
    }

    public void End()
    {
        _registry?.Dispose();
        Services.Unregister<GameSessionEntryPoint>();
        Destroy(gameObject);
    }
}
