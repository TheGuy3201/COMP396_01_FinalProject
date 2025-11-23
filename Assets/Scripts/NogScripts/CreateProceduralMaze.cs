using System;
using System.Collections;
using System.Collections.Generic;
using InitializerCollector;
using UnityEngine;
using Random = UnityEngine.Random;
using IModule= InitializerCollector.IModule;



public class CreateProceduralMaze : MonoBehaviour,IModule
{
    [Header("Maze settings")]
    
    [Header("Internal grid")]
    [SerializeField] private ValueInRange mazeGridWidth;
    [SerializeField] private ValueInRange mazeGridHeight;
    
    [Header("Wall")]
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallLength;
    [SerializeField] private GameObject mazeWallPrefab;
    [SerializeField] private Transform mazeWallsParent;
    
    private Vector3 mazeWallRealSizeInUnityUnits;
    
    [Header("Pathways")]
    [SerializeField] private ValueInRange mainPathSizeInTileCount;
    [SerializeField] private ValueInRange alternativePathInTileCount;
    private Dictionary<Vector2,bool>  coordinatesVisitedInPathCreation;
    private List<Vector2[]> paths;
    
    [Header("Others")]
    [SerializeField] private string nextLevelName;

    private bool[,] coordinatesWithObjects;
    
    //Maze object created at end of this file
    private Dictionary<Vector2,MazeObject>  mazeObjects;

    
    public void InitializeScript()
    {
        CreateInitialPath(false, false);
        Debug.Log(string.Join(" -> ", paths[0]));
    }
    //Functions will be placed here
    public void CreateMaze()
    {
        
    }

    public void CreateInitialPath(bool startFromBorder, bool endOnBorder)
    {
    // 1. Garante inicialização das estruturas
    if (coordinatesVisitedInPathCreation == null)
    {
        coordinatesVisitedInPathCreation = new Dictionary<Vector2, bool>();
    }

    if (paths == null)
    {
        paths = new List<Vector2[]>();
    }

    // 2. Define tamanho inteiro do grid com base nos ranges
    int gridWidth = Mathf.Max(1, Mathf.FloorToInt(mazeGridWidth.MaxValue));
    int gridHeight = Mathf.Max(1, Mathf.FloorToInt(mazeGridHeight.MaxValue));

    // 3. Define o tamanho desejado do caminho principal
    int minPathLen = Mathf.Max(1, Mathf.FloorToInt(mainPathSizeInTileCount.MinValue));
    int maxPathLen = Mathf.Max(minPathLen, Mathf.FloorToInt(mainPathSizeInTileCount.MaxValue));
    int targetPathLength = Random.Range(minPathLen, maxPathLen + 1);

    // 4. Escolhe posição inicial (borda ou interior)
    int startX;
    int startY;

    if (startFromBorder)
    {
        int side = Random.Range(0, 4); // 0=left, 1=right, 2=bottom, 3=top
        switch (side)
        {
            case 0: // left
                startX = 0;
                startY = Random.Range(0, gridHeight);
                break;
            case 1: // right
                startX = gridWidth - 1;
                startY = Random.Range(0, gridHeight);
                break;
            case 2: // bottom
                startX = Random.Range(0, gridWidth);
                startY = 0;
                break;
            default: // top
                startX = Random.Range(0, gridWidth);
                startY = gridHeight - 1;
                break;
        }
    }
    else
    {
        startX = Random.Range(0, gridWidth);
        startY = Random.Range(0, gridHeight);
    }
    
    Vector2 current = new Vector2(startX, startY);
    List<Vector2> currentPath = new List<Vector2> { current };
    coordinatesVisitedInPathCreation[current] = true;
    
    // 5. Funções locais de ajuda
    bool IsOnBorder(int x, int y) =>
        x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1;

    // 6. Caminhada aleatória criando o caminho principal
    int stepsLeft = targetPathLength - 1;
    int safety = gridWidth * gridHeight * 4; // trava de segurança

    while (stepsLeft > 0 && safety-- > 0)
    {
        int cx = Mathf.RoundToInt(current.x);
        int cy = Mathf.RoundToInt(current.y);

        // Vizinhos possíveis (4-direções)
        List<Vector2> neighbours = GetValidNeighbours(cx, cy, gridWidth, gridHeight);

        if (neighbours.Count == 0)
        {
            break;
        }

        // Preferir vizinhos ainda não visitados
        List<Vector2> unvisited = new List<Vector2>();
        foreach (var n in neighbours)
        {
            if (!coordinatesVisitedInPathCreation.ContainsKey(n))
            {
                unvisited.Add(n);
            }
        }

        List<Vector2> source = unvisited.Count > 0 ? unvisited : neighbours;
        Vector2 next = source[Random.Range(0, source.Count)];

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
            List<Vector2> neighbours = GetValidNeighbours(cx, cy, gridWidth, gridHeight);

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
    
    private List<Vector2> GetValidNeighbours(int cx, int cy, int gridWidth, int gridHeight)
    {
        var neighbours = new List<Vector2>(4);

        // 4-directions: up, down, right, left
        int[,] dirs =
        {
            { 0,  1 }, // Up
            { 0, -1 }, // Down
            { 1,  0 }, // Right
            { -1, 0 }  // Left
        };

        for (int i = 0; i < 4; i++)
        {
            int nx = cx + dirs[i, 0];
            int ny = cy + dirs[i, 1];

            if (IsNextCoordInsideGrid(nx, ny, gridWidth, gridHeight))
            {
                neighbours.Add(new Vector2(nx, ny));
            }
        }

        return neighbours;
    }

    [Serializable]
    public class ValueInRange
    {
        [SerializeField]private float minValue;
        [SerializeField]private float maxValue;
        public ValueInRange()
        {
            minValue = 0f;
            maxValue = 1f;
        }
        public ValueInRange(float newMinValue, float newMaxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue cannot be greater than maxValue.", nameof(minValue));
            }

            minValue = newMinValue;
            maxValue = newMaxValue;
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
