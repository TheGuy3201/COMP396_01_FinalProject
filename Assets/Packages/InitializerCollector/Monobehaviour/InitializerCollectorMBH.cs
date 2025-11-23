using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace InitializerCollector
{
    public interface IModule
    {
        void InitializeScript();
    }
    public class InitializerCollectorMBH : MonoBehaviour
    {
        [Header("Collected Initializers")]
        public List<UnityEvent> scriptExecutionOrder = new();

        private void Start()
        {
            foreach (UnityEvent script in scriptExecutionOrder)
            {
                script?.Invoke();
            }
        }
    }
}
