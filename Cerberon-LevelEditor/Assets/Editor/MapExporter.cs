using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MapExporter : MonoBehaviour
{
    public const float MAP_SCALE = 64.0f;

    struct MapData
    {
        public float PlayerPosX;
        public float PlayerPosY;
        public float PlayerRot;

        public WallData[] WallData;
    }

    struct WallData
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public WallSegmentData[] WallSegmentData;
    }

    struct WallSegmentData
    {
        public float X1, Y1, X2, Y2;
    }

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

        var mapData = new MapData();
        var root = rootObjects[0].transform;

        var player = root.Find("Player Character");
        if (player != null)
        {
            EditorUtility.DisplayDialog("Error!", "'Player Character' object is missing!", "OK");
            return;
        }

        //save player data
        mapData.PlayerPosX = player.position.x * MAP_SCALE;
        mapData.PlayerPosY = player.position.y * MAP_SCALE;
        mapData.PlayerRot = player.eulerAngles.z;

        var wallContainer = root.Find("Walls");
        if (wallContainer != null)
        {
            EditorUtility.DisplayDialog("Error!", "'Walls' object is missing!", "OK");
            return;
        }

        //save walls data
        var wallColliders = wallContainer.GetComponentsInChildren<BoxCollider2D>();
        mapData.WallData = new WallData[wallColliders.Length];

        n = 0;
        foreach (var i in wallColliders)
        {
            var b = i.bounds;
            mapData.WallData[n] = new WallData()
            {
                X = b.center.x,
                Y = b.center.y,
                Width = b.size.x,
                Height = b.size.y,
                WallSegmentData = new WallSegmentData[]
                {
                   new WallSegmentData(){ X1 = b.min.x, Y1 = b.max.y, X2 = b.max.x, Y2 = b.max.y },
                   new WallSegmentData(){ X1 = b.max.x, Y1 = b.max.y, X2 = b.max.x, Y2 = b.min.y },
                   new WallSegmentData(){ X1 = b.max.x, Y1 = b.min.y, X2 = b.min.x, Y2 = b.min.y },
                   new WallSegmentData(){ X1 = b.min.x, Y1 = b.min.y, X2 = b.min.x, Y2 = b.max.y },
                }
            };

            n++;
        }

        //save to file
    }
}
