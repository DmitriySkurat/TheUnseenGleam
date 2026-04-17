using UnityEngine;

namespace Utility
{
    public class DeleteOnPlay : MonoBehaviour, ISceneLifecycle
    {
        public InitializationOrder Order => InitializationOrder.GameplayCore;

        public void Initialize()
        {
            if (Application.isPlaying)
                Destroy(gameObject);
        }
        
        public void Dispose()
        {
            
        }
    }
}

