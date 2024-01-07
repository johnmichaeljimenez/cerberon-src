using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WallObject : BaseObject
{
    public override void Export(List<byte> array)
    {
        var box = GetComponent<BoxCollider2D>().bounds;

        array.AddRange(BitConverter.GetBytes(transform.position.x + box.center.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y + box.center.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(box.size.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(box.size.y * MAP_SCALE));
    }
}