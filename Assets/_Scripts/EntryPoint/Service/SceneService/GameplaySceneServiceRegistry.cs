using UnityEngine;
using System.Threading.Tasks;


public class GameplaySceneServiceRegistry : SceneServiceRegistry
{
    [SerializeField] private PlayerScriptableStats _stats;
    [SerializeField] private NoiseScriptableStats _noiseStats;

    public override async Task InitializeAsync()
    {
        Services.Register(new PlayerContext { stats = _stats, noiseStats = _noiseStats });
        await base.InitializeAsync();
    }

    public override void Dispose()
    {
        Services.Unregister<PlayerContext>();
        base.Dispose();
    }
}
