using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Converters;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PrefabExporter : MonoBehaviour
{
    private const float MAP_SCALE = 64.0f;
    private const float MAP_SCALE_Y = -64.0f;
    private const string prefabDir = "Prefabs/Props";

    public enum PrefabComponentType
    {
        Sprite,
        SpriteTiled,
        BoxCollider,
        CircleCollider,
        EdgeCollider,
        Light
    }

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
            data.AddRange(Encoding.ASCII.GetBytes(obj.Key.name.ToFixedLength(16)));

            //component count
            data.AddRange(BitConverter.GetBytes(obj.Value.Count));
            if (obj.Value.Count == 0)
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
        data.AddRange(BitConverter.GetBytes(c.transform.localPosition.x * MAP_SCALE));
        data.AddRange(BitConverter.GetBytes(c.transform.localPosition.y * MAP_SCALE_Y));
        data.AddRange(BitConverter.GetBytes(c.transform.localPosition.z * MAP_SCALE));
        data.AddRange(BitConverter.GetBytes(-c.transform.localEulerAngles.z));

        if (c is SpriteRenderer spriteRenderer)
        {
            data.AddRange(BitConverter.GetBytes((int)PrefabComponentType.Sprite));
            data.AddRange(Encoding.ASCII.GetBytes(spriteRenderer.sprite.name.ToFixedLength(32)));
            data.AddRange(BitConverter.GetBytes(Mathf.Max(c.transform.localScale.x, c.transform.localScale.y)));

            var sortingGroup = 0;
            if (spriteRenderer.sortingLayerName == "Ground")
                sortingGroup = 0;
            else if (spriteRenderer.sortingLayerName == "Entity")
                sortingGroup = 1;
            else if (spriteRenderer.sortingLayerName == "Wall")
                sortingGroup = 2;
            else if (spriteRenderer.sortingLayerName == "Overlay")
                sortingGroup = 3;

            data.AddRange(BitConverter.GetBytes(sortingGroup));
            data.AddRange(BitConverter.GetBytes(spriteRenderer.sortingOrder));
            data.AddRange(BitConverter.GetBytes(spriteRenderer.color.r));
            data.AddRange(BitConverter.GetBytes(spriteRenderer.color.g));
            data.AddRange(BitConverter.GetBytes(spriteRenderer.color.b));

            return;
        }

        if (c is BoxCollider2D box)
        {
            data.AddRange(BitConverter.GetBytes((int)PrefabComponentType.BoxCollider));
            data.AddRange(BitConverter.GetBytes(box.size.x));
            data.AddRange(BitConverter.GetBytes(box.size.y));
            return;
        }

        if (c is CircleCollider2D circle)
        {
            data.AddRange(BitConverter.GetBytes((int)PrefabComponentType.CircleCollider));
            data.AddRange(BitConverter.GetBytes(circle.radius));
            return;
        }
    }
}
