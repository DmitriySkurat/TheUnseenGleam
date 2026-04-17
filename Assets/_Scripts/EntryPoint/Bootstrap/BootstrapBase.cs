using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public abstract class BootstrapBase : MonoBehaviour
    {
        [Header("Registry")]
        [SerializeField] protected ServiceRegistry _registry;

        protected virtual void Awake()
        {
            StartCoroutine(Bootstrap());
        }

        protected abstract IEnumerator Bootstrap();
    }
}
