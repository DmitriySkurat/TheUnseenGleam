using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public abstract class ServiceRegistry : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected Utility.Logger _logger;

    protected readonly List<IService> _services = new();

    public virtual async Task InitializeAsync()
    {
        _logger.Log($"{GetType().Name} Initialize()", this);
        await Task.WhenAll(_services.Select(s => s.InitializeAsync()));
    }

    public virtual void Dispose() { }

    private void OnDestroy()
    {
        Dispose();
    }
}