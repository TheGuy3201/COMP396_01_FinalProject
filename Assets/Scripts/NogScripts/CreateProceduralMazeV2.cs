using System;
using System.Collections.Generic;
using InitializerCollector;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeProceduralGeneratorV2 : MonoBehaviour, IModule
{
    // 1.4 - variáveis privadas com SerializeField
    [SerializeField] private int mazeWidth;
    [SerializeField] private int mazeHeight;

    // largura do chão de cada célula
    [SerializeField] private float tileWidthLengthScale = 1f;

    // espaço entre tiles, preenchido por paredes
    [SerializeField] private float wallThicknessScale = 0.2f;

    [SerializeField] private float wallHeightScale = 1f;
    [SerializeField] private float wallWidthScale = 1f;

    [Header("Components")]
    [MustBeAssigned] [SerializeField] private GameObject mazeWallPrefab;
    [MustBeAssigned] [SerializeField] private GameObject mazeGroundPrefab;
    [MustBeAssigned] [SerializeField] private Transform mazeElementsParent;
    
    public Transform mazeElementsWallSubParent, mazeElementsGroundSubParent;


    private Dictionary<Vector2Int, VirtualTile2DWithWalls> virtualMaze;
    private Dictionary<Vector2Int, VirtualCorner2D> virtualCorners;
    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public void InitializeScript()
    {
        print("Initializing Procedural Maze...");
        VirtualMaze();     // cria a estrutura virtual
        // Aqui você chamaria sua lógica de "carvar" o labirinto usando OpenPassage(...)
        BuildMazeVisual(); // instancia o labirinto 3D
    }

    private void VirtualMaze()
    {
        virtualMaze = new Dictionary<Vector2Int, VirtualTile2DWithWalls>(mazeWidth * mazeHeight);
        virtualCorners = new Dictionary<Vector2Int, VirtualCorner2D>();

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                var key = new Vector2Int(x, y);
                var tile = new VirtualTile2DWithWalls();

                virtualMaze[key] = tile;
            }
        }

        // Agora conectamos cada parede (virtual) a 2 corners
        RegisterAllCornerConnections();
    }
    
    // Direções padrão
    

    private void RegisterAllCornerConnections()
    {
        // Vamos registrar cada parede UMA vez só para não duplicar.
        // Convenção simples: só consideramos UP e RIGHT como "canônicas".
        Vector2Int[] canonicalDirs = { Vector2Int.up, Vector2Int.right };

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);

                foreach (var dir in canonicalDirs)
                {
                    Vector2Int neighbour = tilePos + dir;

                    // se for parede interna (dentro dos limites)
                    if (neighbour.x >= 0 && neighbour.x < mazeWidth &&
                        neighbour.y >= 0 && neighbour.y < mazeHeight)
                    {
                        // essa parede existe inicialmente
                        RegisterWallCorners(tilePos, dir);
                    }
                    else
                    {
                        // opcional: registrar bordas externas também como paredes
                        // se você quiser que corners de borda também "segurem" a forma
                        RegisterWallCorners(tilePos, dir);
                    }
                }
            }
        }
    }

// Conecta uma parede (tilePos + dir) a 2 corners
    private void RegisterWallCorners(Vector2Int tilePos, Vector2Int dir)
    {
        var corners = GetCornersForWall(tilePos, dir);

        foreach (var cornerPos in corners)
        {
            if (!virtualCorners.TryGetValue(cornerPos, out var corner))
            {
                corner = new VirtualCorner2D();
                virtualCorners[cornerPos] = corner;
            }

            corner.RegisterConnection();
        }
    }


    private bool TryOpenWall(Vector2Int tilePos, Vector2Int dir)
    {
        // se nem existe essa parede, não faz nada
        if (!virtualMaze[tilePos].HasWall(dir))
            return false;

        Vector2Int[] corners = GetCornersForWall(tilePos, dir);

        VirtualCorner2D cornerA = virtualCorners[corners[0]];
        VirtualCorner2D cornerB = virtualCorners[corners[1]];

        // Regra: se QUALQUER quina tiver 1 conexão ou menos, bloqueia
        if (!cornerA.CanRemoveOneConnection() || !cornerB.CanRemoveOneConnection())
        {
            return false;
        }

        // Se chegou aqui, as duas quinas permitem a remoção
        cornerA.OnConnectionRemoved();
        cornerB.OnConnectionRemoved();

        // Remove parede nos dois tiles (ida e volta)
        Vector2Int neighbourPos = tilePos + dir;

        if (virtualMaze.TryGetValue(neighbourPos, out var neighbourTile))
        {
            virtualMaze[tilePos].RemoveWall(dir);
            neighbourTile.RemoveWall(-dir);
        }
        else
        {
            // se for borda externa e você não tem tile vizinho,
            // remova só deste lado ou trate como quiser
            virtualMaze[tilePos].RemoveWall(dir);
        }

        return true;
    }


public void BuildMazeVisual()
{
    BuildParentsIfNotExist();

    // Bounds reais do chão (já com o scale do prefab/aplicado)
    Bounds mazeGroundBounds = mazeGroundPrefab.GetComponentInChildren<Renderer>().bounds;

    // Renderer da parede
    var wallRenderer = mazeWallPrefab.GetComponentInChildren<Renderer>();

    // Tamanho base da parede, SEM escala (tamanho do mesh)
    Vector3 wallBaseSize = wallRenderer.localBounds.size;

    // fator pra fazer o Z da parede ficar do mesmo tamanho que o Z do ground (paredes paralelas a Z)
    float zScale = mazeGroundBounds.size.z / wallBaseSize.z;

    // fator pra fazer o X da parede ficar do mesmo tamanho que o X do ground (paredes paralelas a X)
    float xScale = mazeGroundBounds.size.x / wallBaseSize.x;

    // Tamanhos reais dos elementos (como se já estivessem escalados)
    float tileSizeX = mazeGroundBounds.size.x;
    float tileSizeZ = mazeGroundBounds.size.z;

    // ESPESSURA da parede (usada para espaçamento em X e Z)
    float wallThickness = wallBaseSize.x * wallThicknessScale;

    // Tamanho total do labirinto em Unity units (usando a ESPESSURA, não o comprimento)
    float totalMazeSizeX = tileSizeX * mazeWidth  + wallThickness * (mazeWidth  + 1);
    float totalMazeSizeZ = tileSizeZ * mazeHeight + wallThickness * (mazeHeight + 1);

    // Queremos o labirinto centrado no parent (0,0,0)
    float originX = -totalMazeSizeX * 0.5f;
    float originZ = -totalMazeSizeZ * 0.5f;

    // Para instanciar
    GameObject newWall, newTile;

    // ============================
    // 1) PAREDES ESQUERDA/DIREITA + TILES
    // ============================
    // Loop por linhas (Z)
    for (int z = 0; z < mazeHeight; z++)
    {
        float currentPosZ = originZ
                            + wallThickness            // primeira parede de borda
                            + tileSizeZ * 0.5f
                            + z * (tileSizeZ + wallThickness); // passo entre linhas

        // Reinicia X a cada linha
        float currentPosX = originX;

        // Paredes + tiles ao longo do X
        for (int x = 0; x < mazeWidth; x++)
        {
            // 1) Parede à esquerda do tile
            currentPosX += wallThickness * 0.5f; // centro da parede

            newWall = Instantiate(mazeWallPrefab);
            newWall.transform.parent = mazeElementsWallSubParent;
            // paredes esquerda/direita: longas em Z, finas em X
            newWall.transform.localRotation = Quaternion.identity;
            newWall.transform.localScale = new Vector3(
                wallThicknessScale,
                1f,
                zScale
            );
            newWall.transform.localPosition = new Vector3(
                currentPosX,
                0f,
                currentPosZ
            );

            // 2) Tile
            currentPosX += wallThickness * 0.5f; // fim da parede
            currentPosX += tileSizeX * 0.5f;     // centro do tile

            newTile = Instantiate(mazeGroundPrefab);
            newTile.transform.SetParent(mazeElementsGroundSubParent);
            newTile.transform.localPosition = new Vector3(
                currentPosX,
                0f,
                currentPosZ
            );

            // 3) Avança até a borda direita do tile
            currentPosX += tileSizeX * 0.5f;
        }

        // 4) Parede final à direita da última célula (borda direita)
        currentPosX += wallThickness * 0.5f;
        newWall = Instantiate(mazeWallPrefab);
        newWall.transform.parent = mazeElementsWallSubParent;
        newWall.transform.localRotation = Quaternion.identity;
        newWall.transform.localScale = new Vector3(
            wallThicknessScale,
            1f,
            zScale
        );
        newWall.transform.localPosition = new Vector3(
            currentPosX,
            0f,
            currentPosZ
        );
    }

    // ============================
    // 2) PAREDES FRENTE / TRÁS (sem recriar tiles)
    // ============================
    for (int x = 0; x < mazeWidth; x++)
    {
        // centro dos tiles dessa coluna em X:
        float tileCenterX = originX
                            + wallThickness         // primeira parede de borda
                            + tileSizeX * 0.5f
                            + x * (tileSizeX + wallThickness);

        // teremos mazeHeight+1 "linhas de parede" em Z (frente, entre linhas e atrás)
        for (int z = 0; z <= mazeHeight; z++)
        {
            // posição Z do centro de cada parede horizontal
            float wallCenterZ = originZ
                                + wallThickness * 0.5f      // centro da primeira borda
                                + z * (tileSizeZ + wallThickness);

            newWall = Instantiate(mazeWallPrefab);
            newWall.transform.parent = mazeElementsWallSubParent;

            // paredes frente/trás: longas em X, finas em Z
            // rotação coloca o eixo Z do mesh alinhado com o mundo X
            newWall.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            newWall.transform.localScale = new Vector3(
                wallThicknessScale, // continua sendo a espessura
                1f,
                xScale              // comprimento da parede na direção X
            );

            newWall.transform.localPosition = new Vector3(
                tileCenterX,
                0f,
                wallCenterZ
            );
        }
    }
}

    public void BuildParentsIfNotExist()
    {
        // Ground
        Transform foundGround = mazeElementsParent.transform.Find("GroundSubParent");
        if (foundGround == null)
        {
            mazeElementsGroundSubParent = new GameObject("GroundSubParent").transform;
            mazeElementsGroundSubParent.SetParent(mazeElementsParent);
        }
        else
        {
            mazeElementsGroundSubParent = foundGround;
        }

        // Wall
        Transform foundWall = mazeElementsParent.transform.Find("WallSubParent");
        if (foundWall == null)
        {
            mazeElementsWallSubParent = new GameObject("WallSubParent").transform;
            mazeElementsWallSubParent.SetParent(mazeElementsParent);
        }
        else
        {
            mazeElementsWallSubParent = foundWall;
        }
    }

    
    private static Vector2Int[] GetCornersForWall(Vector2Int tilePos, Vector2Int dir)
    {
        // meio da parede
        Vector2Int mid = tilePos + dir;

        // perpendicular a dir: (x,y) -> (-y,x)
        Vector2Int perp = new Vector2Int(-dir.y, dir.x);

        Vector2Int cornerA = mid + perp;
        Vector2Int cornerB = mid - perp;

        return new[] { cornerA, cornerB };
    }

    private class VirtualTile2DWithWalls
    {
        /// <summary>
        /// Lista de direções que ainda possuem parede.
        /// Exemplo de valores: Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right.
        /// </summary>
        public List<Vector2Int> wallDirections;

        public VirtualTile2DWithWalls()
        {
            wallDirections = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
        }

        /// <summary>
        /// Retorna true se ainda existe parede na direção dada.
        /// </summary>
        public bool HasWall(Vector2Int direction)
        {
            return wallDirections.Contains(direction);
        }

        /// <summary>
        /// Remove a parede na direção dada (abre caminho).
        /// </summary>
        public void RemoveWall(Vector2Int direction)
        {
            wallDirections.Remove(direction);
        }
    }
    
    private class VirtualCorner2D
    {
        // Quantas paredes ainda estão ligadas a essa quina
        public int connectionCount;

        public VirtualCorner2D()
        {
            connectionCount = 0;
        }

        public void RegisterConnection()
        {
            connectionCount++;
        }

        public bool CanRemoveOneConnection()
        {
            // Regra: se já está em 1, não pode remover mais nenhuma
            return connectionCount > 1;
        }

        public void OnConnectionRemoved()
        {
            connectionCount--;
        }
    }

    

    
    
    [Serializable]
    public class ValueInRange
    {
        public float minValue; // inclusive
        public float maxValue; // exclusive
        private float currentValue;

        public ValueInRange()
        {
            CurrentValue = MaxValue;
        }

        public ValueInRange(float newMinValue, float newMaxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue cannot be greater than maxValue.", nameof(minValue));
            }

            MinValue = newMinValue;
            MaxValue = newMaxValue;
            CurrentValue = MinValue;
        }

        public float RandomizeValue()
        {
            CurrentValue = Random.Range(MinValue, MaxValue);
            return CurrentValue;
        }

        public float MinValue
        {
            get => minValue;
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
                    currentValue = MinValue;
                    return currentValue;
                }

                if (currentValue > MaxValue)
                {
                    currentValue = MaxValue;
                    return currentValue;
                }

                return currentValue;
            }
            set => currentValue = value;
        }
    }
}