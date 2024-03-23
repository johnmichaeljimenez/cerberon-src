using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource
{
    public System.Numerics.Vector2 Position;
    public float Rotation;
    public float Scale;
    public string LightSpriteID;
    public bool CastShadows;
    public Color Color;
}

public class BaseLight : BaseObject
{
    public bool CastShadows;

    public override object Export()
    {
        var spr = GetComponent<SpriteRenderer>();

        return new LightSource()
        {
            LightSpriteID = spr.sprite.name,
            Position = new System.Numerics.Vector2(transform.position.x * MAP_SCALE, transform.position.y * MAP_SCALE_Y),
            Rotation = -transform.eulerAngles.z * Mathf.Deg2Rad,
            Scale = Mathf.Max(transform.localScale.x, transform.localScale.y) * MAP_SCALE,
            CastShadows = CastShadows,
            Color = new Color(spr.color.r, spr.color.g, spr.color.b, spr.color.a)
        };
    }
}
