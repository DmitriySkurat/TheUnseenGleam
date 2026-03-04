using UnityEngine;
using System.Threading.Tasks;

public class AudioManager : MonoBehaviour, IService
{
    public async Task InitializeAsync()
    {
        // Здесь можно загрузить аудио ресурсы, настроить микшеры и т.д.
        await Task.CompletedTask; // Заглушка для асинхронной инициализации
    }
}