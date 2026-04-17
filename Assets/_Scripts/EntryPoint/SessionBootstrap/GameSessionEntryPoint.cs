using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionBootstrap : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected Utility.Logger _logger;
    
            
    private SessionServiceRegistry _registry;
    
    
    protected virtual void Awake()
        {
            StartCoroutine(Bootstrap());
        }

    protected IEnumerator Bootstrap()
    {
        yield return null;
    }

    private void Dispose()
    {
        _registry?.Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
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
