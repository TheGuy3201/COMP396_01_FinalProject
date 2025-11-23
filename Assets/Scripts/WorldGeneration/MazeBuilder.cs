using UnityEngine;

public class MazeBuilder
{
    private GameObject wallPrefab;
    private float cellSize;
    private Transform parent;
    
    public MazeBuilder(GameObject wallPrefab, float cellSize, Transform parent)
    {
        this.wallPrefab = wallPrefab;
        this.cellSize = cellSize;
        this.parent = parent;
    }
    
    public void BuildMaze(int[,] maze)
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
                    bool hasWallAbove = y < maze.GetLength(1) - 1 && maze[x, y + 1] == 1;
                    bool hasWallBelow = y > 0 && maze[x, y - 1] == 1;
                    bool hasWallRight = x < maze.GetLength(0) - 1 && maze[x + 1, y] == 1;
                    bool hasWallLeft = x > 0 && maze[x - 1, y] == 1;
                    
                    bool isHorizontalWall = (hasWallRight || hasWallLeft) && !hasWallAbove && !hasWallBelow;
                    
                    Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                    Quaternion rotation = isHorizontalWall ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                    
                    GameObject wall = Object.Instantiate(wallPrefab, position, rotation, parent);
                    wall.name = $"Wall_{x}_{y}";
                }
            }
        }
        
        BuildBoundaryWalls(maze);
    }
    
    private void BuildBoundaryWalls(int[,] maze)
    {
        int maxX = maze.GetLength(0) - 1;
        int maxY = maze.GetLength(1) - 1;
        
        for (int x = 0; x <= maxX; x++)
        {
            Object.Instantiate(wallPrefab, new Vector3(x * cellSize, 0, -cellSize), Quaternion.Euler(0, 90, 0), parent).name = $"BoundaryWall_Bottom_{x}";
            Object.Instantiate(wallPrefab, new Vector3(x * cellSize, 0, (maxY + 1) * cellSize), Quaternion.Euler(0, 90, 0), parent).name = $"BoundaryWall_Top_{x}";
        }
        
        for (int y = 0; y <= maxY; y++)
        {
            Object.Instantiate(wallPrefab, new Vector3(-cellSize, 0, y * cellSize), Quaternion.identity, parent).name = $"BoundaryWall_Left_{y}";
            Object.Instantiate(wallPrefab, new Vector3((maxX + 1) * cellSize, 0, y * cellSize), Quaternion.identity, parent).name = $"BoundaryWall_Right_{y}";
        }
    }
}
