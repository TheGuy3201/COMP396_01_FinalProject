using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ProceduralMaze : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private int mazeWidth = 10;
    [SerializeField] private int mazeHeight = 10;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] [Range(0f, 0.3f)] private float extraPathChance = 0.1f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject enemySpawnPointPrefab;
    [SerializeField] private GameObject playerSpawnPointPrefab;
    [SerializeField] private GameObject exitPointPrefab;
    [SerializeField] private GameObject patrolNodePrefab;
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private GameObject speedBoostPrefab;
    [SerializeField] private GameObject superBoostPrefab;
    [SerializeField] private GameObject coinPrefab;
    
    [Header("Powerup Spawn Settings")]
    [SerializeField] private int healthPackCount = 3;
    [SerializeField] private int speedBoostCount = 2;
    [SerializeField] private int superBoostCount = 1;
    [SerializeField] private int coinCount = 10;
    
    [Header("Spawn Points")]
    [SerializeField] private int numberOfSpawnPoints = 4;
    [SerializeField] private float minDistanceFromEnemies = 20f;
    
    [Header("Patrol Nodes")]
    [SerializeField] private int patrolNodesPerEnemy = 5;
    [SerializeField] private float minNodeSpacing = 15f;
    
    private MazeGenerator mazeGen;
    private SpawnPointManager spawnManager;
    
    void Start()
    {
        mazeGen = new MazeGenerator();
        spawnManager = new SpawnPointManager(cellSize, transform);
        
        mazeGen.GenerateMaze(mazeWidth, mazeHeight, extraPathChance);
        
        MazeBuilder builder = new MazeBuilder(wallPrefab, cellSize, transform);
        builder.BuildMaze(mazeGen.Maze);
        
        spawnManager.FindConnectedPositions(mazeGen.Maze);
        spawnManager.CreatePlayerSpawnPoint(playerSpawnPointPrefab, mazeGen.PlayerStartCell);
        spawnManager.CreateExitPoint(exitPointPrefab, mazeGen.ExitCell, GetNextLevelName());
        spawnManager.CreatePatrolNodes(patrolNodePrefab, numberOfSpawnPoints * patrolNodesPerEnemy, minNodeSpacing);
        spawnManager.CreateEnemySpawnPoints(enemySpawnPointPrefab, numberOfSpawnPoints);
        spawnManager.SpawnPowerups(healthPackPrefab, speedBoostPrefab, superBoostPrefab, coinPrefab, 
                                   healthPackCount, speedBoostCount, superBoostCount, coinCount);
    }
    
    private string GetNextLevelName()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level (1)") return "Level (2)";
        if (currentScene == "Level (2)") return "Level (3)";
        if (currentScene == "Level (3)") return "GameOver";
        return "";
    }
    
    public List<GameObject> GetEnemySpawnPoints() => spawnManager.EnemySpawnPoints;
    public GameObject GetPlayerSpawnPoint() => spawnManager.PlayerSpawnPoint;
    public GameObject GetExitPoint() => spawnManager.ExitPoint;
    public List<GameObject> GetPatrolNodes() => spawnManager.PatrolNodes;
}