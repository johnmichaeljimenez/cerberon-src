using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PrefabExporter : MonoBehaviour
{
    private const string prefabDir = "Prefabs/Props";

    [MenuItem("Tools/Export PREFAB file")]
    static void DoSomething()
    {
        var fname = EditorUtility.SaveFilePanel("Export to", "", $"prefab", "dat");
        var objects = new Dictionary<GameObject, List<Component>>();

        if (string.IsNullOrEmpty(fname))
            return;

        var types = new Type[]{
            typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(CircleCollider2D)
        };

        var data = new List<byte>();
        var prefabs = AssetDatabase.FindAssets("t:prefab", new string[] { $"Assets/{prefabDir}" });
        foreach (var item in prefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            var g = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

            objects.Add(g, new List<Component>());

            foreach (var j in g.GetComponentsInChildren<Component>())
            {
                var valid = types.Contains(j.GetType());

                if (!valid)
                    continue;

                objects[g].Add(j);
            }
        }
        //objects = objects.Where(p => p.Value.Count > 0).ToDictionary(p => p.Key, p => p.Value);


        //object count
        data.AddRange(BitConverter.GetBytes(objects.Count));

        foreach (var obj in objects)
        {
            //object name
            data.AddRange(Encoding.ASCII.GetBytes(obj.Key.name.ToFixedLength(32)));

            //component count
            data.AddRange(BitConverter.GetBytes(obj.Value.Count));
            if (obj.Value.Count > 0)
                continue;

            foreach (var item in obj.Value)
            {
                WriteComponent(item, data);
            }
        }


        File.WriteAllBytes(fname, data.ToArray());
        EditorUtility.DisplayDialog("Success!", $"Prefab data exported to {fname}", "OK");
    }

    private static void WriteComponent(Component c, List<byte> data)
    {

    }
}
