using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UsefulClasses;
using Random = UnityEngine.Random;
using IModule= InitializerCollector.IModule;



public class CreateProceduralMaze : MonoBehaviour,IModule
{
    [Header("Maze settings")]
    [SerializeField] private ValueInRange mazeGridWidth;
    [SerializeField] private ValueInRange mazeGridHeight;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] [Range(0f, 0.3f)] private float extraPathChance = 0.1f; // Chance to remove walls for better connectivity 
    private Dictionary<Vector2, int> mazeVirtualMap= new Dictionary<Vector2, int>();
    
    [Header("Components")]
    [MustBeAssigned][SerializeField] private GameObject mazeWallPrefab;
    [MustBeAssigned][SerializeField] private GameObject mazeGroundPrefab;
    [MustBeAssigned][SerializeField] private Transform mazeElementsParent;
    //private Transform mazeGroundElementsParent,mazeWallElementsParent;
 
    

    //private  bool[,] mazeWallXPositions,mazeWallZPositions;
    
    [Header("Pathways")]
    public ValueInRange mainPathSizeInTileCount;
    public ValueInRange alternativePathInTileCount;
    private Dictionary<Vector2,bool>  coordinatesVisitedInPathCreation= new Dictionary<Vector2, bool>() ;
    private List<Vector2[]> paths= new List<Vector2[]>();
    
    [Header("Others")]
    [SerializeField] private string nextLevelName;
    private static int currentLevel=0;
    [SerializeField] private int lastLevel;
    private bool[,] coordinatesWithObjects;
    
    //Maze object created at end of this file
    private Dictionary<Vector2,MazeObject>  mazeObjects;

    [Button]
    public void InitializeScript()
    {
    }
    
    private void GoToNextLevel()
    {
        currentLevel++;
        if (currentLevel > lastLevel)
        {
            currentLevel = 0;
            SceneManager.LoadScene("GameOver");
        }
    }
    
    
    void GenerateVirtualMaze()
    {
        int mazeWallX = (int)mazeGridHeight.CurrentValue;
        int mazeWallY = (int)mazeGridWidth.CurrentValue;

        // Fill with walls
        for (int x = 0; x < mazeWallX; x++)
        {
            for (int y = 0; y < mazeWallY; y++)
            {
                mazeVirtualMap.Add(new Vector2(x, y), 1);
            }
        }// Choose player start position (bottom-left area)

        Vector2 widthAndHeight = new Vector2(
            mazeWallX,
            mazeWallY
            );
        CreateInitialPath(widthAndHeight,false,false);
        
        
        
        
        // First, create a guaranteed path from player to exit
        //CreateGuaranteedPath();
        
        // Then fill in the rest of the maze using recursive backtracking
       
        
        // Verify all cells were visited
        // Add extra paths to improve connectivity and reduce dead ends
    }
    
    
    


    public void CreateInitialPath(Vector2 widthAndHeight,bool startFromBorder, bool endOnBorder)
    {


    // 3. Define o tamanho desejado do caminho principal
    //int minPathLen = Mathf.Max(1, Mathf.FloorToInt(mainPathSizeInTileCount.MinValue));
    //int maxPathLen = Mathf.Max(minPathLen, Mathf.FloorToInt(mainPathSizeInTileCount.MaxValue));
    //int targetPathLength = Random.Range(minPathLen, maxPathLen + 1);



    
    // 4. Escolhe posição inicial (borda ou interior)
    int startX,startY,gridWidth=(int)widthAndHeight.x,gridHeight=(int)widthAndHeight.y;
    

    if (startFromBorder)
    {
        int side = Random.Range(0, 4); // 0=left, 1=right, 2=bottom, 3=top
        switch (side)
        {
            case 0: // left
                startX = 0;
                startY = (int)Random.Range(0, widthAndHeight.y);
                break;
            case 1: // right
                startX = (int)widthAndHeight.x - 1;
                startY = (int)Random.Range(0, widthAndHeight.y);
                break;
            case 2: // bottom
                startY = 0;
                startX = (int)Random.Range(0, widthAndHeight.x);
                break;
            default: // top
                startY = (int)widthAndHeight.y - 1;
                startX = (int)Random.Range(0, widthAndHeight.x);
                break;
        }
    }
    else
    {
        startX = (int)Random.Range(1, -1);
        startY =(int) Random.Range(1, widthAndHeight.y-1);
    }
    
    Vector2 current = new Vector2(startX, startY);
    List<Vector2> currentPath = new List<Vector2> { current };
    coordinatesVisitedInPathCreation[current] = true;
    int mainPathLenght = (int)mainPathSizeInTileCount.RandomizeValue();
    int stepsLeft = mainPathLenght;
    
    
    // 5. Funções locais de ajuda
    bool IsOnBorder(int x, int y) =>
        x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1;
    
    int safety = gridWidth * gridHeight * 4; // trava de segurança

    while (stepsLeft > 0 && safety-- > 0)
    {
        int cx = Mathf.RoundToInt(current.x);
        int cy = Mathf.RoundToInt(current.y);

        // Vizinhos possíveis (4-direções)
        List<Vector2Int> neighbours = GetValidNeighbours(cx, cy, gridWidth, gridHeight);

        if (neighbours.Count == 0)
        {
            break;
        }
        Vector2 next = neighbours[Random.Range(0, neighbours.Count)];

        current = next;
        currentPath.Add(current);
        coordinatesVisitedInPathCreation[current] = true;
        stepsLeft--;
    }

    // 7. Se for obrigatório terminar na borda, continua andando até encostar
    if (endOnBorder)
    {
        int extraSafety = gridWidth * gridHeight * 4;
        int cx = Mathf.RoundToInt(current.x);
        int cy = Mathf.RoundToInt(current.y);

        while (!IsOnBorder(cx, cy) && extraSafety-- > 0)
        {
            List<Vector2Int> neighbours = GetValidNeighbours(cx, cy, gridWidth, gridHeight);

            if (neighbours.Count == 0)
            {
                break;
            }

            // Ainda podemos preferir não revisitados, mas agora a prioridade é só não travar
            Vector2 next = neighbours[Random.Range(0, neighbours.Count)];
            current = next;
            currentPath.Add(current);
            coordinatesVisitedInPathCreation[current] = true;
            
            cx = Mathf.RoundToInt(current.x);
            cy = Mathf.RoundToInt(current.y);
        }
    }

    // 8. Guarda o caminho final
    paths.Add(currentPath.ToArray());
}

    
    public bool IsNextCoordInsideGrid(int x, int y,int gridWidth, int gridHeight){
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private List<Vector2Int> GetValidNeighbours(int cx, int cy, int gridWidth, int gridHeight)
    {
        var neighbours = new List<Vector2Int>(4);
    
        Vector2Int[] dirs =
        {
            new Vector2Int(0,  1), // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(1,  0), // Right
            new Vector2Int(-1, 0)  // Left
        };


        for (int i = 0; i < dirs.Length; i++)
        {
            Vector2Int neighbour = new Vector2Int(cx, cy) + dirs[i];

            if (coordinatesVisitedInPathCreation.ContainsKey(neighbour))
            {
                continue;
            }

            if (IsNextCoordInsideGrid(neighbour.x, neighbour.y, gridWidth, gridHeight)==false)
            {
                continue;
            }
            
            Vector2 fneighbourrA = neighbour+new Vector2Int(dirs[i].y, dirs[i].x);
            Vector2 fneighbourrb = neighbour+new Vector2Int(dirs[i].y, (dirs[i].x)*-1);
            
            bool nextNeighbourCausesLoop= 
                coordinatesVisitedInPathCreation.ContainsKey(fneighbourrA) ||
                coordinatesVisitedInPathCreation.ContainsKey(fneighbourrb);
            
            if (nextNeighbourCausesLoop)
            {
                continue;
            }
            
            neighbours.Add(neighbour);
        }

        return neighbours;
    }




private Transform GetOrCreateChildFromMainParent(Transform mainParent,string childName)
{
    if (mainParent == null)
    {
        Debug.LogError("mazeElementsParent is not assigned.");
        return null;
    }
    // Try to find an existing child called "FloorTiles"
    for (int i = 0; i < mainParent.childCount; i++)
    {
        Transform child = mainParent.GetChild(i);
        if (child.name == childName)
        {
            return child;
        }
    }
    GameObject go = new GameObject(childName);
    go.transform.SetParent(mainParent,false);
    if (go.transform.childCount > 0)
    {
        foreach (Transform child in go.transform)
        {
            Destroy(child);
        } 
    }
        
    return go.transform;
}
    
    [Serializable]
    public class ValueInRange
    {
        public float minValue;//inclusive
        public float maxValue;//exclusive
        private float currentValue;
        
        public ValueInRange()
        {
            CurrentValue=MaxValue;
        }
        public ValueInRange(float newMinValue, float newMaxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue cannot be greater than maxValue.", nameof(minValue));
            }
            

            MinValue = newMinValue;
            MaxValue = newMaxValue;
            CurrentValue=MinValue;
        }


        public float RandomizeValue()
        {
            CurrentValue=Random.Range(MinValue, MaxValue);
            return CurrentValue;
        }
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                if (value > maxValue)
                {
                    throw new ArgumentException("MinValue cannot be greater than MaxValue.");
                }

                minValue = value;
            }
        }
        public float MaxValue
        {
            get => maxValue;
            set
            {
                if (value < minValue)
                {
                    throw new ArgumentException("MaxValue cannot be less than MinValue.");
                }

                maxValue = value;
            }
        }

        public float CurrentValue
        {
            
            get
            {
                if (currentValue < MinValue)
                {
                    currentValue=MinValue;
                    return currentValue;
                }

                if (currentValue>MaxValue)
                {
                    currentValue=MaxValue;
                    return currentValue;
                }
                return currentValue;
            }
            set
            {
                currentValue = value;
                
            }
        }
    }
    
    [Serializable]
    public class MazeObject
    {
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;

        [Min(0)] public int minimumAmount = 1;
        [Min(0)] public int maximumAmount = 10;

        [Range(0.01f, 1f)] public float probability = 1f;

        [Min(0)] public float minimumDistanceFromSimilarity = 0f;
        [Min(0)] public float maximumDistanceFromSimilarity = 10f;

        [Min(0)] public float minimumDistanceFromOtherObjects = 0f;
        [Min(0)] public float maximumDistanceFromOtherObjects = 10f;

        public enum BorderPlacement
        {
            Should,
            Optional,
            ShouldNot
        }

        public BorderPlacement shouldBePlacedOnBorder = BorderPlacement.ShouldNot;

        [Range(0f, 1f)]
        public float probabilityIfOptional = 0.5f;
    }


}
