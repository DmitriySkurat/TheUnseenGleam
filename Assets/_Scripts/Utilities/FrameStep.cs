using UnityEngine;

namespace Utility
{
    public class FrameStep : MonoBehaviour
    {
        void Start()
        {
            Time.timeScale = 0f; // стоп
        }

        void Update()
        {
            StepFrame();
        }

        void StepFrame()
        {
            StartCoroutine(Step());
        }

        System.Collections.IEnumerator Step()
        {
            Time.timeScale = 0.1f;
            yield return null;
        }
    }
}
