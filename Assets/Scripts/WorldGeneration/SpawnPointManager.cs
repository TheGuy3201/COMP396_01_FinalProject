using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager
{
    private float cellSize;
    private Transform parent;
    private List<Vector3> connectedPositions;
    
    public List<GameObject> EnemySpawnPoints { get; private set; } = new List<GameObject>();
    public List<GameObject> PatrolNodes { get; private set; } = new List<GameObject>();
    public GameObject PlayerSpawnPoint { get; private set; }
    public GameObject ExitPoint { get; private set; }
    
    public SpawnPointManager(float cellSize, Transform parent)
    {
        this.cellSize = cellSize;
        this.parent = parent;
    }
    
    // Find all connected open positions in the maze for valid spawn locations
    public void FindConnectedPositions(int[,] maze)
    {
        connectedPositions = new List<Vector3>();
        Vector2Int startCell = Vector2Int.zero;
        bool foundStart = false;
        
        for (int x = 0; x < maze.GetLength(0) && !foundStart; x++)
            for (int y = 0; y < maze.GetLength(1) && !foundStart; y++)
                if (maze[x, y] == 0) { startCell = new Vector2Int(x, y); foundStart = true; }
        
        if (!foundStart) return;
        
        bool[,] visited = new bool[maze.GetLength(0), maze.GetLength(1)];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCell);
        visited[startCell.x, startCell.y] = true;
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            connectedPositions.Add(new Vector3(current.x * cellSize, 0, current.y * cellSize));
            
            Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
            foreach (Vector2Int dir in dirs)
            {
                int newX = current.x + dir.x, newY = current.y + dir.y;
                if (newX >= 0 && newX < maze.GetLength(0) && newY >= 0 && newY < maze.GetLength(1) && 
                    maze[newX, newY] == 0 && !visited[newX, newY])
                {
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }
    }
    
    // Create the player spawn point at a valid connected position
    public void CreatePlayerSpawnPoint(GameObject prefab, Vector2Int playerCell)
    {
        // Use the first connected position as the player spawn to guarantee it's valid
        // Fallback to calculated position if no connected positions found
        Vector3 pos;
        
        if (connectedPositions != null && connectedPositions.Count > 0)
        {
            // Find the closest valid position to the intended player cell
            Vector3 intendedPos = new Vector3(playerCell.x * 2 + 1, 0, playerCell.y * 2 + 1);
            pos = connectedPositions[0];
            float minDist = Vector3.Distance(pos, intendedPos);
            
            foreach (Vector3 p in connectedPositions)
            {
                float dist = Vector3.Distance(p, intendedPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    pos = p;
                }
            }
        }
        else
        {
            // Fallback to original calculation if no connected positions yet
            pos = new Vector3(playerCell.x * 2 + 1, 0, playerCell.y * 2 + 1) * cellSize;
        }
        
        PlayerSpawnPoint = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
        PlayerSpawnPoint.name = "PlayerSpawnPoint";
        Debug.Log($"Player spawn point created at {pos}");
    }
    
    public void CreateExitPoint(GameObject prefab, Vector2Int exitCell, string nextLevel)
    {
        Vector3 pos = new Vector3(exitCell.x * 2 + 1, 0, exitCell.y * 2 + 1) * cellSize;
        ExitPoint = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
        ExitPoint.GetComponent<ExitPoint>().nextLevelName = nextLevel;
        ExitPoint.name = "ExitPoint";
    }
    
    public void CreateEnemySpawnPoints(GameObject prefab, int count)
    {
        List<Vector3> available = new List<Vector3>(connectedPositions);
        int toCreate = Mathf.Min(count, available.Count);
        
        for (int i = 0; i < toCreate; i++)
        {
            int idx = Random.Range(0, available.Count);
            GameObject sp = Object.Instantiate(prefab, available[idx], Quaternion.identity, parent);
            sp.name = $"EnemySpawnPoint_{i}";
            EnemySpawnPoints.Add(sp);
            available.RemoveAt(idx);
        }
    }
    
    public void CreatePatrolNodes(GameObject prefab, int totalNodes, float minSpacing)
    {
        List<Vector3> available = new List<Vector3>(connectedPositions);
        available.RemoveAll(p => Vector3.Distance(p, PlayerSpawnPoint.transform.position) < minSpacing || 
                                 Vector3.Distance(p, ExitPoint.transform.position) < minSpacing);
        
        for (int i = 0; i < totalNodes && available.Count > 0; i++)
        {
            int idx = Random.Range(0, available.Count);
            Vector3 pos = available[idx];
            pos.y += 1f;
            GameObject node = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
            node.name = $"PatrolNode_{i}";
            PatrolNodes.Add(node);
            available.RemoveAll(p => Vector3.Distance(p, pos) < minSpacing);
        }
    }
    
    public void SpawnPowerups(GameObject healthPack, GameObject speedBoost, GameObject superBoost, GameObject coin,
                              int healthCount, int speedCount, int superCount, int coinCount)
    {
        List<Vector3> available = new List<Vector3>(connectedPositions);
        available.RemoveAll(p => Vector3.Distance(p, PlayerSpawnPoint.transform.position) < 5f || 
                                 Vector3.Distance(p, ExitPoint.transform.position) < 5f);
        
        SpawnType(healthPack, healthCount, available, "HealthPack");
        SpawnType(speedBoost, speedCount, available, "SpeedBoost");
        SpawnType(superBoost, superCount, available, "SuperBoost");
        SpawnType(coin, coinCount, available, "Coin");
    }
    
    private void SpawnType(GameObject prefab, int count, List<Vector3> available, string name)
    {
        if (prefab == null || available.Count == 0) return;
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int idx = Random.Range(0, available.Count);
            Vector3 pos = available[idx];
            pos.y += 0.5f;
            Object.Instantiate(prefab, pos, Quaternion.identity, parent).name = $"{name}_{i}";
            available.RemoveAt(idx);
        }
    }
}
