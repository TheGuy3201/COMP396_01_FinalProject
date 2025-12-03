using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField] float boostedSpeed = 35f;
    [SerializeField] int boostTime = 5;

    private GameObject playerGO;
    CharacterController playerCC;
    private float originalSpeed = 0f;

    void Start()
    {
        StartCoroutine(SpeedyINIT(0.8f));
    }

    public void OnTriggerEnter(Collider other) // Detect collision with player to apply speed boost
    {
        Debug.Log("SpeedBoost: OnTriggerEnter with " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player") && playerCC != null)
        {
            originalSpeed = playerCC.velocity.magnitude;
            Debug.Log("SpeedBoost: Applying speed boost for " + boostTime + " seconds");
            StartCoroutine(ApplySpeedBoostOverTime((float)boostTime));
            Destroy(gameObject); // Consume the powerup
        }
    }

    IEnumerator ApplySpeedBoostOverTime(float duration) // Gradually apply speed boost over duration
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration && playerCC != null)
        {
            elapsedTime += Time.deltaTime;
            
            // Get current movement direction
            Vector3 currentVelocity = playerCC.velocity;
            Vector3 moveDirection = currentVelocity.normalized;
            
            // Interpolate between original and boosted speed
            float speedLerpFactor = boostedSpeed / (boostedSpeed + originalSpeed); // Weighted blend
            float currentSpeed = Mathf.Lerp(originalSpeed, boostedSpeed, elapsedTime / duration);
            
            // Apply the boosted movement
            playerCC.Move(moveDirection * currentSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Ensure we're back to original speed when boost ends
        if (playerCC != null)
        {
            Debug.Log("SpeedBoost: Boost expired, returning to normal speed");
        }
    }

    IEnumerator SpeedyINIT(float delay) // Wait for player to spawn then get reference
    {
        yield return new WaitForSeconds(delay);
        playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerCC = playerGO.GetComponent<CharacterController>();
            if (playerCC != null)
            {
                originalSpeed = playerCC.velocity.magnitude;
            }
            else
            {
                Debug.LogWarning("SpeedBoost: Player does not have a CharacterController component!");
            }
        }
        else
        {
            Debug.LogWarning("SpeedBoost: Player not found!");
        }
    }
}