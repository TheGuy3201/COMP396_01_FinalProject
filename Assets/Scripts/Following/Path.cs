using UnityEngine;
public class Path : MonoBehaviour
{
    public bool isDebug = true;
    public bool isLoop = true;
    public Transform[] waypoints;
    public int Length
    {
        get
        {
            return waypoints.Length;
        }
    }
    public Vector3 GetPoint(int index)
    {
        return waypoints[index].position;
    }
    void OnDrawGizmos()
    {
        if (!isDebug)
            return;
        for (int i = 1; i < waypoints.Length; i++)
        {
            Debug.DrawLine(waypoints[i - 1].position, waypoints[i].position, Color.red);
        }
        if (isLoop)
        {
            Debug.DrawLine(waypoints[this.Length - 1].position, waypoints[0].position,
            Color.red);
        }
    }
}