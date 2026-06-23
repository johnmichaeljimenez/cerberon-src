using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace CerberonEditor.Main
{
    //PERSONAL USE ONLY AT THE MOMENT
    public class CerberonExporter : MonoBehaviour
    {
        public Transform playerSpawnPoint;
        public Transform walls;
        public Transform ambientLightContainers;
        public Transform lightContainers;
        public Transform triggerContainers;
        public Transform markerContainers;
        public Transform worldBounds;
        public Transform propsContainers;
        public Transform environmentSprites;
        public Transform entityContainers;
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
            var triggers = new List<object>();
            var markers = new List<object>();

            float posSnap = 0.5f;
            float scaleSnap = 1f;

            foreach (var i in entityContainers.GetComponentsInChildren<EntityObject>(true))
            {
                if (i == entityContainers)
                    continue;

                var en = new Dictionary<string, object>
                {
                    ["Type"] = i.EntityType,
                    ["NameTag"] = i.NameTag,
                    ["Position"] = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    ["Rotation"] = -i.transform.eulerAngles.z,
                    ["IsActive"] = i.gameObject.activeInHierarchy
                };

                if (i is PropObject p)
                {
                    foreach (var j in p.GetProperties())
                    {
                        en[j.Key] = j.Value;
                    }
                }

                var parsedProps = ParseEntityProperties(i.Properties);
                foreach (var kvp in parsedProps)
                {
                    en[kvp.Key] = kvp.Value;
                }

                entities.Add(en);
            }

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
                    Height = wall.color.a <= 0.6f? 0 : 2
                });
            }

            foreach (var i in triggerContainers.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var t = new
                {
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Rotation = -i.transform.eulerAngles.z,
                    Size = new
                    {
                        X = i.size.x,
                        Y = i.size.y
                    },
                    TriggerID = i.gameObject.name,
                    SortingIndex = i.sortingOrder,
                    Enabled = i.gameObject.activeInHierarchy
                };

                triggers.Add(t);
            }

            foreach (var i in ambientLightContainers.GetComponentsInChildren<LightObject>())
            {
                var spr = i.GetComponent<SpriteRenderer>();
                var l = new
                {
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Color = new
                    {
                        R = Mathf.RoundToInt(spr.color.r * 255),
                        G = Mathf.RoundToInt(spr.color.g * 255),
                        B = Mathf.RoundToInt(spr.color.b * 255),
                        A = Mathf.RoundToInt(spr.color.a * 255),
                    },
                    Rotation = -i.transform.eulerAngles.z,
                    Size = new
                    {
                        X = spr.size.x,
                        Y = spr.size.y
                    },
                    Flicker = i.Flicker
                };

                ambientLights.Add(l);
            }

            foreach (var i in lightContainers.GetComponentsInChildren<LightObject>())
            {
                var spr = i.GetComponent<SpriteRenderer>();
                var l = new
                {
                    SpriteID = $"{spr.sprite.name}",
                    GroupID = i.GroupID,
                    Position = new
                    {
                        X = i.transform.position.x,
                        Y = -i.transform.position.y
                    },
                    Color = new
                    {
                        R = Mathf.RoundToInt(spr.color.r * 255),
                        G = Mathf.RoundToInt(spr.color.g * 255),
                        B = Mathf.RoundToInt(spr.color.b * 255),
                        A = Mathf.RoundToInt(spr.color.a * 255),
                    },
                    Rotation = -i.transform.eulerAngles.z,
                    Origin = new
                    {
                        X = 0.5,
                        Y = 0.5
                    },
                    Enabled = true,
                    Scale = Mathf.Max(i.transform.localScale.x, i.transform.localScale.y),

                    ShadowType = i.ShadowType,
                    Flicker = i.Flicker
                };

                lights.Add(l);
            }

            for (int i = 0; i < markerContainers.childCount; i++)
            {
                var t = markerContainers.GetChild(i);

                var m = new
                {
                    Position = new
                    {
                        X = t.transform.position.x,
                        Y = -t.transform.position.y
                    },
                    ID = t.gameObject.name
                };

                markers.Add(m);
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
                AmbientLights = ambientLights,
                Triggers = triggers,
                Markers = markers
            };

            string jsonString = JsonConvert.SerializeObject(world, Formatting.Indented);
            File.WriteAllText($@"E:\Projects\cerberon-src\Assets\Levels\{gameObject.scene.name}.json", jsonString);
        }

        string Normalize(string s)
        {
            var m = Regex.Match(s, @"^(?<base>.+?)(?:\s*\(\d+\))?$");
            return m.Success ? m.Groups["base"].Value : s;
        }

        private Dictionary<string, object> ParseEntityProperties(string propertiesText)
        {
            var dict = new Dictionary<string, object>(StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(propertiesText))
                return dict;

            var lines = propertiesText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || !trimmed.Contains('='))
                    continue;

                var parts = trimmed.Split(new[] { '=' }, 2);
                var key = parts[0].Trim();
                if (string.IsNullOrEmpty(key))
                    continue;

                string valueStr = parts.Length > 1 ? parts[1].Trim() : "";

                if (bool.TryParse(valueStr, out bool boolVal))
                    dict[key] = boolVal;
                else if (int.TryParse(valueStr, out int intVal))
                    dict[key] = intVal;
                else if (float.TryParse(valueStr, out float floatVal))
                    dict[key] = floatVal;
                else
                    dict[key] = valueStr;
            }

            return dict;
        }
    }
}