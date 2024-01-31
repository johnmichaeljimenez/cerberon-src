using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallHeight
{
    Default,
    Low,
    Crawlspace
}

[RequireComponent(typeof(BoxCollider2D))]
public class WallObject : BaseWall
{
    [SerializeField] private WallHeight height;

    public override void Export(List<byte> array)
    {
        var box = GetComponent<BoxCollider2D>().bounds;

        array.AddRange(BitConverter.GetBytes(transform.position.x + box.center.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y + box.center.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(box.size.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(box.size.y * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(false));
        array.AddRange(BitConverter.GetBytes((int)height));
    }
}