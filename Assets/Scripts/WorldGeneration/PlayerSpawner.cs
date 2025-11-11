using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Camera Settings")]
    [SerializeField] private bool attachCameraToPlayer = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -10);
    
    private ProceduralMaze mazeGenerator;
    private GameObject spawnedPlayer;
    
    void Start()
    {
        mazeGenerator = FindObjectOfType<ProceduralMaze>();
        StartCoroutine(SpawnPlayerAfterMaze());
        AudioManager.Play("Escapees_Gameplay");
    }
    
    IEnumerator SpawnPlayerAfterMaze()
    {
        // Wait one frame to ensure the maze has been generated
        yield return new WaitForEndOfFrame();
        
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned in PlayerSpawner!");
            return;
        }
        
        if (mazeGenerator == null)
        {
            Debug.LogError("ProceduralMaze reference is not assigned in PlayerSpawner!");
            return;
        }
        
        GameObject spawnPoint = mazeGenerator.GetPlayerSpawnPoint();
        
        if (spawnPoint == null)
        {
            Debug.LogError("Player spawn point not found! Make sure the maze has been generated.");
            return;
        }
        
        // Spawn the player at the spawn point's position
        spawnedPlayer = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Player";
        
        Debug.Log($"Player spawned at position: {spawnPoint.transform.position}");
        
        // Attach camera to player
        if (attachCameraToPlayer)
        {
            AttachCameraToPlayer();
        }
    }
    
    void AttachCameraToPlayer()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera not found!");
            return;
        }
        
        if (spawnedPlayer == null)
        {
            Debug.LogWarning("Player not spawned yet!");
            return;
        }
        
        // Set camera as child of player
        mainCamera.transform.SetParent(spawnedPlayer.transform);
        
        // Set camera offset position
        mainCamera.transform.localPosition = cameraOffset;
        
        // Make camera look at the player
        mainCamera.transform.LookAt(spawnedPlayer.transform.position);
        
        Debug.Log($"Camera attached to player with offset: {cameraOffset}");
    }
    
    // Public method to get the spawned player
    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
}
