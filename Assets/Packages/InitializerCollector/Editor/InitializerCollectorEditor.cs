using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace InitializerCollector
{
#if UNITY_EDITOR

    [CustomEditor(typeof(InitializerCollectorMBH))]
    #endif
    public class InitializerCollectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Collect Initializers"))
            {
                CollectInitializers((InitializerCollectorMBH)target);
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void CollectInitializers(InitializerCollectorMBH collector)
        {
            Undo.RecordObject(collector, "Collect Initializers");

            collector.scriptExecutionOrder.Clear();

            var modules = collector.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var mono in modules)
            {
                if (mono is IModule)
                {
                    UnityEvent newEvent = new UnityEvent();
                
                    UnityEventTools.AddPersistentListener(newEvent, (mono as IModule).InitializeScript);
                    collector.scriptExecutionOrder.Add(newEvent);
                }
            }

            EditorUtility.SetDirty(collector);
        }

        
    }
    
}
