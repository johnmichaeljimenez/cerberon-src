using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PrefabExporter : MonoBehaviour
{
    private const string prefabDir = "Prefabs/Props";

    [MenuItem("Tools/Export PREFAB file")]
    static void DoSomething()
    {
        var fname = EditorUtility.SaveFilePanel("Export to", "", $"prefab.data", "dat");
        var objects = new Dictionary<GameObject, List<Component>>();

        if (string.IsNullOrEmpty(fname))
            return;

        var data = new List<byte>();
        var prefabs = AssetDatabase.FindAssets("t:prefab", new string[] { $"Assets/{prefabDir}" });
        foreach (var item in prefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            var g = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

            objects.Add(g, new List<Component>());

            foreach (var j in g.GetComponentsInChildren<Component>())
            {
                var valid = false;

                if (!valid)
                    continue;

                objects[g].Add(j);
            }
        }
        objects = objects.Where(p => p.Value.Count > 0).ToDictionary(p => p.Key, p => p.Value);

        foreach (var obj in objects)
        {

        }


        File.WriteAllBytes(fname, data.ToArray());
        EditorUtility.DisplayDialog("Success!", $"Prefab data exported to {fname}", "OK");
    }
}
