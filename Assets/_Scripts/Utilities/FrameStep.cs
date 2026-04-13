using UnityEngine;

namespace Utility
{
    public class FrameStep : MonoBehaviour
    {
        [SerializeField] private float timeScale = 0.1f;

        void Update()
        {
            Time.timeScale = timeScale;
        }
    }
}
