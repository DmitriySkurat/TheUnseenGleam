using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public class MenuEntryPoint : SceneBootstrap
    {
        [SerializeField] private MenuSceneServiceRegistry _menuServiceRegistry;
        
        protected override IEnumerator Bootstrap()
        {    
            // Скрываем курсор для погружения в игру
            //Cursor.visible = true;
            
            //EnsureEventSystem();

            var task = _menuServiceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;
            
            _logger?.Log("Menu scene services initialized", this);
            
            FindInializableObjects();
            InitializeSceneObjects();

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}