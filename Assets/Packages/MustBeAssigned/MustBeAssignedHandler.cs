using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class MustBeAssignedHandler
{

#if UNITY_EDITOR
    #region MustBeAssignedProperty
    [InitializeOnLoadMethod]
    public static void InitializeValidation()
    {
        // Hook into Play mode state changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!ValidateMustBeAssignedFields())
            {
                Debug.LogError("Play mode stopped due to unassigned required fields.");
                EditorApplication.isPlaying = false; // Stop Play mode
            }
        }
    }

    private static bool ValidateMustBeAssignedFields()

    {
        bool isValid = true;

        // Iterate through all active GameObjects in the scene
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (component == null) continue;

                // Use reflection to inspect fields
                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    // Check for the MustBeAssignedAttribute
                    if (field.GetCustomAttribute(typeof(MustBeAssignedAttribute)) != null)
                    {
                        object value = field.GetValue(component);
                        if (value == null || value.Equals(null))
                        {
                            Debug.LogError($"[UtilityHelper] {component.GetType().Name} on '{obj.name}' has unassigned field '{field.Name}'.");
                            isValid = false; // Validation failed
                        }
                    }
                }
            }
        }

        return isValid;
    }

    #endregion
#endif
}

// Attribute: Marks fields as required (must be assigned in the Inspector)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MustBeAssignedAttribute : PropertyAttribute { }