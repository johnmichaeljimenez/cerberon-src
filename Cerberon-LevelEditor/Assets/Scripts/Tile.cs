using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData
{
    public System.Numerics.Vector2 Position;
    public System.Numerics.Vector2 Scale;
    public float Angle;
    public string SpriteID;
    public float Depth;
    public int SortingIndex;
}

public class Tile : BaseObject
{
    public override object Export()
    {
        var spr = GetComponent<SpriteRenderer>();

        return new TileData()
        {
            SpriteID = spr.sprite.name,
            Position = new System.Numerics.Vector2(transform.position.x * MAP_SCALE, transform.position.y * MAP_SCALE_Y),
            Angle = -transform.eulerAngles.z * Mathf.Deg2Rad,
            Scale = new System.Numerics.Vector2(spr.size.x * MAP_SCALE, spr.size.y * MAP_SCALE),
            Depth = 0,
            SortingIndex = spr.sortingOrder
        };
    }
}
