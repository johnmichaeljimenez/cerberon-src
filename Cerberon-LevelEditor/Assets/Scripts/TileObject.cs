using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TileObject : BaseObject
{
    public override void Export(List<byte> array)
    {
        var s = GetComponent<SpriteRenderer>();

        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(transform.position.z));
        array.AddRange(BitConverter.GetBytes(-transform.eulerAngles.z));
        array.AddRange(BitConverter.GetBytes(s.size.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(s.size.y * MAP_SCALE));
        array.AddRange(Encoding.ASCII.GetBytes(s.sprite.name.ToFixedLength(32)));
        array.AddRange(BitConverter.GetBytes(s.sortingOrder));
        array.AddRange(BitConverter.GetBytes(s.color.r));
        array.AddRange(BitConverter.GetBytes(s.color.g));
        array.AddRange(BitConverter.GetBytes(s.color.b));
    }
}
