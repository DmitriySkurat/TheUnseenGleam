using System.Collections.Generic;
using UnityEngine;

public class NoiseVisualizer : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.UI;
    
    [Header("Setup")]
    [SerializeField] private NoiseWave wavePrefab;
    [SerializeField] private NoiseSystem _noiseSystem;


    [Header("Wave Settings")]
    [SerializeField, Min(0.01f)] private float duration = 1f;
    [SerializeField, Min(0f)] private float minInterval = 0.15f;

    [Header("Limits")]
    [SerializeField] private int maxSimultaneousWaves = 32;
    

    private float _lastSpawnTime;
    private readonly Queue<NoiseWave> _activeWaves = new();

    public void Initialize()
    {
        _noiseSystem = Services.Get<NoiseSystem>();

        _noiseSystem.NoiseEmitted += OnNoiseEmitted;
    }

    private void OnDisable()
    {
        _noiseSystem.NoiseEmitted -= OnNoiseEmitted;
    }

    private void OnNoiseEmitted(NoiseEvent noise)
    {
        // Ограничение по частоте
        if (Time.time - _lastSpawnTime < minInterval)
            return;

        _lastSpawnTime = Time.time;

        SpawnWave(noise);
    }

    private void SpawnWave(NoiseEvent noise)
    {
        // Ограничение количества
        if (_activeWaves.Count >= maxSimultaneousWaves)
        {
            var oldWave = _activeWaves.Dequeue();
            if (oldWave != null)
                Destroy(oldWave.gameObject);
        }

        var wave = Instantiate(
            wavePrefab,
            noise.Position,
            Quaternion.identity,
            transform
        );

        wave.Init(noise.Position, noise.Radius, duration);

        _activeWaves.Enqueue(wave);

        // авто-удаление из очереди
        StartCoroutine(RemoveAfterLifetime(wave, duration));
    }

    private System.Collections.IEnumerator RemoveAfterLifetime(NoiseWave wave, float time)
    {
        yield return new WaitForSeconds(time);

        if (_activeWaves.Contains(wave))
        {
            // удаляем конкретно этот объект
            var temp = new Queue<NoiseWave>();

            while (_activeWaves.Count > 0)
            {
                var w = _activeWaves.Dequeue();
                if (w != wave)
                    temp.Enqueue(w);
            }

            while (temp.Count > 0)
                _activeWaves.Enqueue(temp.Dequeue());
        }
    }
}

