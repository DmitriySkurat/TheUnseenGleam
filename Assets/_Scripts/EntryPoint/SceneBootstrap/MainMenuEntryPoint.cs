using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public class MainMenuEntryPoint : SceneBootstrap
    {
        protected override IEnumerator Bootstrap()
        {
            Cursor.visible = true; // Показываем курсор для взаимодействия с меню
            // Здесь можно инициализировать синглтоны, если они есть, и делать другие вещи, которые должны произойти при загрузке сцены
            // Например, можно загрузить сохраненные данные игрока и передать их в соответствующие системы
            yield return null;
        }
    }
}