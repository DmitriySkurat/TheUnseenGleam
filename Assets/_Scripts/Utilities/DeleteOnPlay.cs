using UnityEngine;

namespace Utility
{
    public class DeleteOnPlay : MonoBehaviour, IInitializable
    {
        public InitializationOrder Order => InitializationOrder.GameplayCore;

        public void Initialize()
        {
            if (Application.isPlaying)
                Destroy(gameObject);
        }
    }
}

