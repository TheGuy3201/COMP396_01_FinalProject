using System.Collections.Generic;
using UnityEngine;

public class MazeProceduralGeneratorV2 : MonoBehaviour
{
    // 1.4 - variáveis privadas com SerializeField
    [SerializeField] private int width;
    [SerializeField] private int height;

    // 1.5 - dicionário ainda não inicializado
    // Chave: posição (x, y)
    // Valor: instância da classe do tópico 3
    private Dictionary<Vector2Int, VirtualTile2DWithWalls> virtualMaze;

    // Você pode chamar isso no Start, Awake ou manualmente em outro script.
    private void Start()
    {
        VirtualMaze();
    }

    // 2 - VirtualMaze()
    private void VirtualMaze()
    {
        virtualMaze = new Dictionary<Vector2Int, VirtualTile2DWithWalls>(width * height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var key = new Vector2Int(x, y);
                var tile = new VirtualTile2DWithWalls();

                virtualMaze[key] = tile;
            }
        }
    }
    
    // Exemplo: abrir caminho da célula A para a célula B
    void OpenPassage(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = to - from;        // direção de A -> B
        Vector2Int oppositeDir = -dir;     // direção de B -> A

        var tileA = virtualMaze[from];
        var tileB = virtualMaze[to];

        tileA.RemoveWall(dir);
        tileB.RemoveWall(oppositeDir);
    }

    
    


    // 3 - classe exclusiva para dentro de MazeProceduralGeneratorV2
    // (não pode começar com número, então adaptei o nome)
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
}
