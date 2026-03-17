using UnityEngine;

namespace EntryPoint
{
    public abstract class SceneBootstrap : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Bootstrap();
        }
        
        protected abstract void Bootstrap();
    }
}