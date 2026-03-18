using UnityEngine;

namespace Utility 
{
    public class DisableOnPlay : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying)
                gameObject.SetActive(false);
        }
    }
}