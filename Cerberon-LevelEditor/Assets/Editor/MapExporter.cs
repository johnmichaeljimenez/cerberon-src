using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;
using System.IO.Compression;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using static Codice.Client.BaseCommands.Import.Commit;

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


        string DEF_TYPE = "$t";
        string ENC_TYPE = "_jsonType";

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        var data = new LevelData()
        {
            PlayerPosition = new System.Numerics.Vector2(player.position.x, player.position.y),
            PlayerRotation = player.eulerAngles.z
        };

        var str = JsonConvert.SerializeObject(data, Formatting.Indented, settings).Replace(DEF_TYPE, ENC_TYPE);

        File.WriteAllText(fname, str);
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

    [Serializable]
    public class LevelData
    {
        public System.Numerics.Vector2 PlayerPosition;
        public float PlayerRotation;
    }
}
