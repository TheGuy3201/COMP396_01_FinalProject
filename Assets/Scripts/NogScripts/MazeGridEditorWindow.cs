using UnityEngine;
using UnityEditor;

public class MazeGridEditorWindow : EditorWindow
{
    private bool[,] grid;
    private int width = 10;
    private int height = 10;
    private Vector2 scroll;

    [MenuItem("Tools/Maze Grid Editor")]
    public static void Open()
    {
        GetWindow<MazeGridEditorWindow>("Maze Grid");
    }

    void OnEnable()
    {
        // inicializa grid
        grid = new bool[width, height];
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Maze boolean grid", EditorStyles.boldLabel);

        width  = EditorGUILayout.IntField("Width",  width);
        height = EditorGUILayout.IntField("Height", height);

        if (grid.GetLength(0) != width || grid.GetLength(1) != height)
            grid = new bool[width, height];

        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        // desenhar a grid
        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < width; x++)
            {
                grid[x, y] = GUILayout.Toggle(grid[x, y], GUIContent.none, GUILayout.Width(20), GUILayout.Height(20));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Print Grid"))
        {
            Debug.Log("GRID:");
            for (int yy = 0; yy < height; yy++)
            {
                string row = "";
                for (int xx = 0; xx < width; xx++)
                    row += grid[xx, yy] ? "1 " : "0 ";
                Debug.Log(row);
            }
        }
    }
}