using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabExporter : MonoBehaviour
{
    private const string prefabDir = "Prefabs/Props";

    [MenuItem("Tools/Export PREFAB file")]
    static void DoSomething()
    {
        var prefabs = AssetDatabase.FindAssets("t:prefab", new string[] { $"Assets/{prefabDir}" });
        foreach (var item in prefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            var g = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));


        }
    }
}
