using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CerberonEditor.Main
{
    //PERSONAL USE ONLY AT THE MOMENT
    public class CerberonExporter : MonoBehaviour
    {
        public Transform playerSpawnPoint;
        public Transform walls;
        public Transform ambientLightContainers;
        public Transform lightContainers;
        public Transform worldBounds;
        public Transform propsContainers;
        public Transform environmentSprites;
        public Color ambientColor;

        [ContextMenu("Export")]
        public void Export()
        {
            var rootObject = transform;
            var entities = new List<object>();
            var sprites = new List<object>();
            var ambientLights = new List<object>();
            var lights = new List<object>();
            var colliders = new List<object>();

            float posSnap = 0.5f;
            float scaleSnap = 1f;

            foreach (var wall in walls.GetComponentsInChildren<SpriteRenderer>())
            {
                Vector3 p = wall.transform.position;
                p.x = Mathf.Round(p.x / posSnap) * posSnap;
                p.y = Mathf.Round(p.y / posSnap) * posSnap;
                p.z = 0f;
                wall.transform.position = p;

                Vector3 s = wall.transform.localScale;
                s.x = Mathf.Round(s.x / scaleSnap) * scaleSnap;
                s.y = Mathf.Round(s.y / scaleSnap) * scaleSnap;
                s.z = 1f;
                wall.transform.localScale = s;

                Vector2 size = wall.transform.localScale;

                float centerX = wall.transform.position.x;
                float centerY = -wall.transform.position.y;

                colliders.Add(new
                {
                    Position = new
                    {
                        X = centerX,
                        Y = centerY
                    },
                    Size = new
                    {
                        X = size.x,
                        Y = size.y
                    },
                    Rotation = -wall.transform.eulerAngles.z,
                    Flags = 1,
                    Height = 2
                });
            }

            foreach (var i in ambientLightContainers.GetComponentsInChildren<SpriteRenderer>())
            {
                var l = new
                {
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Color = new
                    {
                        R = Mathf.RoundToInt(i.color.r * 255),
                        G = Mathf.RoundToInt(i.color.g * 255),
                        B = Mathf.RoundToInt(i.color.b * 255),
                        A = Mathf.RoundToInt(i.color.a * 255),
                    },
                    Rotation = -i.transform.eulerAngles.z,
                    Size = new
                    {
                        X = i.size.x,
                        Y = i.size.y
                    },
                };

                ambientLights.Add(l);
            }

            foreach (var i in lightContainers.GetComponentsInChildren<SpriteRenderer>())
            {
                var l = new
                {
                    SpriteID = $"{i.sprite.name}",
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Color = new
                    {
                        R = Mathf.RoundToInt(i.color.r * 255),
                        G = Mathf.RoundToInt(i.color.g * 255),
                        B = Mathf.RoundToInt(i.color.b * 255),
                        A = Mathf.RoundToInt(i.color.a * 255),
                    },
                    Rotation = -i.transform.eulerAngles.z,
                    Origin = new
                    {
                        X = 0.5,
                        Y = 0.5
                    },
                    Enabled = true,
                    Scale = Mathf.Max(i.transform.localScale.x, i.transform.localScale.y),

                    ShadowType = i.CompareTag("Shadow") ? 1 : 0
                };

                lights.Add(l);
            }

            foreach (var i in environmentSprites.GetComponentsInChildren<SpriteRenderer>())
            {
                var spr = new
                {
                    SpriteID = $"env/{i.sprite.name}",
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Parallax = i.transform.position.z,
                    Rotation = -i.transform.eulerAngles.z,
                    SortingGroup = SortingLayer.GetLayerValueFromID(i.sortingLayerID),
                    SortingIndex = i.sortingOrder,
                    RenderType = i.drawMode == SpriteDrawMode.Simple ? 0 : 1,
                    Scale = Mathf.Max(i.transform.localScale.x, i.transform.localScale.y),
                    TileSize = new
                    {
                        X = i.size.x,
                        Y = i.size.y
                    },
                    Tint = new
                    {
                        R = Mathf.RoundToInt(i.color.r * 255),
                        G = Mathf.RoundToInt(i.color.g * 255),
                        B = Mathf.RoundToInt(i.color.b * 255),
                        A = Mathf.RoundToInt(i.color.a * 255),
                    }
                };

                sprites.Add(spr);
            }

            for (int i = 0; i < propsContainers.childCount; i++)
            {
                var p = propsContainers.GetChild(i);
                if (!p.gameObject.activeInHierarchy)
                    continue;

                var propSprites = p.GetComponentsInChildren<SpriteRenderer>();
                var propColliders = p.GetComponentsInChildren<BoxCollider2D>();

                foreach (var c in propColliders)
                {
                    colliders.Add(new
                    {
                        Position = new
                        {
                            X = p.position.x + c.offset.x,
                            Y = -(p.position.y + c.offset.y)
                        },
                        Size = new
                        {
                            X = c.size.x,
                            Y = c.size.y
                        },
                        Rotation = -p.eulerAngles.z,
                        Flags = 0,
                        Height = c.usedByEffector ? 2 : 0
                    });
                }

                foreach (var j in propSprites)
                {
                    var spr = new
                    {
                        SpriteID = $"env/{j.sprite.name}",
                        Position = new
                        {
                            X = j.transform.position.x,
                            Y = -j.transform.position.y
                        },
                        Parallax = j.transform.position.z,
                        Rotation = -j.transform.eulerAngles.z,
                        SortingGroup = SortingLayer.GetLayerValueFromID(j.sortingLayerID),
                        SortingIndex = j.sortingOrder,
                        RenderType = j.drawMode == SpriteDrawMode.Simple ? 0 : 1,
                        Scale = Mathf.Max(j.transform.localScale.x, j.transform.localScale.y),
                        TileSize = new
                        {
                            X = 1,
                            Y = 1
                        },
                        Tint = new
                        {
                            R = Mathf.RoundToInt(j.color.r * 255),
                            G = Mathf.RoundToInt(j.color.g * 255),
                            B = Mathf.RoundToInt(j.color.b * 255),
                            A = Mathf.RoundToInt(j.color.a * 255),
                        }
                    };

                    sprites.Add(spr);
                }
            }

            var world = new
            {
                WorldSettings = new
                {
                    PlayerSpawnPoint = new
                    {
                        X = playerSpawnPoint.position.x,
                        Y = -playerSpawnPoint.position.y
                    },
                    WorldSize = new
                    {
                        X = worldBounds.localScale.x,
                        Y = worldBounds.localScale.y
                    },
                    AmbientColor = new
                    {
                        R = Mathf.RoundToInt(ambientColor.r * 255),
                        G = Mathf.RoundToInt(ambientColor.g * 255),
                        B = Mathf.RoundToInt(ambientColor.b * 255),
                        A = 255
                    }
                },

                Entities = entities,
                EnvironmentSprites = sprites,
                EnvironmentColliders = colliders,
                Lights = lights,
                AmbientLights = ambientLights
            };

            string jsonString = JsonConvert.SerializeObject(world, Formatting.Indented);
            File.WriteAllText($@"E:\Projects\cerberon-src\Assets\Levels\{gameObject.scene.name}.json", jsonString);
        }
    }
}