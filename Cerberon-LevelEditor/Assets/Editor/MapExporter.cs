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
using System.Runtime.Serialization;
using Unity.Plastic.Newtonsoft.Json.Serialization;

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


        //string DEF_TYPE = "$t";
        //string ENC_TYPE = "_jsonType";

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            SerializationBinder = new CustomBinder()
        };

        var data = new LevelData()
        {
            PlayerPosition = new System.Numerics.Vector2(player.position.x * MAP_SCALE, player.position.y * MAP_SCALE_Y),
            PlayerRotation = -player.eulerAngles.z * Mathf.Deg2Rad,

            MapColliders = ExportList<BaseWall, ICollider>(root),
            Tiles = ExportList<Tile, TileData>(root),
            Lights = ExportList<BaseLight, LightSource>(root)
        };

        var str = JsonConvert.SerializeObject(data, Formatting.Indented, settings);//.Replace(DEF_TYPE, ENC_TYPE);

        File.WriteAllText(fname, str);
        EditorUtility.DisplayDialog("Success!", $"Map exported to {fname}", "OK");
    }

    private static List<T2> ExportList<T1, T2>(Transform root) where T1 : BaseObject
    {
        return root.GetComponentsInChildren<T1>().Select(p => p.Export()).Cast<T2>().ToList();
    }

    public class CustomBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}
