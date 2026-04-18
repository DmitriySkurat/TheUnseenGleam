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

            if (Services.IsRegistered<GameplayEntryPoint>())
            {
                Services.Get<GameplayEntryPoint>().DisposeObjects();
                Services.Unregister<GameplayEntryPoint>();
            }

            if (Services.IsRegistered<SessionBootstrap>())
                Services.Get<SessionBootstrap>().EndSession();

            var task = _registry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            _logger?.Log("Menu scene services initialized", this);

            FindObjects();
            InitializeObjects();

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}