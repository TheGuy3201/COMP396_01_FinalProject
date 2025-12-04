using System.Collections;
using UnityEngine;
using StarterAssets;

public class PowerUpManager : MonoBehaviour
{
    private ThirdPersonController playerController;
    private Coroutine activeBoostCoroutine;
    
    // Store original values
    private float originalMoveSpeed;
    private float originalSprintSpeed;
    private float originalJumpHeight;
    private bool hasStoredOriginals = false;

    void Start()
    {
        playerController = GetComponent<ThirdPersonController>();
        if (playerController != null)
        {
            StoreOriginalValues();
        }
    }

    void StoreOriginalValues()
    {
        if (!hasStoredOriginals)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            originalSprintSpeed = playerController.SprintSpeed;
            originalJumpHeight = playerController.JumpHeight;
            hasStoredOriginals = true;
            Debug.Log($"PowerUpManager: Stored original values - Move: {originalMoveSpeed}, Sprint: {originalSprintSpeed}, Jump: {originalJumpHeight}");
        }
    }

    public void ApplySpeedBoost(float speedMultiplier, float duration)
    {
        if (playerController == null) return;
        
        // Store originals if not already stored
        if (!hasStoredOriginals)
        {
            StoreOriginalValues();
        }

        // Cancel any existing boost
        if (activeBoostCoroutine != null)
        {
            StopCoroutine(activeBoostCoroutine);
            ResetToOriginal();
        }

        activeBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(speedMultiplier, duration));
    }

    public void ApplySuperBoost(float speedMultiplier, float jumpMultiplier, float duration)
    {
        if (playerController == null) return;
        
        // Store originals if not already stored
        if (!hasStoredOriginals)
        {
            StoreOriginalValues();
        }

        // Cancel any existing boost
        if (activeBoostCoroutine != null)
        {
            StopCoroutine(activeBoostCoroutine);
            ResetToOriginal();
        }

        activeBoostCoroutine = StartCoroutine(SuperBoostCoroutine(speedMultiplier, jumpMultiplier, duration));
    }

    IEnumerator SpeedBoostCoroutine(float speedMultiplier, float duration)
    {
        // Apply boost
        playerController.MoveSpeed = originalMoveSpeed * speedMultiplier;
        playerController.SprintSpeed = originalSprintSpeed * speedMultiplier;
        playerController.HasPowerUp = true;
        
        Debug.Log($"PowerUpManager: Speed boost active - Move: {playerController.MoveSpeed}, Sprint: {playerController.SprintSpeed}");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Reset
        ResetToOriginal();
        activeBoostCoroutine = null;
    }

    IEnumerator SuperBoostCoroutine(float speedMultiplier, float jumpMultiplier, float duration)
    {
        // Apply boost
        playerController.MoveSpeed = originalMoveSpeed * speedMultiplier;
        playerController.SprintSpeed = originalSprintSpeed * speedMultiplier;
        playerController.JumpHeight = originalJumpHeight * jumpMultiplier;
        playerController.HasPowerUp = true;
        
        Debug.Log($"PowerUpManager: Super boost active - Move: {playerController.MoveSpeed}, Jump: {playerController.JumpHeight}");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Reset
        ResetToOriginal();
        activeBoostCoroutine = null;
    }

    void ResetToOriginal()
    {
        if (playerController != null && hasStoredOriginals)
        {
            playerController.MoveSpeed = originalMoveSpeed;
            playerController.SprintSpeed = originalSprintSpeed;
            playerController.JumpHeight = originalJumpHeight;
            playerController.HasPowerUp = false;
            Debug.Log("PowerUpManager: Boost ended, stats reset to original values");
        }
    }

    void OnDestroy()
    {
        // Ensure we reset values if player is destroyed while boosted
        if (activeBoostCoroutine != null)
        {
            ResetToOriginal();
        }
    }
}
