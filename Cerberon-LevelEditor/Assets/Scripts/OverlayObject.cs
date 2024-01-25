using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class OverlayObject : BaseObject
{
    public override void Export(List<byte> array)
    {
        var s = GetComponent<SpriteRenderer>();

        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(-transform.eulerAngles.z));
        array.AddRange(BitConverter.GetBytes(transform.localScale.x));
        array.AddRange(BitConverter.GetBytes(transform.localScale.y));
        array.AddRange(Encoding.ASCII.GetBytes(s.sprite.name.ToFixedLength(32)));
        array.AddRange(BitConverter.GetBytes(s.color.a));
    }
}
