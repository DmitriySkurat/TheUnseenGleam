using UnityEngine;

namespace EntryPoint
{
    public abstract class SceneBootstrap : MonoBehaviour
    {
        protected virtual void Start()
        {
            Bootstrap();
        }
        
        protected abstract void Bootstrap();
    }
}