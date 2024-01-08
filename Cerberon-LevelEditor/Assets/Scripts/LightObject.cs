using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightObject : BaseObject
{
    public override void Export(List<byte> array)
    {
        var scale = (transform.localScale.x + transform.localScale.y) / 2;
        scale = scale * MAP_SCALE;

        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));
    }
}