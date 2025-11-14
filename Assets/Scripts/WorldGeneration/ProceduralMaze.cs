using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMaze : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private int mazeWidth = 10;
    [SerializeField] private int mazeHeight = 10;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] [Range(0f, 0.3f)] private float extraPathChance = 0.1f; // Chance to remove walls for better connectivity 
    
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject enemySpawnPointPrefab;
    [SerializeField] private GameObject playerSpawnPointPrefab;
    [SerializeField] private GameObject exitPointPrefab;
    [SerializeField] private GameObject patrolNodePrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private int numberOfSpawnPoints = 4;
    [SerializeField] private float minDistanceFromEnemies = 20f; // Minimum distance from enemy spawn points
    
    [Header("Patrol Nodes")]
    [SerializeField] private int patrolNodesPerEnemy = 5;
    [SerializeField] private float minNodeSpacing = 15f; // Minimum distance between patrol nodes
    
    private List<GameObject> enemySpawnPoints = new List<GameObject>();
    private GameObject playerSpawnPoint;
    private GameObject exitPoint;
    private List<GameObject> patrolNodes = new List<GameObject>();
    private Vector2Int playerStartCell;
    private Vector2Int exitCell;
    
    private int[,] maze;
    private bool[,] visited;
    private List<Vector3> connectedPositions = new List<Vector3>();
    
    void Start()
    {
        GenerateMazeWithGuaranteedPath();
        BuildMaze();
        FindConnectedPositions();
        CreatePlayerSpawnPoint();
        CreateExitPoint();
        CreatePatrolNodes();
        CreateEnemySpawnPoints();
    }
    
    void GenerateMazeWithGuaranteedPath()
    {
        // Initialize the maze grid
        // 0 = path, 1 = wall
        maze = new int[mazeWidth * 2 + 1, mazeHeight * 2 + 1];
        visited = new bool[mazeWidth, mazeHeight];
        
        // Fill with walls
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                maze[x, y] = 1;
            }
        }
        
        // Choose player start position (bottom-left area)
        playerStartCell = new Vector2Int(Random.Range(0, mazeWidth / 4), Random.Range(0, mazeHeight / 4));
        
        // Choose exit position (top-right area, far from player)
        exitCell = new Vector2Int(
            Random.Range(mazeWidth * 3 / 4, mazeWidth),
            Random.Range(mazeHeight * 3 / 4, mazeHeight)
        );
        
        Debug.Log($"Player start cell: {playerStartCell}, Exit cell: {exitCell}");
        
        // First, create a guaranteed path from player to exit
        CreateGuaranteedPath();
        
        // Then fill in the rest of the maze using recursive backtracking
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (!visited[x, y])
                {
                    GenerateMazeRecursive(x, y);
                }
            }
        }
        
        // Verify all cells were visited
        int visitedCount = 0;
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (visited[x, y]) visitedCount++;
            }
        }
        Debug.Log($"Visited {visitedCount} out of {mazeWidth * mazeHeight} cells");
        
        // Add extra paths to improve connectivity and reduce dead ends
        if (extraPathChance > 0)
        {
            AddExtraPaths();
        }
    }
    
    void CreateGuaranteedPath()
    {
        // Create a winding path from player start to exit
        Vector2Int current = playerStartCell;
        List<Vector2Int> pathCells = new List<Vector2Int>();
        pathCells.Add(current);
        
        // Mark starting cell
        visited[current.x, current.y] = true;
        int mazeX = current.x * 2 + 1;
        int mazeY = current.y * 2 + 1;
        maze[mazeX, mazeY] = 0;
        
        // Create path towards exit with some randomness
        while (current != exitCell)
        {
            Vector2Int direction = Vector2Int.zero;
            
            // Determine preferred direction towards exit
            int xDiff = exitCell.x - current.x;
            int yDiff = exitCell.y - current.y;
            
            // Randomly choose to move horizontally or vertically (with bias towards exit)
            bool moveHorizontal = Random.value < 0.5f;
            
            if (xDiff != 0 && (moveHorizontal || yDiff == 0))
            {
                direction = new Vector2Int(xDiff > 0 ? 1 : -1, 0);
            }
            else if (yDiff != 0)
            {
                direction = new Vector2Int(0, yDiff > 0 ? 1 : -1);
            }
            
            Vector2Int next = current + direction;
            
            // Ensure we stay within bounds
            if (next.x >= 0 && next.x < mazeWidth && next.y >= 0 && next.y < mazeHeight)
            {
                // Carve the path
                visited[next.x, next.y] = true;
                
                int nextMazeX = next.x * 2 + 1;
                int nextMazeY = next.y * 2 + 1;
                maze[nextMazeX, nextMazeY] = 0;
                
                // Remove wall between current and next
                int wallX = current.x * 2 + 1 + direction.x;
                int wallY = current.y * 2 + 1 + direction.y;
                maze[wallX, wallY] = 0;
                
                current = next;
                pathCells.Add(current);
            }
        }
        
        Debug.Log($"Created guaranteed path with {pathCells.Count} cells");
    }
    
    void AddExtraPaths()
    {
        int pathsAdded = 0;
        
        // Go through interior walls and randomly remove some
        for (int x = 2; x < maze.GetLength(0) - 2; x += 2)
        {
            for (int y = 1; y < maze.GetLength(1) - 1; y += 2)
            {
                if (maze[x, y] == 1 && Random.value < extraPathChance)
                {
                    maze[x, y] = 0;
                    pathsAdded++;
                }
            }
        }
        
        for (int x = 1; x < maze.GetLength(0) - 1; x += 2)
        {
            for (int y = 2; y < maze.GetLength(1) - 2; y += 2)
            {
                if (maze[x, y] == 1 && Random.value < extraPathChance)
                {
                    maze[x, y] = 0;
                    pathsAdded++;
                }
            }
        }
        
        Debug.Log($"Added {pathsAdded} extra paths for better connectivity");
    }
    
    void GenerateMazeRecursive(int x, int y)
    {
        visited[x, y] = true;
        
        // Convert grid coordinates to maze array coordinates
        int mazeX = x * 2 + 1;
        int mazeY = y * 2 + 1;
        maze[mazeX, mazeY] = 0;
        
        // Define directions: North, South, East, West
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // North
            new Vector2Int(0, -1),  // South
            new Vector2Int(1, 0),   // East
            new Vector2Int(-1, 0)   // West
        };
        
        // Shuffle directions for randomness
        ShuffleArray(directions);
        
        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;
            
            // Check if the new position is valid and not visited
            if (IsValidCell(newX, newY) && !visited[newX, newY])
            {
                // Remove wall between current cell and new cell
                int wallX = mazeX + dir.x;
                int wallY = mazeY + dir.y;
                maze[wallX, wallY] = 0;
                
                // Recursively visit the new cell
                GenerateMazeRecursive(newX, newY);
            }
        }
    }
    
    bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < mazeWidth && y >= 0 && y < mazeHeight;
    }
    
    void ShuffleArray(Vector2Int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector2Int temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
    
    void BuildMaze()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Wall Prefab is not assigned!");
            return;
        }
        
        // Build interior walls
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (maze[x, y] == 1)
                {
                    // Determine wall orientation based on neighbors
                    bool hasWallAbove = y < maze.GetLength(1) - 1 && maze[x, y + 1] == 1;
                    bool hasWallBelow = y > 0 && maze[x, y - 1] == 1;
                    bool hasWallRight = x < maze.GetLength(0) - 1 && maze[x + 1, y] == 1;
                    bool hasWallLeft = x > 0 && maze[x - 1, y] == 1;
                    
                    // Check if wall is part of horizontal or vertical corridor
                    bool isVerticalWall = (hasWallAbove || hasWallBelow) && !hasWallRight && !hasWallLeft;
                    bool isHorizontalWall = (hasWallRight || hasWallLeft) && !hasWallAbove && !hasWallBelow;
                    
                    // Create wall with appropriate rotation
                    Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                    Quaternion rotation = Quaternion.identity;
                    
                    // Rotate wall 90 degrees if it's horizontal (running along X-axis)
                    // Wall prefab is Z=5 (depth), so by default it runs along Z-axis
                    if (isHorizontalWall)
                    {
                        rotation = Quaternion.Euler(0, 90, 0);
                    }
                    
                    GameObject wall = Instantiate(wallPrefab, position, rotation, transform);
                    wall.name = $"Wall_{x}_{y}";
                }
            }
        }
        
        // Build boundary walls
        BuildBoundaryWalls();
    }
    
    void BuildBoundaryWalls()
    {
        int maxX = maze.GetLength(0) - 1;
        int maxY = maze.GetLength(1) - 1;
        
        // Bottom wall (along X-axis at Y = -1)
        for (int x = 0; x <= maxX; x++)
        {
            Vector3 position = new Vector3(x * cellSize, 0, -cellSize);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.Euler(0, 90, 0), transform);
            wall.name = $"BoundaryWall_Bottom_{x}";
        }
        
        // Top wall (along X-axis at Y = maxY + 1)
        for (int x = 0; x <= maxX; x++)
        {
            Vector3 position = new Vector3(x * cellSize, 0, (maxY + 1) * cellSize);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.Euler(0, 90, 0), transform);
            wall.name = $"BoundaryWall_Top_{x}";
        }
        
        // Left wall (along Z-axis at X = -1)
        for (int y = 0; y <= maxY; y++)
        {
            Vector3 position = new Vector3(-cellSize, 0, y * cellSize);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
            wall.name = $"BoundaryWall_Left_{y}";
        }
        
        // Right wall (along Z-axis at X = maxX + 1)
        for (int y = 0; y <= maxY; y++)
        {
            Vector3 position = new Vector3((maxX + 1) * cellSize, 0, y * cellSize);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
            wall.name = $"BoundaryWall_Right_{y}";
        }
    }
    
    void FindConnectedPositions()
    {
        // Find the first open position to start flood fill
        Vector2Int startCell = Vector2Int.zero;
        bool foundStart = false;
        
        for (int x = 0; x < maze.GetLength(0) && !foundStart; x++)
        {
            for (int y = 0; y < maze.GetLength(1) && !foundStart; y++)
            {
                if (maze[x, y] == 0)
                {
                    startCell = new Vector2Int(x, y);
                    foundStart = true;
                }
            }
        }
        
        if (!foundStart)
        {
            Debug.LogError("No open positions found in maze!");
            return;
        }
        
        // Use flood fill to find all connected positions
        bool[,] floodVisited = new bool[maze.GetLength(0), maze.GetLength(1)];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCell);
        floodVisited[startCell.x, startCell.y] = true;
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            Vector3 worldPos = new Vector3(current.x * cellSize, 0, current.y * cellSize);
            connectedPositions.Add(worldPos);
            
            // Check all 4 directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // North
                new Vector2Int(0, -1),  // South
                new Vector2Int(1, 0),   // East
                new Vector2Int(-1, 0)   // West
            };
            
            foreach (Vector2Int dir in directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                
                if (newX >= 0 && newX < maze.GetLength(0) && 
                    newY >= 0 && newY < maze.GetLength(1) &&
                    maze[newX, newY] == 0 && 
                    !floodVisited[newX, newY])
                {
                    floodVisited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }
        
        Debug.Log($"Found {connectedPositions.Count} connected positions in the maze");
    }
    
    void CreateEnemySpawnPoints()
    {
        if (enemySpawnPointPrefab == null)
        {
            Debug.LogError("Enemy Spawn Point Prefab is not assigned!");
            return;
        }
        
        if (connectedPositions.Count == 0)
        {
            Debug.LogError("No connected positions found! Cannot create enemy spawn points.");
            return;
        }
        
        // Use only connected positions for spawning
        List<Vector3> availablePositions = new List<Vector3>(connectedPositions);
        
        // Randomly select specified number of spawn points
        int spawnPointsToCreate = Mathf.Min(numberOfSpawnPoints, availablePositions.Count);
        
        for (int i = 0; i < spawnPointsToCreate; i++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPosition = availablePositions[randomIndex];
            
            GameObject spawnPoint = Instantiate(enemySpawnPointPrefab, spawnPosition, Quaternion.identity, transform);
            spawnPoint.name = $"EnemySpawnPoint_{i}";
            
            enemySpawnPoints.Add(spawnPoint);
            
            // Remove this position so we don't spawn multiple points in the same location
            availablePositions.RemoveAt(randomIndex);
        }
        
        Debug.Log($"Created {enemySpawnPoints.Count} enemy spawn points");
    }
    
    void CreatePlayerSpawnPoint()
    {
        if (playerSpawnPointPrefab == null)
        {
            Debug.LogError("Player Spawn Point Prefab is not assigned!");
            return;
        }
        
        // Use the pre-determined player start cell
        Vector3 spawnPosition = new Vector3(playerStartCell.x * 2 + 1, 0, playerStartCell.y * 2 + 1) * cellSize;
        
        playerSpawnPoint = Instantiate(playerSpawnPointPrefab, spawnPosition, Quaternion.identity, transform);
        playerSpawnPoint.name = "PlayerSpawnPoint";
        
        Debug.Log($"Created player spawn point at {spawnPosition}");
    }
    
    void CreateExitPoint()
    {
        if (exitPointPrefab == null)
        {
            Debug.LogError("Exit Point Prefab is not assigned!");
            return;
        }
        
        // Use the pre-determined exit cell
        Vector3 exitPosition = new Vector3(exitCell.x * 2 + 1, 0, exitCell.y * 2 + 1) * cellSize;
        
        exitPoint = Instantiate(exitPointPrefab, exitPosition, Quaternion.identity, transform);
        exitPoint.name = "ExitPoint";
        
        Debug.Log($"Created exit point at {exitPosition}");
    }
    
    // Public method to get all spawn points for enemy spawning later
    public List<GameObject> GetEnemySpawnPoints()
    {
        return enemySpawnPoints;
    }
    
    // Public method to get the player spawn point
    public GameObject GetPlayerSpawnPoint()
    {
        return playerSpawnPoint;
    }
    
    // Public method to get the exit point
    public GameObject GetExitPoint()
    {
        return exitPoint;
    }
    
    void CreatePatrolNodes()
    {
        if (patrolNodePrefab == null)
        {
            Debug.LogError("Patrol Node Prefab is not assigned!");
            return;
        }
        
        if (connectedPositions.Count == 0)
        {
            Debug.LogError("No connected positions found! Cannot create patrol nodes.");
            return;
        }
        
        // Calculate total nodes needed
        int totalNodes = numberOfSpawnPoints * patrolNodesPerEnemy;
        List<Vector3> availablePositions = new List<Vector3>(connectedPositions);
        
        // Remove positions too close to player and exit
        availablePositions.RemoveAll(pos => 
            Vector3.Distance(pos, playerSpawnPoint.transform.position) < minNodeSpacing ||
            Vector3.Distance(pos, exitPoint.transform.position) < minNodeSpacing
        );
        
        for (int i = 0; i < totalNodes && availablePositions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 nodePosition = availablePositions[randomIndex];
            
            // Raise patrol nodes above the ground so NavMesh agents can reach them
            nodePosition.y += 1f;
            
            GameObject patrolNode = Instantiate(patrolNodePrefab, nodePosition, Quaternion.identity, transform);
            patrolNode.name = $"PatrolNode_{i}";
            patrolNodes.Add(patrolNode);
            
            // Remove this position and nearby positions to maintain spacing
            availablePositions.RemoveAll(pos => Vector3.Distance(pos, nodePosition) < minNodeSpacing);
        }
        
        Debug.Log($"Created {patrolNodes.Count} patrol nodes");
    }
    
    // Public method to get all patrol nodes
    public List<GameObject> GetPatrolNodes()
    {
        return patrolNodes;
    }
}
