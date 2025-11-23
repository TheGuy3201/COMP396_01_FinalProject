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
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 currentVelocity = playerCC.velocity;
            Vector3 boostDirection = currentVelocity.normalized;
            playerCC.Move(boostDirection * boostedSpeed * Time.deltaTime);
            Debug.Log("SpeedBoost: Player speed boosted to " + playerCC.velocity.magnitude);
            StartCoroutine(ResetSpeedAfterDelay((float)boostTime));
            Destroy(gameObject); // Consume the powerup
        }
    }

    IEnumerator ResetSpeedAfterDelay(float delay) // Reset player speed after boost duration
    {
        yield return new WaitForSeconds(delay);
        if (playerCC != null)
        {
            Vector3 currentVelocity = playerCC.velocity;
            Vector3 resetDirection = currentVelocity.normalized;
            playerCC.Move(resetDirection * originalSpeed * Time.deltaTime);
            Debug.Log("SpeedBoost: Player speed reset to " + playerCC.velocity.magnitude);
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