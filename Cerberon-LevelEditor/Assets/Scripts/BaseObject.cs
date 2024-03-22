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
public class LevelData
{
    public System.Numerics.Vector2 PlayerPosition;
    public float PlayerRotation;

    public List<ICollider> MapColliders;
    public List<TileData> Tiles;
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
