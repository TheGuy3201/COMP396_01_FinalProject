using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private ProceduralMaze mazeGenerator;
    [SerializeField] private Sprite[] enemySprites;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    void Start()
    {
        mazeGenerator = FindAnyObjectByType<ProceduralMaze>();
        StartCoroutine(SpawnEnemiesAfterMaze());
    }
    
    IEnumerator SpawnEnemiesAfterMaze()
    {
        // Wait one frame to ensure the maze has been generated
        yield return new WaitForEndOfFrame();
        
        SpawnEnemies();
    }
    
    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned in EnemySpawner!");
            return;
        }
        
        if (mazeGenerator == null)
        {
            mazeGenerator = FindObjectOfType<ProceduralMaze>();
            if (mazeGenerator == null)
            {
                Debug.LogError("ProceduralMaze reference is not found!");
                return;
            }
        }
        
        List<GameObject> spawnPoints = mazeGenerator.GetEnemySpawnPoints();
        
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No enemy spawn points found! Make sure the maze has been generated.");
            return;
        }
        
        Debug.Log($"Found {spawnPoints.Count} enemy spawn points. Spawning enemies...");
        
        // Spawn an enemy at each spawn point
        foreach (GameObject spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                // Raise enemy spawn position above ground
                Vector3 spawnPosition = spawnPoint.transform.position + Vector3.up * 1f;
                
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                enemy.name = $"Enemy_{spawnedEnemies.Count}";
                enemy.GetComponentInChildren<SpriteRenderer>().sprite = enemySprites[Random.Range(0, enemySprites.Length)];
                
                // Ensure the enemy has the EnemyAI component
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI == null)
                {
                    enemyAI = enemy.AddComponent<EnemyAI>();
                }
                
                spawnedEnemies.Add(enemy);
                
                // Optionally hide or destroy the spawn point marker after spawning
                spawnPoint.SetActive(false);
            }
        }
        
        Debug.Log($"Successfully spawned {spawnedEnemies.Count} enemies");
    }
    
    // Public method to get all spawned enemies
    public List<GameObject> GetSpawnedEnemies()
    {
        return spawnedEnemies;
    }
}
