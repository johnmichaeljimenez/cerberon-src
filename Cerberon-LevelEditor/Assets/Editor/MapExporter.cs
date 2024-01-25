using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;

public class MapExporter : MonoBehaviour
{
    public const float MAP_SCALE = 64.0f;
    public const float MAP_SCALE_Y = -64.0f;

    [MenuItem("Tools/Export MAP file")]
    static void DoSomething()
    {
        int n = 0;
        var scene = EditorSceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();
        if (rootObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Error!", "Only one root object is allowed!", "OK");
            return;
        }

        var root = rootObjects[0].transform;

        var player = root.Find("Player Character");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error!", "'Player Character' object is missing!", "OK");
            return;
        }

        var fname = EditorUtility.SaveFilePanel("Export to", "", $"{scene.name}.map", "map");

        if (string.IsNullOrEmpty(fname))
            return;

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(player.position.x * MAP_SCALE));
        data.AddRange(BitConverter.GetBytes(player.position.y * MAP_SCALE_Y));
        data.AddRange(BitConverter.GetBytes(player.eulerAngles.z));

        Export<BaseWall>(root, data);
        Export<BaseInteractable>(root, data);
        Export<LightObject>(root, data);
        Export<TileObject>(root, data);
        Export<TriggerObject>(root, data);
        Export<OverlayObject>(root, data);

        File.WriteAllBytes(fname, data.ToArray());
        EditorUtility.DisplayDialog("Success!", $"Map exported to {fname}", "OK");
    }

    static void Export<T>(Transform root, List<byte> array) where T : BaseObject
    {
        var objects = root.GetComponentsInChildren<T>();

        array.AddRange(BitConverter.GetBytes(objects.Length));
        foreach (var i in objects)
        {
            i.Export(array);
        }
    }
}
