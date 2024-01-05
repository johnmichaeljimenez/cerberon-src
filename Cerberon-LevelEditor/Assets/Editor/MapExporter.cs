using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MapExporter : MonoBehaviour
{
    public const float MAP_SCALE = 64.0f;

    [MenuItem("Tools/Export MAP file")]
    static void DoSomething()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();
        if (rootObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Error!", "Only one root object is allowed!", "OK");
            return;
        }

        //open file

        var root = rootObjects[0].transform;

        var player = root.Find("Player Character");
        if (player != null)
        {
            EditorUtility.DisplayDialog("Error!", "'Player Character' object is missing!", "OK");
            return;
        }

        //save player data

        var wallContainer = root.Find("Walls");
        if (wallContainer != null)
        {
            EditorUtility.DisplayDialog("Error!", "'Walls' object is missing!", "OK");
            return;
        }

        //save walls data




        //save to file
    }
}
