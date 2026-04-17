using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SessionServiceRegistry : MonoBehaviour
{
    // Добавляй сюда [SerializeField] префабы сессионных сервисов
    // Аналог GameplaySceneServiceRegistry, но объекты живут между сценами

    private readonly List<GameObject> _sessionObjects = new();

    public async Task InitializeAsync()
    {
        // Пример регистрации сессионного сервиса:
        // var obj = Instantiate(somePrefab);
        // DontDestroyOnLoad(obj);
        // Services.Register(obj.GetComponent<SomeService>());
        // _sessionObjects.Add(obj);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // Пример отмены регистрации:
        // Services.Unregister<SomeService>();

        foreach (var obj in _sessionObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        _sessionObjects.Clear();
    }
}
