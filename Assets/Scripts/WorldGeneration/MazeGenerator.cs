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

    public void GenerateMaze(int width, int height, float extraPathChance) // Generate the 2D maze
    {
        mazeWidth = width;
        mazeHeight = height;

        maze = new int[mazeWidth * 2 + 1, mazeHeight * 2 + 1]; // Create the maze array
        visited = new bool[mazeWidth, mazeHeight]; // Create the visited array
        
        // Fill with walls
        for (int x = 0; x < maze.GetLength(0); x++)
            for (int y = 0; y < maze.GetLength(1); y++)
                maze[x, y] = 1;
        
        // Choose spawn positions
        playerStartCell = new Vector2Int(Random.Range(0, mazeWidth / 4), Random.Range(0, mazeHeight / 4)); // Random position in the top-left quarter
        exitCell = new Vector2Int(Random.Range(mazeWidth * 3 / 4, mazeWidth), Random.Range(mazeHeight * 3 / 4, mazeHeight)); // Random position in the bottom-right quarter
        
        // Create guaranteed path
        CreateGuaranteedPath(); // Create a guaranteed path from the player start to the exit
        
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
        Vector2Int current = playerStartCell; // Start from the player's start cell
        visited[current.x, current.y] = true; // Mark as visited
        maze[current.x * 2 + 1, current.y * 2 + 1] = 0; // Carve out the cell
        
        while (current != exitCell)
        {
            int xDiff = exitCell.x - current.x; // Difference in x
            int yDiff = exitCell.y - current.y; // Difference in y
            bool moveHorizontal = Random.value < 0.5f; // Randomly decide to move horizontally or vertically

            Vector2Int direction = Vector2Int.zero; // Initialize direction
            if (xDiff != 0 && (moveHorizontal || yDiff == 0)) // Prefer horizontal movement
                direction = new Vector2Int(xDiff > 0 ? 1 : -1, 0);
            else if (yDiff != 0)
                direction = new Vector2Int(0, yDiff > 0 ? 1 : -1); // Move vertically

            if (direction == Vector2Int.zero) break; // Safety check

            Vector2Int next = current + direction; // Calculate next position
            if (next.x >= 0 && next.x < mazeWidth && next.y >= 0 && next.y < mazeHeight) // Check bounds
            {
                visited[next.x, next.y] = true;
                maze[next.x * 2 + 1, next.y * 2 + 1] = 0;
                maze[current.x * 2 + 1 + direction.x, current.y * 2 + 1 + direction.y] = 0;
                current = next;
            }
        }
        
        // Ensure exit cell is carved
        visited[exitCell.x, exitCell.y] = true;
        maze[exitCell.x * 2 + 1, exitCell.y * 2 + 1] = 0;
    }

    private void GenerateMazeRecursive(int x, int y) // Recursive backtracking 
    {
        visited[x, y] = true; // Mark as visited
        maze[x * 2 + 1, y * 2 + 1] = 0; // Carve out the cell

        Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) }; // Possible directions
        ShuffleArray(directions); // Randomize directions

        foreach (Vector2Int dir in directions) // Iterate through each direction
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
