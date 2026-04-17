using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public class MenuEntryPoint : SceneBootstrap
    {        
        protected override IEnumerator Bootstrap()
        {
            // Возвращаем курсор
            // Cursor.visible = true;

            if (Services.IsRegistered<SessionBootstrap>())
                Services.Get<SessionBootstrap>().EndSession();

            var task = _registry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            // if (task.IsFaulted)
            // {
            //     Debug.LogError($"[MenuEntryPoint] Service registry initialization failed: {task.Exception?.Flatten().InnerException?.Message}\n{task.Exception?.Flatten().InnerException?.StackTrace}", this);
            //     yield break;
            // }

            _logger?.Log("Menu scene services initialized", this);

            FindSceneObjects();
            InitializeSceneObjects();

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}