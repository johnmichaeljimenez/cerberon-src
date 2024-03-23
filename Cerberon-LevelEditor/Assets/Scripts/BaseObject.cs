using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MapDataTypes
{
    LevelBoxCollider,
    LevelCircleCollider,
    LevelWallCollider,
    Tile,
    Light,
    Door
}

[Serializable]
public struct Color
{
    public byte R, G, B, A;

    public Color(float r, float g, float b, float a)
    {
        R = (byte)(r * 255);
        G = (byte)(g * 255);
        B = (byte)(b * 255);
        A = (byte)(a * 255);
    }
}

[Serializable]
public class LevelData
{
    public System.Numerics.Vector2 PlayerPosition;
    public float PlayerRotation;

    public List<ICollider> MapColliders;
    public List<TileData> Tiles;
    public List<LightSource> Lights;
}

public abstract class BaseObject : MonoBehaviour
{
    public const float MAP_SCALE = 64.0f;
    public const float MAP_SCALE_Y = -64.0f;

    public virtual object Export()
    {
        throw new System.NotImplementedException();
    }
}
