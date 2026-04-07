using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MazeGenerator maze = (MazeGenerator)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Maze"))
        {
            maze.Generate();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Clear Maze"))
        {
            maze.ClearMaze();
        }
    } 
}
