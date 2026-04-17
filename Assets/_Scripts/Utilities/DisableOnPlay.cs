using UnityEngine;

namespace Utility 
{
    public class DisableOnPlay : MonoBehaviour, ISceneLifecycle
    {
        public InitializationOrder Order => InitializationOrder.GameplayCore;
        
        public void Initialize()
        {
            if (Application.isPlaying)
                gameObject.SetActive(false);
        }
        
        public void Dispose()
        {
            
        }
    }
}