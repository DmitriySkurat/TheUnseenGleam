using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace EntryPoint
{
    public class GameplayEntryPoint : SceneBootstrap
    {

        protected override IEnumerator Bootstrap()
        {
            // Скрываем курсор для погружения в игру (не забыть включать при паузе) - сделать отдельный класс
            // Cursor.visible = true;
            
            var task = _serviceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            // if (task.IsFaulted)
            // {
            //     Debug.LogError($"[GameplayEntryPoint] Service registry initialization failed: {task.Exception?.Flatten().InnerException?.Message}\n{task.Exception?.Flatten().InnerException?.StackTrace}", this);
            //     yield break;
            // }

            FindSceneObjects();
            InitializeSceneObjects();

            _logger?.Log("Gameplay scene initialization complete", this);
        }
        
    }
}
