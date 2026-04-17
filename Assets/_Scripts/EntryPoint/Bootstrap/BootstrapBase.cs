using System.Collections;
using UnityEngine;

namespace EntryPoint
{
   public abstract class BootstrapBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            StartCoroutine(Bootstrap());
        }

        protected abstract IEnumerator Bootstrap();
    } 
}
