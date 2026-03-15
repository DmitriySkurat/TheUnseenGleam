using UnityEngine;
using System.Threading.Tasks;

public class SaveSystem : MonoBehaviour, IService
{
    public async Task InitializeAsync()
    {
        // Здесь можно загрузить сохраненные данные, настроить систему сохранения и т.д.
        await Task.CompletedTask; // Заглушка для асинхронной инициализации
    }
}