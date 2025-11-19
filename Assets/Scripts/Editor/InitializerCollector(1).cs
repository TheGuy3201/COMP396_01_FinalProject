using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif


public interface IModule
{
    void InitializeScript();
}

public class InitializerCollector : MonoBehaviour
{
    [Header("Collected Initializers")] public List<UnityEvent> scriptExecutionOrder = new();

    private void Start()
    {
        foreach (var script in scriptExecutionOrder) script?.Invoke();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(InitializerCollector))]
public class InitializerCollectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Collect Initializers")) CollectInitializers((InitializerCollector)target);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void CollectInitializers(InitializerCollector collector)
    {
        Undo.RecordObject(collector, "Collect Initializers");

        collector.scriptExecutionOrder.Clear();

        var modules = collector.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var mono in modules)
            if (mono is IModule)
            {
                var newEvent = new UnityEvent();

                UnityEventTools.AddPersistentListener(newEvent, (mono as IModule).InitializeScript);
                collector.scriptExecutionOrder.Add(newEvent);
            }

        EditorUtility.SetDirty(collector);
    }
}
#endif