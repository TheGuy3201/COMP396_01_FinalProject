using UnityEngine;

public class PatrolNode : MonoBehaviour
{
    // Simple marker component for patrol nodes
    // Can add visual gizmos or additional data here if needed
    
    private void OnDrawGizmos()
    {
        // Draw a small sphere to visualize patrol nodes in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
