//using Unity.VisualScripting;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Utility
{
    public class Logger : MonoBehaviour 
    {
        [Header("Settings")]
        [SerializeField] private bool _showLogs = true;
        [SerializeField] private string _prefix;
        [SerializeField] private Color _prefixColor;
        
        private string _hexColor;

        void OnValidate()
        {
            _hexColor = "#"+ColorUtility.ToHtmlStringRGB(_prefixColor);
        }

        public void Log(object message, Object sender)
        {
            if (_showLogs) {
                Debug.Log($"<color={_hexColor}>{_prefix}: {message}</color>", sender);
            }
        }
    }
}
