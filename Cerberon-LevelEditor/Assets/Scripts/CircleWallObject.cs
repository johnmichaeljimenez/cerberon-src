using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CircleWallObject : BaseWall
{
    public override void Export(List<byte> array)
    {
        var radius = GetComponent<CircleCollider2D>().radius * Mathf.Max(transform.localScale.x, transform.localScale.y);

        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(radius * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(0f));
        array.AddRange(BitConverter.GetBytes(true));
    }
}