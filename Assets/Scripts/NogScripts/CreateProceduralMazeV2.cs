using System;
using System.Collections.Generic;
using InitializerCollector;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeProceduralGeneratorV2 : MonoBehaviour, IModule
{
    [Header("Maze Settings")]
    [SerializeField] private int mazeWidth;
    [SerializeField] private int mazeHeight;

    // floor width/length of each cell (not used right now, but may be helpful later)
    [SerializeField] private float tileWidthLengthScale = 1f;

    // gap between tiles, filled by walls
    [SerializeField] private float wallThicknessScale = 0.2f;

    [SerializeField] private float wallHeightScale = 1f;
    [SerializeField] private float wallWidthScale = 1f;

    [Header("Components")]
    [MustBeAssigned] [SerializeField] private GameObject mazeWallPrefab;
    [MustBeAssigned] [SerializeField] private GameObject mazeGroundPrefab;
    [MustBeAssigned] [SerializeField] private Transform mazeElementsParent;
    [MustBeAssigned] [SerializeField] private GameObject cornerPrefab;

    private Transform mazeElementsWallSubParent, mazeElementsGroundSubParent, mazeElementsObjectsSubParent, mazeElementsCornersSubParent;

    [Header("Pathways")]
    public ValueInRange mainPathSizeInTileCount;

    [Header("Real Distance Control")]
    [Min(1)] [SerializeField] private int maxAttemptsToMatchRealDistance = 5;

    [Header("Objects")]
    [SerializeField] private List<MazeObject> mazeObjectDefinitions = new List<MazeObject>();

    private Dictionary<Vector2Int, VirtualTile2DWithWalls> virtualMaze;
    private Dictionary<Vector2Int, VirtualCorner2D> virtualCorners;
    private Dictionary<Vector2Int, MazeObject> mazeObjects; // cell -> object type

    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // =========================================================
    // Init
    // =========================================================
    public void InitializeScript()
    {
        Debug.Log("Initializing Procedural Maze with real-distance check...");

        Vector2Int start, end;
        ChooseRandomStartEnd(out start, out end);
        Debug.Log($"[MazeProceduralGeneratorV2] Start={start}, End={end}");

        int targetMainLength = Mathf.Max(1, Mathf.RoundToInt(mainPathSizeInTileCount.RandomizeValue()));
        Debug.Log($"[MazeProceduralGeneratorV2] Target main path length = {targetMainLength}");

        List<Vector2Int> bestMainPath = null;
        Dictionary<Vector2Int, VirtualTile2DWithWalls> bestMaze = null;
        int bestRealLen = -1;

        for (int attempt = 0; attempt < maxAttemptsToMatchRealDistance; attempt++)
        {
            Debug.Log($"[MazeProceduralGeneratorV2] Attempt {attempt + 1}/{maxAttemptsToMatchRealDistance}");

            VirtualMaze();

            var mainPath = GenerateMainPath(start, end, targetMainLength);

            int realShortestLen = ComputeShortestPathLength(start, end);

            if (realShortestLen <= 0)
            {
                Debug.LogWarning("[MazeProceduralGeneratorV2] Start and End ended up disconnected on this attempt.");
                continue;
            }

            Debug.Log($"[MazeProceduralGeneratorV2] Attempt {attempt + 1}: realShortestLen={realShortestLen}");

            // if the true shortest path >= target, it's acceptable; we can stop
            if (realShortestLen >= targetMainLength)
            {
                bestMaze = CloneMaze(virtualMaze);
                bestMainPath = new List<Vector2Int>(mainPath);
                bestRealLen = realShortestLen;
                Debug.Log($"[MazeProceduralGeneratorV2] Accepted attempt {attempt + 1} (real >= target).");
                break;
            }

            // otherwise, store the best so far
            if (realShortestLen > bestRealLen)
            {
                bestRealLen = realShortestLen;
                bestMaze = CloneMaze(virtualMaze);
                bestMainPath = new List<Vector2Int>(mainPath);
                Debug.Log($"[MazeProceduralGeneratorV2] New best realShortestLen={bestRealLen}");
            }
        }

        if (bestMaze == null || bestMainPath == null || bestMainPath.Count == 0)
        {
            Debug.LogError("[MazeProceduralGeneratorV2] Failed to generate any valid maze.");
            return;
        }

        virtualMaze = bestMaze;

        mazeObjects ??= new Dictionary<Vector2Int, MazeObject>();
        PlaceMazeObjects(bestMainPath);
        BuildMazeVisual();
        Debug.Log($"[MazeProceduralGeneratorV2] Final real shortest path length = {bestRealLen}");
    }

    private void VirtualMaze()
    {
        virtualMaze = new Dictionary<Vector2Int, VirtualTile2DWithWalls>(mazeWidth * mazeHeight);
        virtualCorners = new Dictionary<Vector2Int, VirtualCorner2D>();
        mazeObjects = new Dictionary<Vector2Int, MazeObject>();

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                var key = new Vector2Int(x, y);
                var tile = new VirtualTile2DWithWalls();
                virtualMaze[key] = tile;
            }
        }

        RegisterAllCornerConnections();
    }

    private void RegisterAllCornerConnections()
    {
        // Only the canonical two wall directions needed to touch all corners:
        Vector2Int[] canonicalDirs = { Vector2Int.up, Vector2Int.right };

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);

                foreach (var dir in canonicalDirs)
                {
                    Vector2Int neighbour = tilePos + dir;

                    // internal + edges; everything counts
                    if (neighbour.x >= 0 && neighbour.x < mazeWidth &&
                        neighbour.y >= 0 && neighbour.y < mazeHeight)
                    {
                        RegisterWallCorners(tilePos, dir);
                    }
                    else
                    {
                        RegisterWallCorners(tilePos, dir);
                    }
                }
            }
        }
    }

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

    private static Vector2Int[] GetCornersForWall(Vector2Int tilePos, Vector2Int dir)
    {
        // wall midpoint in virtual grid coordinates
        Vector2Int mid = tilePos + dir;

        // perpendicular (x,y) -> (-y,x)
        Vector2Int perp = new Vector2Int(-dir.y, dir.x);

        Vector2Int cornerA = mid + perp;
        Vector2Int cornerB = mid - perp;

        return new[] { cornerA, cornerB };
    }

    // Simplified TryOpenWall – only modifies the virtualMaze
    private bool TryOpenWall(Vector2Int tilePos, Vector2Int dir)
    {
        if (!virtualMaze[tilePos].HasWall(dir))
            return false;

        Vector2Int neighbourPos = tilePos + dir;

        if (virtualMaze.TryGetValue(neighbourPos, out var neighbourTile))
        {
            // remove the wall from both sides
            virtualMaze[tilePos].RemoveWall(dir);
            neighbourTile.RemoveWall(-dir);
        }
        else
        {
            // outer border
            virtualMaze[tilePos].RemoveWall(dir);
        }

        return true;
    }

    private List<Vector2Int> GetValidNeighboursForPath(
        Vector2Int current,
        HashSet<Vector2Int> visited,
        bool allowVisited)
    {
        var result = new List<Vector2Int>(4);

        foreach (var dir in CardinalDirections)
        {
            Vector2Int n = current + dir;

            // inside the grid
            if (n.x < 0 || n.x >= mazeWidth || n.y < 0 || n.y >= mazeHeight)
                continue;

            if (!allowVisited && visited.Contains(n))
                continue;

            result.Add(n);
        }

        return result;
    }

    private void ChooseRandomStartEnd(out Vector2Int start, out Vector2Int end)
    {
        if (mazeWidth <= 0 || mazeHeight <= 0)
        {
            Debug.LogError("[MazeProceduralGeneratorV2] Invalid maze size to choose start/end.");
            start = Vector2Int.zero;
            end   = Vector2Int.zero;
            return;
        }

        start = new Vector2Int(
            Random.Range(0, mazeWidth),
            Random.Range(0, mazeHeight)
        );

        // ensure end differs from start
        do
        {
            end = new Vector2Int(
                Random.Range(0, mazeWidth),
                Random.Range(0, mazeHeight)
            );
        } while (end == start);
    }

    public Vector2Int[] GenerateMainPath(Vector2Int start, Vector2Int end, int targetMainLength)
    {
        if (!virtualMaze.ContainsKey(start) || !virtualMaze.ContainsKey(end))
        {
            Debug.LogError($"[MazeProceduralGeneratorV2] Start {start} or End {end} are outside the maze.");
            return Array.Empty<Vector2Int>();
        }

        var mainPath = new List<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        Vector2Int current = start;
        mainPath.Add(current);
        visited.Add(current);

        int stepsLeft = targetMainLength;
        int safety = mazeWidth * mazeHeight * 8;

        int DistManhattan(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        while (current != end && stepsLeft > 0 && safety-- > 0)
        {
            // try unvisited neighbours first
            var neighbours = GetValidNeighboursForPath(current, visited, allowVisited: false);

            // if none, allow revisiting
            if (neighbours.Count == 0)
            {
                neighbours = GetValidNeighboursForPath(current, visited, allowVisited: true);
                if (neighbours.Count == 0)
                    break;
            }

            // sort by approaching the end
            neighbours.Sort((a, b) =>
                DistManhattan(a, end).CompareTo(DistManhattan(b, end)));

            bool moved = false;

            foreach (var next in neighbours)
            {
                Vector2Int dir = next - current;

                if (!TryOpenWall(current, dir))
                    continue;

                current = next;
                mainPath.Add(current);
                visited.Add(current);
                stepsLeft--;
                moved = true;
                break;
            }

            if (!moved)
            {
                break;
            }
        }

        if (current != end)
        {
            Debug.LogWarning($"[MazeProceduralGeneratorV2] Main path did not reach {end}. PathLen={mainPath.Count}");
        }

        // fill remaining maze by carving branches until everything is visited
        FillAllMazeWithBranches(visited);

        return mainPath.ToArray();
    }

    private void FillAllMazeWithBranches(HashSet<Vector2Int> globalVisited)
    {
        int totalCells = mazeWidth * mazeHeight;
        int safetyGlobal = totalCells * 16; // global fuse

        while (globalVisited.Count < totalCells && safetyGlobal-- > 0)
        {
            // choose a visited cell as branch start
            Vector2Int branchStart = GetRandomVisitedCell(globalVisited);

            CarveBranchFrom(branchStart, globalVisited);
        }

        if (globalVisited.Count < totalCells)
        {
            Debug.LogWarning($"[MazeProceduralGeneratorV2] Not all tiles were visited. Visited={globalVisited.Count}/{totalCells}");
        }
    }

    private Vector2Int GetRandomVisitedCell(HashSet<Vector2Int> visited)
    {
        int index = UnityEngine.Random.Range(0, visited.Count);
        int i = 0;
        foreach (var v in visited)
        {
            if (i == index)
                return v;
            i++;
        }

        // fallback (safety; should never get here)
        foreach (var v in visited)
            return v;

        return Vector2Int.zero;
    }

    private void CarveBranchFrom(Vector2Int branchStart, HashSet<Vector2Int> globalVisited)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(branchStart);

        int safetyLocal = mazeWidth * mazeHeight * 4;

        while (stack.Count > 0 && safetyLocal-- > 0 && globalVisited.Count < mazeWidth * mazeHeight)
        {
            Vector2Int current = stack.Peek();

            // collect unvisited neighbours
            List<Vector2Int> unvisitedNeighbours = new List<Vector2Int>();
            foreach (var dir in CardinalDirections)
            {
                Vector2Int n = current + dir;

                if (n.x < 0 || n.x >= mazeWidth || n.y < 0 || n.y >= mazeHeight)
                    continue;

                if (!globalVisited.Contains(n))
                    unvisitedNeighbours.Add(n);
            }

            if (unvisitedNeighbours.Count == 0)
            {
                // no options from here, backtrack
                stack.Pop();
                continue;
            }

            // choose a random unvisited neighbour
            Vector2Int next = unvisitedNeighbours[UnityEngine.Random.Range(0, unvisitedNeighbours.Count)];
            Vector2Int dirToNext = next - current;

            if (TryOpenWall(current, dirToNext))
            {
                globalVisited.Add(next);
                stack.Push(next);
            }
            else
            {
                // couldn't open; next iteration will recalc neighbours
            }
        }
    }

    // =========================================================
    // SHORTEST PATH (BFS) ON virtualMaze
    // =========================================================
    private int ComputeShortestPathLength(Vector2Int start, Vector2Int end)
    {
        if (!virtualMaze.ContainsKey(start) || !virtualMaze.ContainsKey(end))
            return -1;

        var queue = new Queue<Vector2Int>();
        var dist = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        dist[start] = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentDist = dist[current];

            if (current == end)
                return currentDist;

            foreach (var dir in CardinalDirections)
            {
                Vector2Int next = current + dir;

                if (!virtualMaze.ContainsKey(next))
                    continue;

                if (HasWallBetween(current, next))
                    continue;

                if (dist.ContainsKey(next))
                    continue;

                dist[next] = currentDist + 1;
                queue.Enqueue(next);
            }
        }

        return -1; // no path
    }

    private bool HasWallBetween(Vector2Int a, Vector2Int b)
    {
        Vector2Int dir = b - a;

        if (!virtualMaze.TryGetValue(a, out var tileA))
            return true;

        if (!virtualMaze.TryGetValue(b, out var tileB))
            return true;

        // if either side says there's a wall in that direction, it's blocked
        if (tileA.HasWall(dir))
            return true;

        if (tileB.HasWall(-dir))
            return true;

        return false;
    }

    private Dictionary<Vector2Int, VirtualTile2DWithWalls> CloneMaze(
        Dictionary<Vector2Int, VirtualTile2DWithWalls> source)
    {
        var clone = new Dictionary<Vector2Int, VirtualTile2DWithWalls>(source.Count);

        foreach (var kvp in source)
        {
            var newTile = new VirtualTile2DWithWalls
            {
                wallDirections = new List<Vector2Int>(kvp.Value.wallDirections)
            };
            clone[kvp.Key] = newTile;
        }

        return clone;
    }

    // =========================================================
    // Object placement
    // =========================================================
    private void PlaceMazeObjects(IReadOnlyList<Vector2Int> mainPath)
    {
        mazeObjects.Clear();

        if (mazeObjectDefinitions == null || mazeObjectDefinitions.Count == 0)
            return;

        for (int i = 0; i < mazeObjectDefinitions.Count; i++)
        {
            MazeObject objDef = mazeObjectDefinitions[i];
            if (objDef == null || objDef.Prefab == null)
                continue;

            // probability that THIS TYPE of object appears
            if (Random.value > objDef.probability)
                continue;

            int amount = Mathf.Clamp(
                Random.Range(objDef.minimumAmount, objDef.maximumAmount + 1),
                0,
                mazeWidth * mazeHeight
            );
            if (amount <= 0)
                continue;

            for (int k = 0; k < amount; k++)
            {
                Vector2Int? chosen = ChooseCellForObject(objDef, mainPath);
                if (chosen.HasValue)
                {
                    mazeObjects[chosen.Value] = objDef;
                }
                else
                {
                    // couldn't find a valid spot for this instance; skip
                }
            }
        }
    }

    private Vector2Int? ChooseCellForObject(MazeObject objDef, IReadOnlyList<Vector2Int> mainPath)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        // 1) Based on ObjectPlacement
        Vector2Int? forcedPathCell = null;

        switch (objDef.objectPlacement)
        {
            case MazeObject.ObjectPlacement.AtStartOfMainPath:
                if (mainPath != null && mainPath.Count > 0)
                {
                    var c = mainPath[0];
                    candidates.Add(c);
                    forcedPathCell = c;
                }
                break;

            case MazeObject.ObjectPlacement.AtEndOfMainPath:
                if (mainPath != null && mainPath.Count > 0)
                {
                    var c = mainPath[mainPath.Count - 1];
                    candidates.Add(c);
                    forcedPathCell = c;
                }
                break;

            case MazeObject.ObjectPlacement.AnywhereNotYetOccupied:
                for (int x = 0; x < mazeWidth; x++)
                {
                    for (int y = 0; y < mazeHeight; y++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (!mazeObjects.ContainsKey(cell))
                            candidates.Add(cell);
                    }
                }
                break;
        }

        if (candidates.Count == 0)
            return null;

        // 2) BorderPlacement
        bool wantBorder;
        switch (objDef.shouldBePlacedOnBorder)
        {
            case MazeObject.BorderPlacement.Should:
                wantBorder = true;
                candidates = FilterByBorder(candidates, wantBorder);
                break;

            case MazeObject.BorderPlacement.ShouldNot:
                wantBorder = false;
                candidates = FilterByBorder(candidates, wantBorder);
                break;

            case MazeObject.BorderPlacement.Optional:
            default:
                wantBorder = Random.value < objDef.probabilityIfOptional;
                candidates = FilterByBorder(candidates, wantBorder, allowFallback: true);
                break;
        }

        // >>> SPECIAL HANDLING FOR START/END <<<
        if (candidates.Count == 0 && forcedPathCell.HasValue)
        {
            Debug.LogWarning(
                $"[MazeProceduralGeneratorV2] Border rule for {objDef.objectPlacement} " +
                $"eliminated all options. Using cell {forcedPathCell.Value} anyway.");
            candidates.Add(forcedPathCell.Value);
        }

        if (candidates.Count == 0)
            return null;

        // 3) Shuffle candidates to randomize
        Shuffle(candidates);

        // 4) Check minimum distances
        foreach (var cell in candidates)
        {
            if (mazeObjects.ContainsKey(cell))
                continue;

            if (IsTooCloseToOtherObjects(cell, objDef))
                continue;

            return cell;
        }

        // No cell satisfies distances
        Debug.LogWarning(
            $"[MazeProceduralGeneratorV2] Could not place object '{objDef.Prefab?.name}' " +
            $"for placement {objDef.objectPlacement} while respecting distances.");
        return null;
    }

    private List<Vector2Int> FilterByBorder(List<Vector2Int> input, bool border, bool allowFallback = false)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var c in input)
        {
            bool isBorder = IsBorderCell(c);
            if (border && isBorder)
                result.Add(c);
            else if (!border && !isBorder)
                result.Add(c);
        }

        if (result.Count == 0 && allowFallback)
            return input; // couldn't apply preference, keep original list

        return result;
    }

    private bool IsBorderCell(Vector2Int cell)
    {
        return cell.x == 0 ||
               cell.y == 0 ||
               cell.x == mazeWidth - 1 ||
               cell.y == mazeHeight - 1;
    }

    private bool IsTooCloseToOtherObjects(Vector2Int candidate, MazeObject objDef)
    {
        foreach (var kvp in mazeObjects)
        {
            Vector2Int otherPos = kvp.Key;
            MazeObject otherDef = kvp.Value;

            float dist = GridDistance(candidate, otherPos);

            bool sameType = ReferenceEquals(otherDef, objDef);

            if (sameType)
            {
                if (dist < objDef.minimumDistanceFromSimilarity)
                    return true;
            }
            else
            {
                if (dist < objDef.minimumDistanceFromOtherObjects)
                    return true;
            }
        }

        return false;
    }

    private float GridDistance(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    // =========================================================
    // Visual
    // =========================================================
    public void BuildMazeVisual()
    {
        BuildParentsIfNotExist();

        // 1) CLEAR OLD CHILDREN
        if (mazeElementsGroundSubParent != null)
        {
            for (int i = mazeElementsGroundSubParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mazeElementsGroundSubParent.GetChild(i).gameObject);
            }
        }

        if (mazeElementsWallSubParent != null)
        {
            for (int i = mazeElementsWallSubParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mazeElementsWallSubParent.GetChild(i).gameObject);
            }
        }

        if (mazeElementsObjectsSubParent != null)
        {
            for (int i = mazeElementsObjectsSubParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mazeElementsObjectsSubParent.GetChild(i).gameObject);
            }
        }

        if (mazeElementsCornersSubParent != null)
        {
            for (int i = mazeElementsCornersSubParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mazeElementsCornersSubParent.GetChild(i).gameObject);
            }
        }

        // 2) GET PREFAB SIZES
        var groundRenderer = mazeGroundPrefab.GetComponentInChildren<Renderer>();
        var wallRenderer   = mazeWallPrefab.GetComponentInChildren<Renderer>();

        // base materials
        Material floorMat = groundRenderer.sharedMaterial;
        Material wallMat  = wallRenderer.sharedMaterial;

        // world sizes
        float tileSizeX = groundRenderer.bounds.size.x;
        float tileSizeZ = groundRenderer.bounds.size.z;

        Vector3 wallBaseSize   = wallRenderer.bounds.size;
        float baseWallThickness = wallBaseSize.x; // the "thin" axis of the wall prefab
        float baseWallLength    = wallBaseSize.z; // the "long" axis of the wall prefab
        float baseWallHeight    = wallBaseSize.y;

        // floor actual height
        float groundHeight = groundRenderer.bounds.size.y;
        // Y scale for flattened walls to match floor height
        float flatWallScaleY = groundHeight / baseWallHeight;

        // actual wall thickness
        float wallThickness = baseWallThickness * wallThicknessScale;

        // stride from one tile center to the next (tile + the wall gap between them)
        float strideX = tileSizeX + wallThickness;
        float strideZ = tileSizeZ + wallThickness;

        // center the maze under the parent
        float originX = -((mazeWidth  - 1) * strideX) * 0.5f;
        float originZ = -((mazeHeight - 1) * strideZ) * 0.5f;

        GameObject newWall, newTile;

        for (int z = 0; z < mazeHeight; z++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                Vector2Int cellPos = new Vector2Int(x, z);
                var tileData = virtualMaze[cellPos];

                // world center of the tile
                float cx = originX + x * strideX;
                float cz = originZ + z * strideZ;

                // ---------- TILE (floor) ----------
                newTile = Instantiate(mazeGroundPrefab, mazeElementsGroundSubParent);
                newTile.transform.localPosition = new Vector3(
                    cx,
                    0f,
                    cz
                );

                // =====================================================
                // LEFT / RIGHT WALLS
                // =====================================================

                // LEFT: draw only on the left border (x == 0)
                if (x == 0)
                {
                    bool hasLeftWall = tileData.HasWall(Vector2Int.left);
                    float leftX = cx - (tileSizeX * 0.5f + wallThickness * 0.5f);

                    newWall = Instantiate(mazeWallPrefab, mazeElementsWallSubParent);
                    newWall.transform.localRotation = Quaternion.identity; // long axis along Z
                    newWall.transform.localScale = new Vector3(
                        wallThicknessScale,                                   // relative thickness
                        hasLeftWall ? wallHeightScale : flatWallScaleY,      // tall or flattened
                        tileSizeZ / baseWallLength                           // length covers the tile
                    );
                    newWall.transform.localPosition = new Vector3(
                        leftX,
                        0f,
                        cz
                    );

                    var wallInstRenderer = newWall.GetComponentInChildren<Renderer>();
                    wallInstRenderer.sharedMaterial = hasLeftWall ? wallMat : floorMat;
                }

                // RIGHT: always; this cell owns its right wall
                {
                    bool hasRightWall = tileData.HasWall(Vector2Int.right);
                    float rightX = cx + (tileSizeX * 0.5f + wallThickness * 0.5f);

                    newWall = Instantiate(mazeWallPrefab, mazeElementsWallSubParent);
                    newWall.transform.localRotation = Quaternion.identity;
                    newWall.transform.localScale = new Vector3(
                        wallThicknessScale,
                        hasRightWall ? wallHeightScale : flatWallScaleY,
                        tileSizeZ / baseWallLength
                    );
                    newWall.transform.localPosition = new Vector3(
                        rightX,
                        0f,
                        cz
                    );

                    var wallInstRenderer = newWall.GetComponentInChildren<Renderer>();
                    wallInstRenderer.sharedMaterial = hasRightWall ? wallMat : floorMat;
                }

                // =====================================================
                // FRONT / BACK WALLS (DOWN / UP)
                // =====================================================

                // DOWN (front): only on the bottom border (z == 0)
                if (z == 0)
                {
                    bool hasDownWall = tileData.HasWall(Vector2Int.down);
                    float frontZ = cz - (tileSizeZ * 0.5f + wallThickness * 0.5f);

                    newWall = Instantiate(mazeWallPrefab, mazeElementsWallSubParent);
                    newWall.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // long axis along X
                    newWall.transform.localScale = new Vector3(
                        wallThicknessScale,
                        hasDownWall ? wallHeightScale : flatWallScaleY,
                        tileSizeX / baseWallLength
                    );
                    newWall.transform.localPosition = new Vector3(
                        cx,
                        0f,
                        frontZ
                    );

                    var wallInstRenderer = newWall.GetComponentInChildren<Renderer>();
                    wallInstRenderer.sharedMaterial = hasDownWall ? wallMat : floorMat;
                }

                // UP (back): always; this cell owns its top wall
                {
                    bool hasUpWall = tileData.HasWall(Vector2Int.up);
                    float backZ = cz + (tileSizeZ * 0.5f + wallThickness * 0.5f);

                    newWall = Instantiate(mazeWallPrefab, mazeElementsWallSubParent);
                    newWall.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    newWall.transform.localScale = new Vector3(
                        wallThicknessScale,
                        hasUpWall ? wallHeightScale : flatWallScaleY,
                        tileSizeX / baseWallLength
                    );
                    newWall.transform.localPosition = new Vector3(
                        cx,
                        0f,
                        backZ
                    );

                    var wallInstRenderer = newWall.GetComponentInChildren<Renderer>();
                    wallInstRenderer.sharedMaterial = hasUpWall ? wallMat : floorMat;
                }

                // =====================================================
                // OBJECTS AT THE CELL CENTER
                // =====================================================
                if (mazeObjects != null && mazeObjects.TryGetValue(cellPos, out MazeObject objDef))
                {
                    GameObject obj = Instantiate(objDef.Prefab, mazeElementsObjectsSubParent);
                    // Simple Y: on top of the floor (adjust if needed)
                    obj.transform.localPosition = new Vector3(
                        cx,
                        groundHeight,
                        cz
                    );
                }
            }
        }

        // =====================================================
        // CORNERS (grid intersections)
        // =====================================================
        if (cornerPrefab != null && virtualCorners != null && virtualCorners.Count > 0)
        {
            // Each step in the virtual corner grid maps to 'stride'.
            // A corner at virtual coord (i, j) sits at:
            // origin + (i - 0.5) * stride in X, origin + (j - 0.5) * stride in Z.
            foreach (var kvp in virtualCorners)
            {
                Vector2Int c = kvp.Key;

                float wx = originX + (c.x - 0.5f) * strideX;
                float wz = originZ + (c.y - 0.5f) * strideZ;

                var cornerGO = Instantiate(cornerPrefab, mazeElementsCornersSubParent);
                cornerGO.transform.localScale = new Vector3(wallThicknessScale, wallHeightScale, wallThicknessScale);
                cornerGO.transform.localPosition = new Vector3(wx, 0f, wz); // assumes prefab pivot at base
                // If your corner prefab should be flush with floor top, consider y = groundHeight.
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
            mazeElementsGroundSubParent.SetParent(mazeElementsParent, false);
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
            mazeElementsWallSubParent.SetParent(mazeElementsParent, false);
        }
        else
        {
            mazeElementsWallSubParent = foundWall;
        }

        // Objects
        Transform foundObjects = mazeElementsParent.transform.Find("ObjectsSubParent");
        if (foundObjects == null)
        {
            mazeElementsObjectsSubParent = new GameObject("ObjectsSubParent").transform;
            mazeElementsObjectsSubParent.SetParent(mazeElementsParent, false);
        }
        else
        {
            mazeElementsObjectsSubParent = foundObjects;
        }

        // NEW: Corners
        Transform foundCorners = mazeElementsParent.transform.Find("CornersSubParent");
        if (foundCorners == null)
        {
            mazeElementsCornersSubParent = new GameObject("CornersSubParent").transform;
            mazeElementsCornersSubParent.SetParent(mazeElementsParent, false);
        }
        else
        {
            mazeElementsCornersSubParent = foundCorners;
        }
    }

    // =========================================================
    // Inner classes
    // =========================================================
    private class VirtualTile2DWithWalls
    {
        /// <summary>
        /// Directions that still have a wall.
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

        public bool HasWall(Vector2Int direction)
        {
            return wallDirections.Contains(direction);
        }

        public void RemoveWall(Vector2Int direction)
        {
            wallDirections.Remove(direction);
        }
    }

    private class VirtualCorner2D
    {
        // How many walls are still connected to this corner
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
            // if it's already at 1, no more can be removed
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
            if (newMinValue > newMaxValue)
            {
                throw new ArgumentException("minValue cannot be greater than maxValue.", nameof(newMinValue));
            }

            minValue = newMinValue;
            maxValue = newMaxValue;
            CurrentValue = minValue;
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
                }
                else if (currentValue > MaxValue)
                {
                    currentValue = MaxValue;
                }

                return currentValue;
            }
            set => currentValue = value;
        }
    }

    // =========================================================
    // MazeObject
    // =========================================================
    [Serializable]
    public class MazeObject
    {
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;

        [Min(0)] public int minimumAmount = 1;
        [Min(0)] public int maximumAmount = 10;

        [Range(0.01f, 1f)] public float probability = 1f;

        [Min(0)] public float minimumDistanceFromSimilarity = 0f;
        [Min(0)] public float maximumDistanceFromSimilarity = 10f; // max not used yet

        [Min(0)] public float minimumDistanceFromOtherObjects = 0f;
        [Min(0)] public float maximumDistanceFromOtherObjects = 10f; // ditto

        public int priorityIfThereAreDistanceConflicts = 10;

        public enum ObjectPlacement
        {
            AtStartOfMainPath,
            AtEndOfMainPath,
            AnywhereNotYetOccupied
        }

        public ObjectPlacement objectPlacement = ObjectPlacement.AnywhereNotYetOccupied;

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
