using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRestore : MonoBehaviour
{
    [SerializeField] int restoreAmount = 20;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("HealthRestore: Restoring player health by " + restoreAmount);
                playerHealth.Heal(restoreAmount);
                Destroy(gameObject);
            }
        }
    }
}
