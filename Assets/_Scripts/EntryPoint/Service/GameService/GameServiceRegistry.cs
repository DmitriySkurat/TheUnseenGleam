using System.Threading.Tasks;
using UnityEngine;

public class GameServiceRegistry : ServiceRegistry
{
    [SerializeField] private InputManager inputPrefab;
    [SerializeField] private AudioManager audioPrefab;
    [SerializeField] private SceneTransitionManager sceneTransitionManagerPrefab;
    //[SerializeField] private SaveSystem savePrefab;

    public override async Task InitializeAsync()
    {
        var input = Instantiate(inputPrefab);
        var audio = Instantiate(audioPrefab);
        var sceneTransitionManager = Instantiate(sceneTransitionManagerPrefab);
        //var save = Instantiate(savePrefab);

        DontDestroyOnLoad(input.gameObject);
        DontDestroyOnLoad(audio.gameObject);
        DontDestroyOnLoad(sceneTransitionManager.gameObject);
        //DontDestroyOnLoad(save.gameObject);

        Services.Register(input);
        Services.Register(audio);
        Services.Register(sceneTransitionManager);
        Services.Register(new PlayerPersistentState());
        //Services.Register(save);

        _services.Add(input);
        _services.Add(audio);
        _services.Add(sceneTransitionManager);
        //services.Add(save);

        await base.InitializeAsync();
    }
}