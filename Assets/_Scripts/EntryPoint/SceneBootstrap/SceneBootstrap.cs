using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public abstract class SceneBootstrap : MonoBehaviour
    {
        protected virtual void Awake()
        {
            StartCoroutine(Bootstrap());
        }
        
        protected abstract IEnumerator Bootstrap();
    }
}