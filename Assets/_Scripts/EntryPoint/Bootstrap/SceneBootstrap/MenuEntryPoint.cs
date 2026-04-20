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
            {
                Services.Get<SessionBootstrap>().EndSession();
            }

            if (Services.IsRegistered<SessionBootstrap>())
                Services.Get<SessionBootstrap>().EndSession();

            var task = _registry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            _logger?.Log("Menu scene services initialized", this);

            FindObjects();
            InitializeObjects();

            Services.Get<InputManager>().EnableUI();

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}