using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameServiceRegistry : MonoBehaviour
{
    [SerializeField] private InputManager inputPrefab;
    [SerializeField] private AudioManager audioPrefab;
    [SerializeField] private SaveSystem savePrefab;

    public async Task InitializeAsync()
    {
        DontDestroyOnLoad(gameObject);

        var services = new List<IService>();

        var input = Instantiate(inputPrefab);
        var audio = Instantiate(audioPrefab);
        var save = Instantiate(savePrefab);

        DontDestroyOnLoad(input.gameObject);
        DontDestroyOnLoad(audio.gameObject);
        DontDestroyOnLoad(save.gameObject);

        Services.Register(input);
        Services.Register(audio);
        Services.Register(save);

        services.Add(input);
        services.Add(audio);
        services.Add(save);

        // Параллельная инициализация
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
}