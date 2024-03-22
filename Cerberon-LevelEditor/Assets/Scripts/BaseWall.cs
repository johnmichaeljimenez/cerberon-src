using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Flags]
public enum ColliderFlags
{
    None = 0,
    CastShadows = 0 << 1,
    IgnoreLinecast = 0 << 2
}

public interface ICollider
{

}

public class BoxColliderShape : ICollider
{
    public System.Numerics.Vector2 Position;
    public float Rotation;
    public System.Numerics.Vector2 Size;
}

public class CircleColliderShape : ICollider
{
    public System.Numerics.Vector2 Position;
    public float Radius;
}

public class WallColliderShape : ICollider
{
    public System.Numerics.Vector2 From;
    public System.Numerics.Vector2 To;
}

public class BaseWall : BaseObject
{
    public override object Export()
    {
        var _c = GetComponent<Collider2D>();
        var spr = GetComponent<SpriteRenderer>();
        var pos = transform.position + (Vector3)_c.offset;

        if (_c is BoxCollider2D b)
        {
            return new BoxColliderShape()
            {
                Position = new System.Numerics.Vector2(pos.x * MAP_SCALE, pos.y * MAP_SCALE_Y),
                Rotation = -transform.eulerAngles.z * Mathf.Deg2Rad,
                Size = new System.Numerics.Vector2(spr.size.x * MAP_SCALE, spr.size.y * MAP_SCALE)
            };
        }
        else if (_c is CircleCollider2D c)
        {
            return new CircleColliderShape()
            {
                Position = new System.Numerics.Vector2(pos.x * MAP_SCALE, pos.y * MAP_SCALE_Y),
                Radius = c.radius * MAP_SCALE
            };
        }

        return null;
    }
}