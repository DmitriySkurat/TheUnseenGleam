using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameServiceRegistry : MonoBehaviour
{
    [SerializeField] private InputManager inputPrefab;
    [SerializeField] private AudioManager audioPrefab;
    [SerializeField] private SaveSystem savePrefab;
    [SerializeField] private NoiseSystem noisePrefab;

    public async Task InitializeAsync()
    {
        DontDestroyOnLoad(gameObject);

        var services = new List<IService>();

        var input = Instantiate(inputPrefab);
        var audio = Instantiate(audioPrefab);
        var save = Instantiate(savePrefab);
        var noise = Instantiate(noisePrefab);

        DontDestroyOnLoad(input.gameObject);
        DontDestroyOnLoad(audio.gameObject);
        DontDestroyOnLoad(save.gameObject);
        DontDestroyOnLoad(noise.gameObject);

        Services.Register(input);
        Services.Register(audio);
        Services.Register(save);
        Services.Register(noise);

        services.Add(input);
        services.Add(audio);
        services.Add(save);
        services.Add(noise);
        
        // Параллельная инициализация
        await Task.WhenAll(services.Select(s => s.InitializeAsync()));
    }
}