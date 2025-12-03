using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class SuperBoost : MonoBehaviour
{
    [SerializeField] float speedMultiplier = 4.0f;
    [SerializeField] float jumpMultiplier = 2.0f;
    [SerializeField] int boostTime = 8;

    public void OnTriggerEnter(Collider other) // Detect collision with player to apply super boost
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ThirdPersonController playerController = other.GetComponent<ThirdPersonController>();
            if (playerController != null)
            {
                // Check if already boosted - don't stack boosts
                if (playerController.HasPowerUp)
                {
                    Debug.Log("SuperBoost: Player already has a powerup, ignoring");
                    Destroy(gameObject);
                    return;
                }
                
                Debug.Log("SuperBoost: Applying super boost for " + boostTime + " seconds");
                PowerUpManager powerUpManager = other.GetComponent<PowerUpManager>();
                if (powerUpManager == null)
                {
                    powerUpManager = other.gameObject.AddComponent<PowerUpManager>();
                }
                powerUpManager.ApplySuperBoost(speedMultiplier, jumpMultiplier, boostTime);
                Destroy(gameObject); // Consume the powerup
            }
            else
            {
                Debug.LogWarning("SuperBoost: Player does not have ThirdPersonController component!");
            }
        }
    }
}
