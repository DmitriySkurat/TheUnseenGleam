using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class MenuSceneServiceRegistry : SceneServiceRegistry
{
    //[SerializeField] private CameraFollow cameraPrefab;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }
}
