using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public class MenuEntryPoint : SceneBootstrap
    {
        [SerializeField] private AudioClip _menuMusic;

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

            _logger?.Log($"_menuMusic={((_menuMusic != null) ? _menuMusic.name : "NULL")}, AudioManager registered={Services.IsRegistered<AudioManager>()}", this);
            if (_menuMusic != null && Services.IsRegistered<AudioManager>())
                Services.Get<AudioManager>().PlayMusic(_menuMusic);

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}