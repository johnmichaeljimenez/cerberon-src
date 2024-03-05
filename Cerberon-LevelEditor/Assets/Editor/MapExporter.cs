using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;
using ProtoBuf;
using ProtoBuf.Meta;
using static MapExporter;
using System.IO.Compression;

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

        var model = RuntimeTypeModel.Default;
        model.Add<Vector2Surrogate>();
        model.Add<Vector2>(false).SetSurrogate(typeof(Vector2Surrogate));

        var mapData = new MapData();
        mapData.PlayerPosition = new Vector2(player.position.x * MAP_SCALE, player.position.y * MAP_SCALE_Y);
        mapData.PlayerRotation = player.eulerAngles.z;

        using (var i = new FileStream(fname, FileMode.OpenOrCreate))
        {
            using (var gzip = new GZipStream(i, CompressionMode.Compress))
            {
                Serializer.Serialize(gzip, mapData);
            }
        }

        //var data = new List<byte>();
        //data.AddRange(BitConverter.GetBytes(player.position.x * MAP_SCALE));
        //data.AddRange(BitConverter.GetBytes(player.position.y * MAP_SCALE_Y));
        //data.AddRange(BitConverter.GetBytes(player.eulerAngles.z));

        //Export<BaseWall>(root, data);
        //Export<BaseInteractable>(root, data);
        //Export<LightObject>(root, data);
        //Export<TileObject>(root, data);
        //Export<TriggerObject>(root, data);
        //Export<OverlayObject>(root, data);

        //File.WriteAllBytes(fname, data.ToArray());
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

    [ProtoContract]
    public class MapData
    {
        [ProtoMember(1)]
        public Vector2 PlayerPosition;
        [ProtoMember(2)]
        public float PlayerRotation;

        [ProtoMember(3)]
        public List<BaseWall> Walls;
    }

    [ProtoContract]
    public struct Vector2Surrogate
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        public static implicit operator Vector2(Vector2Surrogate vector) =>
            new Vector2(vector.X, vector.Y);

        public static implicit operator Vector2Surrogate(Vector2 vector) =>
            new Vector2Surrogate { X = vector.x, Y = vector.y };
    }
}
