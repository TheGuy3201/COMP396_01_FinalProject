using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    private int[,] maze;
    private bool[,] visited;
    private int mazeWidth;
    private int mazeHeight;
    private Vector2Int playerStartCell;
    private Vector2Int exitCell;
    
    public int[,] Maze => maze;
    public Vector2Int PlayerStartCell => playerStartCell;
    public Vector2Int ExitCell => exitCell;
    
    public void GenerateMaze(int width, int height, float extraPathChance)
    {
        mazeWidth = width;
        mazeHeight = height;
        
        maze = new int[mazeWidth * 2 + 1, mazeHeight * 2 + 1];
        visited = new bool[mazeWidth, mazeHeight];
        
        // Fill with walls
        for (int x = 0; x < maze.GetLength(0); x++)
            for (int y = 0; y < maze.GetLength(1); y++)
                maze[x, y] = 1;
        
        // Choose spawn positions
        playerStartCell = new Vector2Int(Random.Range(0, mazeWidth / 4), Random.Range(0, mazeHeight / 4));
        exitCell = new Vector2Int(Random.Range(mazeWidth * 3 / 4, mazeWidth), Random.Range(mazeHeight * 3 / 4, mazeHeight));
        
        // Create guaranteed path
        CreateGuaranteedPath();
        
        // Fill remaining cells
        for (int x = 0; x < mazeWidth; x++)
            for (int y = 0; y < mazeHeight; y++)
                if (!visited[x, y])
                    GenerateMazeRecursive(x, y);
        
        // Add extra paths
        if (extraPathChance > 0)
            AddExtraPaths(extraPathChance);
    }
    
    private void CreateGuaranteedPath()
    {
        Vector2Int current = playerStartCell;
        visited[current.x, current.y] = true;
        maze[current.x * 2 + 1, current.y * 2 + 1] = 0;
        
        while (current != exitCell)
        {
            int xDiff = exitCell.x - current.x;
            int yDiff = exitCell.y - current.y;
            bool moveHorizontal = Random.value < 0.5f;
            
            Vector2Int direction = Vector2Int.zero;
            if (xDiff != 0 && (moveHorizontal || yDiff == 0))
                direction = new Vector2Int(xDiff > 0 ? 1 : -1, 0);
            else if (yDiff != 0)
                direction = new Vector2Int(0, yDiff > 0 ? 1 : -1);
            
            Vector2Int next = current + direction;
            if (next.x >= 0 && next.x < mazeWidth && next.y >= 0 && next.y < mazeHeight)
            {
                visited[next.x, next.y] = true;
                maze[next.x * 2 + 1, next.y * 2 + 1] = 0;
                maze[current.x * 2 + 1 + direction.x, current.y * 2 + 1 + direction.y] = 0;
                current = next;
            }
        }
    }
    
    private void GenerateMazeRecursive(int x, int y)
    {
        visited[x, y] = true;
        maze[x * 2 + 1, y * 2 + 1] = 0;
        
        Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        ShuffleArray(directions);
        
        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x, newY = y + dir.y;
            if (newX >= 0 && newX < mazeWidth && newY >= 0 && newY < mazeHeight && !visited[newX, newY])
            {
                maze[x * 2 + 1 + dir.x, y * 2 + 1 + dir.y] = 0;
                GenerateMazeRecursive(newX, newY);
            }
        }
    }
    
    private void AddExtraPaths(float chance)
    {
        for (int x = 2; x < maze.GetLength(0) - 2; x += 2)
            for (int y = 1; y < maze.GetLength(1) - 1; y += 2)
                if (maze[x, y] == 1 && Random.value < chance)
                    maze[x, y] = 0;
        
        for (int x = 1; x < maze.GetLength(0) - 1; x += 2)
            for (int y = 2; y < maze.GetLength(1) - 2; y += 2)
                if (maze[x, y] == 1 && Random.value < chance)
                    maze[x, y] = 0;
    }
    
    private void ShuffleArray(Vector2Int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
}
