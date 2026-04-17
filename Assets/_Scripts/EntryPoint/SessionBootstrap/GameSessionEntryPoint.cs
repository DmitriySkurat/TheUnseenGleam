using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionBootstrap : MonoBehaviour
{
    private SessionServiceRegistry _registry;
    

    private void Awake()
    {
        _registry = GetComponent<SessionServiceRegistry>();
    }

    private void OnDestroy()
    {
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
        Services.Unregister<SessionBootstrap>();
        Destroy(gameObject);
    }
}
