using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsefulClasses
{
    public static class VisibilityCheck
    {

        public static bool IsGameObjectVisible(Camera targetCamera, GameObject targetObject, bool showLogs=false)
        {
            if (targetCamera == null)
            {
                if (showLogs)
                {
                    Debug.LogError("Camera not found");
                }
                return false;
            }
            if (targetObject == null)
            {
                if (showLogs)
                {
                    Debug.LogError("Target object not found");
                }
                return false;
            }
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                if (showLogs)
                {
                    Debug.LogError("Renderer not found");
                }
                return false;
            }
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }


  
    }
}
