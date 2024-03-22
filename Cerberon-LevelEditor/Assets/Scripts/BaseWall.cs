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

public class BaseWall : BaseObject
{
    public override void Export(List<byte> array)
    {
        var _c = GetComponent<Collider2D>();
        var pos = transform.position + (Vector3)_c.offset;

        if (_c is BoxCollider2D b)
        {
            array.AddRange(BitConverter.GetBytes((int)MapDataTypes.LevelBoxCollider));
            array.AddRange(BitConverter.GetBytes(pos.x));
            array.AddRange(BitConverter.GetBytes(pos.y));
            array.AddRange(BitConverter.GetBytes(b.size.x));
            array.AddRange(BitConverter.GetBytes(b.size.y));
            array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));
        }
        else if (_c is CircleCollider2D c)
        {
            array.AddRange(BitConverter.GetBytes((int)MapDataTypes.LevelCircleCollider));
            array.AddRange(BitConverter.GetBytes(pos.x));
            array.AddRange(BitConverter.GetBytes(pos.y));
            array.AddRange(BitConverter.GetBytes(c.radius));
        }
    }
}